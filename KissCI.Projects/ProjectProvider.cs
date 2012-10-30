using KissCI.Helpers;
using KissCI.Triggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KissCI.Tasks;
using KissCI.Projects.Tasks;

namespace KissCI.Projects
{
    public class ProjectProvider : IProjectProvider
    {
        void EnsureDirectories(params string[] directories)
        {
            foreach (var dir in directories)
                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);
        }

        Project GetServiceProject()
        {
            var root = @"C:\Projects\Builds\";
            var sourceRoot = Path.Combine(root, "Source");
            var sourceOutput = Path.Combine(sourceRoot, "KissCI");
            var tempRoot = Path.Combine(root, "TempDirectories");
            var outputDir = Path.Combine(root, "BuildOutput");
            var outputTo = Path.Combine(outputDir, "KissCI.Service");
            var outputWebTo = Path.Combine(outputTo, "KissCI.Web");

            EnsureDirectories(root, sourceRoot, sourceOutput, tempRoot, outputDir, outputTo, outputWebTo);

            var serviceTask = TaskHelper.Start()
            .Git("file://C:/Projects/KissCI", sourceOutput)
            .TempMsBuild4_0(tempRoot, Path.Combine(sourceOutput, "KissCI.Tests", "KissCI.Tests.csproj"), "Debug")
            .AddStep((ctx, arg) =>
            {
                return new MsTestArgs(
                    Path.Combine(arg.OutputPath, "KissCI.Tests.dll"),
                    Path.Combine(arg.OutputPath, "results.trx"),
                    Path.Combine(sourceOutput, "KissCI.testsettings"));
            })
            .MsTest()
            .TempMsBuild4_0(tempRoot, Path.Combine(sourceOutput, "KissCI.Service", "KissCI.Service.csproj"), "Debug")
            .AddStep((ctx, arg) =>
            {
                return new RobocopyArgs(arg.OutputPath, outputTo);
            })
            .Robocopy()
            .TempMsBuild4_0(tempRoot, Path.Combine(sourceOutput, "KissCI.Web", "KissCI.Web.csproj"), "Debug")
            .AddStep((ctx, arg) =>
            {
                return new RobocopyArgs(Path.Combine(arg.OutputPath, "_PublishedWebsites", "KissCI.Web"), outputWebTo);
            })
            .Robocopy()
            .AddTask("Ensure Projects folder", (ctx, arg) => {
                var projectsPath = Path.Combine(outputTo, "Projects");
                DirectoryHelper.EnsureDirectory(projectsPath);
                return arg;
            })
            .AddStep((ctx, arg) => {
                return new ZipArgs
                {
                    DirectoryPath = outputTo,
                    FilePath = Path.Combine(outputDir, "KissCI.Service.zip")
                };
            })
            .Zip()
            .AddStep((ctx, arg) => {
                return new GithubUploaderArgs {
                    GithubUser = "jgreene",
                    GithubPassword = "*****",
                    FilePath = arg.FilePath,
                    Owner = "jgreene",
                    Repository = "KissCI",
                    Description = "KissCI latest"
                };
            })
            .GithubUpload()
            .Finalize();

            var project = new Project("KissCI.Service", "Services", serviceTask);
            project.AddTimer(TimeHelper.Now);
            return project;
        }


        public IEnumerable<Project> Projects()
        {
            yield return GetServiceProject();
        }
    }
}
