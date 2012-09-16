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

        ServiceInstaller installer;
        ServiceProcessInstaller processInstaller;

        public KissCIInstaller()
        {
            InitializeComponent();

            processInstaller = new ServiceProcessInstaller();
            processInstaller.Account = ServiceAccount.LocalSystem;
            installer = new ServiceInstaller();

            installer.StartType = ServiceStartMode.Manual;
            installer.ServiceName = "KissCIWebService";

            Installers.Add(installer);
            Installers.Add(processInstaller);
        }

        private void KissCIWebServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
