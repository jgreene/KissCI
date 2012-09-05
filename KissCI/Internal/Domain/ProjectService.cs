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
        IList<Project> GetProjects();
        IEnumerable<ProjectView> GetProjectViews();
        IEnumerable<ProjectBuild> GetBuilds(string projectName);
        ProjectBuild GetMostRecentBuild(string projectName);
        ProjectInfo GetProjectInfo(string projectName);
        bool RunProject(string projectName);
        bool CancelProject(string projectName);
        string BuildLogsDirectory { get; }
        IDataContext OpenContext();
        void RegisterProject(Project project);
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

            EnsureProjectInfos();
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

        void EnsureProjectInfos()
        {
            var projects = GetProjects();

            using (var ctx = _dataProvider())
            {
                var infos = ctx.ProjectInfoService.GetProjectInfos().ToList();
                foreach (var proj in projects)
                {
                    if (infos.Any(i => i.ProjectName == proj.Name) == false)
                    {
                        var info = new ProjectInfo
                        {
                            ProjectName = proj.Name,
                            Activity = Activity.Sleeping,
                            Status = Status.Running
                        };

                        ctx.ProjectInfoService.Save(info);
                    }
                }

                ctx.Commit();
            }
        }

        string _directoryRoot;
        IProjectFactory _factory;
        Func<IDataContext> _dataProvider;

        IList<Project> _registeredProjects = new List<Project>();

        public void RegisterProject(Project project)
        {
            using (var ctx = _dataProvider())
            {
                var infos = ctx.ProjectInfoService.GetProjectInfos().ToList();
                if (infos.Any(i => i.ProjectName == project.Name) == false)
                {
                    var info = new ProjectInfo
                    {
                        ProjectName = project.Name,
                        Activity = Activity.Sleeping,
                        Status = Status.Running
                    };

                    ctx.ProjectInfoService.Save(info);
                    _registeredProjects.Add(project);
                }

                ctx.Commit();
            }
        }

        public string BuildLogsDirectory { get { return _logFolder; } }

        public IDataContext OpenContext() { return _dataProvider(); }

        public IList<Project> GetProjects()
        {
            return _factory.FetchProjects().Concat(_registeredProjects).ToList();
        }

        public IEnumerable<ProjectView> GetProjectViews()
        {
            using(var provider = _dataProvider()){
                var builds = provider.ProjectBuildService.GetBuilds().OrderByDescending(b=>b.BuildTime);
                var messages = provider.TaskMessageService.GetMessages().OrderByDescending(m => m.Time);

                var views = from i in provider.ProjectInfoService.GetProjectInfos()
                           select new ProjectView
                           {
                               Info = i,
                               LastBuild = builds.FirstOrDefault(b => b.ProjectInfoId == i.Id),
                               LastMessage = messages.FirstOrDefault(m => m.ProjectInfoId == i.Id)
                           };

                var result = views.ToList();

                return result.Where(v=> GetProjects().Any(p=>p.Name == v.Info.ProjectName));
            }
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

        public ProjectBuild GetMostRecentBuild(string projectName)
        {
            using (var provider = _dataProvider())
            {
                return provider.ProjectBuildService.GetMostRecentBuild(projectName);
            }
            
        }

        public IEnumerable<ProjectBuild> GetBuilds(string projectName)
        {
            using (var provider = _dataProvider())
            {
                return provider.ProjectBuildService.GetBuildsForProject(projectName).ToList();
            }
        }

        public ProjectInfo GetProjectInfo(string projectName)
        {
            using (var provider = _dataProvider())
            {
                return provider.ProjectInfoService.GetProjectInfo(projectName);
            }
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

                ProjectHelper.Run(project, this);

                RunningTaskInfo tempInfo;
                _runningTasks.TryRemove(projectName, out tempInfo);

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
