using KissCI.Helpers;
using KissCI.Internal.Domain;
using KissCI.NHibernate;
using KissCI.NHibernate.Internal;
using KissCI.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Tests.DataProviders
{
    [TestClass]
    public class NHibernateDataContextTests
    {

        [TestMethod]
        public void CanWriteAndReadMessages()
        {
            var executableDirectory = DirectoryHelper.CurrentDirectory();
            var outputDirectory = Path.Combine(executableDirectory.FullName, "MessageService");

            DirectoryHelper.CleanAndEnsureDirectory(outputDirectory);
            DataHelper.InitDb();


            using (var dataProvider = DataHelper.OpenContext())
            {
                var service = dataProvider.TaskMessageService;

                var message1 = new TaskMessage
                {
                    Time = TimeHelper.Now,
                    ProjectBuildId = 0,
                    Message = "This is a test message"
                };

                var message2 = new TaskMessage
                {
                    Time = TimeHelper.Now,
                    ProjectBuildId = 0,
                    Message = "This is a test message"
                };

                service.WriteMessage(message1);
                service.WriteMessage(message2);

                var messages = service.GetMessages();

                var count = messages.Count();

                Assert.IsTrue(count == 2);

                var read = messages.First();
                Assert.AreEqual(message1.Message, read.Message);
            }
        }

        [TestMethod]
        public void CanSaveProjectInfo()
        {
            SessionManager.InitDb();
            using (var ctx = new KissCI.Internal.NHibernate.NHibernateDataContext())
            {

                var srv = ctx.ProjectInfoService;

                var info = new ProjectInfo
                {
                    ProjectName = "Test",
                    Activity = Activity.Sleeping,
                    Status = Status.Running
                };

                srv.Save(info);

                Assert.IsTrue(info.Id > 0);
            }
        }

        [TestMethod]
        public void CanSaveProjectBuild()
        {
            SessionManager.InitDb();
            using (var ctx = new KissCI.Internal.NHibernate.NHibernateDataContext())
            {

                var srv = ctx.ProjectBuildService;

                var build = new ProjectBuild
                {
                    ProjectInfoId = 0,
                    BuildTime = TimeHelper.Now,
                    BuildResult = null
                };

                srv.Save(build);

                Assert.IsTrue(build.Id > 0);
            }
        }

    }
}
