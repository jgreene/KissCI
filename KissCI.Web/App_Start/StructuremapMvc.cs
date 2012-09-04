using System.Web.Http;
using System.Web.Mvc;
using StructureMap;

namespace KissCI.Web {
    public static class StructuremapMvc {
        public static void RegisterIoc(string projectRoot, string connectionString)
        {
            IContainer container = IoC.Initialize(projectRoot, connectionString);
            DependencyResolver.SetResolver(new StructureMapDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new StructureMapDependencyResolver(container);
        }
    }
}