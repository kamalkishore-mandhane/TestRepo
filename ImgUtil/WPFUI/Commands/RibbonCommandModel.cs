using ImgUtil.WPFUI.ViewModel;
using System.Reflection;

namespace ImgUtil.WPFUI.Commands
{
    class RibbonCommandModel
    {
        #region Fields

        private ImgUtilViewModel _viewModel;

        private ModelCommand _createFromCommand;
        private ModelCommand _openCommand;
        private ModelCommand _openRecentCommand;
        private ModelCommand _openWithCommand;
        private ModelCommand _printCommand;
        private ModelCommand _saveCommand;
        private ModelCommand _saveAsCommand;
        private ModelCommand _saveToZipCommand;
        private ModelCommand _shareCommand;
        private ModelCommand _closeCommand;
        private ModelCommand _newWindowCommand;
        private ModelCommand _exitCommand;
        private ModelCommand _imgSettingsCommand;
        private ModelCommand _windowsIntegrationCommand;
        private ModelCommand _helpCommand;

        private ModelCommand _importImageCommand;
        private ModelCommand _importFromCameraCommand;
        private ModelCommand _importFromScannerCommand;
        private ModelCommand _copyCommand;
        private ModelCommand _convertToCommand;
        private ModelCommand _cropCommand;
        private ModelCommand _removePersonalDataCommand;
        private ModelCommand _resizeImageCommand;
        private ModelCommand _rotateLeftCommand;
        private ModelCommand _rotateRightCommand;
        private ModelCommand _watermarkImageCommand;
        private ModelCommand _addToTeamsBackgroundCommand;
        private ModelCommand _setDesktopBackgroundCommand;

        #endregion

        #region Constructors

        public RibbonCommandModel(ImgUtilViewModel viewModel) => _viewModel = viewModel;

        #endregion

        #region Application Menu Commands

        [Obfuscation(Exclude = true)]
        public ModelCommand CreateFromCommand => _createFromCommand ?? (_createFromCommand = new ModelCommand(ExecuteCreateFromCommand, p => true));

        private async void ExecuteCreateFromCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteCreateFromCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand OpenCommand => _openCommand ?? (_openCommand = new ModelCommand(ExecuteOpenCommand, p => true));

        private async void ExecuteOpenCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteOpenCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand OpenRecentCommand => _openRecentCommand ?? (_openRecentCommand = new ModelCommand(ExecuteOpenRecentCommand, p => true));

        private async void ExecuteOpenRecentCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(() => _viewModel.ExecuteOpenRecentCommandAsync(parameter));
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand OpenWithCommand => _openWithCommand ?? (_openWithCommand = new ModelCommand(ExecuteOpenWithCommand, p => true));

        private async void ExecuteOpenWithCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteOpenWithCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand PrintCommand => _printCommand ?? (_printCommand = new ModelCommand(ExecutePrintCommand, p => true));

        private async void ExecutePrintCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecutePrintCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SaveCommand => _saveCommand ?? (_saveCommand = new ModelCommand(ExecuteSaveCommand, p => true));

        private async void ExecuteSaveCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteSaveCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SaveAsCommand => _saveAsCommand ?? (_saveAsCommand = new ModelCommand(ExecuteSaveAsCommand, p => true));

