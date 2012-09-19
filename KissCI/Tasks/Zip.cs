using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KissCI.Helpers;
using Ionic.Zip;

namespace KissCI.Tasks
{
    public class ZipArgs
    {
        public string DirectoryPath { get; set; }
        public string FilePath { get; set; }
    }

    public class ZipResult
    {
        public string FilePath { get; set; }
    }

    public static class ZipFileExtensions
    {
        public static BuildTask<TArg, ZipResult> Zip<TArg, TResult>(this BuildTask<TArg, TResult> t, string directoryPath, string filePath)
        {
            return t.AddStep((ctx, arg) =>
            {
                return new ZipArgs { DirectoryPath = directoryPath, FilePath = filePath };
            }).Zip();
        }

        public static BuildTask<TArg, ZipResult> Zip<TArg>(this BuildTask<TArg, ZipArgs> t)
        {
            return t.AddTask("Zip directory", (ctx, arg) =>
            {
                using (var zip = new ZipFile())
                {
                    zip.AddDirectory(arg.DirectoryPath);
                    zip.Save(arg.FilePath);
                }


                return new ZipResult {
                    FilePath = arg.FilePath
                };
            });
        }
    }
}
