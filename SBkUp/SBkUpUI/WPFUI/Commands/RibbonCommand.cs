using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SBkUpUI.WPFUI.Utils;
using SBkUpUI.WPFUI.ViewModel;

namespace SBkUpUI.WPFUI.Commands
{
    class RibbonCommand
    {
        private RibbonTabViewModel _ribbonTabViewModel;
        private SBkUpViewModel _viewModel;
        private ModelCommand _helpCommand;
        private ModelCommand _createCommand;
        private ModelCommand _runCommand;
        private ModelCommand _modifyCommand;
        private ModelCommand _deleteCommand;

        private ModelCommand _allCommand;
        private ModelCommand _specificCommand;
        private ModelCommand _openCommand;

        public RibbonCommand(RibbonTabViewModel ribbonTabViewModel, SBkUpViewModel viewModel)
        {
            _ribbonTabViewModel = ribbonTabViewModel;
            _viewModel = viewModel;
            _helpCommand = new ModelCommand(ExecuteHelpCommand, CanExecuteCommontCommand);
            _createCommand = new ModelCommand(ExecuteCreateCommand, CanExecuteCommontCommand);
            _runCommand = new ModelCommand(ExecuteRunCommand, CanExecuteCommontCommand);
            _modifyCommand = new ModelCommand(ExecuteModifyCommand, CanExecuteCommontCommand);
            _deleteCommand = new ModelCommand(ExecuteDeleteCommand, CanExecuteCommontCommand);

            _allCommand = new ModelCommand(ExecuteAllCommand, CanExecuteCommontCommand);
            _specificCommand = new ModelCommand(ExecuteSpecificCommand, CanExecuteCommontCommand);
            _openCommand = new ModelCommand(ExecuteOpenCommand, CanExecuteCommontCommand);
        }

        private bool CanExecuteCommontCommand(object arg)
        {
            return true;
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand HelpCommand => _helpCommand;

        private void ExecuteHelpCommand(object parameter)
        {
            _ribbonTabViewModel.ExecuteHelpCommand();
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

        private void ExecuteDeleteCommand(object obj)
        {
            ExecuteTask(() =>
            {
                _viewModel.SwjfWatcher.EnableRaisingEvents = false;
                _ribbonTabViewModel.ExecuteDeleteCommand();
                _viewModel.SwjfWatcher.EnableRaisingEvents = true;
            });
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ModifyCommand
        {
            get { return _modifyCommand; }
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

        private void ExecuteRunCommand(object obj)
        {
            ExecuteTask(_ribbonTabViewModel.ExecuteRunCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand AllCommand
        {
            get { return _allCommand; }
        }

        private void ExecuteAllCommand(object obj)
        {
            ExecuteTask(_ribbonTabViewModel.ExecuteAllCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SpecificCommand
        {
            get { return _specificCommand; }
        }

        private void ExecuteSpecificCommand(object obj)
        {
            ExecuteTask(_ribbonTabViewModel.ExecuteSpecifcCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand OpenCommand
        {
            get { return _openCommand; }
        }

        private void ExecuteOpenCommand(object obj)
        {
            ExecuteTask(_ribbonTabViewModel.ExecuteOpenCommand);
        }

        private void ExecuteTask(Action action)
        {
            _viewModel.Executor(() => Task.Factory.StartNewTCS(tcs =>
            {
                action();
                tcs.TrySetResult();
            }), RetryStrategy.Create(false, 0)).IgnoreExceptions().WaitWithMsgPump();
        }
    }
}
