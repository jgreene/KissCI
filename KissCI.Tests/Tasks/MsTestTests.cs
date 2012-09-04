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
    public class MsTestTests
    {
        [TestMethod]
        public void CanRunTestProject()
        {
            var executableDirectory = DirectoryHelper.CurrentDirectory();

            var outputDirectory = Path.Combine(executableDirectory.FullName, "TestOutput");
            var testDll = Path.Combine(executableDirectory.FullName, "KissCI.Tests.FakeTests.dll");

            DirectoryHelper.CleanAndEnsureDirectory(outputDirectory);

            var testResultsPath = Path.Combine(outputDirectory, "testresults.trx");

            var tasks = TaskHelper.Start()
            .MsTest(testDll, testResultsPath)
            .Finalize();

            var project = new Project("TestProject", "UI", tasks);

            ProjectHelper.Run(project);

            Assert.IsTrue(File.Exists(testResultsPath));
        }
    }
}
