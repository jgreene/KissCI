﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI.Helpers
{
    public static class TimeHelper
    {
        public static void SetTimeFunc(Func<DateTime> timeFunc)
        {
            _timeFunc = timeFunc;
        }

        public static void Reset()
        {
            _timeFunc = () => DateTime.Now;
        }

        static Func<DateTime> _timeFunc = () => DateTime.Now;

        public static DateTime Now
        {
            get { return _timeFunc(); }
        }
    }
}
