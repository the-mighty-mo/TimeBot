﻿using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace TimeBot.Modules
{
    public class Ping : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ping", "Ping the bot")]
        public async Task PingAsync()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription(":ping_pong:**Pong!**")
                .WithCurrentTimestamp();
            await Context.Interaction.RespondAsync(embed: embed.Build());
            var response = await Context.Interaction.GetOriginalResponseAsync();

            embed.WithDescription(":ping_pong:**Pong!**\n" +
                $"**Server:** {(int)(response.Timestamp - Context.Interaction.CreatedAt).TotalMilliseconds}ms\n" +
                $"**API:** {Context.Client.Latency}ms");

            await response.ModifyAsync(x => x.Embed = embed.Build());
        }
    }
}