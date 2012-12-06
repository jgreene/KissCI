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
            var getAssemblies = new Func<DirectoryInfo, IEnumerable<FileInfo>>((input) => {
                return input.GetFiles().Where(f => f.FullName.ToLower().EndsWith(".dll"));
            });

            var dir = new DirectoryInfo(_directory);
            var files = getAssemblies(dir);
            var execDir = DirectoryHelper.ExecutingDirectory();
            var execFiles = getAssemblies(execDir);

            foreach (var assembly in files.Concat(execFiles))
            {
                try
                {
                    Assembly.LoadFrom(assembly.FullName);
                }
                catch { }
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var allTypes = assemblies.SelectMany(a => { 
                try {
                    return a.GetTypes();
                }
                catch{
                    return new Type[0];
                }
            });

            var typ = typeof(IProjectProvider);
            var providerTypes = allTypes.Where(t => typ.IsAssignableFrom(t) && t != typ);

            var providers = providerTypes.Select(p => (IProjectProvider)Activator.CreateInstance(p));

            _projects = providers.SelectMany(p => p.Projects(null)).ToList();
        }

        public IList<Project> FetchProjects()
        {
            return _projects;
        }
    }
}
