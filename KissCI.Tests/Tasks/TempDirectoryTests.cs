using KissCI.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KissCI.Tasks;
using KissCI.Helpers;

namespace KissCI.Tests.Tasks
{
    [TestClass]
    public class TempDirectoryTests
    {
        [TestMethod]
        public void CanCreateTempDirectory()
        {
            var executableDirectory = DirectoryHelper.CurrentDirectory();

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

            using (var projectService = ServiceHelper.GetService())
            {
                ProjectHelper.Run(project, projectService);
            }
            
        }
    }
}
