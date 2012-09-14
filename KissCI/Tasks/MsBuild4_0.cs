using KissCI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Tasks
{
    public class MsBuildArgs
    {
        public MsBuildArgs(string projectFile, string outputPath, string configuration)
        {
            _projectFile = projectFile;
            _outputPath = outputPath;
            _configuration = configuration;
        }

        string _projectFile;
        string _outputPath;
        string _configuration;

        public string ProjectFile { get { return _projectFile; } }
        public string OutputPath { get { return _outputPath; } }
        public string Configuration { get { return _configuration; } }

        public string Target { get; set; }
        public int? WarningLevel { get; set; }
    }

    public class MsBuildResult {
        public string OutputPath { get; set; }
    }

    public static class MsBuild
    {
        public static BuildTask<TArg, MsBuildResult> TempMsBuild4_0<TArg, TResult>(this BuildTask<TArg, TResult> t, string tempRoot, string projectFile, string configuration)
        {
            return t.AddStep((ctx, arg) =>
            {
                var temp = new TempDirectory(tempRoot);
                //ctx.RegisterCleanup(() => temp.Dispose());

                return new MsBuildArgs(projectFile, temp.DirectoryPath, configuration);
            }).MsBuild4_0();
        }

        public static BuildTask<TArg, MsBuildResult> MsBuild4_0<TArg, TResult>(this BuildTask<TArg, TResult> t, string projectFile, string outputPath, string configuration)
        {
            return t.AddStep((ctx, arg) =>
            {
                return new MsBuildArgs(projectFile, outputPath, configuration);
            }).MsBuild4_0();
        }

        const string MsBuildPath = @"c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe";
        public static BuildTask<TArg, MsBuildResult> MsBuild4_0<TArg>(this BuildTask<TArg, MsBuildArgs> t)
        {
            return t.AddTask("MsBuild", (ctx, arg) =>
            {
                var targetArg = !string.IsNullOrEmpty(arg.Target) ? "target:" + arg.Target : "";
                var warningArg = arg.WarningLevel.HasValue ? string.Format("/property:WarningLevel={0}", arg.WarningLevel.Value) : "";
                var outputArg = string.Format("/p:OutDir=\"{0}\" /p:OutputPath=bin\\", arg.OutputPath);
                var configurationArg = string.Format("/p:Configuration={0}", arg.Configuration);
                var projectArg = arg.ProjectFile;

                var args = new string[] { 
                    projectArg, 
                    outputArg, 
                    configurationArg, 
                    warningArg, 
                    targetArg 
                }.Aggregate("", (acc, a) => string.IsNullOrEmpty(a) ? acc : acc + " " + a);

                ctx.Log("Begin msbuild on: {0} and output to: {1} with args: {2}", arg.ProjectFile, arg.OutputPath, args);

                using (var proc = ProcessHelper.Start(MsBuildPath, args))
                {
                    ctx.Log(proc.StandardOutput.ReadToEnd());
                    ctx.Log(proc.StandardError.ReadToEnd());

                    proc.WaitForExit();

                    if (proc.ExitCode > 0)
                    {
                        throw new Exception(string.Format("MsBuild failed on {0} while attempting to output to: {1} with args: {2} and an exit code of: {3}", arg.ProjectFile, arg.OutputPath, args, proc.ExitCode));
                    }
                }

                return new MsBuildResult { OutputPath = arg.OutputPath };
            });
        }
    }
}
