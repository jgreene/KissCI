using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Tests.Projects
{
    public static class DirectoryHelper
    {
        public static DirectoryInfo CurrentDirectory()
        {

            return new DirectoryInfo(Path.GetDirectoryName(typeof(DirectoryHelper).Assembly.Location));
        }

        public static void EnsureDirectory(string directory)
        {
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);
        }

        public static void CleanAndEnsureDirectory(string directory)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);

            EnsureDirectory(directory);
        }
    }
}
