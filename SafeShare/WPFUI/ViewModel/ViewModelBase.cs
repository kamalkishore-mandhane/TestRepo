using SafeShare.WPFUI.Utils;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SafeShare.WPFUI.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private int _lockCount;
        protected Action<bool> _adjustPaneCursor;
        protected Func<Func<Task>, Func<Exception, int>, Task> _executor;
        protected Func<Func<Task>, Func<Exception, int>, bool, Task> _externalExecutor;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelBase(IntPtr owner, Action<bool> adjustPaneCursor)
        {
            _executor = ExecuteCloudTask;
            _externalExecutor = ExternalExecuteCloudTask;
            _adjustPaneCursor = adjustPaneCursor;
            Owner = owner;
        }

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region HandleExptions

        protected abstract bool HandleException(Exception ex);

        private Task ExternalExecuteCloudTask(Func<Task> action, Func<Exception, int> retry, bool isLockPage)
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                LockPage(isLockPage);
                ExecuteCloudInternalTask(tcs, action, retry);
            });
        }

        private Task ExecuteCloudTask(Func<Task> action, Func<Exception, int> retry)
        {
            return ExternalExecuteCloudTask(action, retry, true);
        }

        private void ExecuteCloudInternalTask(TaskCompletionSource<VoidTaskResult> tcs, Func<Task> action, Func<Exception, int> retry)
        {
            try
            {
                action().ContinueWithTCSTaskInContext(tcs, task =>
                {
                    if (!task.IsFaulted || !ExecuteCloudTaskExceptionHandleOrInternalRetryAction(tcs, task.Exception, action, retry))
                    {
                        LockPage(false);
                        task.TransferToTCS(tcs);
                    }
                });
            }
            catch (Exception ex)
            {
                if (!ExecuteCloudTaskExceptionHandleOrInternalRetryAction(tcs, ex, action, retry))
                {
                    LockPage(false);
                    tcs.TrySetException(ex);
                }
            }
        }

        private bool ExecuteCloudTaskExceptionHandleOrInternalRetryAction(TaskCompletionSource<VoidTaskResult> tcs, Exception ex, Func<Task> action, Func<Exception, int> retry)
        {
            var ae = ex as AggregateException;
            if (ae != null && ae.InnerExceptions.Count == 1)
            {
                return ExecuteCloudTaskExceptionHandleOrInternalRetryAction(tcs, ae.InnerExceptions[0], action, retry);
            }
            if (HandleException(ex))
            {
                return false;
            }
            return ExecuteCloudTaskInternalRetryAction(tcs, ex, action, retry);
        }

        private bool ExecuteCloudTaskInternalRetryAction(TaskCompletionSource<VoidTaskResult> tcs, Exception ex, Func<Task> action, Func<Exception, int> retry)
        {
            switch (retry(ex))
            {
                case -1:
                    return false;

                case -2:
                    return false;

                default:
                    // TODO delay
                    ExecuteCloudInternalTask(tcs, action, retry);
                    return true;
            }
        }

        /// <summary>
        /// Lock UI when loading
        /// </summary>
        /// <param name="dolock"></param>
        public void LockPage(bool dolock)
        {
            if (dolock)
            {
                if (_lockCount++ == 0)
                {
                    _adjustPaneCursor(!dolock);
                }
            }
            else
            {
                if (_lockCount == 0 || --_lockCount == 0)
                {
                    _adjustPaneCursor(!dolock);
                }
            }
        }

        #endregion HandleExptions

        public IntPtr Owner
        {
            get;
            private set;
        }

        public Dispatcher Dispatcher
        {
            get;
            set;
        }
    }

    public class RetryStrategy
    {
        private readonly bool _retry;
        private readonly int _count;
        private readonly int _delay;
        private int _value;

        public static Func<Exception, int> Create(bool retry, int count = 3, int delay = 500)
        {
            if (count == 0 && !retry)
            {
                return _ => -1;
            }
            else
            {
                return new RetryStrategy(retry, count, delay).Handler;
            }
        }

        private RetryStrategy(bool retry, int count, int delay)
        {
            _retry = retry;
            _count = count;
            _delay = delay;
        }

        private int Handler(Exception ex)
        {
            if (_value == _count)
            {
                _value = 0;
                return _retry && CanRetry(ex) ? -2 : -1;
            }
            else
            {
                _value++;
                return CanRetry(ex) ? _delay : -1;
            }
        }

        protected virtual bool CanRetry(Exception ex)
        {
            if (ex is NotSupportedException)
            {
                return false;
            }

            return true;
        }
    }
}