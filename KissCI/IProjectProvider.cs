using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI
{
    public interface IProjectProvider
    {
        IEnumerable<Project> Projects();
    }
}
