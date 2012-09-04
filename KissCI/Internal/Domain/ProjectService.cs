using KissCI.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using KissCI.Helpers;

namespace KissCI.Internal.Domain
{
    public interface IProjectService : IDisposable
    {
        IEnumerable<Project> GetProjects();
        IEnumerable<ProjectBuild> GetBuilds(string projectName);
        ProjectBuild GetMostRecentBuild(string projectName);
        ProjectInfo GetProjectInfo(string projectName);
        bool RunProject(string projectName);
        bool CancelProject(string projectName);
    }

    public class ProjectService : IProjectService
    {
        public ProjectService(string directoryRoot, IProjectFactory factory, Func<IDataContext> dataProvider)
        {
            _directoryRoot = directoryRoot;
            _projectFolder = Path.Combine(directoryRoot, "Projects");
            _logFolder = Path.Combine(directoryRoot, "Logs");
            _buildFolder = Path.Combine(directoryRoot, "Builds");
            EnsureFolders();
            _factory = factory;
            _dataProvider = dataProvider;
        }

        string _projectFolder;
        string _logFolder;
        string _buildFolder;

        void EnsureFolders()
        {
            var folders = new string[]{
                _directoryRoot,
                _projectFolder,
                _logFolder,
                _buildFolder
            };

            foreach (var folder in folders)
                if (Directory.Exists(folder) == false)
                    Directory.CreateDirectory(folder);
        }

        string _directoryRoot;
        IProjectFactory _factory;
        Func<IDataContext> _dataProvider;

        public IEnumerable<Project> GetProjects()
        {
            return _factory.FetchProjects();
        }

        string GetDirectory(string projectName)
        {
            var proj = Path.Combine(_projectFolder, projectName);
            EnsureDirectory(proj);

            return proj;
        }

        public void EnsureDirectory(string dir)
        {
            if (Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);
        }

        Project GetProject(string projectName)
        {
            return GetProjects().FirstOrDefault(p => p.Name == projectName);
        }

        

        //ProjectBuild GetBuild(string projectName, DirectoryInfo info)
        //{
        //    var proj = GetProject(projectName);
        //    var build =  new ProjectBuild();
        //    build.ProjectName = projectName;
        //    build.BuildTime = DateTime.ParseExact(info.Name, "yyyy-MM-dd HH-mm-ss", null);
        //    build.TaskMessages = _messageService.GetMessages();
        //    build.BuildLog = new Lazy<Stream>(() =>
        //    {
        //        return File.Open(Path.Combine(info.FullName, "buildlog.txt"), FileMode.Open);
        //    });
                
        //    return build;
        //}

        public ProjectBuild GetMostRecentBuild(string projectName)
        {
            return _dataProvider().ProjectBuildService.GetMostRecentBuild(projectName);
        }

        public IEnumerable<ProjectBuild> GetBuilds(string projectName)
        {
            return _dataProvider().ProjectBuildService.GetBuilds(projectName);
        }

        class ProjectStatus
        {
            public DateTime LastUpdated { get; set; }
            public Status Status { get; set; }
            public string ProjectName { get; set; }
            
            
        }

        public ProjectInfo GetProjectInfo(string projectName)
        {
            return _dataProvider().ProjectInfoService.GetProjectInfo(projectName);
        }

        ConcurrentDictionary<string, RunningTaskInfo> _runningTasks = new ConcurrentDictionary<string, RunningTaskInfo>();

        class RunningTaskInfo
        {
            public CancellationTokenSource TokenSource { get; set; }
            public Task Task { get; set; }
        }

        public bool RunProject(string projectName)
        {
            var project = GetProject(projectName);
            if (project == null)
                return false;

            var tokenSource = new CancellationTokenSource();
            var task = new Task(() => {

                ProjectHelper.Run(project);

            }, tokenSource.Token);

            var info = new RunningTaskInfo
            {
                TokenSource = tokenSource,
                Task = task
            };

            if (_runningTasks.ContainsKey(projectName))
                return false;

            if (_runningTasks.TryAdd(projectName, info) == false)
            {
                return RunProject(projectName);
            }

            task.Start();

            return true;
            
        }

        public bool CancelProject(string projectName)
        {
            RunningTaskInfo info;
            if (_runningTasks.TryRemove(projectName, out info))
            {
                info.TokenSource.Cancel();
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            _factory.Dispose();
        }
    }
}
