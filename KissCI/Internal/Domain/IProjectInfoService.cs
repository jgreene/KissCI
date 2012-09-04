using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI.Internal.Domain
{
    public interface IProjectInfoService
    {
        IQueryable<ProjectInfo> GetProjectInfos();
        ProjectInfo GetProjectInfo(string projectName);
        void Save(ProjectInfo info);
    }
}
