using KissCI.Internal.Domain;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;

namespace KissCI.NHibernate.Internal
{
    public class ProjectInfoService : IProjectInfoService
    {
        public ProjectInfoService(ISession session)
        {
            _session = session;
        }

        readonly ISession _session;

        public IQueryable<ProjectInfo> GetProjectInfos()
        {
            return _session.Query<ProjectInfo>();
        }

        public ProjectInfo GetProjectInfo(string projectName)
        {
            return _session.Query<ProjectInfo>().FirstOrDefault(i => i.ProjectName == projectName);
        }

        public void Save(ProjectInfo info)
        {
            _session.SaveOrUpdate(info);
        }
    }
}
