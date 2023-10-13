using System.Reflection;
using PdfUtil.WPFUI.ViewModel;

namespace PdfUtil.WPFUI.Commands
{
    class ThumbnailPaneCommandModel
    {
        private ThumbnailPaneContextMenuViewModel _viewModel;

        private ModelCommand _setBackgroundColorCommand;
        private ModelCommand _addBlankPageCommand;
        private ModelCommand _deletePagesCommand;
        private ModelCommand _deleteBlankPagesCommand;
        private ModelCommand _extractImagesCommand;
        private ModelCommand _extractPagesCommand;
        private ModelCommand _importFilesCommand;
        private ModelCommand _importFromCameraCommand;
        private ModelCommand _importFromScannerCommand;
        private ModelCommand _rotatePagesCommand;
        private ModelCommand _watermarkPagesCommand;
        private ModelCommand _selectAllCommand;
        private ModelCommand _movePagesCommand;

        public ThumbnailPaneCommandModel(ThumbnailPaneContextMenuViewModel viewModel)
        {
            _viewModel = viewModel;

            _setBackgroundColorCommand = new ModelCommand(ExecuteSetBackgroundColorCommand, CanExecuteSetBackgroundColorCommand);
            _addBlankPageCommand = new ModelCommand(ExecuteAddBlankPageCommand, CanExecuteAddBlankPageCommand);
            _deletePagesCommand = new ModelCommand(ExecuteDeletePagesCommand, CanExecuteDeletePagesCommand);
            _deleteBlankPagesCommand = new ModelCommand(ExecuteDeleteBlankPagesCommand, CanExecuteDeleteBlankPagesCommand);
            _extractImagesCommand = new ModelCommand(ExecuteExtractImagesCommand, CanExecuteExtractImagesCommand);
            _extractPagesCommand = new ModelCommand(ExecuteExtractPagesCommand, CanExecuteExtractPagesCommand);
            _importFilesCommand = new ModelCommand(ExecuteImportFilesCommand, CanExecuteImportFilesCommand);
            _importFromCameraCommand = new ModelCommand(ExecuteImportFromCameraCommand, CanExecuteImportFromCameraCommand);
            _importFromScannerCommand = new ModelCommand(ExecuteImportFromScannerCommand, CanExecuteImportFromScannerCommand);
            _rotatePagesCommand = new ModelCommand(ExecuteRotatePagesCommand, CanExecuteRotatePagesCommand);
            _watermarkPagesCommand = new ModelCommand(ExecuteWatermarkPagesCommand, CanExecuteWatermarkPagesCommand);
            _selectAllCommand = new ModelCommand(ExecuteSelectAllCommand, CanExecuteSelectAllCommand);
            _movePagesCommand = new ModelCommand(ExecuteMovePagesCommand, CanExecuteMovePagesCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SetBackgroundColorCommand
        {
            get { return _setBackgroundColorCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand AddBlankPageCommand
        {
            get { return _addBlankPageCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand DeletePagesCommand
        {
            get { return _deletePagesCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand DeleteBlankPagesCommand
        {
            get { return _deleteBlankPagesCommand; }
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
        public ModelCommand RotatePagesCommand
        {
            get { return _rotatePagesCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand WatermarkPagesCommand
        {
            get { return _watermarkPagesCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SelectAllCommand
        {
            get { return _selectAllCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand MovePagesCommand
        {
            get { return _movePagesCommand; }
        }

        private bool CanExecuteSetBackgroundColorCommand(object parameter)
        {
            return true;
        }

        private void ExecuteSetBackgroundColorCommand(object parameter)
        {
            _viewModel.ExecuteSetBackgroundColorCommand();
        }

        private bool CanExecuteAddBlankPageCommand(object parameter)
        {
            return true;
        }

        private void ExecuteAddBlankPageCommand(object parameter)
        {
            _viewModel.ExecuteAddBlankPageCommand();
        }

        private bool CanExecuteDeletePagesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteDeletePagesCommand(object parameter)
        {
            _viewModel.ExecuteDeletePagesCommand();
        }

        private bool CanExecuteDeleteBlankPagesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteDeleteBlankPagesCommand(object parameter)
        {
            _viewModel.ExecuteDeleteBlankPagesCommand();
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

        private bool CanExecuteRotatePagesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteRotatePagesCommand(object parameter)
        {
            _viewModel.ExecuteRotatePageCommand();
        }

        private bool CanExecuteWatermarkPagesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteWatermarkPagesCommand(object parameter)
        {
            _viewModel.ExecuteWatermarkPagesCommand();
        }

        private bool CanExecuteSelectAllCommand(object parameter)
        {
            return true;
        }

        private void ExecuteSelectAllCommand(object parameter)
        {
            _viewModel.ExecuteSelectAllCommand();
        }

        private bool CanExecuteMovePagesCommand(object parameter)
        {
            return true;
        }

        private void ExecuteMovePagesCommand(object parameter)
        {
            _viewModel.ExecuteMovePagesCommand(System.Convert.ToBoolean(parameter));
        }
    }
}
