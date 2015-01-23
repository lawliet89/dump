using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dump.Tasks
{
    public static class TaskExtensions
    {
        /// <summary>Transfers the result of a Task to the TaskCompletionSource.</summary>
        /// <typeparam name="TResult">Specifies the type of the result.</typeparam>
        /// <param name="resultSetter">The TaskCompletionSource.</param>
        /// <param name="task">The task whose completion results should be transferred.</param>
        /// https://github.com/openstacknetsdk/openstack.net/blob/master/src/corelib/Core/ParallelExtensionsExtras/TaskCompletionSourceExtensions.cs
        public static void SetFromTask<TResult>(this TaskCompletionSource<TResult> resultSetter, Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion: resultSetter.SetResult(task is Task<TResult> ? ((Task<TResult>)task).Result : default(TResult)); break;
                case TaskStatus.Faulted: resultSetter.SetException(task.Exception.InnerExceptions); break;
                case TaskStatus.Canceled: resultSetter.SetCanceled(); break;
                default: throw new InvalidOperationException("The task was not completed.");
            }
        }

        /// <summary>Transfers the result of a Task to the TaskCompletionSource.</summary>
        /// <typeparam name="TResult">Specifies the type of the result.</typeparam>
        /// <param name="resultSetter">The TaskCompletionSource.</param>
        /// <param name="task">The task whose completion results should be transferred.</param>
        public static void SetFromTask<TResult>(this TaskCompletionSource<TResult> resultSetter, Task<TResult> task)
        {
            SetFromTask(resultSetter, (Task)task);
        }

        public static Task<TResult> Then<TResult, TSource>(this Task<TSource> task,
            Func<Task<TSource>, TResult> continuationFunction)
        {
            TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>();

            // If task was successful
            task.ContinueWith(continuationFunction, TaskContinuationOptions.OnlyOnRanToCompletion)
                .ContinueWith(t =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        completionSource.SetFromTask(t);
                    }
                });

            // If task was unsuccessful
            task.ContinueWith(t => completionSource.SetFromTask(t), TaskContinuationOptions.NotOnRanToCompletion);

            return completionSource.Task;
        }

        public static Task Then<TSource>(this Task<TSource> task,
            Action<Task<TSource>> continuationFunction)
        {
            return task.Then((t) =>
            {
                continuationFunction(t);
                return new VoidResult();
            });
        }

        public static Task StartSTATask(Action action)
        {
            return StartSTATask(() =>
            {
                action();
                return new VoidResult();
            });
        }

        public static Task<T> StartSTATask<T>(Func<T> function)
        {
            var completionSource = new TaskCompletionSource<T>();
            var thread = new Thread(() =>
            {
                try
                {
                    completionSource.SetResult(function());
                }
                catch (Exception e)
                {
                    completionSource.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return completionSource.Task;
        }
    }

    public sealed class VoidResult { }

}
