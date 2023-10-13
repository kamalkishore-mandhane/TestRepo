using PdfUtil.WPFUI.ViewModel;
using System.Reflection;

namespace PdfUtil.WPFUI.Commands
{
    class RibbonCommandModel
    {
        private FakeRibbonTabViewModel _viewModel;

        private ModelCommand _createFromCommand;
        private ModelCommand _openCommand;
        private ModelCommand _printCommand;
        private ModelCommand _saveCommand;
        private ModelCommand _saveAsCommand;
        private ModelCommand _saveToZipCommand;
        private ModelCommand _shareCommand;
        private ModelCommand _closeCommand;
        private ModelCommand _newWindowCommand;
        private ModelCommand _exitCommand;
        private ModelCommand _extractImagesCommand;
        private ModelCommand _extractPagesCommand;
        private ModelCommand _importFilesCommand;
        private ModelCommand _importFromCameraCommand;
        private ModelCommand _importFromScannerCommand;
        private ModelCommand _lockPdfCommand;
        private ModelCommand _unlockPdfCommand;
        private ModelCommand _signPdfCommand;
        private ModelCommand _signatureCommand;
        private ModelCommand _commentPdfCommand;
        private ModelCommand _pdfSettingsCommand;
        private ModelCommand _windowsIntegrationCommand;
        private ModelCommand _helpCommand;
        public RibbonCommandModel(FakeRibbonTabViewModel viewModel)
        {
            _viewModel = viewModel;

            _createFromCommand = new ModelCommand(ExecuteCreateFromCommand, CanExecuteCreateFromCommand);
            _openCommand = new ModelCommand(ExecuteOpenCommand, CanExecuteOpenCommand);
            _printCommand = new ModelCommand(ExecutePrintCommand, CanExecutePrintCommand);
            _saveCommand = new ModelCommand(ExecuteSaveCommand, CanExecuteSaveCommand);
            _saveAsCommand = new ModelCommand(ExecuteSaveAsCommand, CanExecuteSaveAsCommand);
            _saveToZipCommand = new ModelCommand(ExecuteSaveToZipCommand, CanExecuteSaveToZipCommand);
            _shareCommand = new ModelCommand(ExecuteShareCommand, CanExecuteShareCommand);
            _closeCommand = new ModelCommand(ExecuteCloseCommand, CanExecuteCloseCommand);
            _newWindowCommand = new ModelCommand(ExecuteNewWindowCommand, CanExecuteNewWindowCommand);
            _exitCommand = new ModelCommand(ExecuteExitCommand, CanExecuteExitCommand);
            _extractImagesCommand = new ModelCommand(ExecuteExtractImagesCommand, CanExecuteExtractImagesCommand);
            _extractPagesCommand = new ModelCommand(ExecuteExtractPagesCommand, CanExecuteExtractPagesCommand);
            _importFilesCommand = new ModelCommand(ExecuteImportFilesCommand, CanExecuteImportFilesCommand);
            _importFromCameraCommand = new ModelCommand(ExecuteImportFromCameraCommand, CanExecuteImportFromCameraCommand);
            _importFromScannerCommand = new ModelCommand(ExecuteImportFromScannerCommand, CanExecuteImportFromScannerCommand);
            _lockPdfCommand = new ModelCommand(ExecuteLockPdfCommand, CanExecuteLockPdfCommand);
            _unlockPdfCommand = new ModelCommand(ExecuteUnlockPdfCommand, CanExecuteUnlockPdfCommand);
            _signPdfCommand = new ModelCommand(ExecuteSignPdfCommand, CanExecuteSignPdfCommand);
            _signatureCommand = new ModelCommand(ExecuteSignatureCommand, CanExecuteSignatureCommand);
            _commentPdfCommand = new ModelCommand(ExecuteCommentPdfCommand, CanExecuteCommentPdfCommand);
            _pdfSettingsCommand = new ModelCommand(ExecutePdfSettingsCommand, CanExecutePdfSettingsCommand);
            _windowsIntegrationCommand = new ModelCommand(ExecuteWindowsIntegrationCommand, CanExecuteWindowsIntegrationCommand);
            _helpCommand = new ModelCommand(ExecuteHelpCommand, CanExecuteHelpCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand HelpCommand
        {
            get { return _helpCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand CreateFromCommand
        {
            get { return _createFromCommand; }
        }

        private bool CanExecuteCreateFromCommand(object parameter)
        {
            return true;
        }

        private void ExecuteCreateFromCommand(object parameter)
        {
            _viewModel.ExecuteCreateFromCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand OpenCommand
        {
            get { return _openCommand; }
        }

        private bool CanExecuteOpenCommand(object parameter)
        {
            return true;
        }

        private void ExecuteOpenCommand(object parameter)
        {
            _viewModel.ExecuteOpenCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand PrintCommand
        {
            get { return _printCommand; }
        }

        private bool CanExecutePrintCommand(object parameter)
        {
            return true;
        }

        private void ExecutePrintCommand(object parameter)
        {
            _viewModel.ExecutePrintCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SaveCommand
        {
            get { return _saveCommand; }
        }

        private bool CanExecuteSaveCommand(object parameter)
        {
            return true;
        }

        private void ExecuteSaveCommand(object parameter)
        {
            _viewModel.ExecuteSaveCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SaveAsCommand
        {
            get { return _saveAsCommand; }
        }

        private bool CanExecuteSaveAsCommand(object parameter)
        {
            return true;
        }

        private void ExecuteSaveAsCommand(object parameter)
        {
            _viewModel.ExecuteSaveAsCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SaveToZipCommand
        {
            get { return _saveToZipCommand; }
        }

        private bool CanExecuteSaveToZipCommand(object parameter)
        {
            return true;
        }

        private void ExecuteSaveToZipCommand(object parameter)
        {
            _viewModel.ExecuteSaveToZipCommand();
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


        [Obfuscation(Exclude = true)]
        public ModelCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        private bool CanExecuteCloseCommand(object parameter)
        {
            return true;
        }

        private void ExecuteCloseCommand(object parameter)
        {
            _viewModel.ExecuteCloseCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand NewWindowCommand
        {
            get { return _newWindowCommand; }
        }

        private bool CanExecuteNewWindowCommand(object parameter)
        {
            return true;
        }

        private void ExecuteNewWindowCommand(object parameter)
        {
            _viewModel.ExecuteNewWindowCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ExitCommand
        {
            get { return _exitCommand; }
        }

        private bool CanExecuteExitCommand(object parameter)
        {
            return true;
        }

        private void ExecuteExitCommand(object parameter)
        {
            _viewModel.ExecuteExitCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ExtractImagesCommand
        {
            get { return _extractImagesCommand; }
        }

        private bool CanExecuteExtractImagesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteExtractImagesCommand(object parameter)
        {
            _viewModel.ExecuteExtractImagesCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ExtractPagesCommand
        {
            get { return _extractPagesCommand; }
        }

        private bool CanExecuteExtractPagesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteExtractPagesCommand(object parameter)
        {
            _viewModel.ExecuteExtractPagesCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ImportFilesCommand
        {
            get { return _importFilesCommand; }
        }

        private bool CanExecuteImportFilesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteImportFilesCommand(object parameter)
        {
            _viewModel.ExecuteImportFilesCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ImportFromCameraCommand
        {
            get { return _importFromCameraCommand; }
        }

        private bool CanExecuteImportFromCameraCommand(object parameter)
        {
            return true;
        }

        private void ExecuteImportFromCameraCommand(object parameter)
        {
            _viewModel.ExecuteImportFromCameraCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ImportFromScannerCommand
        {
            get { return _importFromScannerCommand; }
        }

        private bool CanExecuteImportFromScannerCommand(object parameter)
        {
            return true;
        }

        private void ExecuteImportFromScannerCommand(object parameter)
        {
            _viewModel.ExecuteImportFromScannerCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand LockPdfCommand
        {
            get { return _lockPdfCommand; }
        }

        private bool CanExecuteLockPdfCommand(object parameter)
        {
            return true;
        }

        private void ExecuteLockPdfCommand(object parameter)
        {
            _viewModel.ExecuteLockPdfCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand UnlockPdfCommand
        {
            get { return _unlockPdfCommand; }
        }

        private bool CanExecuteUnlockPdfCommand(object parameter)
        {
            return true;
        }

        private void ExecuteUnlockPdfCommand(object parameter)
        {
            _viewModel.ExecuteUnlockPdfCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SignPdfCommand
        {
            get { return _signPdfCommand; }
        }

        private bool CanExecuteSignPdfCommand(object parameter)
        {
            return true;
        }

        private void ExecuteSignPdfCommand(object parameter)
        {
            _viewModel.ExecuteSignPdfCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SignatureCommand
        {
            get { return _signatureCommand; }
        }

        private bool CanExecuteSignatureCommand(object parameter)
        {
            return true;
        }

        private void ExecuteSignatureCommand(object parameter)
        {
            _viewModel.ExecuteSignatureCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand CommentPdfCommand
        {
            get { return _commentPdfCommand; }
        }

        private bool CanExecuteCommentPdfCommand(object parameter)
        {
            return true;
        }

        private void ExecuteCommentPdfCommand(object parameter)
        {
            _viewModel.ExecuteCommentPdfCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand PdfSettingsCommand
        {
            get { return _pdfSettingsCommand; }
        }

        private bool CanExecutePdfSettingsCommand(object parameter)
        {
            return true;
        }

        private void ExecutePdfSettingsCommand(object parameter)
        {
            _viewModel.ExecutePdfSettingsCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand WindowsIntegrationCommand
        {
            get { return _windowsIntegrationCommand; }
        }

        private bool CanExecuteWindowsIntegrationCommand(object parameter)
        {
            return true;
        }

        private void ExecuteWindowsIntegrationCommand(object parameter)
        {
            _viewModel.ExecuteWindowsIntegrationCommand();
        }

        private bool CanExecuteHelpCommand(object parameter)
        {
            return true;
        }
        private void ExecuteHelpCommand(object parameter)
        {
            _viewModel.ExecuteHelpCommand();
        }
    }
}
