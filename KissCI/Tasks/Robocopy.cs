using KissCI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Tasks
{
    public class RobocopyArgs
    {
        public RobocopyArgs(string sourceDirectory, string destinationDirectory, string args = null)
        {
            _sourceDirectory = sourceDirectory;
            _destinationDirectory = destinationDirectory;
            _args = args ?? "/MIR";
        }

        string _sourceDirectory;
        string _destinationDirectory;
        string _args;

        public string SourceDirectory { get { return _sourceDirectory; } }
        public string DestinationDirectory { get { return _destinationDirectory; } }

        public string Args { get { return _args; } }
    }

    public class RobocopyResult
    {
        public string CopiedToDirectory { get; set; }
    }

    public static class RobocopyExtensions
    {
        public static KissTask<TArg, RobocopyResult> Robocopy<TArg, TResult>(this KissTask<TArg, TResult> t, string sourceDirectory, string destinationDirectory, string args = null)
        {
            return t.AddStep((ctx, arg) =>
            {
                return new RobocopyArgs(sourceDirectory, destinationDirectory, args);
            }).Robocopy();
        }

        public static KissTask<TArg, RobocopyResult> Robocopy<TArg>(this KissTask<TArg, RobocopyArgs> t)
        {
            return t.AddTask("Robocopy", (ctx, arg) =>
            {
                ctx.Log("Begin robocopy from: {0} to: {1} with args: {2}", arg.SourceDirectory, arg.DestinationDirectory, arg.Args);

                using (var proc = ProcessHelper.Start("robocopy", string.Format("{0} \"{1}\" \"{2}\"", arg.Args, arg.SourceDirectory, arg.DestinationDirectory)))
                {
                    ctx.Log(proc.StandardOutput.ReadToEnd());
                    ctx.Log(proc.StandardError.ReadToEnd());

                    proc.WaitForExit();

                    if (proc.ExitCode > 3)
                    {
                        throw new Exception(string.Format("Robocopy failed copy from: {0} to: {1} with an exit code of: {2}", arg.SourceDirectory, arg.DestinationDirectory, proc.ExitCode));
                    }

                    return new RobocopyResult
                    {
                        CopiedToDirectory = arg.DestinationDirectory
                    };
                }
            });
        }
    }
}
