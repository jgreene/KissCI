using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Configuration;
using NDesk.Options;

namespace KissCI.Service
{
    public partial class KissCIWebService : ServiceBase
    {
        public KissCIWebService()
        {

        }

        public void Start(string[] args)
        {

            var install = false;
            var uninstall = false;
            var serviceName = "";

            var options = new OptionSet()
            {
                { "install=", v => { serviceName = v; install = true; } },
                { "uninstall=", v => { serviceName = v; uninstall = true; } }
            };

            try
            {
                options.Parse(args);
            }
            catch (OptionException ex)
            {
                Console.WriteLine(string.Format("error parsing arguments: {0}", ex));
                return;
            }

            Console.WriteLine(install);
            Console.WriteLine(uninstall);
            Console.WriteLine(serviceName);

            if (install)
            {
                using (var service = new ServiceManager(serviceName, typeof(KissCIWebService).Assembly))
                {
                    service.Install();
                    return;
                }
            }

            if (uninstall)
            {
                using (var service = new ServiceManager(serviceName, typeof(KissCIWebService).Assembly))
                {
                    service.Uninstall();
                    return;
                }
            }



            var root = new DirectoryInfo(Environment.CurrentDirectory).FullName;
            var port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            var webPath = ConfigurationManager.AppSettings["WebPath"];
            if (string.IsNullOrEmpty(webPath))
                webPath = "KissCI.Web";

            var webRoot = Path.IsPathRooted(webPath) ? webPath : Path.Combine(root, webPath);
            var hostName = ConfigurationManager.AppSettings["HostName"];


            var server = new CassiniDev.Server(port, "/", webRoot, IPAddress.Any, hostName);
            server.Start();

            if (Environment.UserInteractive)
            {
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

        private void InitializeComponent()
        {
            // 
            // KissCIWebService
            // 
            this.ServiceName = "KissCIWebService";

        }
    }
}
