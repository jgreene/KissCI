using KissCI.Helpers;
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
        public static void CleanDb()
        {
            SessionManager.Clear();
            var executableDirectory = DirectoryHelper.ExecutingDirectory();
            var dbPath = Path.Combine(executableDirectory.FullName, "KissCI.db3");
            File.Delete(dbPath);
        }

        public static IDataContext OpenContext()
        {
            return TestHelper.GetService().OpenContext();
        }
    }
}
