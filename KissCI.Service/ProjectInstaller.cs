using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace KissCI.Service
{
    [RunInstaller(true)]
    public partial class KissCIInstaller : System.Configuration.Install.Installer
    {
        public KissCIInstaller()
        {
            InitializeComponent();

            var processInstaller = new ServiceProcessInstaller();
            processInstaller.Account = ServiceAccount.LocalSystem;
            var installer = new ServiceInstaller();

            installer.StartType = ServiceStartMode.Manual;
            installer.ServiceName = "KissCIWebService";

            Installers.Add(installer);
            Installers.Add(processInstaller);
        }
    }
}
