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
    public class ProjectModule : NancyModule
    {
        public ProjectModule(IProjectService projectService)
            : base("/project")
        {

            Post["/{projectName}/run/{commandName}"] = (ctx) => {
                
                string projectName = ctx.projectName;
                string commandName = ctx.commandName;
                if(string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(commandName))
                    return false;
                
                return projectService.RunProject(projectName, commandName);
            };

            Post["/{projectName}/cancel/"] = (ctx) => {
                string projectName = ctx.projectName;

                return projectService.CancelProject(projectName);
            };

            Post["/{projectName}/stop"] = (ctx) => {
                string projectName = ctx.projectName;

                projectService.StopProject(projectName);
                return true;
            };

            Post["/{projectName}/start"] = (ctx) =>
            {
                string projectName = ctx.projectName;

                projectService.StartProject(projectName);
                return true;
            };

            Get["/{projectName}/log/{buildId}"] = (ctx) => {
                using (var kissCtx = projectService.OpenContext())
                {
                    long id = ctx.buildId;
                    var build = kissCtx.ProjectBuildService.GetBuilds().FirstOrDefault(b => b.Id == id);
                    if (build == null)
                        return null;

                    var info = kissCtx.ProjectInfoService.GetProjectInfos().FirstOrDefault(i => i.Id == build.ProjectInfoId);

                    var messages = kissCtx.TaskMessageService.GetMessagesForBuild(id).OrderBy(m => m.Time).ToList();

                    var model = new BuildLogView
                    {
                        Info = info,
                        Build = build,
                        Messages = messages.Where(m => m.Type == MessageType.TaskMessage),
                        Logs = messages
                    };

                    return View["BuildLog", model];
                }
            };

            Get["/{projectName}/builds"] = (ctx) => {
                string projectName = ctx.projectName;

                using (var kissCtx = projectService.OpenContext())
                {
                    var info = kissCtx.ProjectInfoService.GetProjectInfo(projectName);
                    var builds = kissCtx.ProjectBuildService
                        .GetBuildsForProject(projectName)
                        .OrderByDescending(b => b.BuildTime)
                        .Take(100)
                        .ToList();

                    var model = new ProjectBuildView
                    {
                        Info = info,
                        Builds = builds
                    };

                    return View["Project/ProjectBuilds", model];
                }
            };

            
        }
    }
}
