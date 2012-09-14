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

            using (var tempService = ServiceHelper.GetService(outputDirectory))
            {
                tempService.RegisterProject(project);
                ProjectHelper.Run(project, tempService);
            }            

            return ServiceHelper.GetService(outputDirectory);
        }

        public static DirectoryInfo FlintCIRoot()
        {
            var current = DirectoryHelper.ExecutingDirectory();

            return current.Parent.Parent.Parent;
        }
    }
}
