using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ImgUtil.WPFUI.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private int _lockCount;
        protected Action<bool> _adjustPaneCursor;

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual IntPtr Owner { get; private set; }

        public Dispatcher Dispatcher { get; set; }

        public ViewModelBase(IntPtr owner, Action<bool> adjustPaneCursor)
        {
            _adjustPaneCursor = adjustPaneCursor;
            Owner = owner;
        }

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Common Executor

        public async Task<bool> ExecuteAsync(Func<Task<bool>> func)
        {
            bool success = false;
            try
            {
                LockPage(true);
                success = await func();
            }
            catch (Exception e)
            {
                success = false;
                HandleException(e);
            }
            finally
            {
                LockPage(false);
            }
            return success;
        }

        public bool Execute(Func<bool> func)
        {
            bool success = false;
            try
            {
                LockPage(true);
                success = func();
            }
            catch (Exception e)
            {
                success = false;
                HandleException(e);
            }
            finally
            {
                LockPage(false);
            }
            return success;
        }

        protected T PopUp<T>(Func<T> func)
        {
            bool assoctiated = Dispatcher?.CheckAccess() ?? true;
            return assoctiated ? func() : Dispatcher.Invoke(func);
        }

        #endregion

        #region HandleExptions

        protected abstract bool HandleException(Exception ex);

        #endregion HandleExptions

        /// <summary>
        /// Lock UI when loading
        /// </summary>
        /// <param name="dolock"></param>
        public void LockPage(bool dolock)
        {
            lock (this)
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
                    if ((_lockCount == 0) || (--_lockCount == 0))
                    {
                        _adjustPaneCursor(!dolock);
                    }
                }
            }
        }
    }

    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
