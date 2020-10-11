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
            if (await channelsDatabase.GetTimeChannelAsync(Context.Guild) == null)
            {
                await Context.Channel.SendMessageAsync("You already do not have a channel set.");
                return;
            }

            await Task.WhenAll
            (
                channelsDatabase.RemoveTimeChannelAsync(Context.Guild),
                Context.Channel.SendMessageAsync("\"Time\" messages will no longer be sent.")
            );
        }

        [Command("setchannel")]
        [Alias("set-channel")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannelAsync(SocketTextChannel channel)
        {
            if (await channelsDatabase.GetTimeChannelAsync(Context.Guild) == channel)
            {
                await Context.Channel.SendMessageAsync($"{channel.Mention} is already configured for \"Time\" messages.");
                return;
            }

            await Task.WhenAll
            (
                channelsDatabase.SetTimeChannelAsync(channel),
                Context.Channel.SendMessageAsync($"\"Time\" messages will now be sent to {channel.Mention}.")
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
