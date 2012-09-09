using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI.Triggers
{
    public interface ITrigger
    {
        DateTime? NextBuild { get; }
        bool IsActive { get; }
        void Start(Action toRun);
        void Stop();
    }
}
