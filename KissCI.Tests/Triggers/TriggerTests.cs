using KissCI.Helpers;
using KissCI.Triggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace KissCI.Tests.Triggers
{
    [TestClass]
    public class TriggerTests
    {
        [TestCleanup]
        public void ResetTime()
        {
            TimeHelper.Reset();
        }

        [TestMethod]
        public void MinuteTriggerWorks()
        {
            TimeHelper.SetTimeFunc(() => DateTime.Parse("06/12/2012 12:00:00 PM"));
            var nextBuild = DateTime.Parse("06/12/2012 12:01:00 PM");
            var trigger = new IntervalTrigger(nextBuild, IntervalTrigger.Interval.Minute);

            Assert.IsTrue(trigger.NextBuild.HasValue);

            Assert.IsTrue(trigger.NextBuild.Value == nextBuild);
        }

        [TestMethod]
        public void WeeklyTriggerWorks()
        {
            TimeHelper.SetTimeFunc(() => DateTime.Parse("06/01/2012 12:00:01 PM"));
            var firstBuild = DateTime.Parse("01/01/2012 12:00:00 PM");
            var nextBuild = DateTime.Parse("06/03/2012 12:00:00 PM");
            var trigger = new IntervalTrigger(firstBuild, IntervalTrigger.Interval.Weekly);

            Assert.IsTrue(trigger.NextBuild.HasValue);

            Assert.IsTrue(trigger.NextBuild.Value == nextBuild);
        }

        [TestMethod]
        public void MonthlyTriggerWorks()
        {
            TimeHelper.SetTimeFunc(() => DateTime.Parse("06/01/2012 12:00:01 PM"));
            var firstBuild = DateTime.Parse("01/01/2012 12:00:00 PM");
            var nextBuild = DateTime.Parse("07/01/2012 12:00:00 PM");
            var trigger = new IntervalTrigger(firstBuild, IntervalTrigger.Interval.Monthly);

            Assert.IsTrue(trigger.NextBuild.HasValue);

            Assert.IsTrue(trigger.NextBuild.Value == nextBuild);
        }
    }
}
