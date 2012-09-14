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

namespace KissCI.Projects
{
    public class ProjectProvider : IProjectProvider
    {
        void EnsureDirectories(params string[] directories)
        {
            foreach (var dir in directories)
                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);
        }

        Project GetServiceProject()
        {
            var root = @"C:\Projects\Builds\";
            var sourceRoot = Path.Combine(root, "Source");
            var sourceOutput = Path.Combine(sourceRoot, "KissCI");
            var tempRoot = Path.Combine(root, "TempDirectories");
            var outputDir = Path.Combine(root, "BuildOutput");
            var outputTo = Path.Combine(outputDir, "KissCI.Service");
            var outputWebTo = Path.Combine(outputTo, "KissCI.Web");

            EnsureDirectories(root, sourceRoot, sourceOutput, tempRoot, outputDir, outputTo, outputWebTo);

            var serviceTask = TaskHelper.Start()
            .Git("file://C:/Projects/Builds/KissCI", sourceOutput)
            .TempMsBuild4_0(tempRoot, Path.Combine(sourceOutput, "KissCI.Tests", "KissCI.Tests.csproj"), "Debug")
            .AddStep((ctx, arg) => {
                return new MsTestArgs(
                    Path.Combine(arg.OutputPath, "KissCI.Tests.dll"), 
                    Path.Combine(arg.OutputPath, "results.trx"), 
                    Path.Combine(sourceRoot, "KissCI.testsettings"));
            })
            .MsTest()
            .TempMsBuild4_0(tempRoot, Path.Combine(sourceOutput, "KissCI.Service", "KissCI.Service.csproj"), "Debug")
            .AddStep((ctx, arg) =>
            {
                return new RobocopyArgs(arg.OutputPath, outputTo);
            })
            .Robocopy()
            .TempMsBuild4_0(tempRoot, Path.Combine(sourceOutput, "KissCI.Web", "KissCI.Web.csproj"), "Debug")
            .AddStep((ctx, arg) =>
            {
                return new RobocopyArgs(Path.Combine(arg.OutputPath, "_PublishedWebsites", "KissCI.Web"), outputWebTo);
            })
            .Robocopy()
            .Finalize();
            


            var project = new Project("KissCI.Service", "Services", serviceTask);
            project.AddTimer(TimeHelper.Now);
            return project;
        }




        public IEnumerable<Project> Projects()
        {
            yield return GetServiceProject();

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
