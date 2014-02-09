using KissCI.Internal.Domain;
using KissCI.NHibernate.Internal;
using KissCI.Web.Models.Project;
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

        public ActionResult List(string categoryName)
        {
            var projects = ProjectService.GetProjectViews();

            if (string.IsNullOrEmpty(categoryName) == false)
                projects = projects.Where(p => p.Info.Category == categoryName);

            return View("List", Tuple.Create(categoryName, projects));
        }

        public ActionResult Categories()
        {
            var cats = ProjectService.GetCategories();

            return PartialView("Categories", cats);
        }

        

        public ActionResult Grid(string categoryName)
        {
            var projects = ProjectService.GetProjectViews();

            if (string.IsNullOrEmpty(categoryName) == false)
                projects = projects.Where(p => p.Info.Category == categoryName);



            return PartialView("ProjectViewTable", Tuple.Create(categoryName, projects));
        }

        [HttpPost]
        public ActionResult Command(string projectName, string command)
        {
            ProjectService.RunProject(projectName, command);
            return Json(true);
        }

        [HttpPost]
        public ActionResult Cancel(string projectName, string command)
        {
            ProjectService.CancelProject(projectName, command);
            return Json(true);
        }

        [HttpPost]
        public ActionResult Stop(string projectName)
        {
            ProjectService.StopProject(projectName);

            return Json(true);
        }

        [HttpPost]
        public ActionResult Start(string projectName)
        {
            ProjectService.StartProject(projectName);

            return Json(true);
        }

        public ActionResult Builds(string projectName)
        {
            using(var ctx = ProjectService.OpenContext()){
                var info = ctx.ProjectInfoService.GetProjectInfo(projectName);
                var builds = ctx.ProjectBuildService
                    .GetBuildsForProject(projectName)
                    .OrderByDescending(b => b.BuildTime)
                    .Take(100)
                    .ToList();

                var view = new ProjectBuildView{
                    Info = info,
                    Builds = builds
                };

                return View("ProjectBuilds", view);
            }
            
        }

        public ActionResult Log(long id)
        {
            using (var ctx = ProjectService.OpenContext())
            {
                var build = ctx.ProjectBuildService.GetBuilds().FirstOrDefault(b => b.Id == id);
                if (build == null)
                    return new HttpNotFoundResult();

                var info = ctx.ProjectInfoService.GetProjectInfos().FirstOrDefault(i => i.Id == build.ProjectInfoId);

                var messages = ctx.TaskMessageService.GetMessagesForBuild(id).OrderBy(m => m.Time).ToList();

                var view = new BuildLogView
                {
                    Info = info,
                    Build = build,
                    Messages = messages.Where(m=>m.Type == MessageType.TaskMessage),
                    Logs = messages
                };

                return View("BuildLog", view);
            }
        }

    }
}
