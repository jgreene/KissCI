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
        [TestInitialize]
        public void Setup()
        {
            DataHelper.CleanDb();
        }

        [TestMethod]
        public void CanSaveProjectInfo()
        {
            using (var ctx = DataHelper.OpenContext())
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
            using (var ctx = DataHelper.OpenContext())
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


        [TestMethod]
        public void CanAddKey()
        {
            using (var service = TestHelper.GetService())
            {
                using (var context = service.OpenContext())
                {
                    context.ConfigurationService.Save("test", "1");
                    context.Commit();
                }

                using (var context = service.OpenContext())
                {
                    var value = context.ConfigurationService.Get("test");

                    Assert.AreEqual(value, "1");
                }
            }
        }

        [TestMethod]
        public void CanRemoveKey()
        {
            using (var service = TestHelper.GetService())
            {
                using (var context = DataHelper.OpenContext())
                {
                    context.ConfigurationService.Save("test", "1");
                    context.Commit();

                }

                using (var context = DataHelper.OpenContext())
                {
                    context.ConfigurationService.Remove("test");
                    context.Commit();
                }

                using (var context = DataHelper.OpenContext())
                {
                    var value = context.ConfigurationService.Get("test");

                    Assert.IsNull(value);
                }
            }
        }
    }
}
