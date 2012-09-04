using KissCI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Tasks
{
    public class MsTestArgs
    {
        public MsTestArgs(string assemblyPath, string resultsFile)
        {
            _assemblyPath = assemblyPath;
            _resultsFile = resultsFile;
        }

        string _assemblyPath;
        string _resultsFile;

        public string AssemblyPath { get { return _assemblyPath; } }
        public string ResultsFile { get { return _resultsFile; } }
    }

    public class MsTestResult
    {
        
    }

    public static class MsTestExtensions
    {
        static string[] MsTestPaths = new string[] {
            @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\MSTest.exe",
            @"C:\Program Files\Microsoft Visual Studio 10.0\Common7\IDE\MSTest.exe"
        };

        static string _msTestPath = null;

        static string GetMsTestPath()
        {
            if (string.IsNullOrEmpty(_msTestPath) == false)
                return _msTestPath;


            _msTestPath = MsTestPaths.FirstOrDefault(p => File.Exists(p));

            if (_msTestPath == null)
                throw new Exception("Could not find MsTest.exe are you sure it is installed?");

            return _msTestPath;
        }

        public static BuildTask<TArg, MsTestResult> MsTest<TArg, TResult>(this BuildTask<TArg, TResult> t, string assemblyPath, string resultsFile)
        {
            return t.AddStep((ctx, arg) =>
            {
                return new MsTestArgs(assemblyPath, resultsFile);
            }).MsTest();
        }

        public static BuildTask<TArg, MsTestResult> MsTest<TArg>(this BuildTask<TArg, MsTestArgs> t)
        {
            return t.AddTask("MsTest", (ctx, arg) =>
            {
                var args = string.Format("/testcontainer:{0} /resultsfile:{1}", arg.AssemblyPath, arg.ResultsFile);

                ctx.Log("Begin MsTest on: {0} and output to: {1}", arg.AssemblyPath, arg.ResultsFile);

                using (var proc = ProcessHelper.Start(GetMsTestPath(), args))
                {
                    ctx.Log(proc.StandardOutput.ReadToEnd());
                    ctx.Log(proc.StandardError.ReadToEnd());

                    proc.WaitForExit();

                    if (proc.ExitCode > 0)
                    {
                        throw new Exception(string.Format("MsTest failed on {0} while attempting to output to: {1} with an exit code of: {2}", arg.AssemblyPath, arg.ResultsFile, proc.ExitCode));
                    }
                }

                return new MsTestResult();
            });
        }
    }
}
