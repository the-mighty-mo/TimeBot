﻿using System;
using System.Threading.Tasks;

namespace TimeBot
{
    public class TimeEventHandler
    {
        private readonly TimeSpan time;

        public TimeEventHandler(TimeSpan time) => this.time = time;

        public event Func<Task>? Time;

        public event Func<Exception, Task>? TimeEventError;

        public async void StartProcess()
        {
            while (true)
            {
                DateTime targetTime = DateTime.Today + time;
                while (DateTime.Now > targetTime)
                {
                    targetTime = targetTime.AddHours(12);
                }

                TimeSpan timeLeft;
                while ((timeLeft = targetTime - DateTime.Now) >= TimeSpan.FromSeconds(2))
                {
                    await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                }
                if (timeLeft > TimeSpan.Zero)
                {
                    await Task.Delay(timeLeft).ConfigureAwait(false);
                }

                if (timeLeft > TimeSpan.FromMinutes(-1))
                {
                    try
                    {
                        if (Time != null)
                        {
                            await Time().ConfigureAwait(false);
                        }
                    }
                    catch (Exception e)
                    {
                        if (TimeEventError != null)
                        {
                            await TimeEventError(e).ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }
}