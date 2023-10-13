using SafeShare.WPFUI.ViewModel;
using System.Reflection;

namespace SafeShare.WPFUI.Commands
{
    internal class FrontPageViewModelCommand
    {
        private FrontPageViewModel _viewModel;
        private ModelCommand _selectFilesCommand;
        private ModelCommand _hyperlinkClickCommand;

        public FrontPageViewModelCommand(FrontPageViewModel viewModel)
        {
            _viewModel = viewModel;
            _selectFilesCommand = new ModelCommand(ExecuteSelectFilesCommand, CanExecuteSelectFilesCommand);
            _hyperlinkClickCommand = new ModelCommand(ExecuteHyperlinkClickCommand, CanExecuteHyperlinkClickCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SelectFilesCommand
        {
            get { return _selectFilesCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand HyperlinkClickCommand
        {
            get { return _hyperlinkClickCommand; }
        }

        private bool CanExecuteSelectFilesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteSelectFilesCommand(object parameter)
        {
            _viewModel.ExecuteSelectFilesCommand();
        }

        private bool CanExecuteHyperlinkClickCommand(object parameter)
        {
            return true;
        }

        private void ExecuteHyperlinkClickCommand(object parameter)
        {
            _viewModel.ExecuteHyperlinkClickCommand();
        }
    }
}