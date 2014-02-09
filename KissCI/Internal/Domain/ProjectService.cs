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
        IList<KissProject> GetProjects();
        IEnumerable<ProjectView> GetProjectViews();
        IEnumerable<ProjectBuild> GetBuilds(string projectName);
        ProjectBuild GetMostRecentBuild(string projectName);
        ProjectInfo GetProjectInfo(string projectName);
        bool RunProject(string projectName, string command);
        bool CancelProject(string projectName, string command);
        IDataContext OpenContext();
        void RegisterProject(KissProject kissProject);
        void StopProject(string projectName);
        void StartProject(string projectName);
    }

    public class MainProjectService : ProjectService
    {
        public MainProjectService(string root) : base(root, 
            new MainProjectFactory(Path.Combine(root, "Projects")), 
            () => new NHibernate.NHibernateDataContext(root))
        {
            
        }
    }

    public class ProjectService : IProjectService
    {
        public ProjectService(string directoryRoot, IProjectFactory factory, Func<IDataContext> dataProvider)
        {
            _directoryRoot = directoryRoot;
            _projectFolder = Path.Combine(directoryRoot, "Projects");
            EnsureFolders();
            _factory = factory;
            _dataProvider = dataProvider;
            EnsureProjectInfos();
        }

        readonly string _projectFolder;

        void EnsureFolders()
        {
            var folders = new string[]{
                _directoryRoot,
                _projectFolder
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

        void StopTriggers()
        {
            var projects = this.GetProjects();

            var allTriggers = projects.NotNull().SelectMany(p => p.Commands.NotNull().SelectMany(c => c.Triggers));
            foreach(var trigger in allTriggers)
                trigger.Stop();
        }

        readonly string _directoryRoot;
        readonly IProjectFactory _factory;
        readonly Func<IDataContext> _dataProvider;

        readonly IList<KissProject> _registeredProjects = new List<KissProject>();

        void UpdateInfo(IDataContext ctx, KissProject kissProject, IList<ProjectInfo> infos, bool isRegistration = false)
        {
            if (infos.Any(i => i.ProjectName == kissProject.Name) == false)
            {
                var info = new ProjectInfo
                {
                    ProjectName = kissProject.Name,
                    Category = kissProject.Category,
                    Activity = Activity.Sleeping,
                    Status = Status.Running
                };

                ctx.ProjectInfoService.Save(info);

                if(isRegistration)
                    _registeredProjects.Add(kissProject);
            }
            else
            {
                //we update the category just in case it's been modified
                var info = infos.FirstOrDefault(i => i.ProjectName == kissProject.Name);
                if (info != null)
                {
                    info.Category = kissProject.Category;
                    ctx.ProjectInfoService.Save(info);
                }
            }
        }

        public void RegisterProject(KissProject kissProject)
        {
            using (var ctx = _dataProvider())
            {
                var infos = ctx.ProjectInfoService.GetProjectInfos().ToList();
                UpdateInfo(ctx, kissProject, infos, true);

                ctx.Commit();
            }
        }

        public IDataContext OpenContext() { return _dataProvider(); }

        public IList<KissProject> GetProjects()
        {
            return _factory.FetchProjects().Concat(_registeredProjects).ToList();
        }

        public IEnumerable<ProjectView> GetProjectViews()
        {

            //nhibernate linq sucks so we are really inneficient here with n + 2 queries
            using(var provider = this.OpenContext()){
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

                    var project = GetProject(i.ProjectName);

                    DateTime? nextBuildTime = project == null ? null : project.Commands.SelectMany(c=>c.Triggers).OrderBy(t => t.NextBuild).Select(t => t.NextBuild).FirstOrDefault();

                    var view = new ProjectView
                    {
                        Info = i,
                        LastBuild = lastBuild,
                        LastMessage = lastMessage,
                        NextBuildTime = nextBuildTime
                    };
                    views.Add(view);
                }

                var result = views.Distinct().ToList();

                return result.Where(v=> GetProjects().Any(p=>p.Name == v.Info.ProjectName));
            }
        }

        public void EnsureDirectory(string dir)
        {
            if (Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);
        }

        KissProject GetProject(string projectName)
        {
            return GetProjects().FirstOrDefault(p => p.Name == projectName);
        }

        public IEnumerable<string> GetCategories()
        {
            return GetProjects().Select(p => p.Category).Distinct();
        }

        public ProjectBuild GetMostRecentBuild(string projectName)
        {
            using (var provider = this.OpenContext())
            {
                return provider.ProjectBuildService.GetMostRecentBuild(projectName);
            }
            
        }

        public IEnumerable<ProjectBuild> GetBuilds(string projectName)
        {
            using (var provider = this.OpenContext())
            {
                return provider.ProjectBuildService.GetBuildsForProject(projectName).ToList();
            }
        }

        public ProjectInfo GetProjectInfo(string projectName)
        {
            using (var provider = this.OpenContext())
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

        public bool RunProject(string projectName, string command)
        {
            var project = GetProject(projectName);
            if (project == null)
                return false;

            var kissCommand = project.Commands.FirstOrDefault(c=> c.Name == command);
            if(kissCommand == null)
                return false;

            var tokenSource = new CancellationTokenSource();
            var task = new Task(() => {

                try
                {
                    ProjectHelper.Run(project, kissCommand, this, tokenSource.Token);
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
                return RunProject(projectName, command);
            }

            task.Start();

            return true;
            
        }

        public bool CancelProject(string projectName, string command)
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

                foreach (var trigger in project.Commands.SelectMany(c=>c.Triggers))
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

                foreach (var command in project.Commands)
                    foreach(var trigger in command.Triggers)
                        trigger.Start(() => RunProject(project.Name, command.Name));

                ctx.Commit();
            }
        }

        public void Dispose()
        {
            StopTriggers();
        }
    }
}
