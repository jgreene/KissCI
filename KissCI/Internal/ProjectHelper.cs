using KissCI.Helpers;
using KissCI.Internal;
using KissCI.Internal.Domain;
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
        public static void Run(KissProject kissProject, KissCommand command, IProjectService projectService, CancellationToken? token = null)
        {
            if (kissProject == null)
                throw new ArgumentNullException("kissProject");

            if(command == null)
                throw new ArgumentNullException("command");

            ProjectInfo info = projectService.GetProjectInfo(kissProject.Name);

            if (info.Status == Status.Stopped)
                throw new Exception("Project can not be built as it is currently stopped.  Reactivate the project to build it.");

            var now = TimeHelper.Now;
            ProjectBuild build = new ProjectBuild {
                BuildTime = now,
                ProjectInfoId = info.Id,
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

            var context = new TaskContext(projectService, info, build, command.Tasks.Count, token);

            context.LogMessage("Beginning tasks for {0}", context.ProjectName);

            setActivity(Activity.Building);

            try
            {
                command.Tasks.Binder(context, new BuildTaskStart());

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
