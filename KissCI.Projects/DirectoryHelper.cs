using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Projects
{
    public static class DirectoryHelper
    {
        public static DirectoryInfo CurrentDirectory()
        {
            return new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        }

        public static DirectoryInfo FlintCIRoot()
        {
            var current = CurrentDirectory();

            return current.Parent.Parent.Parent;
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
