using System;
using System.Windows.Input;

namespace ImgUtil.WPFUI.Commands
{
    public class ModelCommand : ICommand
    {
        private Action<object> _executeAction;
        private Func<object, bool> _canExecute;

        public ModelCommand(Action<object> executeAction, Func<object, bool> canExecute)
        {
            _executeAction = executeAction;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }
    }
}
