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
        static readonly object cleanLock = new object();

        public static void CleanDb()
        {
            lock (cleanLock)
            {
                SessionManager.Clear();
                var executableDirectory = DirectoryHelper.ExecutingDirectory();
                var dbPath = Path.Combine(executableDirectory.FullName, "KissCI.db3");

                try
                {
                    File.Delete(dbPath);
                }
                catch
                {
                    try { File.Delete(dbPath); }
                    catch { }
                }
            }
        }
    }
}
