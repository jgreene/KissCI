using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI.Helpers
{
    public static class TaskHelper
    {
        public static BuildTask<TArg, TNewResult> AddTask<TArg, TResult, TNewResult>(this BuildTask<TArg, TResult> t, string taskName, Func<TaskContext, TResult, TNewResult> bind)
        {
            var taskNum = t.Count + 1;
            return new BuildTask<TArg, TNewResult>(taskNum, taskName, (ctx, arg) =>
            {
                var res = t.Binder(ctx, arg);

                ctx.ThrowIfCancellationRequested();

                ctx.LogMessage("Beginning task #{0} of {1} : {2}", taskNum, ctx.TaskCount, taskName);

                return bind(ctx, res);
            });
        }

        public static BuildTask<TArg, TNewResult> AddStep<TArg, TResult, TNewResult>(this BuildTask<TArg, TResult> t, Func<TaskContext, TResult, TNewResult> bind)
        {
            return new BuildTask<TArg, TNewResult>(t.Count, t.Name, (ctx, arg) =>
            {
                var res = t.Binder(ctx, arg);

                ctx.ThrowIfCancellationRequested();

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
                ctx.LogMessage("Ending tasks for {0} successfully", ctx.ProjectName);
                return new BuildTaskEnd();
            });
        }

        public static BuildTask<BuildTaskStart, BuildTaskEnd> Create<TResult>(Func<BuildTask<BuildTaskStart, BuildTaskStart>, BuildTask<BuildTaskStart, TResult>> act)
        {
            return act(Start()).Finalize();
        }

        //public static BuildTask<TArg1, TResult2> Combine<TArg1, TResult1, TArg2, TResult2>(BuildTask<TArg1, TResult1> t1, BuildTask<TArg2, TResult2> t2)
        //{
        //    return t1.AddTask(t2.Name, t2.Binder);
        //}
    }
}
