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
            yield return 
                this.CreateProject("SleepProject", "Sleep Projects")
                .WithCommand(
                    "build"
                    , command =>
                        command.AddTask("Sleeping", (ctx, arg) => {
                            Thread.Sleep(1000);
                            return 1;
                        })
                    , new IntervalTrigger(DateTime.Parse("09/08/2012 6:20:00 PM"))
                );

            yield return
                this.CreateProject("Fail project", "Fail Projects")
                .WithCommand(
                    "build"
                    , command =>
                        command.AddTask("Fail", (ctx, arg) => {
                            throw new Exception("Fail");
                            return 1;
                        })
                );

            var taskCounter = 1;

            foreach (var p in Enumerable.Range(1, 5))
                yield return
                    this.CreateProject("Project" + p.ToString(), "UI")
                    .WithCommand(
                        "build"
                        , command =>
                            command.AddTask("Fake task", (ctx, arg) => {
                                ctx.Log("Fake task run : {0}", taskCounter);
                                taskCounter++;
                                return taskCounter;
                            })
                    );
        }
    }
}
