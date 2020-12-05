using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static TimeBot.DatabaseManager;

namespace TimeBot.Modules
{
    public class SetChannel : ModuleBase<SocketCommandContext>
    {
        [Command("setchannel")]
        [Alias("set-channel")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannelAsync()
        {
            if (await channelsDatabase.Channels.GetTimeChannelAsync(Context.Guild) == null)
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription("You already do not have a channel set.");

                await Context.Channel.SendMessageAsync(embed: emb.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription("\"Time\" messages will no longer be sent.");

            await Task.WhenAll
            (
                channelsDatabase.Channels.RemoveTimeChannelAsync(Context.Guild),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );
        }

        [Command("setchannel")]
        [Alias("set-channel")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannelAsync(SocketTextChannel channel)
        {
            if (await channelsDatabase.Channels.GetTimeChannelAsync(Context.Guild) == channel)
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{channel.Mention} is already configured for \"Time\" messages.");

                await Context.Channel.SendMessageAsync(embed: emb.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"\"Time\" messages will now be sent to {channel.Mention}.");

            await Task.WhenAll
            (
                channelsDatabase.Channels.SetTimeChannelAsync(channel),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );
        }

        [Command("setchannel")]
        [Alias("set-channel")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannelAsync(string channel)
        {
            SocketTextChannel c;
            if (ulong.TryParse(channel, out ulong channelID) && (c = Context.Guild.GetTextChannel(channelID)) != null)
            {
                await SetChannelAsync(c);
                return;
            }
            await Context.Channel.SendMessageAsync("Error: the given text channel does not exist.");
        }
    }
}