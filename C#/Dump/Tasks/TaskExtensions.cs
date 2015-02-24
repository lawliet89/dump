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
        /// Copyright (c) Microsoft Corporation.  All rights reserved.
        public static void SetFromTask<TResult>(this TaskCompletionSource<TResult> resultSetter, Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    var taskResult = task as Task<TResult>;
                    resultSetter.SetResult(taskResult != null ? taskResult.Result : default(TResult));
                    break;
                case TaskStatus.Faulted:
                    resultSetter.SetException(task.Exception.InnerExceptions);
                    break;
                case TaskStatus.Canceled:
                    resultSetter.SetCanceled();
                    break;
                default:
                    throw new InvalidOperationException("The task was not completed.");
            }
        }

        /// <summary>Transfers the result of a Task to the TaskCompletionSource.</summary>
        /// <typeparam name="TResult">Specifies the type of the result.</typeparam>
        /// <param name="resultSetter">The TaskCompletionSource.</param>
        /// <param name="task">The task whose completion results should be transferred.</param>
        /// Copyright (c) Microsoft Corporation.  All rights reserved.
        public static void SetFromTask<TResult>(this TaskCompletionSource<TResult> resultSetter, Task<TResult> task)
        {
            SetFromTask(resultSetter, (Task) task);
        }

        /// <summary>
        ///     Schedule another task to continue after the initial task is successful.
        ///     If the initial task did not succeed, we will not execute the continuation and will simply
        ///     pass on the exception
        ///     Credits to OpenStack:
        ///     https://github.com/openstacknetsdk/openstack.net/blob/master/src/corelib/Core/CoreTaskExtensions.cs
        /// </summary>
        /// <typeparam name="TResult">Type of result returned by the continuation task</typeparam>
        /// <typeparam name="TSource">Type of result returned by the initial task</typeparam>
        /// <param name="task">Initial task</param>
        /// <param name="continuationFunction">Delegate to run in the continuation task</param>
        /// <returns>
        ///     A task that represents the continuation task if the preceding task succeeded.
        ///     Otherwise, returns a task that contains the exception from the preceding task.
        /// </returns>
        public static Task<TResult> Then<TResult, TSource>(this Task<TSource> task,
            Func<Task<TSource>, TResult> continuationFunction)
        {
            var completionSource = new TaskCompletionSource<TResult>();

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

        /// <summary>
        ///     Schedule another task to continue after the initial task is successful.
        ///     If the initial task did not succeed, we will not execute the continuation and will simply
        ///     pass on the exception
        ///     Credits to OpenStack:
        ///     https://github.com/openstacknetsdk/openstack.net/blob/master/src/corelib/Core/CoreTaskExtensions.cs
        /// </summary>
        /// <typeparam name="TSource">Type of result returned by the initial task</typeparam>
        /// <param name="task">Initial task</param>
        /// <param name="continuationFunction">Delegate to run in the continuation task</param>
        /// <returns>
        ///     A task that represents the continuation task if the preceding task succeeded.
        ///     Otherwise, returns a task that contains the exception from the preceding task.
        /// </returns>
        public static Task Then<TSource>(this Task<TSource> task,
            Action<Task<TSource>> continuationFunction)
        {
            return task.Then(t =>
            {
                continuationFunction(t);
                return Nothing.AtAll;
            });
        }

        /// <summary>
        ///     Start an action in a STA thread
        /// </summary>
        /// <param name="action">Delegate to run in the STA Thread</param>
        /// <returns>A task representing the action to run</returns>
        public static Task StartSTATask(Action action)
        {
            return StartSTATask(() =>
            {
                action();
                return Nothing.AtAll;
            });
        }

        /// <summary>
        ///     Start a function to run in a STA thread
        /// </summary>
        /// <typeparam name="T">Type of result returned by the function</typeparam>
        /// <param name="function">Delegate function to run in the STA thread</param>
        /// <returns>A task representing the function to be run in the STA thread</returns>
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

    /// <summary>
    ///     Unit class. Alternatively, you can use System.Reactive.Unit
    /// </summary>
    public sealed class Nothing
    {
        private static readonly Nothing atAll = new Nothing();

        private Nothing()
        {
        }

        public static Nothing AtAll
        {
            get { return atAll; }
        }
    }
}