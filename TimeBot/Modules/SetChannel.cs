using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace TimeBot.Modules
{
    public class SetChannel : ModuleBase<SocketCommandContext>
    {
        [Command("setchannel")]
        [Alias("set-channel")]
        public async Task SetChannelAsync()
        {
            if (await GetTimeChannelAsync(Context.Guild) == null)
            {
                await Context.Channel.SendMessageAsync("You already do not have a channel set.");
                return;
            }

            await Task.WhenAll
            (
                RemoveTimeChannelAsync(Context.Guild),
                Context.Channel.SendMessageAsync("\"Time\" messages will no longer be sent.")
            );
        }

        [Command("setchannel")]
        [Alias("set-channel")]
        public async Task SetChannelAsync(SocketTextChannel channel)
        {
            if (await GetTimeChannelAsync(Context.Guild) == channel)
            {
                await Context.Channel.SendMessageAsync($"{channel.Mention} is already configured for \"Time\" messages.");
                return;
            }

            await Task.WhenAll
            (
                SetTimeChannelAsync(channel),
                Context.Channel.SendMessageAsync($"\"Time\" messages will now be sent to {channel.Mention}.")
            );
        }

        [Command("setchannel")]
        [Alias("set-channel")]
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

        public static async Task<SocketTextChannel> GetTimeChannelAsync(SocketGuild g)
        {
            SocketTextChannel channel = null;

            string getChannel = "SELECT channel_id FROM Channels WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getChannel, Program.cnChannels))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                    channel = g.GetTextChannel(channelID);
                }
                reader.Close();
            }

            return channel;
        }

        public static async Task SetTimeChannelAsync(SocketTextChannel channel)
        {
            string update = "UPDATE Channels SET channel_id = @channel_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Channels (guild_id, channel_id) SELECT @guild_id, @channel_id WHERE (SELECT Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnChannels))
            {
                cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveTimeChannelAsync(SocketGuild g)
        {
            string delete = "DELETE FROM Channels WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnChannels))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
