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

        public void LogMessage(string format, params object[] parameters)
        {
            using (var ctx = _projectService.OpenContext())
            {
                ctx.TaskMessageService.WriteMessage(new Internal.Domain.TaskMessage
                {
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
            foreach(var act in _cleanupActions)
                act();
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

    public static class TaskHelper
    {
        public static BuildTask<TArg, TNewResult> AddTask<TArg, TResult, TNewResult>(this BuildTask<TArg, TResult> t, string taskName, Func<TaskContext, TResult, TNewResult> bind)
        {
            var taskNum = t.Count + 1;
            return new BuildTask<TArg, TNewResult>(taskNum, taskName, (ctx, arg) =>
            {
                var res = t.Binder(ctx, arg);

                ctx.Log("Beginning task #{0} of {1} : {2}", taskNum, ctx.TaskCount, taskName);

                return bind(ctx, res);
            });
        }

        public static BuildTask<TArg, TNewResult> AddStep<TArg, TResult, TNewResult>(this BuildTask<TArg, TResult> t, Func<TaskContext, TResult, TNewResult> bind)
        {
            return new BuildTask<TArg, TNewResult>(t.Count, t.Name, (ctx, arg) =>
            {
                var res = t.Binder(ctx, arg);

                return bind(ctx, res);
            });
        }

        public static BuildTask<BuildTaskStart, BuildTaskStart> Start()
        {
            return new BuildTask<BuildTaskStart, BuildTaskStart>(0, "Task Start", (ctx, arg) =>
            {
                return new BuildTaskStart();
            });
        }

        public static BuildTask<BuildTaskStart, BuildTaskEnd> Finalize<TResult>(this BuildTask<BuildTaskStart, TResult> t)
        {
            return t.AddStep((ctx, arg) =>
            {
                ctx.Log("Ending tasks for {0} successfully", ctx.ProjectName);
                return new BuildTaskEnd();
            });
        }
    }
}
