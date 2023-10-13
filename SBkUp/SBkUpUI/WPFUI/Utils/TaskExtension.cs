using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBkUpUI.WPFUI.Utils
{
    public struct VoidTaskResult
    {
    };

    public static class TaskFactoryExtension
    {
        public static Task<T> CompleteTask<T>(this TaskFactory factory, T result)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);
            return tcs.Task;
        }

        public static Task CompleteTask(this TaskFactory factory)
        {
            return CompleteTask(null, default(VoidTaskResult));
        }

        public static Task<T> CancelledTask<T>(this TaskFactory<T> factory)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        public static Task CancelledTask(this TaskFactory factory)
        {
            return CancelledTask(null);
        }

        public static Task<T> FaultedTask<T>(this TaskFactory<T> factory, Exception ex)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetException(ex);
            return tcs.Task;
        }

        public static Task FaultedTask(this TaskFactory factory, Exception ex)
        {
            return FaultedTask(null, ex);
        }

        public static Task StartNewTCS(this TaskFactory factory, Action<TaskCompletionSource<VoidTaskResult>> action)
        {
            var tcs = new TaskCompletionSource<VoidTaskResult>();
            tcs.Execute(() => action(tcs));
            return tcs.Task;
        }

        public static Task<T> StartNewTCS<T>(this TaskFactory<T> factory, Action<TaskCompletionSource<T>> action)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.Execute(() => action(tcs));
            return tcs.Task;
        }

        public static void ContinueWhenAnyTCSTask<T, R>(this TaskFactory factory, Task<T>[] tasks, TaskCompletionSource<R> tcs, Action<Task<T>> action)
        {
            factory.ContinueWhenAny(tasks, task => tcs.Execute(() => action(task)), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public static void ContinueWhenAllTCSTask(this TaskFactory factory, Task[] tasks, TaskCompletionSource<VoidTaskResult> tcs, Action<Task[]> action)
        {
            factory.ContinueWhenAll(tasks, task => tcs.Execute(() => action(task)), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public static void ContinueWhenAllTCSTask<T>(this TaskFactory factory, Task<T>[] tasks, TaskCompletionSource<VoidTaskResult> tcs, Action<Task<T>[]> action)
        {
            factory.ContinueWhenAll(tasks, task => tcs.Execute(() => action(task)), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public static void ContinueWhenAllTCSTask<R>(this TaskFactory factory, Task[] tasks, TaskCompletionSource<R> tcs, Action<Task[]> action)
        {
            factory.ContinueWhenAll(tasks, task => tcs.Execute(() => action(task)), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
    }

    public static class TaskExtension
    {
        public static void GetResult(this Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.Canceled:
                    throw new TaskCanceledException(task);
                case TaskStatus.Faulted:
                    throw task.Exception;
            }
        }

        public static Task IgnoreExceptions(this Task task)
        {
            task.ContinueWith(task1 =>
            {
                var ex = task1.Exception;
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return task;
        }

        public static Task<T> IgnoreExceptions<T>(this Task<T> task)
        {
            task.ContinueWith(task1 =>
            {
                var ex = task1.Exception;
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return task;
        }

        public static void ContinueWithTCSTask<T>(this Task task, TaskCompletionSource<T> tcs, Action<Task> action)
        {
            task.ContinueWith(task1 => tcs.Execute(() => action(task1)), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public static void ContinueWithTCSTask<T, R>(this Task<R> task, TaskCompletionSource<T> tcs, Action<Task<R>> action)
        {
            task.ContinueWith(task1 => tcs.Execute(() => action(task1)), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public static void ContinueWithTCSTaskInContext<T>(this Task task, TaskCompletionSource<T> tcs, Action<Task> action)
        {
            task.ContinueWith(task1 => tcs.Execute(() => action(task1)), TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static void ContinueWithTCSTaskInContext<T, R>(this Task<R> task, TaskCompletionSource<T> tcs, Action<Task<R>> action)
        {
            task.ContinueWith(task1 => tcs.Execute(() => action(task1)), TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static void ContinueWithTCS<T>(this Task task, TaskCompletionSource<T> tcs, Action action)
        {
            task.ContinueWith(task1 =>
            {
                if (task1.CheckStatus(tcs))
                {
                    tcs.Execute(action);
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public static void ContinueWithTCS<T, R>(this Task<R> task, TaskCompletionSource<T> tcs, Action<R> action)
        {
            task.ContinueWith(task1 =>
            {
                if (task1.CheckStatus(tcs))
                {
                    tcs.Execute(() => action(task1.Result));
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public static void ContinueWithTCSInContext<T>(this Task task, TaskCompletionSource<T> tcs, Action action)
        {
            task.ContinueWith(task1 =>
            {
                if (task1.CheckStatus(tcs))
                {
                    tcs.Execute(action);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static void ContinueWithTCSInContext<T, R>(this Task<R> task, TaskCompletionSource<T> tcs, Action<R> action)
        {
            task.ContinueWith(task1 =>
            {
                if (task1.CheckStatus(tcs))
                {
                    tcs.Execute(() => action(task1.Result));
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static void CompleteWithTCS(this Task task, TaskCompletionSource<VoidTaskResult> tcs)
        {
            task.ContinueWith(task1 => task1.TransferToTCS(tcs), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public static void CompleteWithTCS<T>(this Task<T> task, TaskCompletionSource<T> tcs)
        {
            task.ContinueWith(task1 => task1.TransferToTCS(tcs), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public static void ContinueWithExceptionHandlerTCS<T>(this Task task, TaskCompletionSource<T> tcs, Action action, Action<TaskCompletionSource<T>, AggregateException> handler)
        {
            task.ContinueWithTCSTask(tcs, task1 =>
            {
                switch (task1.Status)
                {
                    case TaskStatus.RanToCompletion:
                        action();
                        break;
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        handler(tcs, task1.Exception);
                        break;
                    default:
                        throw new InvalidOperationException("Task not completed");
                }
            });
        }

        public static void ContinueWithExceptionHandlerTCS<T>(this Task task, TaskCompletionSource<T> tcs, Action action, Action<TaskCompletionSource<T>, AggregateException, object> handler, object extraExceptionParam)
        {
            task.ContinueWithTCSTask(tcs, task1 =>
            {
                switch (task1.Status)
                {
                    case TaskStatus.RanToCompletion:
                        action();
                        break;
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        handler(tcs, task1.Exception, extraExceptionParam);
                        break;
                    default:
                        throw new InvalidOperationException("Task not completed");
                }
            });
        }

        public static void ContinueWithExceptionHandlerTCS<T, R>(this Task<R> task, TaskCompletionSource<T> tcs, Action<R> action, Action<TaskCompletionSource<T>, AggregateException> handler)
        {
            task.ContinueWithTCSTask(tcs, task1 =>
            {
                switch (task1.Status)
                {
                    case TaskStatus.RanToCompletion:
                        action(task1.Result);
                        break;
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        handler(tcs, task1.Exception);
                        break;
                    default:
                        throw new InvalidOperationException("Task not completed");
                }
            });
        }

        public static void ContinueWithExceptionHandlerTCS<T, R>(this Task<R> task, TaskCompletionSource<T> tcs, Action<R> action, Action<TaskCompletionSource<T>, AggregateException, object> handler, object extraExceptionParam)
        {
            task.ContinueWithTCSTask(tcs, task1 =>
            {
                switch (task1.Status)
                {
                    case TaskStatus.RanToCompletion:
                        action(task1.Result);
                        break;
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        handler(tcs, task1.Exception, extraExceptionParam);
                        break;
                    default:
                        throw new InvalidOperationException("Task not completed");
                }
            });
        }

        public static void CompleteWithExceptionHandlerTCS(this Task task, TaskCompletionSource<VoidTaskResult> tcs, Action<TaskCompletionSource<VoidTaskResult>, AggregateException> handler)
        {
            task.ContinueWithTCSTask(tcs, task1 =>
            {
                if (task1.Status == TaskStatus.Faulted)
                {
                    handler(tcs, task1.Exception);
                }
                else
                {
                    task1.TransferToTCS(tcs);
                }
            });
        }

        public static void CompleteWithExceptionHandlerTCS(this Task task, TaskCompletionSource<VoidTaskResult> tcs, object extraExceptionParam, Action<TaskCompletionSource<VoidTaskResult>, AggregateException, object> handler)
        {
            task.ContinueWithTCSTask(tcs, task1 =>
            {
                if (task1.Status == TaskStatus.Faulted)
                {
                    handler(tcs, task1.Exception, extraExceptionParam);
                }
                else
                {
                    task1.TransferToTCS(tcs);
                }
            });
        }

        public static void CompleteWithExceptionHandlerTCS<T>(this Task<T> task, TaskCompletionSource<T> tcs, Action<TaskCompletionSource<T>, AggregateException> handler)
        {
            task.ContinueWithTCSTask(tcs, task1 =>
            {
                if (task1.Status == TaskStatus.Faulted)
                {
                    handler(tcs, task1.Exception);
                }
                else
                {
                    task1.TransferToTCS(tcs);
                }
            });
        }

        public static void CompleteWithExceptionHandlerTCS<T>(this Task<T> task, TaskCompletionSource<T> tcs, object extraExceptionParam, Action<TaskCompletionSource<T>, AggregateException, object> handler)
        {
            task.ContinueWithTCSTask(tcs, task1 =>
            {
                if (task1.Status == TaskStatus.Faulted)
                {
                    handler(tcs, task1.Exception, extraExceptionParam);
                }
                else
                {
                    task1.TransferToTCS(tcs);
                }
            });
        }

        public static bool TransferToTCS(this Task task, TaskCompletionSource<VoidTaskResult> tcs)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    return tcs.TrySetResult();
                case TaskStatus.Faulted:
                    return tcs.TrySetException(task.Exception.InnerExceptions);
                case TaskStatus.Canceled:
                    return tcs.TrySetCanceled();
                default:
                    throw new InvalidOperationException("Task not completed");
            }
        }

        public static bool TransferToTCS<T>(this Task<T> task, TaskCompletionSource<T> tcs)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    return tcs.TrySetResult(task.Result);
                case TaskStatus.Faulted:
                    return tcs.TrySetException(task.Exception.InnerExceptions);
                case TaskStatus.Canceled:
                    return tcs.TrySetCanceled();
                default:
                    throw new InvalidOperationException("Task not completed");
            }
        }

        private static bool CheckStatus<T>(this Task task, TaskCompletionSource<T> tcs)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    return true;
                case TaskStatus.Faulted:
                    tcs.TrySetException(task.Exception.InnerExceptions);
                    return false;
                case TaskStatus.Canceled:
                    tcs.TrySetCanceled();
                    return false;
                default:
                    throw new InvalidOperationException("Task not completed");
            }
        }

        private static bool FilterMessage(ref NativeMethods.MSG msg)
        {
            int message = msg.message;
            if (message >= NativeMethods.WM_KEYFIRST && message <= NativeMethods.WM_KEYLAST
                || message >= NativeMethods.WM_MOUSEFIRST && message <= NativeMethods.WM_MOUSELAST)
            {
                return false;
            }

            return true;
        }

        public static void WaitWithMsgPump(this Task task)
        {
            if (!task.IsCompleted)
            {
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    using (CancellationTokenSource cts = new CancellationTokenSource())
                    {
                        using (Task evtTask = task.ContinueWith(_ => evt.Set(), cts.Token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current))
                        {
                            IntPtr[] handles = { evt.SafeWaitHandle.DangerousGetHandle() };
                            NativeMethods.MSG msg;
                            do
                            {
                                while (NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, NativeMethods.PM_REMOVE))
                                {
                                    if (msg.message == NativeMethods.WM_QUIT)
                                    {
                                        NativeMethods.PostQuitMessage((int)msg.wParam);
                                        cts.Cancel();
                                        goto EndPump;
                                    }
                                    if (FilterMessage(ref msg))
                                    {
                                        NativeMethods.TranslateMessage(ref msg);
                                        NativeMethods.DispatchMessage(ref msg);
                                    }
                                }
                            }
                            while (NativeMethods.MsgWaitForMultipleObjectsEx(1, handles, -1, NativeMethods.QS_ALLINPUT, 0) != 0);
                        EndPump:
                            evtTask.Wait();
                        }
                    }
                }
            }
            task.Wait();
        }

        public static void WaitWithMsgPump(this IntPtr handle)
        {
            IntPtr[] handles = { handle };
            NativeMethods.MSG msg;
            do
            {
                while (NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, NativeMethods.PM_REMOVE))
                {
                    if (msg.message == NativeMethods.WM_QUIT)
                    {
                        NativeMethods.PostQuitMessage((int)msg.wParam);
                        return;
                    }
                    if (FilterMessage(ref msg))
                    {
                        NativeMethods.TranslateMessage(ref msg);
                        NativeMethods.DispatchMessage(ref msg);
                    }
                }
            }
            while (NativeMethods.MsgWaitForMultipleObjectsEx(1, handles, -1, NativeMethods.QS_ALLINPUT, 0) != 0);
        }
    }

    public static class TaskCompletionSourceExtension
    {
        public static bool TrySetTaskException<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            if (e is OperationCanceledException)
            {
                return tcs.TrySetCanceled();
            }
            if (e is AggregateException exception)
            {
                return tcs.TrySetException(exception.InnerExceptions);
            }
            return tcs.TrySetException(e);
        }

        public static void SetResult(this TaskCompletionSource<VoidTaskResult> tcs)
        {
            tcs.SetResult(default(VoidTaskResult));
        }

        public static bool TrySetResult(this TaskCompletionSource<VoidTaskResult> tcs)
        {
            return tcs.TrySetResult(default(VoidTaskResult));
        }

        public static void Execute<T>(this TaskCompletionSource<T> tcs, Action action)
        {
            if (StackGuard.CheckForSufficientStack())
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    tcs.TrySetTaskException(e);
                }
            }
            else
            {
                tcs.ExecuteNew(action);
            }
        }

        public static void ExecuteNew<T>(this TaskCompletionSource<T> tcs, Action action)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    tcs.TrySetTaskException(e);
                }
            });
        }

        static class StackGuard
        {
            // The ReservedSize is large than actual needs. And CLR2 x64 need more size than CLR4.
            private const int ReservedSize = 0x30000;

            [ThreadStatic]
            private unsafe static void* _guard;

            public unsafe static bool CheckForSufficientStack()
            {
                void* guard = _guard;
                void* ptr = &guard;
                if (guard == null)
                {
                    NativeMethods.MEMORY_BASIC_INFORMATION memInfo = new NativeMethods.MEMORY_BASIC_INFORMATION();
                    NativeMethods.VirtualQuery((IntPtr)ptr, ref memInfo, (UIntPtr)Marshal.SizeOf(typeof(NativeMethods.MEMORY_BASIC_INFORMATION)));
                    _guard = guard = (byte*)memInfo.AllocationBase.ToPointer() + ReservedSize;
                }
                return ptr > guard;
            }

            public static class NativeMethods
            {
                [StructLayout(LayoutKind.Sequential)]
                public struct MEMORY_BASIC_INFORMATION
                {
                    public IntPtr BaseAddress;
                    public IntPtr AllocationBase;
                    public uint AllocationProtect;
                    public UIntPtr RegionSize;
                    public uint State;
                    public uint Protect;
                    public uint Type;
                }

                [DllImport("kernel32.dll", SetLastError = true)]
                public static extern UIntPtr VirtualQuery(IntPtr address, ref MEMORY_BASIC_INFORMATION buffer, UIntPtr sizeOfBuffer);
            }
        }
    }
}
