using KissCI.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI.Helpers
{
    public static class ProjectHelper
    {
        public static void Run(Project project, ILogger logger = null)
        {
            if (project == null)
                throw new NullReferenceException("Project was null");

            if (project.Tasks == null)
                throw new NullReferenceException("Project has no tasks to run");

            if (logger == null)
                logger = new ConsoleLogger();

            var context = new TaskContext(logger, project.Name, project.Tasks.Count);

            context.Log("Beginning tasks for {0}", context.ProjectName);

            project.Tasks.Binder(context, new BuildTaskStart());

            context.Cleanup();
        }
    }
}
