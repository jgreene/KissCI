using KissCI.Helpers;
using KissCI.Triggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KissCI.Tasks;

namespace KissCI.Tests.Projects
{
    public class ProjectProvider : IProjectProvider
    {
        void EnsureDirectories(params string[] directories)
        {
            foreach (var dir in directories)
                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);
        }

        public IEnumerable<KissProject> Projects()
        {
            var current = DirectoryHelper.CurrentDirectory();

            var sleepTask = TaskHelper.Start()
            .AddTask("Sleeping", (ctx, arg) => {
                Thread.Sleep(1000);
                return 1;
            })
            .Finalize();

            var sleepCommand = new KissCommand("build", sleepTask, new IntervalTrigger(DateTime.Parse("09/08/2012 6:20:00 PM")));

            yield return new KissProject("SleepProject", "Sleep Projects", sleepCommand);

            var failTask = TaskHelper.Start()
            .AddTask("Fail", (ctx, arg) => {
                throw new Exception("Fail");
                return 1;
            })
            .Finalize();

            var failCommand = new KissCommand("build", failTask);

            yield return new KissProject("Fail project", "Fail Projects", failCommand);

            var taskCounter = 1;
            var tasks = TaskHelper.Start()
            .AddTask("Fake task", (ctx, arg) => {
                ctx.Log("Fake task run : {0}", taskCounter);
                taskCounter++;
                return taskCounter;
            })
            .Finalize();

            var taskCommand = new KissCommand("build", tasks);

            foreach(var p in Enumerable.Range(1, 5))
                yield return new KissProject("Project" + p.ToString(), "UI", taskCommand);
        }
    }
}
