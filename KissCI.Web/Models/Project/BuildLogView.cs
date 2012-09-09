using KissCI.Internal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KissCI.Web.Models.Project
{
    public class BuildLogView
    {
        public ProjectInfo Info { get; set; }
        public ProjectBuild Build { get; set; }
        public IEnumerable<TaskMessage> Messages { get; set; }
    }
}