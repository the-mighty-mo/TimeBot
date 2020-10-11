using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimeBot.Databases
{
    public class ChannelsDatabase
    {
        private readonly SqliteConnection cnChannels = new SqliteConnection("Filename=Channels.db");

        public ChannelsTable Channels;

        public ChannelsDatabase()
        {
            Channels = new ChannelsTable(cnChannels);
        }

        public async Task InitAsync()
        {
            await cnChannels.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Channels (guild_id TEXT PRIMARY KEY, channel_id TEXT NOT NULL);", cnChannels))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        public async Task CloseAsync() => await cnChannels.CloseAsync();

        public class ChannelsTable
        {
            private readonly SqliteConnection cnChannels;

            public ChannelsTable(SqliteConnection cnChannels) => this.cnChannels = cnChannels;

            public async Task<SocketTextChannel> GetTimeChannelAsync(SocketGuild g)
            {
                SocketTextChannel channel = null;

                string getChannel = "SELECT channel_id FROM Channels WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getChannel, cnChannels))
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

            public async Task SetTimeChannelAsync(SocketTextChannel channel)
            {
                string update = "UPDATE Channels SET channel_id = @channel_id WHERE guild_id = @guild_id;";
                string insert = "INSERT INTO Channels (guild_id, channel_id) SELECT @guild_id, @channel_id WHERE (SELECT Changes() = 0);";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, cnChannels))
                {
                    cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveTimeChannelAsync(SocketGuild g)
            {
                string delete = "DELETE FROM Channels WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, cnChannels))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
