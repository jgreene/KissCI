using KissCI.Internal.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KissCI.Tasks;
using KissCI.Helpers;
using KissCI.Internal;
using KissCI.Internal.Helpers;

namespace KissCI.Tests.Helpers
{
    public static class TestHelper
    {
        public static IProjectService GetService()
        {
            DataHelper.CleanDb();

            var executableDirectory = DirectoryHelper.ExecutingDirectory();
            //var outputDirectory = Path.Combine(executableDirectory.FullName, "ServiceTests");
            var outputDirectory = executableDirectory.FullName;

            var copyTo = Path.Combine(outputDirectory, "Projects");
            DirectoryHelper.EnsureDirectory(copyTo);

            var projectsFile = "KissCI.Tests.Projects.dll";

            try
            {
                File.Copy(Path.Combine(outputDirectory, projectsFile), Path.Combine(copyTo, projectsFile));
            }
            catch { }

            return ServiceHelper.GetService(outputDirectory);
        }

        public static DirectoryInfo FlintCIRoot()
        {
            var current = DirectoryHelper.ExecutingDirectory();

            return current.Parent.Parent.Parent;
        }
    }
}
