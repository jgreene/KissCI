using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KissCI.Internal.Domain;

namespace KissCI.Internal
{
    public interface IDataContext : IDisposable
    {
        ITaskMessageService TaskMessageService { get; }
        IProjectBuildService ProjectBuildService { get; }
        IProjectInfoService ProjectInfoService { get; }
    }
}
