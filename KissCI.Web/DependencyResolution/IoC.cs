using KissCI.Internal.Domain;
using KissCI.Internal;
using StructureMap;
using System.IO;
using KissCI.NHibernate.Internal;
namespace KissCI.Web {
    public static class IoC {
        public static IContainer Initialize(string projectRoot) {
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