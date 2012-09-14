﻿using KissCI.Internal.Domain;
using KissCI.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KissCI.Tasks;
using KissCI.Helpers;
using KissCI.Internal;
using System.Threading;
using KissCI.NHibernate.Internal;

namespace KissCI.Tests.Domain
{
    [TestClass]
    public class ProjectServiceTests
    {
        [TestInitialize]
        public void Setup()
        {
            DataHelper.CleanDb();
        }

        [TestMethod]
        public void CanGetProjects()
        {
            using (var service = TestHelper.GetService())
            {
                Assert.IsTrue(service.GetProjects().Any());
            }
        }

        [TestMethod]
        public void CanRunProject()
        {
            var current = DirectoryHelper.ExecutingDirectory();
            var writeTo = Path.Combine(current.FullName, "TempProjectDirectory");

            DirectoryHelper.CleanAndEnsureDirectory(writeTo);
            var file = Path.Combine(writeTo, "test.txt");

            using (var service = TestHelper.GetService())
            {
                var project = service.RunProject("WriteFileProject");

                Thread.Sleep(500);

                var fileContents = File.ReadAllText(file);
                Assert.IsNotNull(fileContents);
            }
        }

        [TestMethod]
        public void CanCancelProject()
        {
            using (var service = TestHelper.GetService())
            {
                var project = "SleepProject";
                service.RunProject(project);

                Thread.Sleep(500);

                Assert.IsTrue(service.CancelProject(project));
                
            }
        }

        [TestMethod]
        public void CanGetProjectViews()
        {
            using (var provider = DataHelper.OpenContext())
            {
                var projectInfo = new ProjectInfo
                {
                    ProjectName = "Project1",
                    Status = Status.Running,
                    Activity = Activity.Building
                };

                provider.ProjectInfoService.Save(projectInfo);

                var projectBuild = new ProjectBuild
                {
                    ProjectInfoId = projectInfo.Id,
                    BuildTime = TimeHelper.Now,
                    LogFile = "build.txt"
                };

                provider.ProjectBuildService.Save(projectBuild);

                var message = new TaskMessage
                {
                    ProjectInfoId = projectInfo.Id,
                    ProjectBuildId = projectBuild.Id,
                    Time = TimeHelper.Now,
                    Message = "This is a task message"
                };

                provider.TaskMessageService.WriteMessage(message);

                provider.Commit();
            }

            using (var service = TestHelper.GetService())
            {
                var views = service.GetProjectViews();

                Assert.IsTrue(views.Count() > 0);
            }
        }

        [TestMethod]
        public void ProjectInfoExistsPerTask()
        {

            using (var service = TestHelper.GetService())
            {
                var projects = service.GetProjects();
                var views = service.GetProjectViews();

                var projectCount = projects.Count;
                var viewCount = projects.Count();
                Assert.IsTrue(projectCount == viewCount);
            }
        }
    }
}
