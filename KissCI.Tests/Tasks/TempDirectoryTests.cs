using KissCI.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KissCI.Tasks;
using KissCI.Helpers;
using KissCI.Internal.Helpers;

namespace KissCI.Tests.Tasks
{
    [TestClass]
    public class TempDirectoryTests
    {
        [TestInitialize]
        public void Setup()
        {
            DataHelper.CleanDb();
        }

        [TestMethod]
        public void CanCreateTempDirectory()
        {
            var executableDirectory = DirectoryHelper.ExecutingDirectory();

            var outputDirectory = Path.Combine(executableDirectory.FullName, "TempDirectory");

            var tasks = TaskHelper.Start()
            .CreateTempDirectory(outputDirectory)
            .AddTask("Ensure exists", (ctx, arg) =>{
                var exists = Directory.Exists(arg.Path);
                Assert.IsTrue(exists);
                return exists;
            })
            .Finalize();

            var project = new Project("TestProject", "UI", tasks);

            using (var projectService = TestHelper.GetService())
            {
                projectService.RegisterProject(project);
                ProjectHelper.Run(project, projectService);
            }
            
        }
    }
}
