using KissCI.Internal.Domain;
using KissCI.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KissCI.Tasks;
using KissCI.Helpers;
using KissCI.Internal;
using System.Threading;
using KissCI.NHibernate.Internal;

namespace KissCI.Tests.Domain
{
    [TestClass]
    public class ProjectServiceTests
    {
        IProjectService GetService()
        {
            var executableDirectory = DirectoryHelper.CurrentDirectory();
            var outputDirectory = Path.Combine(executableDirectory.FullName, "ServiceTests");

            var copyTo = Path.Combine(outputDirectory, "Projects");
            DirectoryHelper.EnsureDirectory(copyTo);
            
            var projectsDirectory = Path.Combine(executableDirectory.Parent.Parent.Parent.FullName, "KissCI.Projects");
            var projectFile = Path.Combine(projectsDirectory, "KissCI.Projects.csproj");

            var tasks = TaskHelper.Start()
            .CreateTempDirectory(outputDirectory)
            .AddStep((ctx, arg) =>
            {
                return new MsBuildArgs(projectFile, arg.Path, "Debug");
            })
            .MsBuild4_0()
            .AddTask("Copy assembly", (ctx, arg) =>
            {
                try
                {
                    File.Copy(Path.Combine(arg.OutputPath, "KissCI.Projects.dll"), Path.Combine(copyTo, "KissCI.Projects.dll"), true);
                }
                catch { }
                return true;
            })
            .Finalize();

            var project = new Project("Test", "UI", tasks);

            ProjectHelper.Run(project);

            return new ProjectService(
                outputDirectory, 
                new MainProjectFactory(copyTo),
                () => new KissCI.NHibernate.NHibernateDataContext()
            );
        }

        [TestMethod]
        public void CanGetProjects()
        {
            using (var service = GetService())
            {
                Assert.IsTrue(service.GetProjects().Any());
            }
        }

        [TestMethod]
        public void CanRunProject()
        {
            var current = DirectoryHelper.CurrentDirectory();
            var writeTo = Path.Combine(current.FullName, "TempProjectDirectory");

            DirectoryHelper.CleanAndEnsureDirectory(writeTo);
            var file = Path.Combine(writeTo, "test.txt");

            using (var service = GetService())
            {
                var project = service.RunProject("WriteFileProject");

                Thread.Sleep(500);

                var fileContents = File.ReadAllText(file);
                Assert.IsNotNull(fileContents);
            }
        }

        [TestMethod]
        public void CanCancelProject()
        {
            using (var service = GetService())
            {
                var project = "SleepProject";
                service.RunProject(project);

                Thread.Sleep(500);

                Assert.IsTrue(service.CancelProject(project));
                
            }
        }

        [TestMethod]
        public void CanSaveProjectInfo()
        {
            SessionManager.InitDb();
            using (var ctx = new KissCI.NHibernate.NHibernateDataContext())
            {
                
                var srv = ctx.ProjectInfoService;

                var info = new ProjectInfo
                {
                    ProjectName = "Test",
                    Activity = Activity.Sleeping,
                    Status = Status.Running
                };

                srv.Save(info);

                Assert.IsTrue(info.Id > 0);
            }
        }
    }
}
