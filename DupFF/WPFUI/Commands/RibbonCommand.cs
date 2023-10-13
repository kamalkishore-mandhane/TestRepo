using System;
using System.Reflection;
using System.Threading.Tasks;
using DupFF.WPFUI.Controls;
using DupFF.WPFUI.Utils;
using DupFF.WPFUI.ViewModel;

namespace DupFF.WPFUI.Commands
{
    class RibbonCommand
    {
        private RibbonTabViewModel _ribbonTabViewModel;
        private DupFFViewModel _viewModel;
        private ModelCommand _createCommand;
        private ModelCommand _runCommand;
        private ModelCommand _modifyCommand;
        private ModelCommand _deleteCommand;
        private ModelCommand _undoDeleteCommand;

        private ModelCommand _actionCommand;
        private ModelCommand _dismissCommand;
        private ModelCommand _dismissAllCommand;

        public RibbonCommand(RibbonTabViewModel ribbonTabViewModel, DupFFViewModel viewModel)
        {
            _ribbonTabViewModel = ribbonTabViewModel;
            _viewModel = viewModel;
            _createCommand = new ModelCommand(ExecuteCreateCommand, CanExecuteCommontCommand);
            _runCommand = new ModelCommand(ExecuteRunCommand, CanExecuteRunCommand);
            _modifyCommand = new ModelCommand(ExecuteModifyCommand, CanExecuteModifyCommand);
            _deleteCommand = new ModelCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);
            _undoDeleteCommand = new ModelCommand(ExecuteUndoDeleteCommand, CanExecuteUndoDeleteCommand);

            _actionCommand = new ModelCommand(ExecuteActionCommand, CanExecuteActionCommand);
            _dismissCommand = new ModelCommand(ExecuteDismissCommand, CanExecuteDismissCommand);
            _dismissAllCommand = new ModelCommand(ExecuteDismissAllCommand, CanExecuteDismissAllCommand);
        }

        private bool CanExecuteCommontCommand(object arg)
        {
            return true;
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand CreateCommand
        {
            get { return _createCommand; }
        }

        private void ExecuteCreateCommand(object obj)
        {
            ExecuteTask(_ribbonTabViewModel.ExecuteCreateCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand DeleteCommand
        {
            get { return _deleteCommand; }
        }

        private bool CanExecuteDeleteCommand(object arg)
        {
            foreach (var item in _viewModel.SelectedItems)
            {
                if (!item.Available || item.IsCanned)
                {
                    return false;
                }
            }

            return _viewModel.SelectedItems.Count > 0;
        }

        private void ExecuteDeleteCommand(object obj)
        {
            ExecuteTask(() =>
            {
                _ribbonTabViewModel.ExecuteDeleteCommand();
            });
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand UndoDeleteCommand
        {
            get { return _undoDeleteCommand; }
        }

        private bool CanExecuteUndoDeleteCommand(object arg)
        {
            foreach (var item in _viewModel.SelectedItems)
            {
                if (item.ItemStatus != ItemStatus.Delete)
                {
                    return false;
                }
            }

            return _viewModel.SelectedItems.Count > 0;
        }

        private void ExecuteUndoDeleteCommand(object obj)
        {
            ExecuteTask(() =>
            {
                _ribbonTabViewModel.ExecuteUndoDeleteCommand();
            });
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ModifyCommand
        {
            get { return _modifyCommand; }
        }

        private bool CanExecuteModifyCommand(object arg)
        {
            if (_viewModel.SelectedItems.Count != 1 || !_viewModel.SelectedItems[0].Available)
            {
                return false;
            }

            return true;
        }

        private void ExecuteModifyCommand(object obj)
        {
            ExecuteTask(_ribbonTabViewModel.ExecuteModifyCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand RunCommand
        {
            get { return _runCommand; }
        }

        private bool CanExecuteRunCommand(object arg)
        {
            foreach (var item in _viewModel.SelectedItems)
            {
                if (!item.Available)
                {
                    return false;
                }
            }

            return _viewModel.SelectedItems.Count > 0;
        }

        private void ExecuteRunCommand(object obj)
        {
            ExecuteTask(_ribbonTabViewModel.ExecuteRunCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ActionCommand
        {
            get { return _actionCommand; }
        }

        private bool CanExecuteActionCommand(object arg)
        {
            if (_viewModel.SelectedActionItems.Count != 1 || _viewModel.SelectedActionItems[0].Processed)
            {
                return false;
            }
            return true;
        }

        private void ExecuteActionCommand(object obj)
        {
            ExecuteTask(_ribbonTabViewModel.ExecuteActionCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand DismissCommand
        {
            get { return _dismissCommand; }
        }

        private bool CanExecuteDismissCommand(object arg)
        {
            if (_viewModel.SelectedActionItems.Count == 0)
            {
                return false;
            }

            return true;
        }

        private void ExecuteDismissCommand(object obj)
        {
            ExecuteTask(_ribbonTabViewModel.ExecuteDismissCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand DismissAllCommand
        {
            get { return _dismissAllCommand; }
        }

        private bool CanExecuteDismissAllCommand(object arg)
        {
            if (_viewModel.ActionItems.Count == 0)
            {
                return false;
            }

            return true;
        }

        private void ExecuteDismissAllCommand(object obj)
        {
            ExecuteTask(_ribbonTabViewModel.ExecuteDismissAllCommand);
        }

        public void ExecuteTask(Action action)
        {
            _viewModel.Executor(() => Task.Factory.StartNewTCS(tcs =>
            {
                action();
                tcs.TrySetResult();
            }), RetryStrategy.Create(false, 0)).IgnoreExceptions().WaitWithMsgPump();
        }
    }
}
