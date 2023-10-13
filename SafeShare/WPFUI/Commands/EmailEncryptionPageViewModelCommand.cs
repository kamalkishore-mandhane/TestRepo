using SafeShare.WPFUI.ViewModel;
using System.Reflection;

namespace SafeShare.WPFUI.Commands
{
    public class EmailEncryptionPageViewModelCommand
    {
        private EmailEncryptionPageViewModel _viewModel;

        private ModelCommand _nextCommand;

        public EmailEncryptionPageViewModelCommand(EmailEncryptionPageViewModel viewModel)
        {
            _viewModel = viewModel;

            _nextCommand = new ModelCommand(ExecuteNextCommand, CanExecuteNextCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand NextCommand
        {
            get { return _nextCommand; }
        }

        private bool CanExecuteNextCommand(object parameter)
        {
            return true;
        }

        private void ExecuteNextCommand(object parameter)
        {
            _viewModel.ExecuteNextCommand();
        }
    }
}