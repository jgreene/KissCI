using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Configuration;

namespace KissCI.Service
{
    public partial class KissCIWebService : ServiceBase
    {
        public KissCIWebService()
        {

        }

        public void Start(string[] args)
        {
            var root = new DirectoryInfo(Environment.CurrentDirectory).FullName;
            var port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            var webPath = ConfigurationManager.AppSettings["WebPath"];
            if (string.IsNullOrEmpty(webPath))
                webPath = "KissCI.Web";

            var webRoot = Path.IsPathRooted(webPath) ? webPath : Path.Combine(root, webPath);
            var hostName = ConfigurationManager.AppSettings["HostName"];


            using (var server = new CassiniDev.Server(port, "/", webRoot, IPAddress.Any, hostName))
            {
                server.Start();

                Console.ReadLine();
            }
        }

        protected override void OnStart(string[] args)
        {
            Start(args);
        }

        protected override void OnStop()
        {

        }
    }
}
