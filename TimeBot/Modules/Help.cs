﻿using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace TimeBot.Modules
{
    public class Help : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("help", "List of commands")]
        public async Task HelpAsync()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle(SecurityInfo.botName);

            EmbedFieldBuilder prefix = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Prefix")
                .WithValue("\\" +
                    "\n**or**\n" +
                    Context.Client.CurrentUser.Mention + "\n\u200b");
            embed.AddField(prefix);

            EmbedFieldBuilder field = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Commands")
                .WithValue(
                    "ping\n" +
                    "  - Returns the bot's Server and API latencies\n\n" +
                    "setchannel [channel mention/channel ID]\n" +
                    "  - Sets the time channel"
                );
            embed.AddField(field);

            await Context.Interaction.RespondAsync(embed: embed.Build());
        }
    }
}