﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KissCI.Tests.Helpers;
using KissCI.Tasks;
using KissCI.Helpers;
using KissCI.Internal.Helpers;

namespace KissCI.Tests.Tasks
{
    [TestClass]
    public class RobocopyTests
    {
        [TestInitialize]
        public void Setup()
        {
            DataHelper.CleanDb();
        }

        [TestMethod]
        public void CanRobocopy()
        {
            var executableDirectory = DirectoryHelper.ExecutingDirectory();

            var outputDirectory = Path.Combine(executableDirectory.FullName, "Robo");

            var tasks = TaskHelper.Start()
            .AddTask("Initialize directories", (ctx, arg) => {
                var tempDir1 = new TempDirectory(outputDirectory, ctx.ProjectName);
                var tempDir2 = new TempDirectory(outputDirectory, ctx.ProjectName);

                File.AppendAllText(Path.Combine(tempDir1.DirectoryPath, "testfile.txt"), "test data");

                ctx.RegisterCleanup(() => { tempDir1.Dispose(); tempDir2.Dispose(); });

                return new RobocopyArgs(tempDir1.DirectoryPath, tempDir2.DirectoryPath);
            })
            .Robocopy()
            .AddStep((ctx, arg) => {
                var exists = File.Exists(Path.Combine(arg.CopiedToDirectory, "testfile.txt"));
                Assert.IsTrue(exists);
                return exists;
            })
            .Finalize();

            var command = new KissCommand("build", tasks);

            var project = new KissProject("TestProject", "UI", command);

            using (var projectService = TestHelper.GetService())
            {
                projectService.RegisterProject(project);
                ProjectHelper.Run(project, command, projectService);
            }
        }
    }
}
