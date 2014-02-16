using KissCI.Internal.Domain;
using KissCI.Web.Internal;
using KissCI.Web.Models.Project;
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
        readonly IProjectService _projectService;
        ProjectListView GetListView(string category = null)
        {
            var projects = _projectService.GetProjectViews();

            if(!string.IsNullOrEmpty(category))
                projects = projects.Where(p => p.Info.Category == category);
            
            return new ProjectListView
            {
                CategoryName = category,
                ProjectViews = projects.ToArray()
            };
        }

        public CategoryModule(IProjectService projectService) : base()
        {
            _projectService = projectService;

            Get["/"] = (ctx) =>
            {
                var model = GetListView();
                return View["Category/List", model];
            };

            Get["/{category}", true] = async (ctx, ct) =>
            {
                var model = GetListView(ctx.category);
                return View["Category/List", model];
            };

            Get["/category/grid"] = (ctx) => {
                var model = GetListView(Request.Query.categoryName);

                return View["Category/ProjectViewTable", model];
            };
        }
    }
}
