using ImgUtil.Util;
using ImgUtil.WPFUI.Model;
using ImgUtil.WPFUI.Utils;
using ImgUtil.WPFUI.View;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace ImgUtil.WPFUI.ViewModel
{
    internal partial class ImgUtilViewModel : ViewModelBase
    {
        #region Fields

        private bool _isImageDirty;
        private bool _isCurrentImageExtentionMatchFormat = true;
        private ImgUtilView _imgUtilView;
        private readonly RecentFileList _recentFileList;
        private ImageService _imageService;

        private const int _defaultItemsCount = 256;
        private const string ShareAppName32 = "SafeShare32.exe";
        private const string ShareAppName64 = "SafeShare64.exe";

        private ExceptionHandlerType _handlerType = ExceptionHandlerType.Normal;

        private static Mutex recentImageFileXmlMutex = new Mutex(false, "recentImageFileXmlMutex");

        #endregion

        #region Constructors

        public ImgUtilViewModel(ImgUtilView view, ImageService imageService, Action<bool> adjustPaneCursor) : base(view.WindowHandle, adjustPaneCursor)
        {
            _imgUtilView = view;
            Dispatcher = _imgUtilView.Dispatcher;
            _recentFileList = new RecentFileList(Dispatcher);
            _imageService = imageService;
        }

        #endregion

        #region Properties for Binding

        [Obfuscation(Exclude = true)]
        public Visibility IsShowWindowsIntegration
        {
            get
            {
#if WZ_APPX
                return Visibility.Collapsed;
#else
                return Visibility.Visible;
#endif
            }
        }

        public string ImgUtilTitle
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentImageFilePath))
                {
                    return Properties.Resources.IMAGE_UTILITY_TITLE + " - " + Properties.Resources.DEFAULT_NEW_IMAGE_NAME;
                }
                else if (IsImageDirty)
                {
                    return Properties.Resources.IMAGE_UTILITY_TITLE + " - * " + _imageService.CurrentImageName;
                }
                else
                {
                    return Properties.Resources.IMAGE_UTILITY_TITLE + " - " + _imageService.CurrentImageName;
                }
            }
        }

        public bool IsTeamsInstall => ImageHelper.CheckTeamsBackgroundsFolderExist();

        public BitmapSource CurrentPreviewImage => _imageService.CurrentPreviewImage;

        public RecentFileList RecentFileList => _recentFileList;

        public bool IsCurrentImageFileExist => _imageService.HasImageOpened;

        public bool AlreadySetDefault => IntegrationViewModel.IsImgUtilDefault();

        #endregion

        #region Common Properties

        private ImgUtilView ImgUtilView => _imgUtilView;

        private string ShareAppletName => WinzipMethods.Is32Bit() ? ShareAppName32 : ShareAppName64;

        private string CurrentImageFilePath => _imageService.CurrentImageLocalPath;

        private bool IsImageDirty
        {
            get
            {
                return _isImageDirty;
            }
            set
            {
                _isImageDirty = value;
                Notify(nameof(ImgUtilTitle));
            }
        }

        private bool IsNewImage { get; set; }

        public bool IsCurrentImageExtentionMatchFormat
        {
            get
            {
                return _isCurrentImageExtentionMatchFormat;
            }
            set
            {
                if (_isCurrentImageExtentionMatchFormat != value)
                {
                    _isCurrentImageExtentionMatchFormat = value;
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => System.Windows.Input.CommandManager.InvalidateRequerySuggested()));
                }
            }
        }

        public bool IsImageChanged { get; set; }

        public override IntPtr Owner => _imgUtilView.WindowHandle;

        public WzCloudItem4? CurrentOpenedItem { get; set; }

        public WzCloudItem4? UploadedItem { get; set; }

        #endregion

        #region Public Functions

        public void DoCrop(Int32Rect rect)
        {
            _imageService.CropCurrentImage(rect);
            IsImageDirty = true;
            Notify(nameof(CurrentPreviewImage));
        }

        public void LoadRecentFilesXML()
        {
            try
            {
                recentImageFileXmlMutex.WaitOne();
                var path = ApplicationHelper.DefaultLocalUserRecentFilesPath;
                if (File.Exists(path))
                {
                    var formatter = new XmlSerializer(typeof(ObservableCollection<RecentFile>));
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, (int)stream.Length);
                        var memoryStream = new MemoryStream(buffer);
                        var oldList = _recentFileList.RecentListData;
                        _recentFileList.RecentListData = (ObservableCollection<RecentFile>)formatter.Deserialize(memoryStream);
                        foreach (var item in _recentFileList.RecentListData)
                        {
                            item.UpdateFileInfo();

                            var oldFile = oldList.FirstOrDefault(x => x.RecentFileTooltip == item.RecentFileTooltip);
                            if (oldFile != null)
                            {
                                item.UpdateThumbnail(oldFile);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            finally 
            {
               recentImageFileXmlMutex.ReleaseMutex();
            }
        }

        public void SaveRecentFilesXML()
        {
            try
            {
                recentImageFileXmlMutex.WaitOne();
                var path = ApplicationHelper.DefaultLocalUserRecentFilesPath;
                var formatter = new XmlSerializer(typeof(ObservableCollection<RecentFile>));
                using (var stream = File.Create(path))
                {
                    formatter.Serialize(stream, _recentFileList.RecentListData);
                }
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                recentImageFileXmlMutex.ReleaseMutex();
            }
        }

        // Containing both asynchronous path and synchronous path
        public async Task<bool> HandleCurrentImageStatusAsync()
        {
            bool handled = true;

            if (_imageService.HasImageOpened)
            {
                CheckResult checkResult = PopUp(CheckToSaveImage);

                switch (checkResult)
                {
                    case CheckResult.NeedSave:
                        handled = await ExecuteSaveCommandAsync();
                        break;

                    case CheckResult.NeedSaveAs:
                        handled = ExecuteSaveAsCommand();
                        break;

                    case CheckResult.DontSave:
                        handled = true;
                        break;

                    case CheckResult.Cancel:
                    default:
                        handled = false;
                        break;
                }
            }

            return handled;
        }

        public async Task<bool> DoCommandLineOperationAsync()
        {
            bool success = false;
            switch (EnvironmentService.CommandLineOperation)
            {
                case CommandLineOperation.OpenPickerFromFilePane:
                    {
                        RibbonCommands.OpenCommand.Execute(null);
                        break;
                    }

                case CommandLineOperation.OpenImageFromFilePane:
                    {
                        ImgUtilView.SetWindowLoadingStatus(true);

                        var items = EnvironmentService.GetCommandLineFilePaneItems();
                        success = await ExecuteAsync(() => Task.Run(() =>
                        {
                            try
                            {
                                return LoadImageFromWzCloudItem(items[0], 1);
                            }
                            catch
                            {
                                RefreshRecentFiles(new RecentFile(CurrentOpenedItem.Value, CurrentPreviewImage), true);
                                throw;
                            }
                        }));

                        ImgUtilView.SetWindowLoadingStatus(false);

                        if (success)
                        {
                            IsImageDirty = false;
                            IsNewImage = false;
                            RefreshRecentFiles(new RecentFile(CurrentOpenedItem.Value, CurrentPreviewImage), false);
                            TrackHelper.LogFileOpenEvent(CurrentImageFilePath);
                        }
                        break;
                    }

                case CommandLineOperation.OpenImageFromZipPane:
                    {
                        ImgUtilView.SetWindowLoadingStatus(true);

                        string sourceFile = EnvironmentService.GetCommandLineSourceFiles()[0];
                        success = await ExecuteAsync(() => Task.Run(() => LoadImage(sourceFile)));

                        ImgUtilView.SetWindowLoadingStatus(false);

                        if (success)
                        {
                            IsImageDirty = false;
                            IsNewImage = false;
                            TrackHelper.LogFileOpenEvent(CurrentImageFilePath);
                        }
                        break;
                    }

                case CommandLineOperation.ModifyImage:
                    {
                        TrackHelper.LogShellMenuEvent("modify");
                        ImgUtilView.SetWindowLoadingStatus(true);

                        string sourceFile = EnvironmentService.GetCommandLineSourceFiles()[0];
                        CurrentOpenedItem = ImageHelper.InitCloudItemFromPath(sourceFile);
                        success = await ExecuteAsync(() => Task.Run(() =>
                        {
                            try
                            {
                                return LoadImage(sourceFile);
                            }
                            catch
                            {
                                RefreshRecentFiles(new RecentFile(CurrentOpenedItem.Value, CurrentPreviewImage), true);
                                throw;
                            }
                        }));

                        ImgUtilView.SetWindowLoadingStatus(false);

                        if (success)
                        {
                            IsImageDirty = false;
                            IsNewImage = false;
                            RefreshRecentFiles(new RecentFile(CurrentOpenedItem.Value, CurrentPreviewImage), false);
                            TrackHelper.LogFileOpenEvent(CurrentImageFilePath);
                        }
                        break;
                    }

                case CommandLineOperation.CropImage:
                    {
                        TrackHelper.LogShellMenuEvent("crop");
                        ImgUtilView.SetWindowLoadingStatus(true);

                        string sourceFile = EnvironmentService.GetCommandLineSourceFiles()[0];
                        CurrentOpenedItem = ImageHelper.InitCloudItemFromPath(sourceFile);
                        success = await ExecuteAsync(() => Task.Run(() =>
                        {
                            try
                            {
                                return LoadImage(sourceFile);
                            }
                            catch
                            {
                                RefreshRecentFiles(new RecentFile(CurrentOpenedItem.Value, CurrentPreviewImage), true);
                                throw;
                            }
                        }));

                        ImgUtilView.SetWindowLoadingStatus(false);

                        if (success)
                        {
                            IsImageDirty = false;
                            IsNewImage = false;
                            RefreshRecentFiles(new RecentFile(CurrentOpenedItem.Value, CurrentPreviewImage), false);
                            RibbonCommands.CropCommand.Execute(null);
                            TrackHelper.LogFileOpenEvent(CurrentImageFilePath);
                        }

                        break;
                    }

                case CommandLineOperation.DropImageOnIcon:
                    {
                        string sourceFile = EnvironmentService.GetCommandLineSourceFiles()[0];
                        MainViewCommands.DragFromExplorerCommand.Execute(sourceFile);

                        break;
                    }

                case CommandLineOperation.None:
                default:
                    throw new NotImplementedException();
            }

            EnvironmentService.CommandLineOperation = CommandLineOperation.None;
            return success;
        }

        #endregion

        #region Private Functions

        private string PrepareImageFilePath(string sourcePath)
        {
            try
            {
                var format = ImageHelper.GetImageRealFormatFromPath(sourcePath);
                var definedExtensions = ImageHelper.GetFileExtensionsFromFormat(format);

                if (definedExtensions.Length == 0)
                {
                    // return original path if we cannot parse the image's format.
                    return sourcePath;
                }

                if (!definedExtensions.Contains(Path.GetExtension(sourcePath).ToLower()))
                {
                    string formatString = ImageHelper.GetFormatStringFromFormat(format);

                    if (string.IsNullOrEmpty(formatString))
                    {
                        // return original path if we do not support the image's format.
                        return sourcePath;
                    }

                    var result = PopUp(() => ImageHelper.AskToModifyFileExtension(formatString, Path.GetExtension(sourcePath), false, false));
                    switch (result)
                    {
                        case System.Windows.Forms.DialogResult.Yes:
                            var newPath = ImageHelper.ChangeFileExtension(sourcePath, definedExtensions[0]);
                            File.Move(sourcePath, newPath);
                            sourcePath = newPath;
                            if (CurrentOpenedItem.HasValue)
                            {
                                var removeFile = new RecentFile(CurrentOpenedItem.Value);
                                RefreshRecentFiles(removeFile, true);
                                if (ImageHelper.IsCloudItem(CurrentOpenedItem.Value.profile.Id) || ImageHelper.IsLocalPortableDeviceItem(CurrentOpenedItem.Value.profile.Id))
                                {
                                    var localItem = ImageHelper.InitCloudItemFromPath(newPath);
                                    var localItems = new WzCloudItem4[] { localItem };
                                    int count = 1;
                                    bool success = WinzipMethods.UploadToCloud(Owner, localItems, ref count, CurrentOpenedItem.Value, false, true);
                                    if (success && localItems.Length > 0)
                                    {
                                        UploadedItem = localItems[0];
                                    }
                                }
                                else if (File.Exists(CurrentOpenedItem.Value.itemId))
                                {
                                    newPath = ImageHelper.ChangeFileExtension(CurrentOpenedItem.Value.itemId, definedExtensions[0]);
                                    File.Move(CurrentOpenedItem.Value.itemId, newPath);
                                    var newItem = CurrentOpenedItem.Value;
                                    newItem.itemId = newPath;
                                    newItem.name = Path.GetFileName(newPath);
                                    newItem.path = newPath;
                                    var newFileName = Path.Combine(Path.GetDirectoryName(sourcePath), Path.GetFileName(newPath));
                                    File.Move(sourcePath, newFileName);
                                    sourcePath = newFileName;
                                    CurrentOpenedItem = newItem;
                                }
                            }
                            break;

                        case System.Windows.Forms.DialogResult.No:
                            break;

                        case System.Windows.Forms.DialogResult.Cancel:
                            return null;
                    }
                }
            }
            catch
            {
                // ignore exceptions, use original path
            }

            return sourcePath;
        }

        private bool LoadImage(string path)
        {
            path = PrepareImageFilePath(path);

            try
            {
                _imageService.LoadImage(path);
            }
            catch (OutOfMemoryException)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                throw;
            }
            catch (IOException e)
            {
                throw new ImageInUseException(e.Message, e);
            }
            catch (Exception e)
            {
                if (e is BadImageDimensionException)
                {
                    throw;
                }
                else
                {
                    throw new InvalidImageException(path, e.Message, e);
                }
            }
            finally
            {
                if(_imageService.CurrentPreviewImage == null)
                {
                    _imageService._currentImageFile = null;
                }
            }

            Notify(nameof(IsCurrentImageFileExist));
            ImgUtilView.ReCalculateSizeToFit();
            Notify(nameof(CurrentPreviewImage));
            Notify(nameof(ImgUtilTitle));
            IsCurrentImageExtentionMatchFormat = _imageService.IsCurrentImageExtensionMatchFormat;
            return true;
        }

        private bool LoadImageFromWzCloudItem(WzCloudItem4 selectedItem, int count)
        {
            CurrentOpenedItem = selectedItem;
            var tempFolder = FileOperation.Instance.CreateTempFolder();
            string imageToLoad = null;

            if (ImageHelper.IsCloudItem(selectedItem.profile.Id) || ImageHelper.IsLocalPortableDeviceItem(selectedItem.profile.Id))
            {
                var folderItem = ImageHelper.InitCloudItemFromPath(tempFolder);
                var selectedItems = new WzCloudItem4[count];
                selectedItems[0] = selectedItem;

                int downloadErrorCode = 0;
                bool success = WinzipMethods.DownloadFromCloud(Owner, selectedItems, count, folderItem, false, false, ref downloadErrorCode);
                imageToLoad = Path.Combine(tempFolder, selectedItems[0].name);

                if (!success || !File.Exists(imageToLoad))
                {
                    Directory.Delete(tempFolder, true);
                    throw new ImageNotFoundException(imageToLoad);
                }
            }
            else
            {
                if (!File.Exists(selectedItem.itemId))
                {
                    Directory.Delete(tempFolder, true);
                    throw new ImageNotFoundException(selectedItem.itemId);
                }

                imageToLoad = Path.Combine(tempFolder, Path.GetFileName(selectedItem.itemId));
                EDPHelper.FileCopy(selectedItem.itemId, imageToLoad, true);

                if (!File.Exists(imageToLoad))
                {
                    Directory.Delete(tempFolder, true);
                    throw new ImageNotFoundException(selectedItem.itemId);
                }
            }

            return LoadImage(imageToLoad);
        }

        private void RefreshRecentFiles(RecentFile file, bool doRemove)
        {
            LoadRecentFilesXML();
            if (!doRemove)
            {
                RecentFileList.TryPush(file);
            }
            else
            {
                RecentFileList.TryRemove(file);
            }
            SaveRecentFilesXML();
        }

        #endregion

        #region Exception Handlers

        protected override bool HandleException(Exception ex)
        {
            ImgUtilView.SetWindowLoadingStatus(false);

            if (_handlerType == ExceptionHandlerType.FileOperation)
            {
                bool handled = FileOperation.HandleFileException(ex, Owner);
                _handlerType = ExceptionHandlerType.Normal;
                if (handled)
                {
                    return true;
                }
            }

            if (ex is InvalidImageException invalidImageException)
            {
                FlatMessageWindows.DisplayWarningMessage(Owner, string.Format(Properties.Resources.WARNING_INVALID_FILE, Path.GetFileName(invalidImageException.ImagePath)));
                return true;
            }
            else if (ex is ImageNotFoundException imageNotFoundException)
            {
                FlatMessageWindows.DisplayWarningMessage(Owner, string.Format(Properties.Resources.FILE_NOT_FOUND, Path.GetFileName(imageNotFoundException.ImagePath)));
                return true;
            }
            else if (ex is AsposeBadException asposeBadException)
            {
                FlatMessageWindows.DisplayWarningMessage(Owner, asposeBadException.Message);
                return true;
            }
            else if (ex is BadImageDimensionException badImageDimensionException)
            {
                FlatMessageWindows.DisplayWarningMessage(Owner, string.Format(Properties.Resources.WARNING_BAD_IMAGE_DIMENSION, badImageDimensionException.Width, badImageDimensionException.Height));
                return true;
            }
            else if (ex is ImageReadOnlyException imageReadOnlyException)
            {
                FlatMessageWindows.DisplayWarningMessage(Owner, Properties.Resources.IMAGE_IS_READ_ONLY);
                return true;
            }
            else if (ex is ImageNotSupportException imageNotSupportException)
            {
                FlatMessageWindows.DisplayWarningMessage(Owner, string.Format(Properties.Resources.WARNING_UNSUPPORTED_FILE, Path.GetFileName(imageNotSupportException.ImagePath)));
                return true;
            }
            else if (ex is NotCalledByWinZipException notCalledByWinZipException)
            {
                FlatMessageWindows.DisplayWarningMessage(Owner, Properties.Resources.EXECUTE_SAVE_TO_ZIP_INDEPENDENTLY_TIPS);
                return true;
            }
            else if (ex is NoOpenedImageException noOpenedImageException)
            {
                FlatMessageWindows.DisplayWarningMessage(Owner, Properties.Resources.ERROR_NO_OPENED_IMAGE);
                return true;
            }
            else if (ex is OperationNotSupportException operationNotSupportException)
            {
                FlatMessageWindows.DisplayErrorMessage(Owner, Properties.Resources.ERROR_ACTION_NOT_SUPPORT);
                return true;
            }
            else if (ex is ImageInUseException imageInUseException)
            {
                FlatMessageWindows.DisplayWarningMessage(Owner, Properties.Resources.FILE_IN_USE_WARNING);
                return false;
            }
            else
            {
                FlatMessageWindows.DisplayWarningMessage(Owner, ex.Message);
                return true;
            }
        }

        #endregion
    }
}
