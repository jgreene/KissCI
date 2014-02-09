using KissCI.Internal.Domain;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Web.Modules
{
    public class CategoryModule : NancyModule
    {
        public CategoryModule(IProjectService projectService) : base()
        {
            Get["/", true] = async (ctx, ct) =>
            {
                return View["Project/List", null];
            };
        }
    }
}