        private void ExecuteSaveAsCommand(object parameter)
        {
            _viewModel.Execute(_viewModel.ExecuteSaveAsCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SaveToZipCommand => _saveToZipCommand ?? (_saveToZipCommand = new ModelCommand(ExecuteSaveToZipCommand, p => true));

        private async void ExecuteSaveToZipCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteSaveToZipCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ShareCommand => _shareCommand ?? (_shareCommand = new ModelCommand(ExecuteShareCommand, p => true));

        private async void ExecuteShareCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteShareCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ImgSettingsCommand => _imgSettingsCommand ?? (_imgSettingsCommand = new ModelCommand(ExecuteImgSettingsCommand, p => true));

        private async void ExecuteImgSettingsCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteImgSettingsCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand WindowsIntegrationCommand => _windowsIntegrationCommand ?? (_windowsIntegrationCommand = new ModelCommand(ExecuteWindowsIntegrationCommand, p => true));

        private void ExecuteWindowsIntegrationCommand(object parameter)
        {
            _viewModel.Execute(_viewModel.ExecuteWindowsIntegrationCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand CloseCommand => _closeCommand ?? (_closeCommand = new ModelCommand(ExecuteCloseCommand, p => true));

        private async void ExecuteCloseCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteCloseCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand NewWindowCommand => _newWindowCommand ?? (_newWindowCommand = new ModelCommand(ExecuteNewWindowCommand, p => true));

        private void ExecuteNewWindowCommand(object parameter)
        {
            _viewModel.Execute(_viewModel.ExecuteNewWindowCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ExitCommand => _exitCommand ?? (_exitCommand = new ModelCommand(ExecuteExitCommand, p => true));

        private void ExecuteExitCommand(object parameter)
        {
            _viewModel.Execute(_viewModel.ExecuteExitCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand HelpCommand => _helpCommand ?? (_helpCommand = new ModelCommand(ExecuteHelpCommand, p => true));

        private void ExecuteHelpCommand(object parameter)
        {
            _viewModel.Execute(_viewModel.ExecuteHelpCommand);
        }

        #endregion

        #region Actions Commands

        [Obfuscation(Exclude = true)]
        public ModelCommand ImportImageCommand => _importImageCommand ?? (_importImageCommand = new ModelCommand(ExecuteImportImageCommand, p => true));

        private async void ExecuteImportImageCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteImportImageCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ImportFromCameraCommand => _importFromCameraCommand ?? (_importFromCameraCommand = new ModelCommand(ExecuteImportFromCameraCommand, p => true));

        private async void ExecuteImportFromCameraCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteImportFromCameraCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ImportFromScannerCommand => _importFromScannerCommand ?? (_importFromScannerCommand = new ModelCommand(ExecuteImportFromScannerCommand, p => true));

        private async void ExecuteImportFromScannerCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteImportFromScannerCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand CopyCommand => _copyCommand ?? (_copyCommand = new ModelCommand(ExecuteCopyCommand, p => true));

        private async void ExecuteCopyCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteCopyCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ConvertToCommand => _convertToCommand ?? (_convertToCommand = new ModelCommand(ExecuteConvertToCommand, p => _viewModel.IsCurrentImageExtentionMatchFormat));

        private async void ExecuteConvertToCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteConvertToCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand CropCommand => _cropCommand ?? (_cropCommand = new ModelCommand(ExecuteCropCommand, p => true));

        private async void ExecuteCropCommand(object parameter)
        {
            await _viewModel.ExecuteCropCommandAsync();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand RemovePersonalDataCommand => _removePersonalDataCommand ?? (_removePersonalDataCommand = new ModelCommand(ExecuteRemovePersonalDataCommand, p => _viewModel.IsCurrentImageExtentionMatchFormat));

        private async void ExecuteRemovePersonalDataCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteRemovePersonalDataCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ResizeImageCommand => _resizeImageCommand ?? (_resizeImageCommand = new ModelCommand(ExecuteResizeImageCommand, p => _viewModel.IsCurrentImageExtentionMatchFormat));

        private async void ExecuteResizeImageCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteResizeImageCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand RotateLeftCommand => _rotateLeftCommand ?? (_rotateLeftCommand = new ModelCommand(ExecuteRotateLeftCommand, p => true));

        private async void ExecuteRotateLeftCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteRotateLeftCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand RotateRightCommand => _rotateRightCommand ?? (_rotateRightCommand = new ModelCommand(ExecuteRotateRightCommand, p => true));

        private async void ExecuteRotateRightCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteRotateRightCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand WatermarkImageCommand => _watermarkImageCommand ?? (_watermarkImageCommand = new ModelCommand(ExecuteWatermarkImageCommand, p => _viewModel.IsCurrentImageExtentionMatchFormat));

        private async void ExecuteWatermarkImageCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteWatermarkImageCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand AddToTeamsBackgroundCommand => _addToTeamsBackgroundCommand ?? (_addToTeamsBackgroundCommand = new ModelCommand(ExecuteAddToTeamsBackgroundCommand, p => true));

        private async void ExecuteAddToTeamsBackgroundCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteAddToTeamsBackgroundCommandAsync);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SetDesktopBackgroundCommand => _setDesktopBackgroundCommand ?? (_setDesktopBackgroundCommand = new ModelCommand(ExecuteSetDesktopBackgroundCommand, p => true));

        private async void ExecuteSetDesktopBackgroundCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(_viewModel.ExecuteSetDesktopBackgroundCommandAsync);
        }

        #endregion
    }
}
