using KissCI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Tasks
{
    public class TempDirectoryArgs
    {
        public string RootDirectory { get; set; }
    }

    public class TempDirectoryResult
    {
        public string Path { get; set; }
    }

    public class TempDirectory : IDisposable
    {
        public TempDirectory(string rootDirectory, string prefix = null)
        {
            _rootDirectory = rootDirectory;
            _prefix = prefix;
            _directoryPath = Create();
        }

        readonly string _rootDirectory;
        readonly string _prefix;
        readonly string _directoryPath;

        public string RootDirectory { get { return _rootDirectory; } }
        public string Prefix { get { return _prefix; } }
        public string DirectoryPath { get { return _directoryPath; } }

        string Create()
        {
            if (Directory.Exists(_rootDirectory) == false)
                Directory.CreateDirectory(_rootDirectory);

            var now = TimeHelper.Now.ToString("yyyy-MM-dd__hh-mm-ss");

            var name = now + "_" + Guid.NewGuid().ToString().Split('-')[0];

            if (string.IsNullOrEmpty(_prefix) == false)
                name = _prefix + "_" + name;

            var dir = Path.Combine(_rootDirectory, name);

            if (Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);

            return dir;
        }
    
        public void Dispose()
        {
            //Directory.Delete(_directoryPath, true);
        }
    }

    public static class TempDirectoryExtensions
    {
        public static BuildTask<TArg, TempDirectoryResult> CreateTempDirectory<TArg, TResult>(this BuildTask<TArg, TResult> t, string rootDirectory)
        {
            return t.AddStep((ctx, arg) =>
            {
                return new TempDirectoryArgs { RootDirectory = rootDirectory };
            }).CreateTempDirectory();
        }

        public static BuildTask<TArg, TempDirectoryResult> CreateTempDirectory<TArg>(this BuildTask<TArg, TempDirectoryArgs> t)
        {
            return t.AddTask("Create Temp Directory", (ctx, arg) =>
            {
                var tempDir = new TempDirectory(arg.RootDirectory, ctx.ProjectName);

                ctx.RegisterCleanup(() => tempDir.Dispose());

                return new TempDirectoryResult { Path = tempDir.DirectoryPath };
            });
        }
    }
}
