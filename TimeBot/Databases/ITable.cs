using System.Threading.Tasks;

namespace TimeBot.Databases
{
    interface ITable
    {
        public Task InitAsync();
    }
}
