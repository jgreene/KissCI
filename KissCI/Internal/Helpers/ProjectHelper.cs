using KissCI.Exceptions;
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
                throw new ProjectException("Project can not be built as it is currently stopped.  Reactivate the project to build it.");

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

            var logger = new KissCI.Internal.Logging.KissCILoggingAdapter(projectService, info, build);

            var context = new TaskContext(projectService, info, build, logger, command.Tasks.Count, token);

            context.LogMessage("Beginning {0} for {1}", command.Name, context.ProjectName);

            setActivity(Activity.Building);

            try
            {
                command.Tasks.Binder(context, new BuildTaskStart());

                setBuildStatus(BuildResult.Success);
            }
            catch (OperationCanceledException ex)
            {
                context.Logger.FatalFormat("{0} canceled with an exception of: {1}", command.Name, ex);
                setBuildStatus(BuildResult.Cancelled);
            }
            catch (Exception ex)
            {
                context.Logger.ErrorFormat("{0} failed with an exception of: {1}", command.Name, ex);
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
