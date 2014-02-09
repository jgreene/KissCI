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
        public static KissTask<TArg, GitResult> Git<TArg, TResult>(this KissTask<TArg, TResult> t, string repositoryUrl, string destination)
        {
            return t.AddStep((ctx, arg) =>
            {
                return new GitArgs { RepositoryUrl = repositoryUrl, OutputDirectory = destination };
            }).Git();
        }

        static string GetCheckoutArgs(GitArgs arg)
        {
            return string.Format("clone \"{0}\" \"{1}\"", arg.RepositoryUrl, arg.OutputDirectory);
        }

        static string GetUpdateArgs(GitArgs arg)
        {
            return string.Format("--git-dir=\"{0}\\.git\" pull", arg.OutputDirectory);
        }

        public static KissTask<TArg, GitResult> Git<TArg>(this KissTask<TArg, GitArgs> t)
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
