using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            var webService = new KissCIWebService();

            
            if (Environment.UserInteractive)
            {
                //is a console app
                webService.Start(args);
            }
            else
            {
                //is a windows service
                var servicesToRun = new ServiceBase[] 
                { 
                    webService
                };
                ServiceBase.Run(servicesToRun);
            }

            
        }
    }
}
