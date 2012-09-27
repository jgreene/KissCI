using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace KissCI.Projects
{
    public class GithubUser
    {
        public GithubUser(string username, string password)
        {
            _username = username;
            _password = password;
        }

        readonly string _username;
        readonly string _password;

        public string Username { get { return _username; } }
        public string Password { get { return _password; } }
    }

    public class GithubRepo
    {
        public GithubRepo(GithubUser user, string owner, string repo)
        {
            _user = user;
            _owner = owner;
            _repo = repo;
        }

        readonly string _owner;
        readonly string _repo;
        GithubUser _user;

        public string Owner { get { return _owner; } }
        public string Repository { get { return _repo; } }
        public GithubUser User { get { return _user; } }
    }

    class MyStreamWriter : IDisposable
    {
        public MyStreamWriter(Stream stream, Encoding encoding)
        {
            _encoding = encoding;
            _stream = stream;
        }

        readonly Stream _stream;
        readonly Encoding _encoding;

        public void WriteLine(string line = "")
        {
            var lineEnding = "\r\n";
            var toWrite = _encoding.GetBytes(line + lineEnding);
            Console.Write(line + lineEnding);
            _stream.Write(toWrite, 0, toWrite.Length);
        }

        public void Write(string line)
        {
            var toWrite = _encoding.GetBytes(line);
            Console.Write(line);
            _stream.Write(toWrite, 0, toWrite.Length);
        }

        public void WriteLine(string format, params string[] args)
        {
            WriteLine(string.Format(format, args));
        }

        public void Dispose()
        {

        }
    }


    public static class GithubV3ApiExtensions
    {
        const string githubApiRoot = @"https://api.github.com/";

        public static GithubRepo WithRepo(this GithubUser user, string owner, string repo)
        {
            return new GithubRepo(user, owner, repo);
        }

        public static List<GithubDownload> GetDownloads(this GithubRepo repo)
        {
            var url = string.Format("/repos/{0}/{1}/downloads", repo.Owner, repo.Repository);
            return JsonRequest<List<GithubDownload>>(repo.User, url);
        }

        public static void DeleteByName(this GithubRepo repo, string name)
        {
            var downloads = repo.GetDownloads();

            var download = downloads.FirstOrDefault(d => d.name == name);
            if (download == null)
                return;

            repo.Delete(download.id);
        }

        public static void Delete(this GithubRepo repo, long id)
        {
            var user = repo.User;
            var url = string.Format("/repos/{0}/{1}/downloads/{2}", repo.Owner, repo.Repository, id);
            var client = new RestClient(githubApiRoot);
            client.Authenticator = new HttpBasicAuthenticator(user.Username, user.Password);
            var request = new RestRequest(url, Method.DELETE);

            var response = client.Execute(request);
        }

        public static GithubUploadResponse Upload(this GithubRepo repo, string filePath, string description = null)
        {
            var githubResponse = repo.GetUploadData(filePath, description);

            Console.WriteLine(JsonConvert.SerializeObject(githubResponse));

            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            var req = (HttpWebRequest)HttpWebRequest.Create(githubResponse.s3_url);
            req.Method = "POST";
            req.ContentType = "multipart/form-data; boundary=" + boundary;
            req.AllowWriteStreamBuffering = true;

            boundary = "\r\n--" + boundary;

            var items = new Dictionary<string, string>
            {
                { "key", githubResponse.path }
                , { "acl", githubResponse.acl }
                , { "success_action_status", "201" }
                , { "Filename", githubResponse.name }
                , { "AWSAccessKeyId", githubResponse.accesskeyid }
                , { "Policy", githubResponse.policy }
                , { "Signature", githubResponse.signature }
                , { "Content-Type", githubResponse.mime_type }
            };

            using (var reqStream = req.GetRequestStream())
            using (var writer = new MyStreamWriter(reqStream, Encoding.UTF8))
            {
                foreach (var item in items)
                {
                    writer.WriteLine(boundary);
                    writer.WriteLine("Content-Disposition: form-data; name=\"{0}\"", item.Key);
                    writer.WriteLine();
                    writer.Write(item.Value);
                }

                writer.WriteLine(boundary);
                writer.WriteLine("Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"", githubResponse.name);
                writer.WriteLine("Content-Type: {0}", "application/octet-stream");
                writer.WriteLine();

                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    CopyStream(file, reqStream);
                }

                writer.WriteLine(boundary + "--");
            }

            using (var response = req.GetResponse())
            using (var responseStream = response.GetResponseStream())
            using (var reader = new StreamReader(responseStream))
            {
                reader.ReadToEnd();

                return githubResponse;
            }
        }

        static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[4096];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        public static GithubUploadResponse GetUploadData(this GithubRepo repo, string filePath, string description = null)
        {
            var info = new FileInfo(filePath);

            var args = new GithubUploadArgs
            {
                name = info.Name,
                size = info.Length,
                description = description
            };

            string url = string.Format("repos/{0}/{1}/downloads", repo.Owner, repo.Repository);

            return JsonRequest<GithubUploadResponse>(repo.User, url, args, Method.POST);
        }

        public static TModel JsonRequest<TModel>(GithubUser user, string url, object args = null, Method method = Method.GET) where TModel : new()
        {
            var client = new RestClient(githubApiRoot);
            client.Authenticator = new HttpBasicAuthenticator(user.Username, user.Password);
            var request = new RestRequest(url, method);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(args);

            var result = client.Execute<TModel>(request);

            return result.Data;
        }
    }

    public class GithubDownload
    {
        public string url { get; set; }
        public string html_url { get; set; }
        public long id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public long size { get; set; }
        public long download_count { get; set; }
        public string content_type { get; set; }
    }

    public class GithubUploadArgs
    {
        public string name { get; set; }
        public long size { get; set; }
        public string description { get; set; }
        public string content_type { get; set; }
    }

    public class GithubUploadResponse
    {
        public string url { get; set; }
        public string html_url { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public long size { get; set; }
        public int download_count { get; set; }
        public string content_type { get; set; }
        public string policy { get; set; }
        public string signature { get; set; }
        public string bucket { get; set; }
        public string accesskeyid { get; set; }
        public string path { get; set; }
        public string acl { get; set; }
        public DateTime expirationdate { get; set; }
        public string prefix { get; set; }
        public string mime_type { get; set; }
        public bool redirect { get; set; }
        public string s3_url { get; set; }
    }
}
