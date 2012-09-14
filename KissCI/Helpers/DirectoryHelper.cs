using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Helpers
{
    public static class DirectoryHelper
    {
        public static DirectoryInfo ExecutingDirectory()
        {
            return new DirectoryInfo(Path.GetDirectoryName(typeof(DirectoryHelper).Assembly.Location));
        }

        public static void EnsureDirectory(string directory) { EnsureDirectories(directory); }

        public static void EnsureDirectories(params string[] directories)
        {
            foreach (var dir in directories)
                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);
        }

        public static void CleanAndEnsureDirectories(params string[] directories)
        {
            foreach (var dir in directories)
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }

            EnsureDirectories(directories);
        }

        public static void CleanAndEnsureDirectory(string directory)
        {
            CleanAndEnsureDirectories(directory);
        }
    }
}
