using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeBot.Core.Communication;
using System.Timers;
using HomeBot.Core.Command.Announce;
using NLog;

namespace HomeBot
{
    public class Scheduler : IScheduler
    {
        private readonly Timer _timer = new Timer { Interval = 60000 };
        private readonly IDictionary<IAnnounce, DateTime> _scheduledAnnouncements = new Dictionary<IAnnounce, DateTime>();
        private ICommunicator _comm;

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Start(IEnumerable<IAnnounce> tasks, ICommunicator comm)
        {
            _comm = comm;

            var startTime = DateTime.Now;
            foreach (var task in tasks)
            {
                _scheduledAnnouncements.Add(task, startTime.Add(task.Interval));
            }

            _timer.Elapsed += HandleResult;
            _timer.Start();

            _logger.Info("HomeBot is active");
        }

        private void HandleResult(object state, ElapsedEventArgs elapsedEventArgs)
        {
            var now = DateTime.Now;

            var currentItems = _scheduledAnnouncements.Where(c => c.Value < now).ToList();

            foreach (var scheduleItem in currentItems)
            {
                var announcement = scheduleItem.Key;

                announcement.Execute(_comm);
                _scheduledAnnouncements[announcement] = now.Add(announcement.Interval);
            }
        }

        public void Stop()
        {
            _timer.Stop();

            _logger.Info("HomeBot shutting down...");
        }
    }
}
