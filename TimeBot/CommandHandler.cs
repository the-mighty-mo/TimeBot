﻿using Discord;
using Discord.Commands;
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
        public static int argPos = 0;

        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;
        private readonly TimeEventHandler time;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            this.client = client;
            this.services = services;

            CommandServiceConfig config = new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async
            };
            commands = new CommandService(config);

            time = new TimeEventHandler(new TimeSpan(2, 17, 00));
        }

        public async Task InitCommandsAsync()
        {
            client.Connected += SendConnectMessage;
            client.Disconnected += SendDisconnectError;
            client.MessageReceived += HandleCommandAsync;

            time.Time += SendTimeMessageAsync;
            time.TimeEventError += HandleTimeEventErrorAsync;
            time.StartProcess();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            commands.CommandExecuted += SendErrorAsync;
        }

        private async Task SendErrorAsync(Optional<CommandInfo> info, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess && info.Value.RunMode == RunMode.Async && result.Error != CommandError.UnknownCommand && result.Error != CommandError.UnmetPrecondition)
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }

        private async Task SendConnectMessage()
        {
            await Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");
        }

        private async Task SendDisconnectError(Exception e)
        {
            await Console.Out.WriteLineAsync(e.Message);
        }

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

        private async Task HandleTimeEventErrorAsync(Exception e)
        {
            await Task.Run(() => Console.WriteLine("Error: Time message failed (see below).\n" + e.Message));
        }

        private async Task<bool> CanBotRunCommandsAsync(SocketUserMessage msg) => await Task.Run(() => false);
        private async Task<bool> ShouldDeleteBotCommands() => await Task.Run(() => true);

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg) || (msg.Author.IsBot && !await CanBotRunCommandsAsync(msg)))
            {
                return;
            }

            SocketCommandContext Context = new SocketCommandContext(client, msg);
            bool isCommand = msg.HasMentionPrefix(client.CurrentUser, ref argPos) || msg.HasStringPrefix(prefix, ref argPos);

            if (isCommand)
            {
                var result = await commands.ExecuteAsync(Context, argPos, services);

                List<Task> cmds = new List<Task>();
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
