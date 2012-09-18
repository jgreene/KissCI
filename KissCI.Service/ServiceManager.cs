using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration.Install;
using System.ServiceProcess;
using System.Reflection;

namespace KissCI.Service
{
    public class ServiceManager : IDisposable
    {
        public ServiceManager(string serviceName, Assembly assembly)
        {
            _serviceName = serviceName;
            _controller = new ServiceController(_serviceName);
            _assembly = assembly;
        }

        readonly string _serviceName;
        readonly ServiceController _controller;
        readonly Assembly _assembly;

        public bool IsInstalled()
        {
            try
            {
                var status = _controller.Status;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsRunning()
        {
            return IsInstalled() && _controller.Status == ServiceControllerStatus.Running;
        }

        AssemblyInstaller GetInstaller()
        {
            var installer = new AssemblyInstaller(_assembly, new string[] { string.Format("/ServiceName={0}", _serviceName) });
            installer.UseNewContext = true;
            return installer;
        }

        public void Install()
        {
            if (IsInstalled())
                return;

            using (var installer = GetInstaller())
            {
                var state = new System.Collections.Hashtable();
                try
                {
                    installer.Install(state);
                    installer.Commit(state);
                }
                catch
                {
                    try
                    {
                        installer.Rollback(state);
                    }
                    catch
                    {

                    }
                    throw;
                }
            }
        }

        public void Uninstall()
        {
            using (var installer = GetInstaller())
            {
                var state = new System.Collections.Hashtable();
                installer.Uninstall(state);
            }
        }

        public void Start()
        {
            if (IsInstalled() == false) return;

            if (_controller.Status == ServiceControllerStatus.Running) return;

            _controller.Start();

        }

        public void Stop()
        {
            if (IsInstalled() == false) return;

            if (_controller.Status == ServiceControllerStatus.Stopped) return;

            _controller.Stop();
        }


        public void Dispose()
        {
            _controller.Dispose();
        }
    }
}
