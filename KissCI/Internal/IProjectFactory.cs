using KissCI.Helpers;
using KissCI.Internal.Helpers;
using KissCI.Triggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KissCI.Internal
{
    public interface IProjectFactory
    {
        IList<Project> FetchProjects();
    }

    public class MainProjectFactory : IProjectFactory
    {

        public MainProjectFactory(string directory)
        {
            _directory = directory;
            DirectoryHelper.EnsureDirectories(_directory);
            LoadAssemblies();
        }

        readonly string _directory;
        IList<Project> _projects = new List<Project>();


        void LoadAssemblies()
        {
            var dir = new DirectoryInfo(_directory);
            var files = dir.GetFiles().Where(f => f.FullName.ToLower().EndsWith(".dll"));

            var assemblies = files.Select(f => Assembly.LoadFrom(f.FullName));

            var allTypes = assemblies.SelectMany(a => a.GetTypes());

            var typ = typeof(IProjectProvider);
            var providerTypes = allTypes.Where(t => typ.IsAssignableFrom(t));

            var providers = providerTypes.Select(p => (IProjectProvider)Activator.CreateInstance(p));

            _projects = providers.SelectMany(p => p.Projects()).ToList();
        }

        public IList<Project> FetchProjects()
        {
            return _projects;
        }
    }
}
