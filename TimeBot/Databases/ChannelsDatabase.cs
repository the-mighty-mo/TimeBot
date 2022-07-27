using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeBot.Databases.ChannelsDatabaseTables;

namespace TimeBot.Databases
{
    public class ChannelsDatabase
    {
        private readonly SqliteConnection connection = new("Filename=Channels.db");
        private readonly Dictionary<System.Type, ITable> tables = new();

        public ChannelsTable Channels => tables[typeof(ChannelsTable)] as ChannelsTable;

        public ChannelsDatabase() =>
            tables.Add(typeof(ChannelsTable), new ChannelsTable(connection));

        public async Task InitAsync()
        {
            IEnumerable<Task> GetTableInits()
            {
                foreach (var table in tables.Values)
                {
                    yield return table.InitAsync();
                }
            }
            await connection.OpenAsync();
            await Task.WhenAll(GetTableInits());
        }

        public Task CloseAsync() => connection.CloseAsync();
    }
}