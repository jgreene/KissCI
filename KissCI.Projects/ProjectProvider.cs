using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KissCI.Projects
{
    public class ProjectProvider : IProjectProvider
    {
        public IEnumerable<Project> Projects()
        {
            var current = DirectoryHelper.CurrentDirectory().Parent.Parent;

            var writeTo = Path.Combine(current.FullName, "TempProjectDirectory");

            var fileTask = TaskHelper.Start()
            .AddTask("Write file", (ctx, arg) => {
                DirectoryHelper.CleanAndEnsureDirectory(writeTo);
                var writeFile = Path.Combine(writeTo, "test.txt");
                File.WriteAllText(writeFile, "Test");
                return 1;
            })
            .Finalize();

            yield return new Project("WriteFileProject", "IO Projects", fileTask);

            var sleepTask = TaskHelper.Start()
            .AddTask("Sleeping", (ctx, arg) => {
                Thread.Sleep(10000);
                return 1;
            })
            .Finalize();

            yield return new Project("SleepProject", "Sleep Projects", sleepTask);

            var taskCounter = 1;
            var tasks = TaskHelper.Start()
            .AddTask("Fake task", (ctx, arg) => {
                ctx.Log("Fake task run : {0}", taskCounter);
                taskCounter++;
                return taskCounter;
            })
            .Finalize();

            foreach(var p in Enumerable.Range(1, 50))
                yield return new Project("Project" + p.ToString(), "UI", tasks);
        }
    }
}
