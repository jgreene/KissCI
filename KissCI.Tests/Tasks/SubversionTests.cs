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
    public class SubversionTests
    {
        [TestInitialize]
        public void Setup()
        {
            DataHelper.CleanDb();
        }

        [TestMethod]
        public void CanCheckoutProject()
        {
            var executableDirectory = DirectoryHelper.ExecutingDirectory();

            var outputDirectory = Path.Combine(executableDirectory.FullName, "SubversionOutput");

            var tasks = TaskHelper.Start()
            .Subversion("http://openjpeg.googlecode.com/svn/trunk/", outputDirectory)
            .Finalize();

            var command = new KissCommand("build", tasks);

            var project = new KissProject("TestProject", "UI", command);

            using (var projectService = TestHelper.GetService())
            {
                projectService.RegisterProject(project);

                ProjectHelper.Run(project, command, projectService);

                var output = new DirectoryInfo(outputDirectory);

                Assert.IsTrue(output.EnumerateFiles().Any());
            }

            
        }
    }
}
