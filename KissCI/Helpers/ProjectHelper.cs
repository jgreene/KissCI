using KissCI.Internal;
using KissCI.Internal.Domain;
using KissCI.Loggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Helpers
{
    public static class ProjectHelper
    {
        static string GetLogFileName(string directory, string projectName, DateTime time)
        {
            var date = time.ToString("yyyy-MM-dd_hh-mm-ss");
            var unique = Guid.NewGuid().ToString().Split('-')[0];
            var fileName = string.Format("{0}_{1}_{2}.txt", projectName, date, unique);
            return Path.Combine(directory, fileName);
        }

        public static void Run(Project project, IProjectService projectService)
        {
            if (project == null)
                throw new NullReferenceException("Project was null");

            if (project.Tasks == null)
                throw new NullReferenceException("Project has no tasks to run");

           

            ProjectInfo info = projectService.GetProjectInfo(project.Name);
            var now = TimeHelper.Now;
            ProjectBuild build = new ProjectBuild {
                BuildTime = now,
                ProjectInfoId = info.Id,
                LogFile = GetLogFileName(projectService.BuildLogsDirectory, project.Name, now),
            };

            using (var ctx = projectService.OpenContext())
            {
                ctx.ProjectBuildService.Save(build);
            }

            var logger = new BuildLogger(build);

            var context = new TaskContext(projectService, info, build, logger, project.Tasks.Count);

            context.Log("Beginning tasks for {0}", context.ProjectName);

            project.Tasks.Binder(context, new BuildTaskStart());

            context.Cleanup();
        }
    }
}
