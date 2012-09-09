using KissCI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace KissCI.Triggers
{
    public class IntervalTrigger : ITrigger
    {
        public enum Interval
        {
            Minute,
            FifteenMinutes,
            ThirtyMinutes,
            Hourly,
            Daily,
            Weekly,
            Monthly,
            Yearly
        }

        public IntervalTrigger(DateTime startTime, Interval? interval = null)
        {
            _startTime = startTime;
            _interval = interval ?? Interval.Daily;
        }

        readonly DateTime _startTime;
        readonly Interval _interval;
        DateTime? _lastBuild;
        Timer _timer;
        bool _isActive = true;

        DateTime Increment(DateTime time)
        {
            switch (_interval)
            {
                case Interval.Minute:
                    return time.AddMinutes(1);
                case Interval.FifteenMinutes:
                    return time.AddMinutes(15);
                case Interval.ThirtyMinutes:
                    return time.AddMinutes(30);
                case Interval.Hourly:
                    return time.AddHours(1);
                case Interval.Daily:
                    return time.AddDays(1);
                case Interval.Weekly:
                    return time.AddDays(7);
                case Interval.Monthly:
                    return time.AddMonths(1);
                case Interval.Yearly:
                    return time.AddYears(1);
                default:
                    return time.AddDays(1);
            }
        }

        DateTime GetNextBuild(DateTime? arg = null)
        {
            if (arg.HasValue)
            {
                var now = TimeHelper.Now;

                if (_lastBuild.HasValue && _lastBuild.Value > now && arg.Value > _lastBuild.Value)
                    return arg.Value;

                if (arg.Value >= now && arg.Value >= _startTime)
                    return arg.Value;

                return GetNextBuild(Increment(arg.Value));

            }

            if (_lastBuild.HasValue)
            {
                return GetNextBuild(_lastBuild);
            }

            return GetNextBuild(_startTime);
        }

        public bool IsActive { get { return _isActive; } }

        public DateTime? NextBuild
        {
            get
            {
                return GetNextBuild();
            }
        }

        void Init(Action toRun)
        {
            var nextBuild = GetNextBuild();
            var nextInterval = nextBuild - TimeHelper.Now;

            _timer = new Timer(nextInterval.TotalMilliseconds);
            _timer.Elapsed += (obj, args) =>
            {
                try
                {
                    _lastBuild = TimeHelper.Now;
                    toRun();
                }
                finally
                {
                    _timer.Stop();
                    _timer.Dispose();
                    Init(toRun);
                }
            };

            _timer.Start();
        }

        public void Start(Action toRun)
        {
            _isActive = true;
            Init(toRun);
        }

        public void Stop()
        {
            _isActive = false;
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
