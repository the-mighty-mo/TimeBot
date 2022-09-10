using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace TimeBot.Databases.ChannelsDatabaseTables
{
    public class ChannelsTable : ITable
    {
        private readonly SqliteConnection connection;

        public ChannelsTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS Channels (guild_id TEXT PRIMARY KEY, channel_id TEXT NOT NULL);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<SocketTextChannel?> GetTimeChannelAsync(SocketGuild g)
        {
            SocketTextChannel? channel = null;

            string getChannel = "SELECT channel_id FROM Channels WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getChannel, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                _ = ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                channel = g.GetTextChannel(channelID);
            }
            reader.Close();

            return channel;
        }

        public async Task SetTimeChannelAsync(SocketTextChannel channel)
        {
            string update = "UPDATE Channels SET channel_id = @channel_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Channels (guild_id, channel_id) SELECT @guild_id, @channel_id WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
            cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveTimeChannelAsync(SocketGuild g)
        {
            string delete = "DELETE FROM Channels WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
