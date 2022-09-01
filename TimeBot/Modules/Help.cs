using Discord;
using Discord.Interactions;
using System.Collections.Generic;
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

            List<EmbedFieldBuilder> fields = new();

            EmbedFieldBuilder field = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Commands")
                .WithValue(
                    "ping\n" +
                    "  - Returns the bot's Server and API latencies\n\n" +
                    "setchannel [channel mention/channel ID]\n" +
                    "  - Sets the time channel"
                );
            fields.Add(field);
            embed.WithFields(fields);

            await Context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}