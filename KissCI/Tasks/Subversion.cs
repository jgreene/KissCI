using KissCI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Tasks
{
    public class SubversionArgs
    {
        public SubversionArgs(string trunkUrl, string destination, string userName = null, string password = null)
        {
            _trunkUrl = trunkUrl;
            _destination = destination;
            _userName = userName;
            _password = password;
        }

        readonly string _trunkUrl;
        readonly string _destination;
        readonly string _userName;
        readonly string _password;

        public string TrunkUrl { get { return _trunkUrl; } }
        public string Destination { get { return _destination; } }
        public string UserName { get { return _userName; } }
        public string Password { get { return _password; } }

        public string Args { get; set; }
    }

    public class SubversionResult
    {
        public string Destination { get; set; }
    }

    public static class SubversionExtensions
    {
        static string[] SubversionPaths = new string[] {
            @"C:\Program Files (x86)\Subversion\bin\svn.exe",
            @"C:\Program Files\Subversion\bin\svn.exe"
        };

        static string _subversionPath = null;

        static string GetSubversionPath()
        {
            if (string.IsNullOrEmpty(_subversionPath) == false)
                return _subversionPath;

            _subversionPath = SubversionPaths.FirstOrDefault(p => File.Exists(p));

            if (_subversionPath == null)
                throw new FileNotFoundException("Could not find svn.exe are you sure it is installed?");

            return _subversionPath;
        }

        public static BuildTask<TArg, SubversionResult> Subversion<TArg, TResult>(this BuildTask<TArg, TResult> t, string trunkUrl, string destination, string userName = null, string password = null)
        {
            return t.AddStep((ctx, arg) =>
            {
                return new SubversionArgs(trunkUrl, destination, userName, password);
            }).Subversion();
        }

        static string GetArgs(SubversionArgs arg)
        {
            var svnExists = Directory.Exists(Path.Combine(arg.Destination, ".svn"));

            var args = svnExists ? 
                string.Format("update \"{0}\"", arg.Destination) 
                :
                string.Format("checkout \"{0}\" \"{1}\"", arg.TrunkUrl, arg.Destination);

            var authArgs = string.Format("--username {0} --password {1}", arg.UserName, arg.Password);
            if (string.IsNullOrEmpty(arg.UserName) == false && string.IsNullOrEmpty(arg.Password) == false)
                args = string.Format("{0} {1}", args, authArgs);

            var defaultArgs = " --non-interactive --trust-server-cert --no-auth-cache";

            if (string.IsNullOrEmpty(arg.Args) == false)
                args = args + " " + arg.Args;

            return args + defaultArgs;
        }
        

        public static BuildTask<TArg, SubversionResult> Subversion<TArg>(this BuildTask<TArg, SubversionArgs> t)
        {
            return t.AddTask("Subversion checkout", (ctx, arg) =>
            {
                var args = GetArgs(arg);

                ctx.Log("Begin svn checkout from: {0} and output to: {1} with args: {2}", 
                    arg.TrunkUrl, 
                    arg.Destination, 
                    args);

                using (var proc = ProcessHelper.Start(GetSubversionPath(), args))
                {
                    ctx.Log(proc.StandardOutput.ReadToEnd());
                    ctx.Log(proc.StandardError.ReadToEnd());

                    proc.WaitForExit();

                    if (proc.ExitCode > 0)
                    {
                        throw new Exception(
                            string.Format("Subversion checkout of {0} failed while attempting to output to: {1} with an exit code of: {2}", 
                            arg.TrunkUrl, 
                            arg.Destination, 
                            proc.ExitCode));
                    }
                }

                return new SubversionResult();
            });
        }
    }
}
