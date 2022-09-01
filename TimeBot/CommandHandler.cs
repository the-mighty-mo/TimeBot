﻿using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using static TimeBot.DatabaseManager;

namespace TimeBot
{
    public class CommandHandler
    {
        public const string prefix = "\\";
        private static int argPos = 0;

        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly InteractionService interactions;
        private readonly IServiceProvider services;
        private readonly TimeEventHandler time;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            this.client = client;
            this.services = services;

            InteractionServiceConfig interactionCfg = new()
            {
                DefaultRunMode = Discord.Interactions.RunMode.Async
            };
            interactions = new(client.Rest, interactionCfg);

            CommandServiceConfig commandCfg = new()
            {
                DefaultRunMode = Discord.Commands.RunMode.Async
            };
            commands = new(commandCfg);

            time = new TimeEventHandler(new TimeSpan(2, 17, 00));
        }

        public async Task InitCommandsAsync()
        {
            client.Ready += ReadyAsync;
            client.Connected += SendConnectMessage;
            client.Disconnected += SendDisconnectError;
            client.MessageReceived += HandleCommandAsync;
            client.SlashCommandExecuted += HandleSlashCommandAsync;

            time.Time += SendTimeMessageAsync;
            time.TimeEventError += HandleTimeEventErrorAsync;
            time.StartProcess();

            await Task.WhenAll(
                interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services),
                commands.AddModulesAsync(Assembly.GetEntryAssembly(), services)
            );
            interactions.SlashCommandExecuted += SendInteractionErrorAsync;
            commands.CommandExecuted += SendCommandErrorAsync;
        }

        private async Task ReadyAsync()
        {
            await interactions.RegisterCommandsGloballyAsync(true);
        }

        private async Task SendInteractionErrorAsync(SlashCommandInfo info, IInteractionContext context, Discord.Interactions.IResult result)
        {
            if (!result.IsSuccess && info.RunMode == Discord.Interactions.RunMode.Async && result.Error is not (InteractionCommandError.UnknownCommand or InteractionCommandError.UnmetPrecondition))
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }

        private async Task SendCommandErrorAsync(Optional<CommandInfo> info, ICommandContext context, Discord.Commands.IResult result)
        {
            if (!result.IsSuccess && info.GetValueOrDefault()?.RunMode == Discord.Commands.RunMode.Async && result.Error is not (CommandError.UnknownCommand or CommandError.UnmetPrecondition))
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }

        private Task SendConnectMessage() =>
            Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");

        private Task SendDisconnectError(Exception e) =>
            Console.Out.WriteLineAsync(e.Message);

        private async Task SendTimeMessageAsync()
        {
            foreach (SocketGuild g in client.Guilds)
            {
                SocketTextChannel channel = await channelsDatabase.Channels.GetTimeChannelAsync(g);
                if (channel != null)
                {
                    await channel.SendMessageAsync("Time");
                }
            }
        }

        private Task HandleTimeEventErrorAsync(Exception e) =>
            Console.Out.WriteLineAsync("Error: Time message failed (see below).\n" + e.Message);

        private static Task<bool> CanBotRunCommandsAsync(SocketUser usr) => Task.Run(() => false);

        private static Task<bool> ShouldDeleteBotCommands() => Task.Run(() => true);

        private async Task HandleSlashCommandAsync(SocketSlashCommand m)
        {
            if (m.User.IsBot && !await CanBotRunCommandsAsync(m.User))
            {
                return;
            }

            SocketInteractionContext Context = new(client, m);

            var result = await interactions.ExecuteCommandAsync(Context, services);

            List<Task> cmds = new();
            if (m.User.IsBot && await ShouldDeleteBotCommands())
            {
                cmds.Add(m.DeleteOriginalResponseAsync());
            }
            else if (!result.IsSuccess && result.Error == InteractionCommandError.UnmetPrecondition)
            {
                cmds.Add(Context.Channel.SendMessageAsync(result.ErrorReason));
            }

            await Task.WhenAll(cmds);
        }

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (m is not SocketUserMessage msg || (msg.Author.IsBot && !await CanBotRunCommandsAsync(msg.Author)))
            {
                return;
            }

            SocketCommandContext Context = new(client, msg);
            bool isCommand = msg.HasMentionPrefix(client.CurrentUser, ref argPos) || msg.HasStringPrefix(prefix, ref argPos);

            if (isCommand)
            {
                var result = await commands.ExecuteAsync(Context, argPos, services);

                List<Task> cmds = new();
                if (msg.Author.IsBot && await ShouldDeleteBotCommands())
                {
                    cmds.Add(msg.DeleteAsync());
                }
                else if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
                {
                    cmds.Add(Context.Channel.SendMessageAsync(result.ErrorReason));
                }

                await Task.WhenAll(cmds);
            }
        }
    }
}