using KissCI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Tasks
{
    public class GitArgs
    {
        public string RepositoryUrl { get; set; }
        public string OutputDirectory { get; set; }
    }

    public class GitResult
    {
        public string OutputDirectory { get; set; }
    }

    public static class GitExtensions
    {
        public static BuildTask<TArg, GitResult> Git<TArg, TResult>(this BuildTask<TArg, TResult> t, string repositoryUrl, string destination)
        {
            return t.AddStep((ctx, arg) =>
            {
                return new GitArgs { RepositoryUrl = repositoryUrl, OutputDirectory = destination };
            }).Git();
        }

        static string GetCheckoutArgs(GitArgs arg)
        {
            var args = string.Format("\"{0}\" \"{1}\"", arg.RepositoryUrl, arg.OutputDirectory);

            return string.Format("checkout {0}", args);
        }

        static string GetUpdateArgs(GitArgs arg)
        {
            var args = string.Format("remote update \"{0}\"", arg.OutputDirectory);

            return args;
        }

        public static BuildTask<TArg, GitResult> Git<TArg>(this BuildTask<TArg, GitArgs> t)
        {
            return t.AddTask("Git checkout", (ctx, arg) =>
            {
                var gitExists = Directory.Exists(Path.Combine(arg.OutputDirectory, ".git"));
                var args = gitExists ? GetUpdateArgs(arg) : GetCheckoutArgs(arg);

                ctx.Log("Begin git checkout from: {0} and output to: {1} with args: {2}",
                    arg.RepositoryUrl,
                    arg.OutputDirectory,
                    args);

                using (var proc = ProcessHelper.Start("git", args))
                {
                    ctx.Log(proc.StandardOutput.ReadToEnd());
                    ctx.Log(proc.StandardError.ReadToEnd());

                    proc.WaitForExit();

                    if (proc.ExitCode > 0)
                    {
                        throw new Exception(
                            string.Format("Git checkout of {0} failed while attempting to output to: {1} with an exit code of: {2}",
                            arg.RepositoryUrl,
                            arg.OutputDirectory,
                            proc.ExitCode));
                    }
                }

                return new GitResult { OutputDirectory = arg.OutputDirectory };
            });
        }
    }
}
