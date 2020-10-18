using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace TimeBot.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpAsync()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle(SecurityInfo.botName)
                .WithCurrentTimestamp();

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
                    "setchannel [channel mention/channel ID]\n" +
                    "  - Sets the time channel"
                );
            embed.AddField(field);

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
