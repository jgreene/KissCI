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
        [TestMethod]
        public void CanCheckoutProject()
        {
            var executableDirectory = DirectoryHelper.CurrentDirectory();

            var outputDirectory = Path.Combine(executableDirectory.FullName, "SubversionOutput");

            var tasks = TaskHelper.Start()
            .Subversion("http://openjpeg.googlecode.com/svn/trunk/", outputDirectory)
            .Finalize();

            var project = new Project("TestProject", "UI", tasks);

            using (var projectService = ServiceHelper.GetService())
            {
                ProjectHelper.Run(project, projectService);

                var output = new DirectoryInfo(outputDirectory);

                Assert.IsTrue(output.EnumerateFiles().Any());
            }

            
        }
    }
}
