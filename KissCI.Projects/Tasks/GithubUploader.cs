using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KissCI;
using KissCI.Helpers;
using KissCI.Tasks;
using GitHubUploader.Core;

namespace KissCI.Projects.Tasks
{
    public class GithubUploaderArgs
    {
        public string Repository { get; set; }
        public string GithubUser { get; set; }
        public string GithubPassword { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public string FileType { get; set; }
    }

    public class GithubUploaderResult
    {
        public string FileLocation { get; set; }
    }

    public static class GithubUploaderExtensions
    {
        public static BuildTask<TArg, GithubUploaderResult> GithubUpload<TArg, TResult>(this BuildTask<TArg, TResult> t, string repository, string username, string password, string filePath, string fileType, string description = null)
        {
            return t.AddStep((ctx, arg) =>
            {
                return new GithubUploaderArgs { 
                    GithubUser = username,
                    GithubPassword = password,
                    FilePath = filePath,
                    FileType = fileType
                };
            }).GithubUpload();
        }

        public static BuildTask<TArg, GithubUploaderResult> GithubUpload<TArg>(this BuildTask<TArg, GithubUploaderArgs> t)
        {
            return t.AddTask("Github upload", (ctx, arg) =>
            {
                var uploader = new GithubUploader(arg.GithubUser, arg.GithubPassword);

                var info = new System.IO.FileInfo(arg.FilePath);

                string fileLocation = uploader.Upload(new UploadInfo
                {
                    ContentType = arg.FileType,
                    FileName = arg.FilePath,
                    Description = arg.Description,
                    Name = info.Name,
                    Repository = arg.Repository
                });

                return new GithubUploaderResult
                {
                    FileLocation = fileLocation
                };
            });
        }
    }
}
