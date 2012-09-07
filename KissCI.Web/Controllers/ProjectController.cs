﻿using KissCI.Internal.Domain;
using KissCI.NHibernate.Internal;
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

            return View("Categories", cats);
        }

        [HttpPost]
        public ActionResult Force(string projectName)
        {
            ProjectService.RunProject(projectName);
            return Json(true);
        }

        public ActionResult Grid(string categoryName)
        {
            var projects = ProjectService.GetProjectViews();

            if (string.IsNullOrEmpty(categoryName) == false)
                projects = projects.Where(p => p.Info.Category == categoryName);



            return PartialView("ProjectViewTable", Tuple.Create(categoryName, projects));
        }

    }
}
