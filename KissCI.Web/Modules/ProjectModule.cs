using KissCI.Internal.Domain;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Web.Modules
{
    public class ProjectModule : NancyModule
    {
        public ProjectModule(IProjectService projectService)
            : base("/project")
        {
            Get["/", true] = async (ctx, ct) => {
                return "hello world";
            };
        }
    }
}
