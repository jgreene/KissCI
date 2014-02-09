using KissCI.Helpers;
using KissCI.Internal.Domain;
using Nancy;
using System;

namespace KissCI.Web
{
    

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        // The bootstrapper enables you to reconfigure the composition of the framework,
        // by overriding the various methods and properties.
        // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper

        protected override void ConfigureApplicationContainer(Nancy.TinyIoc.TinyIoCContainer container)
        {
            var projectRoot = AppDomain.CurrentDomain.BaseDirectory;
            var projectService = new MainProjectService(projectRoot);
            container.Register<IProjectService>(projectService);
            
            //base.ConfigureApplicationContainer(container);
        }
    }
}