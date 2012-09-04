using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace KissCI.Service
{
    public partial class KissCIWebService : ServiceBase
    {
        public KissCIWebService()
        {

        }

        public void Start(string[] args)
        {
            var root = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            var webRoot = Path.Combine(root, "KissCI.Web");
            using (var server = new CassiniDev.Server(38594, webRoot))
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
