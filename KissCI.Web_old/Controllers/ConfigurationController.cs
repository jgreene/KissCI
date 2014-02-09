using KissCI.Internal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KissCI.Web.Controllers
{
    public class ConfigurationController : Controller
    {
        public ConfigurationController(IProjectService projectService)
        {
            ProjectService = projectService;
        }

        protected IProjectService ProjectService;

        public ActionResult Index()
        {
            using (var context = ProjectService.OpenContext())
            {
                var keys = context.ConfigurationService.All();

                return View("List", keys);
            }
        }

        public ActionResult Table()
        {
            using (var context = ProjectService.OpenContext())
            {
                var keys = context.ConfigurationService.All();

                return PartialView("ConfigurationViewTable", keys);
            }
        }

        [HttpPost]
        public ActionResult Save(string key, string value)
        {
            using (var context = ProjectService.OpenContext())
            {
                context.ConfigurationService.Save(key, value);
                context.Commit();
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Remove(string key)
        {
            using (var context = ProjectService.OpenContext())
            {
                context.ConfigurationService.Remove(key);
                context.Commit();
            }

            return Json(new { success = true });
        }
    }
}