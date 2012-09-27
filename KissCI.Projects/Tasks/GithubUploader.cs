using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KissCI;
using KissCI.Helpers;
using KissCI.Tasks;

namespace KissCI.Projects.Tasks
{
    public class GithubUploaderArgs
    {
        public string Owner { get; set; }
        public string Repository { get; set; }
        public string GithubUser { get; set; }
        public string GithubPassword { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
    }

    public class GithubUploaderResult
    {
        public string FileLocation { get; set; }
        public string GithubFileUrl { get; set; }
    }

    public static class GithubUploaderExtensions
    {
        public static BuildTask<TArg, GithubUploaderResult> GithubUpload<TArg, TResult>(this BuildTask<TArg, TResult> t, string owner, string repository, string username, string password, string filePath, string description = null)
        {
            return t.AddStep((ctx, arg) =>
            {
                return new GithubUploaderArgs { 
                    GithubUser = username,
                    GithubPassword = password,
                    Owner = owner,
                    Repository = repository,
                    FilePath = filePath,
                };
            }).GithubUpload();
        }

        public static BuildTask<TArg, GithubUploaderResult> GithubUpload<TArg>(this BuildTask<TArg, GithubUploaderArgs> t)
        {
            return t.AddTask("Github upload", (ctx, arg) =>
            {
                var user = new GithubUser(arg.GithubUser, arg.GithubPassword);
                var repo = user.WithRepo(arg.Owner, arg.Repository);

                var fileName = Path.GetFileName(arg.FilePath);

                repo.DeleteByName(fileName);

                var upload = repo.Upload(arg.FilePath, arg.Description);

                return new GithubUploaderResult
                {
                    FileLocation = arg.FilePath,
                    GithubFileUrl = upload.url
                };
            });
        }
    }
}
