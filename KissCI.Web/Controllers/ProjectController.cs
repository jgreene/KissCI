using KissCI.Internal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KissCI.Web.Controllers
{
    public class ProjectController : Controller
    {
        public ProjectController(IProjectService projectService)
        {
            ProjectService = projectService;
        }

        protected IProjectService ProjectService;

        public ActionResult Index()
        {
            return List(null);
        }

        public ActionResult List(string category)
        {
            var projects = ProjectService.GetProjectViews();

            return View("List", projects);
        }

    }
}
