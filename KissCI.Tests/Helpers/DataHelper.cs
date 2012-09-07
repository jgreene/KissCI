using KissCI.Internal;
using KissCI.NHibernate.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Tests.Helpers
{
    public static class DataHelper
    {
        public static void InitDb()
        {
            File.Delete("KissCI.db3");
            SessionManager.InitDb();
        }

        public static IDataContext OpenContext()
        {
            return new NHibernate.NHibernateDataContext();
        }
    }
}
