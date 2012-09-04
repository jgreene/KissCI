using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI.Internal.Domain
{
    public interface IProjectBuildService
    {
        IQueryable<ProjectBuild> GetBuilds(string projectName);
        ProjectBuild GetMostRecentBuild(string projectName);
    }
}
