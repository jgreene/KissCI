using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace KissCI.Service
{
    [RunInstaller(true)]
    public class KissCIInstaller : System.Configuration.Install.Installer
    {
        public static string ServiceName = "KissCIWebService";

        public KissCIInstaller()
        {

            var processInstaller = new ServiceProcessInstaller();
            processInstaller.Account = ServiceAccount.LocalSystem;
            var installer = new ServiceInstaller();

            installer.StartType = ServiceStartMode.Automatic;
            installer.ServiceName = ServiceName;

            Installers.Add(installer);
            Installers.Add(processInstaller);
        }
    }
}
