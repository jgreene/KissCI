using KissCI.Internal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Web.Models.Project
{
    public class ProjectListView
    {
        public string CategoryName { get; set; }
        public ProjectView[] ProjectViews { get; set; }
    }
}
