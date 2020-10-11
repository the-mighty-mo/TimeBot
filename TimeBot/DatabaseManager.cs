using System.Threading.Tasks;
using TimeBot.Databases;

namespace TimeBot
{
    public static class DatabaseManager
    {
        public static readonly ChannelsDatabase channelsDatabase;

        public static async Task InitAsync()
        {
            await Task.WhenAll(
                channelsDatabase.InitAsync()
            );
        }

        public static async Task CloseAsync()
        {
            await Task.WhenAll(
                channelsDatabase.CloseAsync()
            );
        }
    }
}
