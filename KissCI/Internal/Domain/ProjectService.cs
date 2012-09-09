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
using KissCI.Internal.Helpers;

namespace KissCI.Internal.Domain
{
    public interface IProjectService : IDisposable
    {
        IEnumerable<string> GetCategories();
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
        void StopProject(string projectName);
        void StartProject(string projectName);
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

                foreach (var project in projects)
                    UpdateInfo(ctx, project, infos);

                ctx.Commit();
            }
        }

        void StartTriggers()
        {
            var projects = this.GetProjects();

            foreach (var project in projects)
            {
                foreach (var trigger in project.Triggers)
                {
                    trigger.Start(() => this.RunProject(project.Name));
                }
            }
        }

        void StopTriggers()
        {
            var projects = this.GetProjects();

            foreach (var project in projects)
                foreach (var trigger in project.Triggers)
                    trigger.Stop();
        }

        string _directoryRoot;
        IProjectFactory _factory;
        Func<IDataContext> _dataProvider;

        IList<Project> _registeredProjects = new List<Project>();

        void UpdateInfo(IDataContext ctx, Project project, IList<ProjectInfo> infos, bool isRegistration = false)
        {
            if (infos.Any(i => i.ProjectName == project.Name) == false)
            {
                var info = new ProjectInfo
                {
                    ProjectName = project.Name,
                    Category = project.Category,
                    Activity = Activity.Sleeping,
                    Status = Status.Running
                };

                ctx.ProjectInfoService.Save(info);

                if(isRegistration)
                    _registeredProjects.Add(project);
            }
            else
            {
                //we update the category just in case it's been modified
                var info = infos.FirstOrDefault(i => i.ProjectName == project.Name);
                if (info != null)
                {
                    info.Category = project.Category;
                    ctx.ProjectInfoService.Save(info);
                }
            }
        }

        public void RegisterProject(Project project)
        {
            using (var ctx = _dataProvider())
            {
                var infos = ctx.ProjectInfoService.GetProjectInfos().ToList();
                UpdateInfo(ctx, project, infos, true);

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

            //nhibernate linq sucks so we are really inneficient here with n + 2 queries
            using(var provider = _dataProvider()){
                var builds = provider.ProjectBuildService.GetBuilds().OrderByDescending(b => b.BuildTime).ThenByDescending(m => m.Id);
                var messages = provider.TaskMessageService.GetMessages().OrderByDescending(m => m.Time).ThenByDescending(m => m.Id);
                var infos = provider.ProjectInfoService.GetProjectInfos().ToList();

                var views = new List<ProjectView>();

                foreach (var i in infos)
                {
                    var lastBuild = builds.FirstOrDefault(f=>f.ProjectInfoId == i.Id);
                    var lastMessage = lastBuild == null ? null : messages.FirstOrDefault(m =>
                            lastBuild != null
                            && m.ProjectInfoId == i.Id
                            && m.ProjectBuildId == lastBuild.Id
                            );

                    var view = new ProjectView
                    {
                        Info = i,
                        LastBuild = lastBuild,
                        LastMessage = lastMessage,
                        NextBuildTime = GetProject(i.ProjectName).Triggers.OrderBy(t=>t.NextBuild).Select(t=>t.NextBuild).FirstOrDefault()
                    };
                    views.Add(view);
                }

                var result = views.Distinct().ToList();

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

        public IEnumerable<string> GetCategories()
        {
            return GetProjects().Select(p => p.Category).Distinct();
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

                try
                {
                    ProjectHelper.Run(project, this);
                }
                finally
                {
                    RunningTaskInfo tempInfo;
                    _runningTasks.TryRemove(projectName, out tempInfo);
                }

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
            using (var ctx = _dataProvider())
            {
                var projectInfo = ctx.ProjectInfoService.GetProjectInfo(projectName);
                if (projectInfo.Activity != Activity.Building && projectInfo.Activity != Activity.CleaningUp)
                    return false;

                RunningTaskInfo taskInfo;
                if (_runningTasks.TryRemove(projectName, out taskInfo))
                {
                    taskInfo.TokenSource.Cancel();

                    projectInfo.Activity = Activity.Sleeping;
                    ctx.ProjectInfoService.Save(projectInfo);

                    var build = ctx.ProjectBuildService.GetMostRecentBuild(projectName);
                    build.BuildResult = BuildResult.Cancelled;

                    ctx.TaskMessageService.WriteMessage(new TaskMessage
                    {
                        ProjectInfoId = projectInfo.Id,
                        ProjectBuildId = build.Id,
                        Time = TimeHelper.Now,
                        Message = string.Format("Canceled build for project: {0}", projectInfo.ProjectName)
                    });
                    
                    ctx.Commit();
                    return true;
                }
            }
            

            return false;
        }

        public void StopProject(string projectName)
        {
            var project = GetProject(projectName);
            using (var ctx = this.OpenContext())
            {
                var info = ctx.ProjectInfoService.GetProjectInfo(projectName);
                info.Activity = Activity.Sleeping;
                info.Status = Status.Stopped;

                ctx.ProjectInfoService.Save(info);

                foreach (var trigger in project.Triggers)
                    trigger.Stop();

                ctx.Commit();
            }
        }

        public void StartProject(string projectName)
        {
            var project = GetProject(projectName);

            using (var ctx = this.OpenContext())
            {
                var info = ctx.ProjectInfoService.GetProjectInfo(projectName);
                info.Activity = Activity.Sleeping;
                info.Status = Status.Running;

                foreach (var trigger in project.Triggers)
                    trigger.Start(() => RunProject(project.Name));

                ctx.Commit();
            }
        }

        public void Dispose()
        {
            StopTriggers();
            _factory.Dispose();
        }
    }
}
