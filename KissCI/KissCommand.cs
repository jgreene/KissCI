﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KissCI.Helpers;
using KissCI.Triggers;

namespace KissCI
{
    public class KissCommand
    {
        readonly string _name;
        readonly KissTask<KissCITaskStart, KissCITaskEnd> _tasks;
        readonly ITrigger[] _triggers;

        public KissCommand(string name, KissTask<KissCITaskStart, KissCITaskEnd> tasks, params ITrigger[] triggers)
        {
            if(string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if(tasks == null)
                throw new ArgumentNullException("tasks");

            _name = name;
            _tasks = tasks;
            _triggers = triggers.NotNull().ToArray();
        }

        public string Name { get { return _name; } }

        public KissTask<KissCITaskStart, KissCITaskEnd> Tasks { get { return _tasks; } }

        public ITrigger[] Triggers { get { return _triggers; } }
    }
}
