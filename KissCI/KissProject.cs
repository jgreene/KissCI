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
    public class KissProject
    {
        public KissProject(string name, string category, params KissCommand[] commands)
        {
            if(string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if(string.IsNullOrEmpty(category))
                throw new ArgumentNullException("category");

            if(commands == null || commands.Length < 1)
                throw new ArgumentException("commands cannot be null or empty");

            _name = name;
            _category = category;
            _commands = commands;
        }

        readonly string _name;
        public string Name { get { return _name; } }

        readonly string _category;
        public string Category { get { return _category; } }

        readonly KissCommand[] _commands;
        public KissCommand[] Commands { get { return _commands; } }
    }

    //public static class ProjectExtensions
    //{
    //    public static void AddTimer(this KissProject kissProject, 
    //        DateTime startTime, 
    //        IntervalTrigger.Interval? interval = null)
    //    {
    //        var trigger = new IntervalTrigger(startTime, interval);

    //        kissProject.Triggers.Add(trigger);
    //    }
    //}

    

    

    
}
