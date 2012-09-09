using KissCI.Helpers;
using KissCI.Internal;
using KissCI.Internal.Domain;
using KissCI.Internal.Loggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace KissCI.Internal.Helpers
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

        public static void Run(Project project, IProjectService projectService, CancellationToken? token = null)
        {
            if (project == null)
                throw new NullReferenceException("Project was null");

            if (project.Tasks == null)
                throw new NullReferenceException("Project has no tasks to run");

            ProjectInfo info = projectService.GetProjectInfo(project.Name);

            if (info.Status == Status.Stopped)
                throw new Exception("Project can not be built as it is currently stopped.  Reactivate the project to build it.");

            var now = TimeHelper.Now;
            ProjectBuild build = new ProjectBuild {
                BuildTime = now,
                ProjectInfoId = info.Id,
                LogFile = GetLogFileName(projectService.BuildLogsDirectory, project.Name, now),
            };

            using (var ctx = projectService.OpenContext())
            {
                ctx.ProjectBuildService.Save(build);
                ctx.Commit();
            }

            var setActivity = new Action<Activity>((act) =>
            {
                using (var ctx = projectService.OpenContext())
                {
                    info.Activity = act;
                    ctx.ProjectInfoService.Save(info);
                    ctx.Commit();
                }
            });

            var setBuildStatus = new Action<BuildResult>((stat) =>
            {
                using (var ctx = projectService.OpenContext())
                {
                    build.BuildResult = stat;
                    build.CompleteTime = TimeHelper.Now;
                    ctx.ProjectBuildService.Save(build);

                    ctx.Commit();
                }
            });

            var logger = new BuildLogger(build);

            var context = new TaskContext(projectService, info, build, logger, project.Tasks.Count, token);

            context.LogMessage("Beginning tasks for {0}", context.ProjectName);

            setActivity(Activity.Building);

            try
            {
                project.Tasks.Binder(context, new BuildTaskStart());

                setBuildStatus(BuildResult.Success);
            }
            catch (OperationCanceledException ex)
            {
                context.Log(ex.ToString());
                setBuildStatus(BuildResult.Cancelled);
            }
            catch (Exception ex)
            {
                context.LogMessage("Build failed with an exception of: {0}", ex);
                setBuildStatus(BuildResult.Failure);
                throw;
            }
            finally
            {
                setActivity(Activity.CleaningUp);

                try
                {
                    context.Cleanup();
                }
                finally
                {
                    setActivity(Activity.Sleeping);
                }
            }
        }
    }
}
