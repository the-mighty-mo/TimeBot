using System;
using System.Threading.Tasks;

namespace TimeBot
{
    public class TimeEventHandler
    {
        private readonly TimeSpan time;

        public TimeEventHandler(TimeSpan time) => this.time = time;

        public event Func<Task> Time;

        public async void StartProcess()
        {
            while (true)
            {
                DateTime targetTime = DateTime.Today + time;
                while (DateTime.Now > targetTime)
                {
                    targetTime = targetTime.AddHours(12);
                }

                while (targetTime - DateTime.Now >= TimeSpan.FromSeconds(2))
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                await Task.Delay(targetTime - DateTime.Now);
                await Time();
            }
        }
    }
}
