using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI.Helpers
{
    public static class TaskHelper
    {
        public static KissTask<TArg, TNewResult> AddTask<TArg, TResult, TNewResult>(this KissTask<TArg, TResult> t, string taskName, Func<TaskContext, TResult, TNewResult> bind)
        {
            var taskNum = t.Count + 1;
            return new KissTask<TArg, TNewResult>(taskNum, taskName, (ctx, arg) =>
            {
                var res = t.Binder(ctx, arg);

                ctx.ThrowIfCancellationRequested();

                ctx.LogMessage("Beginning task #{0} of {1} : {2}", taskNum, ctx.TaskCount, taskName);

                return bind(ctx, res);
            });
        }

        public static KissTask<TArg, TNewResult> AddStep<TArg, TResult, TNewResult>(this KissTask<TArg, TResult> t, Func<TaskContext, TResult, TNewResult> bind)
        {
            return new KissTask<TArg, TNewResult>(t.Count, t.Name, (ctx, arg) =>
            {
                var res = t.Binder(ctx, arg);

                ctx.ThrowIfCancellationRequested();

                return bind(ctx, res);
            });
        }

        public static KissTask<KissCITaskStart, KissCITaskStart> Start()
        {
            return new KissTask<KissCITaskStart, KissCITaskStart>(0, "Task Start", (ctx, arg) =>
            {
                return new KissCITaskStart();
            });
        }

        public static KissTask<KissCITaskStart, KissCITaskEnd> Finalize<TResult>(this KissTask<KissCITaskStart, TResult> t)
        {
            return t.AddStep((ctx, arg) =>
            {
                ctx.LogMessage("Ending tasks for {0} successfully", ctx.ProjectName);
                return new KissCITaskEnd();
            });
        }

        public static KissTask<KissCITaskStart, KissCITaskEnd> Create<TResult>(Func<KissTask<KissCITaskStart, KissCITaskStart>, KissTask<KissCITaskStart, TResult>> act)
        {
            return act(Start()).Finalize();
        }

        //public static KissTask<TArg1, TResult2> Combine<TArg1, TResult1, TArg2, TResult2>(KissTask<TArg1, TResult1> t1, KissTask<TArg2, TResult2> t2)
        //{
        //    return t1.AddTask(t2.Name, t2.Binder);
        //}
    }
}
