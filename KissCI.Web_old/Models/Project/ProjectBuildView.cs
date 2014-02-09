using KissCI.Internal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KissCI.Web.Models.Project
{
    public class ProjectBuildView
    {
        public ProjectInfo Info { get; set; }
        public IEnumerable<ProjectBuild> Builds { get; set; }
    }
}