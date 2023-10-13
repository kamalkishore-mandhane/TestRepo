using SafeShare.WPFUI.ViewModel;
using System.Reflection;

namespace SafeShare.WPFUI.Commands
{
    public class FileListPageViewModelCommand
    {
        private FileListPageViewModel _viewModel;

        private ModelCommand _addFilesCommand;
        private ModelCommand _removeFilesCommand;
        private ModelCommand _continueCommand;

        public FileListPageViewModelCommand(FileListPageViewModel viewModel)
        {
            _viewModel = viewModel;

            _addFilesCommand = new ModelCommand(ExecuteAddFilesCommand, CanExecuteAddFilesCommand);
            _removeFilesCommand = new ModelCommand(ExecuteRemoveFilesCommand, CanExecuteRemoveFilesCommand);
            _continueCommand = new ModelCommand(ExecuteContinueCommand, CanExecuteContinueCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand AddFilesCommand
        {
            get { return _addFilesCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand RemoveFilesCommand
        {
            get { return _removeFilesCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ContinueCommand
        {
            get { return _continueCommand; }
        }

        private bool CanExecuteAddFilesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteAddFilesCommand(object parameter)
        {
            _viewModel.ExecuteAddFilesCommand();
        }

        private bool CanExecuteRemoveFilesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteRemoveFilesCommand(object parameter)
        {
            _viewModel.ExecuteRemoveFilesCommand();
        }

        private bool CanExecuteContinueCommand(object parameter)
        {
            return true;
        }

        private void ExecuteContinueCommand(object parameter)
        {
            _viewModel.ExecuteContinueCommand();
        }
    }
}