using KissCI.Helpers;
using KissCI.Internal;
using KissCI.Internal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI
{
    public class BuildTaskStart { }
    public class BuildTaskEnd { }

    public class TaskContext
    {
        public TaskContext(
            IProjectService projectService, 
            ProjectInfo info, 
            ProjectBuild build,
            ILogger logger, 
            int taskCount)
        {
            if (projectService == null)
                throw new NullReferenceException("project service was null");
            if (info == null)
                throw new NullReferenceException("project info was null");
            if (build == null)
                throw new NullReferenceException("build was null");
            if (logger == null)
                throw new NullReferenceException("Logger was null");

            _logger = logger;
            _taskCount = taskCount;
            _projectService = projectService;
            _info = info;
            _build = build;
        }

        readonly ILogger _logger;
        readonly int _taskCount;
        readonly ProjectInfo _info;
        readonly ProjectBuild _build;
        readonly IProjectService _projectService;

        public ILogger Logger { get { return _logger; } }
        public string ProjectName { get { return _info.ProjectName; } }
        public int TaskCount { get { return _taskCount; } }

        internal void LogMessage(string format, params object[] parameters)
        {
            Log(format, parameters);
            using (var ctx = _projectService.OpenContext())
            {
                ctx.TaskMessageService.WriteMessage(new Internal.Domain.TaskMessage
                {
                    ProjectInfoId = _info.Id,
                    ProjectBuildId = _build.Id,
                    Time = TimeHelper.Now,
                    Message = string.Format(format, parameters),
                });
                ctx.Commit();
            }
        }
        
        public void Log(string format, params object[] parameters)
        {
            _logger.Log(format, parameters);
        }

        IList<Action> _cleanupActions = new List<Action>();

        public void RegisterCleanup(Action act)
        {
            _cleanupActions.Add(act);
        }

        internal void Cleanup()
        {
            var exceptions = new List<Exception>();
            foreach (var act in _cleanupActions)
            {
                try
                {
                    act();
                }
                catch (Exception ex)
                {
                    Log("Cleanup failed on with an exception of: {0}", ex);
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }

    public class BuildTask<TArg, TResult>
    {
        public BuildTask(int count, string taskName, Func<TaskContext, TArg, TResult> binder)
        {
            _binder = binder;
            _count = count;
            _taskName = taskName;
        }

        int _count;
        string _taskName;

        public int Count { get { return _count; } }
        public string Name { get { return _taskName; } }

        Func<TaskContext, TArg, TResult> _binder;
        public Func<TaskContext, TArg, TResult> Binder { get { return _binder; } }
    }
}
