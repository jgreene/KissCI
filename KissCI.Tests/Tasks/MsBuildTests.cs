using KissCI.Helpers;
using KissCI.Internal.Helpers;
using KissCI.Tasks;
using KissCI.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Tests.Tasks
{
    [TestClass]
    public class MsBuildTests
    {
        [TestMethod]
        public void CanBuildKissCI()
        {
            var executableDirectory = DirectoryHelper.ExecutingDirectory();

            var outputDirectory = Path.Combine(executableDirectory.FullName, "BuildOutput");

            DirectoryHelper.CleanAndEnsureDirectory(outputDirectory);

            var flintRoot = TestHelper.FlintCIRoot();
            var flintProject = Path.Combine(flintRoot.FullName, "KissCI", "KissCI.csproj");

            var tasks = TaskHelper.Start()
            .MsBuild4_0(flintProject, outputDirectory, "Debug")
            .Finalize();

            var project = new Project("TestProject", "UI", tasks);

            using (var projectService = TestHelper.GetService())
            {
                projectService.RegisterProject(project);
                ProjectHelper.Run(project, projectService);
            }

            var output = new DirectoryInfo(outputDirectory);

            Assert.IsTrue(output.EnumerateFiles().Any());

        }
    }
}
