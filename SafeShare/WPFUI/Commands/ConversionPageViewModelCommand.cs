using SafeShare.WPFUI.ViewModel;
using System.Reflection;

namespace SafeShare.WPFUI.Commands
{
    public class ConversionPageViewModelCommand
    {
        private ConversionPageViewModel _viewModel;

        private ModelCommand _shareCommand;

        public ConversionPageViewModelCommand(ConversionPageViewModel viewModel)
        {
            _viewModel = viewModel;

            _shareCommand = new ModelCommand(ExecuteShareCommand, CanExecuteShareCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ShareCommand
        {
            get { return _shareCommand; }
        }

        private bool CanExecuteShareCommand(object parameter)
        {
            return true;
        }

        private void ExecuteShareCommand(object parameter)
        {
            _viewModel.ExecuteShareCommand();
        }
    }
}