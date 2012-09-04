using KissCI.Internal.Domain;
using KissCI.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI.Tests.DataProviders
{
    [TestClass]
    public class SqliteDataProviderTests
    {
        //[TestMethod]
        //public void CanInsertProjectInfo()
        //{
        //    DataHelper.InitializeDatabase();
        //    var provider = DataHelper.DataContext;
        //    var service = provider.ProjectInfoService;

        //    var info = new ProjectInfo
        //    {
        //        ProjectName = "TestProject",
        //        Activity = Activity.Building,
        //        Status = Status.Running
        //    };

        //    service.Save(info);

        //    Assert.IsTrue(info.Id > 0);
        //}

        //[TestMethod]
        //public void CanGetProjectInfos()
        //{
        //    CanInsertProjectInfo();

        //    var service = DataHelper.DataContext.ProjectInfoService;

        //    var infos = service.GetProjectInfos();
        //    Assert.IsTrue(infos.Any());
        //}
    }
}
