using KissCI.Helpers;
using KissCI.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace KissCI
{
    public class Project
    {
        public Project(string name, string category, BuildTask<BuildTaskStart, BuildTaskEnd> tasks)
        {
            _name = name;
            _category = category;
            _tasks = tasks;
        }

        readonly string _name;
        public string Name { get { return _name; } }

        readonly string _category;
        public string Category { get { return _category; } }

        BuildTask<BuildTaskStart, BuildTaskEnd> _tasks;
        public BuildTask<BuildTaskStart, BuildTaskEnd> Tasks { get { return _tasks; } }

        readonly IList<ITrigger> _triggers = new List<ITrigger>();

        public IList<ITrigger> Triggers { get { return _triggers; } }
    }

    public static class ProjectExtensions
    {
        public static void AddTimer(this Project project, 
            DateTime startTime, 
            IntervalTrigger.Interval? interval = null)
        {
            var trigger = new IntervalTrigger(startTime, interval);

            project.Triggers.Add(trigger);
        }
    }

    

    

    
}
