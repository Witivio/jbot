using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services
{
    public class Scheduler : IScheduler
    {
        private Timer _timer;
        private TimeSpan _time;

        public void Start(Func<Task> callback, TimeSpan time)
        {
            _time = time;
            _timer = new Timer(async (s) =>
            {
                Pause();
                await callback();
                Reset();
            }, null, time, time);
        }

        public void Pause()
        {
            _timer?.Change(-1, -1);
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        public void Reset()
        {
            _timer?.Change(_time, _time);
        }

    }

    public interface IScheduler
    {
        void Pause();
        void Reset();
        void Start(Func<Task> callback, TimeSpan time);
        void Stop();
    }
}
