using Common.Logging;
using KissCI.Helpers;
using KissCI.Internal;
using KissCI.Internal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KissCI
{
    public class KissCITaskStart { }
    public class KissCITaskEnd { }

    public class TaskContext
    {
        public TaskContext(
            IProjectService projectService, 
            ProjectInfo info, 
            ProjectBuild build,
            ILog logger,
            int taskCount,
            CancellationToken? token)
        {
            if (projectService == null)
                throw new NullReferenceException("project service was null");
            if (info == null)
                throw new NullReferenceException("project info was null");
            if (build == null)
                throw new NullReferenceException("build was null");

            _taskCount = taskCount;
            _projectService = projectService;
            _info = info;
            _build = build;
            _logger = logger;
            _token = token;
        }

        readonly int _taskCount;
        readonly ProjectInfo _info;
        readonly ProjectBuild _build;
        readonly ILog _logger;
        readonly IProjectService _projectService;
        readonly CancellationToken? _token;

        public string ProjectName { get { return _info.ProjectName; } }
        public int TaskCount { get { return _taskCount; } }

        public ILog Logger { get { return _logger; } }

        internal void LogMessage(string format, params object[] parameters)
        {
            var message = string.Format(format, parameters);
            using (var ctx = _projectService.OpenContext())
            {
                ctx.TaskMessageService.WriteMessage(new Internal.Domain.TaskMessage
                {
                    ProjectInfoId = _info.Id,
                    ProjectBuildId = _build.Id,
                    Time = TimeHelper.Now,
                    Message = message,
                    Type = MessageType.TaskMessage,
                    LogType = LogType.Info
                });
                ctx.Commit();
            }

            Console.WriteLine(message);
        }
        
        public void Log(string format, params object[] parameters)
        {         
            _logger.InfoFormat(format, parameters);
        }

        readonly IList<Action> _cleanupActions = new List<Action>();

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
                    _logger.ErrorFormat("Cleanup failed on with an exception of: {0}", ex);
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        public void ThrowIfCancellationRequested()
        {
            if(_token.HasValue)
                _token.Value.ThrowIfCancellationRequested();
        }
    }

    public class KissTask<TArg, TResult>
    {
        public KissTask(int count, string taskName, Func<TaskContext, TArg, TResult> binder)
        {
            _binder = binder;
            _count = count;
            _taskName = taskName;
        }

        readonly int _count;
        readonly string _taskName;

        public int Count { get { return _count; } }
        public string Name { get { return _taskName; } }

        readonly Func<TaskContext, TArg, TResult> _binder;
        public Func<TaskContext, TArg, TResult> Binder { get { return _binder; } }
    }
}
