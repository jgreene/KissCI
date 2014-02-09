using System.Web.Http;
using System.Web.Mvc;
using StructureMap;
using KissCI.Internal.Domain;

namespace KissCI.Web {
    public static class StructuremapMvc {
        public static void RegisterIoc(string projectRoot)
        {
            var projectService = new MainProjectService(projectRoot);

            ObjectFactory.Initialize(x =>
            {
                x.For<IProjectService>().Add(projectService);
            });
            DependencyResolver.SetResolver(new StructureMapDependencyResolver(ObjectFactory.Container));
            GlobalConfiguration.Configuration.DependencyResolver = new StructureMapDependencyResolver(ObjectFactory.Container);
        }
    }
}