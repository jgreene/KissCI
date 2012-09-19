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

        public IEnumerable<Project> Projects()
        {
            var current = DirectoryHelper.CurrentDirectory().Parent;

            var writeTo = Path.Combine(current.FullName, "TempProjectDirectory");

            var fileTask = TaskHelper.Start()
            .AddTask("Write file", (ctx, arg) => {
                DirectoryHelper.CleanAndEnsureDirectory(writeTo);
                var writeFile = Path.Combine(writeTo, "test.txt");
                File.WriteAllText(writeFile, "Test");
                return 1;
            })
            .Finalize();

            var project = new Project("WriteFileProject", "IO Projects", fileTask);

            yield return project;

            var sleepTask = TaskHelper.Start()
            .AddTask("Sleeping", (ctx, arg) => {
                Thread.Sleep(10000);
                return 1;
            })
            .Finalize();

            project = new Project("SleepProject", "Sleep Projects", sleepTask);
            project.AddTimer(DateTime.Parse("09/08/2012 6:20:00 PM"));

            yield return project;

            var failTask = TaskHelper.Start()
            .AddTask("Fail", (ctx, arg) => {
                throw new Exception("Fail");
                return 1;
            })
            .Finalize();

            project = new Project("Fail project", "Fail Projects", failTask);

            yield return project;

            var taskCounter = 1;
            var tasks = TaskHelper.Start()
            .AddTask("Fake task", (ctx, arg) => {
                ctx.Log("Fake task run : {0}", taskCounter);
                taskCounter++;
                return taskCounter;
            })
            .Finalize();

            foreach(var p in Enumerable.Range(1, 5))
                yield return new Project("Project" + p.ToString(), "UI", tasks);
        }
    }
}
