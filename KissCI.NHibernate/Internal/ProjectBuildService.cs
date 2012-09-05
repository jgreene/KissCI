using KissCI.Internal.Domain;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;

namespace KissCI.NHibernate.Internal
{
    public class ProjectBuildService : IProjectBuildService
    {
        public ProjectBuildService(ISession session)
        {
            _session = session;
        }

        readonly ISession _session;

        public IQueryable<ProjectBuild> GetBuilds()
        {
            return _session.Query<ProjectBuild>().OrderByDescending(b => b.BuildTime);
        }

        public IQueryable<ProjectBuild> GetBuildsForProject(string projectName)
        {
            var builds = from i in _session.Query<ProjectInfo>()
                         from b in _session.Query<ProjectBuild>()
                         where i.ProjectName == projectName &&
                         i.Id == b.ProjectInfoId
                         select b;

            return builds.OrderByDescending(b=> b.BuildTime);
        }

        public ProjectBuild GetMostRecentBuild(string projectName)
        {
            return GetBuildsForProject(projectName).FirstOrDefault();
        }

        public void Save(ProjectBuild build)
        {
            _session.Save(build);
        }

        
    }
}
