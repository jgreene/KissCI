using KissCI.Internal.Domain;
using KissCI.Internal;
using StructureMap;
using System.IO;
namespace KissCI.Web {
    public static class IoC {
        public static IContainer Initialize(string projectRoot, string connectionString) {
            var projectFactory = new MainProjectFactory(Path.Combine(projectRoot, "Projects"));
            var projectService = new ProjectService(projectRoot, projectFactory, () => new KissCI.NHibernate.NHibernateDataContext());

            ObjectFactory.Initialize(x =>
                        {
                            x.For<IProjectService>().Add(projectService);
                        });
            return ObjectFactory.Container;
        }
    }
}