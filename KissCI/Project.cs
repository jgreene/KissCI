using KissCI.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        string _name;
        public string Name { get { return _name; } }

        string _category;
        public string Category { get { return _category; } }

        BuildTask<BuildTaskStart, BuildTaskEnd> _tasks;
        public BuildTask<BuildTaskStart, BuildTaskEnd> Tasks { get { return _tasks; } }
    }
}
