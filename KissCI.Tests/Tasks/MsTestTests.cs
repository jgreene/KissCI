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
    public class MsTestTests
    {
        [TestInitialize]
        public void Setup()
        {
            DataHelper.CleanDb();
        }

        [TestMethod]
        public void CanRunTestProject()
        {
            var executableDirectory = DirectoryHelper.ExecutingDirectory();

            var outputDirectory = Path.Combine(executableDirectory.FullName, "TestOutput");
            var testDll = Path.Combine(executableDirectory.FullName, "KissCI.Tests.FakeTests.dll");

            DirectoryHelper.CleanAndEnsureDirectory(outputDirectory);

            var testResultsPath = Path.Combine(outputDirectory, "testresults.trx");

            var tasks = TaskHelper.Start()
            .MsTest(testDll, testResultsPath)
            .Finalize();

            var command = new KissCommand("build", tasks);

            var project = new KissProject("TestProject", "UI", command);

            using (var projectService = TestHelper.GetService())
            {
                projectService.RegisterProject(project);
                ProjectHelper.Run(project, command, projectService);
            }

            Assert.IsTrue(File.Exists(testResultsPath));
        }
    }
}
