using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KissCI.Internal
{
    public interface IProjectFactory : IDisposable
    {
        IEnumerable<Project> FetchProjects();
    }

    public class MainProjectFactory : IProjectFactory
    {
        public MainProjectFactory(string directory)
        {
            _directory = directory;
            _watcher = new FileSystemWatcher(_directory);
            _watcher.Changed += _watcher_Changed;
            _watcher.Created += _watcher_Changed;
            _watcher.Deleted += _watcher_Changed;
            _watcher.Renamed += _watcher_Changed;
            _watcher.EnableRaisingEvents = true;
            LoadAssemblies();
        }

        void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            LoadAssemblies();
        }

        string _directory;
        FileSystemWatcher _watcher;
        IList<Project> _projects = new List<Project>();

        void LoadAssemblies()
        {
            var dir = new DirectoryInfo(_directory);
            var files = dir.GetFiles().Where(f => f.FullName.ToLower().EndsWith(".dll"));

            var assemblies = files.Select(f => Assembly.LoadFile(f.FullName));

            var allTypes = assemblies.SelectMany(a => a.GetTypes());

            var typ = typeof(IProjectProvider);
            var providerTypes = allTypes.Where(t => typ.IsAssignableFrom(t));

            var providers = providerTypes.Select(p => (IProjectProvider)Activator.CreateInstance(p));

            _projects = providers.SelectMany(p => p.Projects()).ToList();
        }

        public IEnumerable<Project> FetchProjects()
        {
            return _projects;
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }
    }
}
