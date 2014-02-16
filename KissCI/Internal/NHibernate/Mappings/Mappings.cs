using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate;
using KissCI.Internal.Domain;
using FluentNHibernate.Mapping;

namespace KissCI.NHibernate.Internal.Mappings
{
    public class ProjectInfoMap : ClassMap<ProjectInfo>
    {
        public ProjectInfoMap()
        {
            Id(x => x.Id);
            Map(x => x.ProjectName);
            Map(x => x.Category);
            Map(x => x.Status);
            Map(x => x.Activity);
        }
    }

    public class ProjectBuildMap : ClassMap<ProjectBuild>
    {
        public ProjectBuildMap()
        {
            Id(x => x.Id);
            Map(x => x.ProjectInfoId);
            Map(x => x.BuildTime);
            Map(x => x.CompleteTime);
            Map(x => x.BuildResult);
        }
    }

    public class TaskMessageMap : ClassMap<TaskMessage>
    {
        public TaskMessageMap()
        {
            Id(x => x.Id);
            Map(x => x.ProjectInfoId);
            Map(x => x.ProjectBuildId);
            Map(x => x.Time);
            Map(x => x.Message);
            Map(x => x.Type);
            Map(x => x.LogType);
        }
    }

    public class ConfigurationItemMap : ClassMap<ConfigurationItem>
    {
        public ConfigurationItemMap()
        {
            Id(x => x.Id);
            Map(x => x.Key).UniqueKey("KeyIsUnique");
            Map(x => x.Value);
        }
    }
}
