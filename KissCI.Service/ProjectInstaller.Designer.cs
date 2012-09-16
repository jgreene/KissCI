namespace KissCI.Service
{
    partial class KissCIInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.KissCIWebServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // KissCIWebServiceInstaller
            // 
            this.KissCIWebServiceInstaller.ServiceName = "KissCIWebService";
            this.KissCIWebServiceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.KissCIWebServiceInstaller_AfterInstall);
            // 
            // KissCIInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.KissCIWebServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceInstaller KissCIWebServiceInstaller;
    }
}