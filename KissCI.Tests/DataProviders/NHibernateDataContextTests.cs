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

    }
}
