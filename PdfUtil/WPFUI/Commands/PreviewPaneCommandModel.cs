using System.Reflection;
using PdfUtil.WPFUI.ViewModel;

namespace PdfUtil.WPFUI.Commands
{
    class PreviewPaneCommandModel
    {
        private PreviewPaneContextMenuViewModel _viewModel;

        private ModelCommand _addBookmarkCommand;
        private ModelCommand _removeBookmarkCommand;
        private ModelCommand _addCommentCommand;
        private ModelCommand _deleteCommentCommand;
        private ModelCommand _deleteCommentsCommand;
        private ModelCommand _addHighlightCommand;
        private ModelCommand _changeHighlightCommand;
        private ModelCommand _deleteHighlightsCommand;
        private ModelCommand _extractImagesCommand;
        private ModelCommand _extractPagesCommand;
        private ModelCommand _importFilesCommand;
        private ModelCommand _importFromCameraCommand;
        private ModelCommand _importFromScannerCommand;
        private ModelCommand _deleteSignatureCommand;
        private ModelCommand _deleteSignaturesCommand;

        public PreviewPaneCommandModel(PreviewPaneContextMenuViewModel viewModel)
        {
            _viewModel = viewModel;

            _addBookmarkCommand = new ModelCommand(ExecuteAddBookmarkCommand, CanExecuteAddBookmarkCommand);
            _removeBookmarkCommand = new ModelCommand(ExecuteRemoveBookmarkCommand, CanExecuteRemoveBookmarkCommand);
            _addCommentCommand = new ModelCommand(ExecuteAddCommentCommand, CanExecuteAddCommentCommand);
            _deleteCommentCommand = new ModelCommand(ExecuteDeleteCommentCommand, CanExecuteDeleteCommentCommand);
            _deleteCommentsCommand = new ModelCommand(ExecuteDeleteCommentsCommand, CanExecuteDeleteCommentsCommand);
            _addHighlightCommand = new ModelCommand(ExecuteAddHighlightCommand, CanExecuteAddHighlightCommand);
            _changeHighlightCommand = new ModelCommand(ExecuteChangeHighlightCommand, CanExecuteChangeHighlightCommand);
            _deleteHighlightsCommand = new ModelCommand(ExecuteDeleteHighlightsCommand, CanExecuteDeleteHighlightsCommand);
            _extractImagesCommand = new ModelCommand(ExecuteExtractImagesCommand, CanExecuteExtractImagesCommand);
            _extractPagesCommand = new ModelCommand(ExecuteExtractPagesCommand, CanExecuteExtractPagesCommand);
            _importFilesCommand = new ModelCommand(ExecuteImportFilesCommand, CanExecuteImportFilesCommand);
            _importFromCameraCommand = new ModelCommand(ExecuteImportFromCameraCommand, CanExecuteImportFromCameraCommand);
            _importFromScannerCommand = new ModelCommand(ExecuteImportFromScannerCommand, CanExecuteImportFromScannerCommand);
            _deleteSignatureCommand = new ModelCommand(ExecuteDeleteSignatureCommand, CanExecuteDeleteSignatureCommand);
            _deleteSignaturesCommand = new ModelCommand(ExecuteDeleteSignaturesCommand, CanExecuteDeleteSignaturesCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand AddBookmarkCommand
        {
            get { return _addBookmarkCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand RemoveBookmarkCommand
        {
            get { return _removeBookmarkCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand AddCommentCommand
        {
            get { return _addCommentCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand DeleteCommentCommand
        {
            get { return _deleteCommentCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand DeleteCommentsCommand
        {
            get { return _deleteCommentsCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand AddHighlightCommand
        {
            get { return _addHighlightCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ChangeHighlightCommand
        {
            get { return _changeHighlightCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand DeleteHighlightsCommand
        {
            get { return _deleteHighlightsCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ExtractImagesCommand
        {
            get { return _extractImagesCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ExtractPagesCommand
        {
            get { return _extractPagesCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ImportFilesCommand
        {
            get { return _importFilesCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ImportFromCameraCommand
        {
            get { return _importFromCameraCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ImportFromScannerCommand
        {
            get { return _importFromScannerCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand DeleteSignatureCommand
        {
            get { return _deleteSignatureCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand DeleteSignaturesCommand
        {
            get { return _deleteSignaturesCommand; }
        }

        private bool CanExecuteAddBookmarkCommand(object parameter)
        {
            return true;
        }

        private void ExecuteAddBookmarkCommand(object parameter)
        {
            _viewModel.ExecuteAddBookmarkCommand();
        }

        private bool CanExecuteRemoveBookmarkCommand(object parameter)
        {
            return true;
        }

        private void ExecuteRemoveBookmarkCommand(object parameter)
        {
            _viewModel.ExecuteRemoveBookmarkCommand();
        }

        private bool CanExecuteAddCommentCommand(object parameter)
        {
            return true;
        }

        private void ExecuteAddCommentCommand(object parameter)
        {
            _viewModel.ExecuteAddCommentCommand();
        }

        private bool CanExecuteDeleteCommentCommand(object parameter)
        {
            return true;
        }

        private void ExecuteDeleteCommentCommand(object parameter)
        {
            _viewModel.ExecuteDeleteCommentCommand(false);
        }

        private bool CanExecuteDeleteCommentsCommand(object parameter)
        {
            return true;
        }

        private void ExecuteDeleteCommentsCommand(object parameter)
        {
            _viewModel.ExecuteDeleteCommentCommand(true);
        }

        private bool CanExecuteAddHighlightCommand(object parameter)
        {
            return true;
        }

        private void ExecuteAddHighlightCommand(object parameter)
        {
            _viewModel.ExecuteAddChangeHighlightCommand();
        }

        private bool CanExecuteChangeHighlightCommand(object parameter)
        {
            return true;
        }

        private void ExecuteChangeHighlightCommand(object parameter)
        {
            _viewModel.ExecuteAddChangeHighlightCommand();
        }

        private bool CanExecuteDeleteHighlightsCommand(object parameter)
        {
            return true;
        }

        private void ExecuteDeleteHighlightsCommand(object parameter)
        {
            _viewModel.ExecuteDeleteHighlightsCommand();
        }

        private bool CanExecuteExtractImagesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteExtractImagesCommand(object parameter)
        {
            _viewModel.ExecuteExtractImagesCommand();
        }

        private bool CanExecuteExtractPagesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteExtractPagesCommand(object parameter)
        {
            _viewModel.ExecuteExtractPagesCommand();
        }

        private bool CanExecuteImportFilesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteImportFilesCommand(object parameter)
        {
            _viewModel.ExecuteImportFilesCommand();
        }

        private bool CanExecuteImportFromCameraCommand(object parameter)
        {
            return true;
        }

        private void ExecuteImportFromCameraCommand(object parameter)
        {
            _viewModel.ExecuteImportFromCameraCommand();
        }

        private bool CanExecuteImportFromScannerCommand(object parameter)
        {
            return true;
        }

        private void ExecuteImportFromScannerCommand(object parameter)
        {
            _viewModel.ExecuteImportFromScannerCommand();
        }

        private bool CanExecuteDeleteSignatureCommand(object parameter)
        {
            return true;
        }

        private void ExecuteDeleteSignatureCommand(object parameter)
        {
            _viewModel.ExecuteDeleteSignatureCommand(false);
        }

        private bool CanExecuteDeleteSignaturesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteDeleteSignaturesCommand(object parameter)
        {
            _viewModel.ExecuteDeleteSignatureCommand(true);
        }
    }
}
