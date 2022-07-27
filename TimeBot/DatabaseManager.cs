using System.Threading.Tasks;
using TimeBot.Databases;

namespace TimeBot
{
    public static class DatabaseManager
    {
        public static readonly ChannelsDatabase channelsDatabase = new();

        public static Task InitAsync() =>
            Task.WhenAll(
                channelsDatabase.InitAsync()
            );

        public static Task CloseAsync() =>
            Task.WhenAll(
                channelsDatabase.CloseAsync()
            );
    }
}