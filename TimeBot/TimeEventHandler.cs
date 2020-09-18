using System;
using System.Threading.Tasks;

namespace TimeBot
{
    public class TimeEventHandler
    {
        public event Func<Task> Time;

        public async Task StartProcess()
        {
            while (true)
            {
                DateTime targetTime = DateTime.Today.AddHours(2).AddMinutes(17);
                while (DateTime.Now > targetTime)
                {
                    targetTime = targetTime.AddHours(12);
                }
                TimeSpan timeUntilTarget = targetTime - DateTime.Now;

                await Task.Delay(timeUntilTarget);
                if (Time != null)
                {
                    await Time.Invoke();
                }
            }
        }
    }
}
