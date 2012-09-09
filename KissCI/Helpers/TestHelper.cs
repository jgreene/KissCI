using KissCI.Internal;
using KissCI.Internal.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Helpers
{
    public static class ServiceHelper
    {
        public static IProjectService GetService(string root)
        {
            var projectDir = Path.Combine(root, "Projects");
            var service = new ProjectService(
                root,
                new MainProjectFactory(projectDir),
                () => new KissCI.Internal.NHibernate.NHibernateDataContext()
            );

            return service;
        }
    }
}
