using KissCI.Helpers;
using KissCI.Internal.Domain;
using Nancy;
using Nancy.Conventions;
using Nancy.Diagnostics;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Nancy.Bootstrapper;
using Nancy.ViewEngines;

namespace KissCI.Web
{
    

    public class KissCIBootstrapper : DefaultNancyBootstrapper
    {
        // The bootstrapper enables you to reconfigure the composition of the framework,
        // by overriding the various methods and properties.
        // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper

        protected override void ConfigureApplicationContainer(Nancy.TinyIoc.TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var projectRoot = AppDomain.CurrentDomain.BaseDirectory;
            var projectService = new MainProjectService(projectRoot);
            container.Register<IProjectService>(projectService);

            AppDomainAssemblyTypeScanner.AddAssembliesToScan("KissCI.dll");
        }

        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = @"password" }; }
        }

        protected virtual KissCIConfiguration KissCIConfiguration
        {
            get { return new KissCIConfiguration(); }
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get { return NancyInternalConfiguration.WithOverrides(x => x.ViewLocationProvider = typeof(ResourceViewLocationProvider)); }
        }

        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            pipelines.BeforeRequest += (ctx) => {
                var projectService = container.Resolve<IProjectService>();
                ctx.ViewBag.Categories = projectService.GetCategories().ToArray();
                return null;
            };
        }
    }
}