using ImgUtil.Util;
using ImgUtil.WPFUI.Commands;
using ImgUtil.WPFUI.Controls;
using ImgUtil.WPFUI.Model;
using ImgUtil.WPFUI.Utils;
using ImgUtil.WPFUI.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImgUtil.WPFUI.ViewModel
{
    internal enum CheckResult
    {
        NeedSave,
        NeedSaveAs,
        DontSave,
        Cancel
    }

    internal partial class ImgUtilViewModel
    {
        #region Fields for Commands

        private RibbonCommandModel _ribbonCommands;
        private MainViewCommandModel _mainViewCommands;

        #endregion

        #region Properties for Commands

        public RibbonCommandModel RibbonCommands => _ribbonCommands ?? (_ribbonCommands = new RibbonCommandModel(this));

        public MainViewCommandModel MainViewCommands => _mainViewCommands ?? (_mainViewCommands = new MainViewCommandModel(this));

        #endregion

        #region Execute Ribbon Commands

        public async Task<bool> ExecuteCreateFromCommandAsync()
        {
            bool success = false;
            bool canContinue = await HandleCurrentImageStatusAsync();

            if (canContinue)
            {
                ImgUtilView.SetWindowLoadingStatus(true);

                success = await Task.Run(ExecuteCreateFrom);

                ImgUtilView.SetWindowLoadingStatus(false);
            }

            return success;
        }

        private bool ExecuteCreateFrom()
        {
            var defaultFolder = ImageHelper.GetOpenPickerDefaultFolder();
            int count = _defaultItemsCount;
            var selectedItems = new WzCloudItem4[count];

            bool success = WinzipMethods.FileSelection(Owner, Properties.Resources.CREATE_FROM_IMAGE_FILE, Properties.Resources.CREATE_FROM_PICKER_BUTTON,
                    Properties.Resources.OPEN_PICKER_FILTER, defaultFolder, selectedItems, ref count, false, true, true, true, false, false);

            if (success)
            {
                bool isCloudItem = ImageHelper.IsCloudItem(selectedItems[0].profile.Id);
                bool isLocalPortableDeviceItem = ImageHelper.IsLocalPortableDeviceItem(selectedItems[0].profile.Id);

                if (!isCloudItem && !isLocalPortableDeviceItem)
                {
                    try
                    {
                        selectedItems = FileOperation.FilterUnreadableFilesWithExceptionThrown(selectedItems, ref count, Owner);
                    }
                    catch
                    {
                        _handlerType = ExceptionHandlerType.FileOperation;
                        throw;
                    }

                    if (count == 0)
                    {
                        return false;
                    }
                }

                ImageHelper.SetOpenPickerDefaultFolder(selectedItems[0]);
                ImageHelper.SetSavePickerDefaultFolder(selectedItems[0]);

                // New.xxx
                var newName = Properties.Resources.NEW_NAME;
                var tempFolder = FileOperation.Instance.CreateTempFolder();
                string imagePath = null;

                if (isCloudItem || isLocalPortableDeviceItem)
                {
                    var folderItem = ImageHelper.InitCloudItemFromPath(tempFolder);

                    int downloadErrorCode = 0;
                    success = WinzipMethods.DownloadFromCloud(Owner, selectedItems, count, folderItem, false, false, ref downloadErrorCode);

                    if (!success)
                    {
                        Directory.Delete(tempFolder, true);
                        return false;
                    }

                    newName += Path.GetExtension(selectedItems[0].name);
                    var downloadPath = Path.Combine(tempFolder, selectedItems[0].name);
                    imagePath = Path.Combine(tempFolder, newName);
                    try
                    {
                        if (!downloadPath.Equals(imagePath))
                        {
                            EDPHelper.FileCopy(downloadPath, imagePath, true);
                        }

                        File.SetAttributes(imagePath, FileAttributes.Normal);
                    }
                    catch
                    {
                        Directory.Delete(tempFolder, true);
                        throw;
                    }
                }
                else
                {
                    newName += Path.GetExtension(selectedItems[0].itemId);
                    imagePath = Path.Combine(tempFolder, newName);
                    try
                    {
                        EDPHelper.FileCopy(selectedItems[0].itemId, imagePath, true);
                        File.SetAttributes(imagePath, FileAttributes.Normal);
                    }
                    catch
                    {
                        Directory.Delete(tempFolder, true);
                        throw;
                    }
                }

                CurrentOpenedItem = null;

                success = LoadImage(imagePath);
                if (success)
                {
                    IsNewImage = true;
                    IsImageDirty = true;
                    IsImageChanged = false;
                    TrackHelper.LogFileImportEvent(TrackHelper.ImportWay.ImageWay, imagePath);
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> ExecuteOpenCommandAsync()
        {
            bool success = false;
            bool canContinue = await HandleCurrentImageStatusAsync();

            if (canContinue)
            {
                ImgUtilView.SetWindowLoadingStatus(true);

                success = await Task.Run(ExecuteOpen);

                ImgUtilView.SetWindowLoadingStatus(false);
            }

            return success;
        }

        private bool ExecuteOpen()
        {
            var defaultFolder = ImageHelper.GetOpenPickerDefaultFolder();
            int count = _defaultItemsCount;
            var selectedItems = new WzCloudItem4[count];

            bool success = WinzipMethods.FileSelection(Owner, Properties.Resources.OPEN, Properties.Resources.OPEN_PICKER_BUTTON,
                    Properties.Resources.OPEN_PICKER_FILTER, defaultFolder, selectedItems, ref count, false, true, true, true, false, false);

            if (success)
            {
                if (!ImageHelper.IsCloudItem(selectedItems[0].profile.Id) && !ImageHelper.IsLocalPortableDeviceItem(selectedItems[0].profile.Id))
                {
                    try
                    {
                        selectedItems = FileOperation.FilterUnreadableFilesWithExceptionThrown(selectedItems, ref count, Owner);
                    }
                    catch
                    {
                        _handlerType = ExceptionHandlerType.FileOperation;
                        throw;
                    }

                    if (count == 0)
                    {
                        return false;
                    }
                }

                ImageHelper.SetOpenPickerDefaultFolder(selectedItems[0]);
                ImageHelper.SetSavePickerDefaultFolder(selectedItems[0]);

                try
                {
                    success = LoadImageFromWzCloudItem(selectedItems[0], count);
                }
                catch
                {
                    RefreshRecentFiles(new RecentFile(CurrentOpenedItem.Value, CurrentPreviewImage), true);
                    throw;
                }

                if (success)
                {
                    IsNewImage = false;
                    IsImageDirty = false;
                    IsImageChanged = false;
                    TrackHelper.LogFileOpenEvent(CurrentImageFilePath);
                    RefreshRecentFiles(new RecentFile(CurrentOpenedItem.Value, CurrentPreviewImage), false);
                }
            }

            return success;
        }

        public async Task<bool> ExecuteOpenRecentCommandAsync(object parameter)
        {
            bool success = false;
            bool canContinue = await HandleCurrentImageStatusAsync();

            if (canContinue)
            {
                ImgUtilView.SetWindowLoadingStatus(true);

                success = await Task.Run(() => ExecuteOpenRecent(parameter as RecentFile));

                ImgUtilView.SetWindowLoadingStatus(false);
            }

            return success;
        }

        private bool ExecuteOpenRecent(RecentFile recentFile)
        {
            // recent file is null means it is invoked by the framework.
            // so use the first(default) item in the recent file list if it has.
            bool useDefaultRecentFile = false;
            if (recentFile == null)
            {
                if (RecentFileList.Count > 0)
                {
                    recentFile = RecentFileList[0];
                    useDefaultRecentFile = true;
                }
                else
                {
                    return false;
                }
            }

            bool success = false;
            try
            {
                success = LoadImageFromWzCloudItem(recentFile.FileItem, 1);
            }
            catch
            {
                RefreshRecentFiles(new RecentFile(CurrentOpenedItem.Value, CurrentPreviewImage), true);
                throw;
            }

            if (success)
            {
                IsNewImage = false;
                IsImageDirty = false;
                IsImageChanged = false;
                if (!useDefaultRecentFile)
                {
                    ImageHelper.SetOpenPickerDefaultFolder(recentFile.FileItem);
                    RefreshRecentFiles(new RecentFile(CurrentOpenedItem.Value, CurrentPreviewImage), false);
                }
                TrackHelper.LogFileOpenEvent(CurrentImageFilePath);
            }

            return success;
        }

        public async Task<bool> ExecuteOpenWithCommandAsync()
        {
            bool canExecuteOpenWith = false;

            // If no image is opened, let the user pick one
            if (!_imageService.HasImageOpened)
            {
                canExecuteOpenWith = await ExecuteOpenCommandAsync();
            }
            // If have image opened, check if need to save changes
            else
            {
                CheckResult checkResult = PopUp(CheckToSaveBeforeOpenWith);

                switch (checkResult)
                {
                    case CheckResult.NeedSave:
                        canExecuteOpenWith = await ExecuteSaveCommandAsync();
                        break;

                    case CheckResult.NeedSaveAs:
                        canExecuteOpenWith = ExecuteSaveAsCommand();
                        break;

                    case CheckResult.DontSave:
                        canExecuteOpenWith = true;
                        break;

                    case CheckResult.Cancel:
                    default:
                        canExecuteOpenWith = false;
                        break;
                }
            }

            return canExecuteOpenWith ? await Task.Run(ExecuteOpenWith) : false;
        }

        private bool ExecuteOpenWith()
        {
            var currentItem = RecentFileList.Count == 0 ? null : RecentFileList[0];

            if (currentItem != null)
            {
                if (Environment.OSVersion.Version.Major > 5)
                {
                    var openAsInfo = new NativeMethods.OPENASINFO
                    {
                        cszFile = currentItem.FileItem.itemId,
                        cszClass = string.Empty,
                        oaifInFlags = NativeMethods.OPEN_AS_INFO_FLAGS.OAIF_ALLOW_REGISTRATION | NativeMethods.OPEN_AS_INFO_FLAGS.OAIF_EXEC
                    };

                    return NativeMethods.SHOpenWithDialog(Owner, ref openAsInfo) == 0;
                }
                else
                {
                    var sei = new NativeMethods.ShellExecuteInfo();
                    sei.Size = System.Runtime.InteropServices.Marshal.SizeOf(sei);
                    sei.Verb = "openas";
                    sei.Mask = NativeMethods.SEE_MASK_INVOKEIDLIST;
                    sei.File = currentItem.FileItem.itemId;
                    sei.Show = NativeMethods.SW_NORMAL;
                    sei.hwnd = Owner;
                    return NativeMethods.ShellExecuteEx(ref sei);
                }
            }

            return false;
        }

        public async Task<bool> ExecutePrintCommandAsync()
        {
            if (!_imageService.HasImageOpened)
            {
                bool success = await ExecuteOpenCommandAsync();
                if (!success)
                {
                    return false;
                }
            }

            var printDlg = new System.Windows.Controls.PrintDialog();
            if (printDlg.ShowDialog() == true)
            {
                var dv = new System.Windows.Media.DrawingVisual();
                using (var dc = dv.RenderOpen())
                {
                    double imgWidth = CurrentPreviewImage.Width;
                    double imgHeight = CurrentPreviewImage.Height;
                    Rect printRect;

                    if (imgWidth <= printDlg.PrintableAreaWidth && imgHeight <= printDlg.PrintableAreaHeight)
                    {
                        printRect = new Rect((printDlg.PrintableAreaWidth - imgWidth) / 2, (printDlg.PrintableAreaHeight - imgHeight) / 2, imgWidth, imgHeight);
                    }
                    else if (imgWidth <= printDlg.PrintableAreaWidth && imgHeight > printDlg.PrintableAreaHeight)
                    {
                        double ratio = printDlg.PrintableAreaHeight / imgHeight;
                        printRect = new Rect((printDlg.PrintableAreaWidth - imgWidth * ratio) / 2, 0, imgWidth * ratio, imgHeight * ratio);
                    }
                    else if (imgWidth > printDlg.PrintableAreaWidth && imgHeight <= printDlg.PrintableAreaHeight)
                    {
                        double ratio = printDlg.PrintableAreaWidth / imgWidth;
                        printRect = new Rect(0, (printDlg.PrintableAreaHeight - imgHeight * ratio) / 2, imgWidth * ratio, imgHeight * ratio);
                    }
                    else
                    {
                        double ratio = Math.Min(printDlg.PrintableAreaWidth / imgWidth, printDlg.PrintableAreaHeight / imgHeight);
                        double newWidth = imgWidth * ratio;
                        double newHeight = imgHeight * ratio;
                        printRect = new Rect((printDlg.PrintableAreaWidth - newWidth) / 2, (printDlg.PrintableAreaHeight - newHeight) / 2, newWidth, newHeight);
                    }

                    // Create a new bitmapsource in current thread for use
                    var bitmapSource = new CroppedBitmap(CurrentPreviewImage, new Int32Rect(0, 0, CurrentPreviewImage.PixelWidth, CurrentPreviewImage.PixelHeight));
                    dc.DrawImage(bitmapSource, printRect);
                }
                printDlg.PrintVisual(dv, _imageService.CurrentImageName);
            }

            return true;
        }

        public async Task<bool> ExecuteSaveCommandAsync()
        {
            TrackHelper.SendImgToolsEvent();
            TrackHelper.SendImgViewerEvent();

            bool success = false;
            if (!_imageService.HasImageOpened)
            {
                throw new NoOpenedImageException();
            }
            else if (!IsImageDirty)
            {
                return true;
            }
            else if (EnvironmentService.IsCalledByWinZipZipPane)
            {
                success = await ExecuteSaveToZipCommandAsync();
                if (success)
                {
                    IsNewImage = false;
                    IsImageDirty = false;
                }
                return success;
            }

            if (IsNewImage)
            {
                success = ExecuteSaveAs();
            }
            else
            {
                success = await Task.Run(ExecuteSave);
            }

            if (success && !IsNewImage)
            {
                // update recent files date info after save to local
                RecentFileList[0].UpdateFileInfo();
                RecentFileList[0].UpdateThumbnail(CurrentPreviewImage);
            }

            return success;
        }

        private bool ExecuteSave()
        {
            var currentItem = RecentFileList.Count == 0 ? null : RecentFileList[0];
            if (currentItem != null)
            {
                if (currentItem.IsCloudItem)
                {
                    string fileToUpload = _imageService.SaveCurrentImageToTempFolder();
                    bool success = PopUp(() => DoUploadCloudItem(fileToUpload, currentItem.FileItem, true));

                    if (!success)
                    {
                        return false;
                    }

                    success = LoadImage(fileToUpload);

                    if (!success)
                    {
                        return false;
                    }

                    RefreshRecentFiles(new RecentFile(UploadedItem.Value, CurrentPreviewImage), false);
                }
                else
                {
                    _imageService.SaveCurrentImage(currentItem.FileItem.itemId);
                }

                IsImageDirty = false;
                IsNewImage = false;
                IsImageChanged = true;

                return true;
            }

            return false;
        }

        public bool ExecuteSaveAsCommand()
        {
            if (!_imageService.HasImageOpened)
            {
                if (!ExecuteOpen())
                {
                    return false;
                }
            }
            return ExecuteSaveAs();
        }

        private bool ExecuteSaveAs()
        {
            // Step 1 : Get the destnation location from Save As Picker
            var defaultFolder = ImageHelper.GetSavePickerDefaultFolder();
            var destItem = ImageHelper.InitWzCloudItem();
            destItem.profile.Id = WzSvcProviderIDs.SPID_UNKNOWN;
            bool success = WinzipMethods.SaveAsDialog(Owner, Properties.Resources.SAVE_TO_PC_OR_CLOUD, Properties.Resources.SAVE_AS, _imageService.CurrentImageName, NativeMethods.SaveAsPickerFilterDic[_imageService.CurrentImageFormat], defaultFolder, ref destItem);

            if (!success)
            {
                return false;
            }

            // Step 2 : Save to the right destnation.
            ImageHelper.SetSavePickerDefaultFolder(destItem);
            string destFilePath = Path.Combine(destItem.parentId, destItem.name);
            string destFileExt = Path.GetExtension(destItem.name).ToLower();

            bool SaveToCloud = ImageHelper.IsCloudItem(destItem.profile.Id);
            bool SaveToZip = destFileExt == ".zip" || destFileExt == ".zipx";
            bool NeedTransfrom = destFileExt != Path.GetExtension(_imageService.CurrentImageName).ToLower();

            string imageToReload = string.Empty;

            success = false;

            // There are 6 cases.
            if (SaveToCloud)
            {
                string tempFolder = FileOperation.Instance.CreateTempFolder();
                string fileToUpload;
                // Case 1 : Save to zip(x) ---> upload to cloud
                // No need to reload
                if (SaveToZip)
                {
                    fileToUpload = Path.Combine(tempFolder, destItem.name);
                    success = SaveCurrentImageToZip(fileToUpload);
                }
                // Case 2 : Save with a different file format ---> upload to cloud
                else if (NeedTransfrom)
                {
                    fileToUpload = Path.Combine(tempFolder, destItem.name);
                    success = _imageService.SaveCurrentImageToDifferentFormat(fileToUpload);
                    imageToReload = fileToUpload;
                }
                // Case 3 : Save with the same file format ---> upload to cloud
                else
                {
                    fileToUpload = _imageService.SaveCurrentImageToTempFolder();
                    imageToReload = fileToUpload;
                    success = File.Exists(imageToReload);
                }

                if (success)
                {
                    success = DoUploadCloudItem(fileToUpload, destItem, false);
                }
            }
            else
            {
                // Case 4 : Save to zip(x)
                if (SaveToZip)
                {
                    success = SaveCurrentImageToZip(destFilePath);
                }
                // Case 5 : Save with a different file format
                else if (NeedTransfrom)
                {
                    success = _imageService.SaveCurrentImageToDifferentFormat(destFilePath);
                    imageToReload = destFilePath;
                }
                // Case 6 : Save with the same file format
                else
                {
                    _imageService.SaveCurrentImage(destFilePath);
                    imageToReload = destFilePath;
                    success = true;
                }
            }

            if (!success)
            {
                return false;
            }

            // Step 3 : Load changed image if needed, change status and refresh recent file list.
            if (!string.IsNullOrEmpty(imageToReload))
            {
                string tempFolder = FileOperation.Instance.CreateTempFolder();
                string fileName = Path.GetFileName(imageToReload);
                string imagePath = Path.Combine(tempFolder, fileName);
                EDPHelper.FileCopy(imageToReload, imagePath, true);
                success = LoadImage(imagePath);

                if (success)
                {
                    IsImageDirty = false;
                    IsNewImage = false;
                    IsImageChanged = true;

                    if (SaveToCloud)
                    {
                        RefreshRecentFiles(new RecentFile(UploadedItem.Value, CurrentPreviewImage), false);
                    }
                    else
                    {
                        destItem.itemId = imageToReload;
                        CurrentOpenedItem = destItem;
                        RefreshRecentFiles(new RecentFile(destItem, CurrentPreviewImage), false);
                    }
                }
            }

            TrackHelper.SendImgToolsEvent();
            TrackHelper.SendImgViewerEvent();
            TrackHelper.LogFileSaveAsEvent(destFileExt);

            return success;
        }

        public async Task<bool> ExecuteSaveToZipCommandAsync()
        {
            if (!EnvironmentService.IsCalledByWinZip)
            {
                throw new NotCalledByWinZipException();
            }

            bool success = false;
            if (!_imageService.HasImageOpened)
            {
                success = await ExecuteOpenCommandAsync();
                if (!success)
                {
                    return false;
                }
            }

            string filename = _imageService.CurrentImageName;
            if (IsNewImage)
            {
                filename = new NameNewImageDialog(Path.GetExtension(_imageService.CurrentImageName).ToLower()).RunDialog();
                if (string.IsNullOrEmpty(filename))
                {
                    return false;
                }
            }

            success = await Task.Run(() => ExecuteSaveToZip(filename));

            return success;
        }

        private bool ExecuteSaveToZip(string filename)
        {
            var fileInfo = new FileInfo(_imageService.CurrentImageLocalPath);
            if (fileInfo.IsReadOnly)
            {
                throw new ImageReadOnlyException(_imageService.CurrentImageLocalPath);
            }

            if (!EnvironmentService.IsCalledByWinZip)
            {
                throw new NotCalledByWinZipException();
            }

            bool success = false;
            string imageToLoad = string.Empty;
            try
            {
                // Step 1 : Check if current archive is read only
                string parameter = "CheckCurrentArchiveIsReadOnly";
                parameter += "\t";
                var bytes = Encoding.Unicode.GetBytes(parameter);

                EnvironmentService.PipeServer.Flush();
                EnvironmentService.PipeServer.Write(bytes, 0, bytes.Length);

                byte[] resByte = new byte[10];

                EnvironmentService.PipeServer.Read(resByte, 0, 10);

                string resString = Encoding.Unicode.GetString(resByte);
                bool isReadOnly = int.Parse(resString) != 0;

                if (isReadOnly)
                {
                    return false;
                }

                // Step 2 : Prepare current image
                string tempFolder = FileOperation.Instance.CreateTempFolder();
                string zipItemPath = Path.Combine(tempFolder, filename);

                zipItemPath = _imageService.SaveCurrentImageToTempFolder(zipItemPath);

                if (!File.Exists(zipItemPath))
                {
                    return false;
                }

                // Step 3 : Save current image to zip file
                parameter = ImgUtilView.WindowHandle.ToInt64().ToString();
                parameter = parameter + "\t" + zipItemPath;
                bytes = Encoding.Unicode.GetBytes(parameter);

                EnvironmentService.PipeServer.Flush();
                EnvironmentService.PipeServer.Write(bytes, 0, bytes.Length);

                resByte = new byte[10];

                EnvironmentService.PipeServer.Read(resByte, 0, 10);

                resString = Encoding.Unicode.GetString(resByte);
                success = int.Parse(resString) != 0;

                imageToLoad = zipItemPath;
            }
            catch (ObjectDisposedException)
            {
                ImgUtilView.Close();
                throw;
            }

            if (success)
            {
                success = LoadImage(imageToLoad);
            }

            return success;
        }

        public async Task<bool> ExecuteShareCommandAsync()
        {
            bool success = false;
            if (!_imageService.HasImageOpened)
            {
                success = await ExecuteOpenCommandAsync();
                if (!success)
                {
                    return false;
                }
            }

            string filename = _imageService.CurrentImageName;
            if (IsNewImage)
            {
                filename = new NameNewImageDialog(Path.GetExtension(_imageService.CurrentImageName).ToLower()).RunDialog();
                if (string.IsNullOrEmpty(filename))
                {
                    return false;
                }
            }

            success = await Task.Run(() => ExecuteShare(filename));

            return success;
        }

        private bool ExecuteShare(string filename)
        {
            var tempFolderName = FileOperation.Instance.CreateTempFolder();
            Directory.CreateDirectory(tempFolderName);
            var tempFilePath = Path.Combine(tempFolderName, filename);

            _imageService.SaveCurrentImage(tempFilePath);

            if (File.Exists(tempFilePath))
            {
                string callfrom = "-cimgutil";
                Process current = Process.GetCurrentProcess();
                string filepath = string.Format("&\"{0}\"", tempFilePath);
                string cmd = string.Format("-show {0} -h:{1} /processid {2} {3} -cmd:/notrk", callfrom, Owner.ToInt32(), current.Id, filepath);

                string shareAppletFilePath = string.Empty;
                string winzipPath = RegeditOperation.GetWinZipExePath();
                if (!string.IsNullOrEmpty(winzipPath))
                {
                    shareAppletFilePath = Path.Combine(winzipPath, ShareAppletName);
                }

                if (!File.Exists(shareAppletFilePath))
                {
                    winzipPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    shareAppletFilePath = Path.Combine(winzipPath, ShareAppletName);
                }

                if (!File.Exists(shareAppletFilePath))
                {
                    shareAppletFilePath = ShareAppletName;
                }

                Process.Start(shareAppletFilePath, cmd);

                TrackHelper.LogFileShareEvent(filename);

                return true;
            }

            return false;
        }

        public async Task<bool> ExecuteImgSettingsCommandAsync()
        {
            bool success = await Task.Run(ExecuteImgSettings);
            return success;
        }

        private bool ExecuteImgSettings()
        {
            var spids = new int[] { (int)WzSvcProviderIDs.SPID_CONVERTPHOTOS_TRANSFORM, (int)WzSvcProviderIDs.SPID_ENLARGE_REDUCE_IMAGE, (int)WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM, -1 };
            return WinzipMethods.ShowConversionSettings(Owner, spids);
        }

        public bool ExecuteWindowsIntegrationCommand()
        {
            bool canAddDesktopIcon = false;
            bool canAddStartMenu = false;
            int addFileAssociation = 0;
            RegeditOperation.GetWinzipPolicyIntegrationRegistryValue(ref canAddDesktopIcon, ref canAddStartMenu, ref addFileAssociation);

            if (!canAddDesktopIcon && !canAddStartMenu && addFileAssociation == 0)
            {
                // integration change is disabled by administrator
                FlatMessageWindows.DisplayInformationMessage(ImgUtilView.WindowHandle, Properties.Resources.INTEGRATION_DISABLE_BY_ADMIN);
            }
            else
            {
                var dialog = new IntegrationView(canAddDesktopIcon, canAddStartMenu, addFileAssociation);
                dialog.InitDataContext();
                if (dialog.ShowDialog() == true)
                {
                    bool needAdminPriority = false;
                    if (dialog.HasChanges(ref needAdminPriority))
                    {
                        if (needAdminPriority)
                        {
                            StartAdminProgressAndChangeIntegration();
                        }
                        else
                        {
                            ChangeIntegrationWithoutAdmin();
                        }
                        RegeditOperation.DeleteAdminConfigRegistryStringValue();
                    }
                }
            }

            return true;
        }

        public async Task<bool> ExecuteCloseCommandAsync()
        {
            if (!_imageService.HasImageOpened)
            {
                throw new NoOpenedImageException();
            }

            bool success = false;
            bool canContinue = await HandleCurrentImageStatusAsync();

            if (canContinue)
            {
                ClearCurrentImage();
                success = true;
            }

            return success;
        }

        public bool ExecuteExitCommand()
        {
            ImgUtilView.Close();
            return true;
        }

        public async Task<bool> ExecuteImportImageCommandAsync()
        {
            bool success = false;
            bool canContinue = await HandleCurrentImageStatusAsync();

            if (canContinue)
            {
                ImgUtilView.SetWindowLoadingStatus(true);

                success = await Task.Run(ExecuteImportImage);

                ImgUtilView.SetWindowLoadingStatus(false);
            }

            return success;
        }

        private bool ExecuteImportImage()
        {
            var defaultFolder = ImageHelper.GetOpenPickerDefaultFolder();
            int count = _defaultItemsCount;
            var selectedItems = new WzCloudItem4[count];

            bool success = false;

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                success = WinzipMethods.FileSelection(Owner, Properties.Resources.IMPORT_IMAGE, Properties.Resources.IMPORT,
                    Properties.Resources.OPEN_PICKER_FILTER, defaultFolder, selectedItems, ref count, false, true, true, true, false, false);
            }));

            if (success)
            {
                bool isCloudItem = ImageHelper.IsCloudItem(selectedItems[0].profile.Id);
                bool isLocalPortableDeviceItem = ImageHelper.IsLocalPortableDeviceItem(selectedItems[0].profile.Id);

                if (!isCloudItem && !isLocalPortableDeviceItem)
                {
                    try
                    {
                        selectedItems = FileOperation.FilterUnreadableFilesWithExceptionThrown(selectedItems, ref count, Owner);
                    }
                    catch
                    {
                        _handlerType = ExceptionHandlerType.FileOperation;
                        throw;
                    }

                    if (count == 0)
                    {
                        return false;
                    }
                }

                ImageHelper.SetOpenPickerDefaultFolder(selectedItems[0]);

                // New.xxx
                string newName = Properties.Resources.NEW_NAME;
                string tempFolder = FileOperation.Instance.CreateTempFolder();
                string imagePath = null;

                if (isCloudItem || isLocalPortableDeviceItem)
                {
                    var folderItem = ImageHelper.InitCloudItemFromPath(tempFolder);

                    int downloadErrorCode = 0;
                    success = WinzipMethods.DownloadFromCloud(Owner, selectedItems, count, folderItem, false, false, ref downloadErrorCode);

                    if (!success)
                    {
                        Directory.Delete(tempFolder, true);
                        return false;
                    }

                    newName += Path.GetExtension(selectedItems[0].name);
                    string downloadPath = Path.Combine(tempFolder, selectedItems[0].name);
                    imagePath = Path.Combine(tempFolder, newName);

                    try
                    {
                        EDPHelper.FileCopy(downloadPath, imagePath, true);
                    }
                    catch
                    {
                        Directory.Delete(tempFolder, true);
                        throw;
                    }
                }
                else
                {
                    newName += Path.GetExtension(selectedItems[0].itemId);
                    imagePath = Path.Combine(tempFolder, newName);
                    try
                    {
                        EDPHelper.FileCopy(selectedItems[0].itemId, imagePath, true);
                    }
                    catch
                    {
                        Directory.Delete(tempFolder, true);
                        throw;
                    }
                }

                CurrentOpenedItem = null;
                success = LoadImage(imagePath);
                if (success)
                {
                    IsNewImage = true;
                    IsImageDirty = true;
                    IsImageChanged = false;
                    TrackHelper.LogFileImportEvent(TrackHelper.ImportWay.ImageWay, imagePath);
                }
                else
                {
                    Directory.Delete(tempFolder, true);
                }
            }

            return success;
        }

        public async Task<bool> ExecuteImportFromCameraCommandAsync()
        {
            bool success = false;
            bool canContinue = await HandleCurrentImageStatusAsync();

            if (canContinue)
            {
                ImgUtilView.SetWindowLoadingStatus(true);

                success = await Task.Run(ExecuteImportFromCamera);

                ImgUtilView.SetWindowLoadingStatus(false);
            }

            return success;
        }

        private bool ExecuteImportFromCamera()
        {
            string newName = Properties.Resources.NEW_NAME;
            string destFolder = FileOperation.Instance.CreateTempFolder();
            bool success = WinzipMethods.ImportFromCamera(Owner, destFolder);

            if (success)
            {
                var dir = new DirectoryInfo(destFolder);
                var fileList = new List<string>();
                RecursiveDir(dir, ref fileList);

                newName += Path.GetExtension(fileList[0]);
                string imagePath = Path.Combine(FileOperation.Instance.CreateTempFolder(), newName);
                try
                {
                    EDPHelper.FileCopy(fileList[0], imagePath, true);
                }
                catch (Exception)
                {
                    imagePath = fileList[0];
                }

                CurrentOpenedItem = null;
                success = LoadImage(imagePath);
                if (success)
                {
                    IsNewImage = true;
                    IsImageDirty = true;
                    IsImageChanged = false;
                    TrackHelper.LogFileImportEvent(TrackHelper.ImportWay.CameraWay);
                }
            }

            return success;
        }

        public async Task<bool> ExecuteImportFromScannerCommandAsync()
        {
            bool success = false;
            bool canContinue = await HandleCurrentImageStatusAsync();

            if (canContinue)
            {
                ImgUtilView.SetWindowLoadingStatus(true);

                success = await Task.Run(ExecuteImportFromScanner);

                ImgUtilView.SetWindowLoadingStatus(false);
            }

            return success;
        }

        private bool ExecuteImportFromScanner()
        {
            // New.xxx
            string newName = Properties.Resources.NEW_NAME;
            string destFolder = FileOperation.Instance.CreateTempFolder();
            bool succees = WinzipMethods.ImportFromScanner(Owner, destFolder);

            if (succees)
            {
                DirectoryInfo dir = new DirectoryInfo(destFolder);
                List<string> fileList = new List<string>();
                RecursiveDir(dir, ref fileList);

                newName += Path.GetExtension(fileList[0]);
                string imagePath = Path.Combine(FileOperation.Instance.CreateTempFolder(), newName);
                try
                {
                    EDPHelper.FileCopy(fileList[0], imagePath, true);
                }
                catch (Exception)
                {
                    imagePath = fileList[0];
                }

                CurrentOpenedItem = null;
                succees = LoadImage(imagePath);
                if (succees)
                {
                    IsNewImage = true;
                    IsImageDirty = true;
                    IsImageChanged = false;
                    TrackHelper.LogFileImportEvent(TrackHelper.ImportWay.ScannerWay);
                }
            }

            return succees;
        }

        public async Task<bool> ExecuteCopyCommandAsync()
        {
            if (!_imageService.HasImageOpened)
            {
                bool success = await ExecuteOpenCommandAsync();
                if (!success)
                {
                    return false;
                }
            }

            if (!_imageService.CheckImageSupportCapatility(ImageCapability.SupportConvert))
            {
                throw new OperationNotSupportException();
            }

            Clipboard.SetImage(CurrentPreviewImage);
            TrackHelper.LogImgToolsEvent("copy", string.Empty);

            return true;
        }

        public async Task<bool> ExecuteConvertToCommandAsync()
        {
            bool success = false;
            if (!_imageService.HasImageOpened)
            {
                success = await ExecuteOpenCommandAsync();
                if (!success)
                {
                    return false;
                }
            }

            success = await TransformDisplayAndLoadAsync(ImageCapability.SupportConvert, WzSvcProviderIDs.SPID_CONVERTPHOTOS_TRANSFORM);

            if (success)
            {
                IsNewImage = true;
                IsImageDirty = true;
            }

            return success;
        }

        public async Task<bool> ExecuteCropCommandAsync()
        {
            try
            {
                if (!_imageService.HasImageOpened)
                {
                    bool success = await ExecuteAsync(ExecuteOpenCommandAsync);
                    if (!success)
                    {
                        return false;
                    }
                }

                var fileInfo = new FileInfo(_imageService.CurrentImageLocalPath);
                if (fileInfo.IsReadOnly)
                {
                    throw new ImageReadOnlyException(_imageService.CurrentImageLocalPath);
                }

                if (!_imageService.CheckImageSupportCapatility(ImageCapability.SupportCrop))
                {
                    throw new OperationNotSupportException();
                }

                ImgUtilView.AddCroppingAdorner();
                return true;
            }
            catch (Exception e)
            {
                HandleException(e);
                return false;
            }
        }

        public async Task<bool> ExecuteRemovePersonalDataCommandAsync()
        {
            bool success = false;
            if (!_imageService.HasImageOpened)
            {
                success = await ExecuteOpenCommandAsync();
                if (!success)
                {
                    return false;
                }
            }

            var fileInfo = new FileInfo(_imageService.CurrentImageLocalPath);
            if (fileInfo.IsReadOnly)
            {
                throw new ImageReadOnlyException(_imageService.CurrentImageLocalPath);
            }

            success = await TransformDisplayAndLoadAsync(ImageCapability.SupportRemoveData, WzSvcProviderIDs.SPID_REMOVEPERSONALDATA_TRANSFORM);

            if (success)
            {
                IsImageDirty = true;
            }

            return success;
        }

        public async Task<bool> ExecuteResizeImageCommandAsync()
        {
            bool success = false;
            if (!_imageService.HasImageOpened)
            {
                success = await ExecuteOpenCommandAsync();
                if (!success)
                {
                    return false;
                }
            }

            var fileInfo = new FileInfo(_imageService.CurrentImageLocalPath);
            if (fileInfo.IsReadOnly)
            {
                throw new ImageReadOnlyException(_imageService.CurrentImageLocalPath);
            }

            success = await TransformDisplayAndLoadAsync(ImageCapability.SupportResize, WzSvcProviderIDs.SPID_ENLARGE_REDUCE_IMAGE);

            if (success)
            {
                IsImageDirty = true;
            }

            return success;
        }

        public async Task<bool> ExecuteRotateLeftCommandAsync()
        {
            if (!_imageService.HasImageOpened)
            {
                bool success = await ExecuteOpenCommandAsync();
                if (!success)
                {
                    return false;
                }
            }

            var fileInfo = new FileInfo(_imageService.CurrentImageLocalPath);
            if (fileInfo.IsReadOnly)
            {
                throw new ImageReadOnlyException(_imageService.CurrentImageLocalPath);
            }

            if (!_imageService.CheckImageSupportCapatility(ImageCapability.SupportRotate))
            {
                throw new OperationNotSupportException();
            }

            _imageService.RotateCurrentImageLeft();

            IsImageDirty = true;
            ImgUtilView.ReCalculateSizeToFit();
            Notify(nameof(CurrentPreviewImage));

            TrackHelper.LogImgToolsEvent("rotate", "left");

            return true;
        }

        public async Task<bool> ExecuteRotateRightCommandAsync()
        {
            if (!_imageService.HasImageOpened)
            {
                bool success = await ExecuteOpenCommandAsync();
                if (!success)
                {
                    return false;
                }
            }

            if (!_imageService.CheckImageSupportCapatility(ImageCapability.SupportRotate))
            {
                throw new OperationNotSupportException();
            }

            var fileInfo = new FileInfo(_imageService.CurrentImageLocalPath);
            if (fileInfo.IsReadOnly)
            {
                throw new ImageReadOnlyException(_imageService.CurrentImageLocalPath);
            }

            _imageService.RotateCurrentImageRight();

            IsImageDirty = true;
            ImgUtilView.ReCalculateSizeToFit();
            Notify(nameof(CurrentPreviewImage));

            TrackHelper.LogImgToolsEvent("rotate", "right");

            return true;
        }

        public async Task<bool> ExecuteWatermarkImageCommandAsync()
        {
            bool success = false;
            if (!_imageService.HasImageOpened)
            {
                success = await ExecuteOpenCommandAsync();
                if (!success)
                {
                    return false;
                }
            }

            var fileInfo = new FileInfo(_imageService.CurrentImageLocalPath);
            if (fileInfo.IsReadOnly)
            {
                throw new ImageReadOnlyException(_imageService.CurrentImageLocalPath);
            }

            success = await TransformDisplayAndLoadAsync(ImageCapability.SupportWatermark, WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM);

            if (success)
            {
                IsImageDirty = true;
            }

            return success;
        }

        public async Task<bool> ExecuteAddToTeamsBackgroundCommandAsync()
        {
            bool success = false;
            if (!_imageService.HasImageOpened)
            {
                success = await ExecuteOpenCommandAsync();
                if (!success)
                {
                    return false;
                }
            }

            if (!_imageService.CheckImageSupportCapatility(ImageCapability.SupportAddToTeamsBG))
            {
                throw new OperationNotSupportException();
            }

            var filePath = Path.Combine(ImageHelper.GetTeamsBackgroundFolder(), _imageService.CurrentImageName);
            if (File.Exists(filePath))
            {
                if (!FlatMessageWindows.DisplayConfirmationMessage(Owner, string.Format(Properties.Resources.TEXT_EXIST_AND_REPLACE, _imageService.CurrentImageName)))
                {
                    return false;
                }
            }

            success = await Task.Run(ExecuteAddToTeams);

            if (success)
            {
                FlatMessageWindows.DisplayInformationMessage(Owner, Properties.Resources.TEXT_ADD_TO_TEAMS_BG_SUCCESS);
                TrackHelper.LogImgToolsEvent("addToTeams", string.Empty);
            }

            return success;
        }

        private bool ExecuteAddToTeams()
        {
            string filePath = CurrentImageFilePath;
            if (IsImageDirty)
            {
                filePath = _imageService.SaveCurrentImageToTempFolder();
            }

            var item = ImageHelper.InitCloudItemFromPath(filePath);

            bool success = WinzipMethods.AddToTeamsBGFolder(Owner, item);

            return success;
        }

        public async Task<bool> ExecuteSetDesktopBackgroundCommandAsync()
        {
            bool success = false;
            if (!_imageService.HasImageOpened)
            {
                success = await ExecuteOpenCommandAsync();
                if (!success)
                {
                    return false;
                }
            }

            if (!_imageService.CheckImageSupportCapatility(ImageCapability.SupportSetDesktopBG))
            {
                throw new OperationNotSupportException();
            }

            if (!FlatMessageWindows.DisplayConfirmationMessage(Owner, Properties.Resources.TEXT_CONFIRM_SET_DESKTOP_BG))
            {
                return false;
            }

            success = await Task.Run(ExecuteSetDesktopBackground);

            return success;
        }

        private bool ExecuteSetDesktopBackground()
        {
            string filePath = CurrentImageFilePath;
            if (IsImageDirty)
            {
                filePath = _imageService.SaveCurrentImageToTempFolder();
            }

            var item = ImageHelper.InitCloudItemFromPath(filePath);

            bool success = WinzipMethods.SetWindowsDesktopBG(Owner, item);

            if (success)
            {
                TrackHelper.LogImgToolsEvent("setbg", string.Empty);
            }

            return success;
        }

        public bool ExecuteNewWindowCommand()
        {
            // To get applet stub path or real path
            var moduleFilePath = Process.GetCurrentProcess().MainModule.FileName;
            var appPath = Path.Combine(Path.GetDirectoryName(moduleFilePath), Path.GetFileNameWithoutExtension(moduleFilePath) + ".exe");

            ImgUtilSettings.Instance.WindowPosLeft = ImgUtilView.Left;
            ImgUtilSettings.Instance.WindowPosTop = ImgUtilView.Top;
            ImgUtilSettings.SaveImgUtilSettingsXML();

            string args = "/position " + $"{ImgUtilView.Left} {ImgUtilView.Top} {ImgUtilView.Width} {ImgUtilView.Height}";
            Process.Start(appPath, args);

            return true;
        }

        public bool ExecuteHelpCommand()
        {
            var dialog = new AboutDialog(ImgUtilView);
            dialog.ShowDialog();
            return true;
        }

        #endregion

        #region Execute Main View Commands

        public async Task<bool> ExecuteDragFromExplorerAsync(object parameter)
        {
            bool success = false;
            bool canContinue = await HandleCurrentImageStatusAsync();

            if (canContinue)
            {
                ImgUtilView.SetWindowLoadingStatus(true);

                success = await Task.Run(() => ExecuteDragFromExplorer(parameter as string));

                ImgUtilView.SetWindowLoadingStatus(false);
            }

            return success;
        }

        private bool ExecuteDragFromExplorer(string file)
        {
            bool isFileUndefined = ImageHelper.GetImageFormatFromPath(file) == Aspose.Imaging.FileFormat.Undefined;
            if (isFileUndefined)
            {
                var fileFormat = ImageHelper.GetImageRealFormatFromPath(file);
                string fileExt = ImageHelper.GetFormatStringFromFormat(fileFormat);
                if (string.IsNullOrEmpty(fileExt))
                {
                    if (fileFormat != Aspose.Imaging.FileFormat.Undefined)
                    {
                        throw new ImageNotSupportException(file);
                    }
                    else
                    {
                        throw new InvalidImageException(file);
                    }
                }
            }

            bool success = false;
            var fileItem = ImageHelper.InitCloudItemFromPath(file);
            try
            {
                success = LoadImageFromWzCloudItem(fileItem, 1);
            }
            catch
            {
                RefreshRecentFiles(new RecentFile(CurrentOpenedItem.Value, CurrentPreviewImage), true);
                throw;
            }

            if (success)
            {
                IsNewImage = false;
                IsImageDirty = false;
                IsImageChanged = false;
                TrackHelper.LogFileOpenEvent(CurrentImageFilePath);
                RefreshRecentFiles(new RecentFile(CurrentOpenedItem.Value, CurrentPreviewImage), false);
            }

            return success;
        }

        #endregion

        #region Private Helper Functions for Commands

        private bool SaveCurrentImageToZip(string destZipPath)
        {
            if (string.IsNullOrEmpty(destZipPath))
            {
                return false;
            }

            string password = string.Empty;
            bool canContinue = PopUp(() =>
            {
                var dialog = new EncryptZipFileDialog(ImgUtilView);
                if (dialog.Show() != System.Windows.Forms.DialogResult.OK)
                {
                    return false;
                }
                password = dialog.GetPassword();
                return true;
            });

            if (!canContinue)
            {
                return false;
            }

            string tempFolder = FileOperation.Instance.CreateTempFolder();
            string tempZipPath = Path.Combine(tempFolder, Path.GetExtension(destZipPath));

            string zipItemPath = _imageService.SaveCurrentImageToTempFolder();
            var zipItem = ImageHelper.InitCloudItemFromPath(zipItemPath);
            var zipItems = new WzCloudItem4[] { zipItem };
            bool success = WinzipMethods.SaveToZip(Owner, zipItems, 1, tempZipPath, true, true, password);

            if (success)
            {
                try
                {
                    // Copy file from temp path to selected place.
                    EDPHelper.FileCopy(tempZipPath, destZipPath, true);
                }
                catch (UnauthorizedAccessException e)
                {
                    Directory.Delete(tempFolder, true);
                    throw new SaveLocationNoAccessException(e.Message, e);
                }
            }

            return success;
        }

        private bool DoUploadCloudItem(string srcFilePath, WzCloudItem4 destItem, bool isOverwrite = false)
        {
            var localItem = ImageHelper.InitCloudItemFromPath(srcFilePath);
            var localItems = new WzCloudItem4[] { localItem };
            int count = 1;

            bool success = WinzipMethods.UploadToCloud(Owner, localItems, ref count, destItem, false, isOverwrite);
            if (success && localItems.Length > 0)
            {
                UploadedItem = localItems[0];
            }

            return success;
        }

        private string DoTransform(WzSvcProviderIDs id, bool isShow = true)
        {
            var spids = new WzSvcProviderIDs[] { id };

            string tempFolder = FileOperation.Instance.CreateTempFolder();
            string tempPath = Path.Combine(tempFolder, _imageService.CurrentImageName);

            if (IsImageDirty)
            {
                _imageService.SaveCurrentImageToTempFolder(tempPath);
            }
            else
            {
                EDPHelper.FileCopy(CurrentImageFilePath, tempPath, true);
            }

            string[] itemsPath = new string[1] { tempPath };

            var resultFiles = new List<WinzipMethods.ConvertFileResultPath>();

            bool success = WinzipMethods.ConvertFile(Owner, spids, 1, itemsPath, 1, null, 0, null, resultFiles, isShow, true, true);

            string resultFileName = string.Empty;
            if (success && resultFiles.Count > 0)
            {
                resultFileName = resultFiles[0].path;
                if (EDPAPIHelper.IsProcessProtectedByEDP())
                {
                    string enterpriseId = EDPAPIHelper.GetEnterpriseId(CurrentImageFilePath);
                    using (var autoRestoreTempEnterpriseId = new EDPAutoRestoreTempEnterpriseID(enterpriseId))
                    {
                        EDPAPIHelper.ProtectNewItem(resultFileName);
                    }
                }

                if (id == WzSvcProviderIDs.SPID_CONVERTPHOTOS_TRANSFORM)
                {
                    resultFileName = Path.Combine(Path.GetDirectoryName(tempPath), Path.GetFileNameWithoutExtension(tempPath) + Path.GetExtension(resultFileName));
                    File.Move(resultFiles[0].path, resultFileName);
                }

                TrackHelper.TrackImgToolsEvent(id, resultFiles[0].path);
            }

            return !string.IsNullOrEmpty(resultFileName) && File.Exists(resultFileName) ? resultFileName : string.Empty;
        }

        private async Task<bool> TransformDisplayAndLoadAsync(ImageCapability capability, WzSvcProviderIDs id, bool isShow = true)
        {
            if (!_imageService.CheckImageSupportCapatility(capability))
            {
                throw new OperationNotSupportException();
            }

            string resultFile = await Task.Run(() => DoTransform(id, isShow));

            if (string.IsNullOrEmpty(resultFile))
            {
                return false;
            }

            FlatMessageWindows.DisplayInformationMessage(Owner, Properties.Resources.TEXT_CONVERSION_SUCCESS);

            bool success = await Task.Run(() => LoadImage(resultFile));

            return success;
        }

        private void RecursiveDir(DirectoryInfo dirInfo, ref List<string> fileList)
        {
            foreach (var fileInfo in dirInfo.GetFiles())
            {
                fileList.Add(fileInfo.FullName);
            }

            if (dirInfo.GetDirectories().Length == 0)
            {
                return;
            }

            foreach (var dir in dirInfo.GetDirectories())
            {
                RecursiveDir(dir, ref fileList);
            }
        }

        private CheckResult CheckToSaveImage()
        {
            int dialogWidth = 250;

            if (IsNewImage)
            {
                TASKDIALOG_BUTTON[] buttons = new[]
                {
                        new TASKDIALOG_BUTTON()
                        {
                            id = (int)System.Windows.Forms.DialogResult.Yes,
                            text = Properties.Resources.TASKDLG_SAVE_BUTTON
                        },
                        new TASKDIALOG_BUTTON()
                        {
                            id = (int)System.Windows.Forms.DialogResult.No,
                            text = Properties.Resources.TASKDLG_DONT_SAVE_BUTTON + "\n" + Properties.Resources.TASKDLG_DONT_SAVE_BUTTON_TIP
                        }
                    };

                var taskDialog = new TaskDialog(Properties.Resources.IMAGE_UTILITY_TITLE, Properties.Resources.TASKDLG_ASK_TO_SAVE_TITLE,
                    Properties.Resources.TASKDLG_SELECT_ONE, null, buttons, SystemIcons.Warning, dialogWidth);

                var result = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);

                switch (result.dialogResult)
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        return CheckResult.NeedSaveAs;

                    case System.Windows.Forms.DialogResult.No:
                        return CheckResult.DontSave;

                    case System.Windows.Forms.DialogResult.Cancel:
                        return CheckResult.Cancel;
                }
            }
            else if (IsImageDirty)
            {
                TASKDIALOG_BUTTON[] buttons = new[]
                {
                        new TASKDIALOG_BUTTON()
                        {
                            id = (int)System.Windows.Forms.DialogResult.Yes,
                            text = Properties.Resources.TASKDLG_SAVE_BUTTON + "\n" + Properties.Resources.TASKDLG_SAVE_BUTTON_TIP
                        },
                        new TASKDIALOG_BUTTON()
                        {
                            id = (int)System.Windows.Forms.DialogResult.OK,
                            text = Properties.Resources.TASKDLG_SAVE_AS_BUTTON + "\n" + Properties.Resources.TASKDLG_SAVE_AS_BUTTON_TIP
                        },
                        new TASKDIALOG_BUTTON()
                        {
                            id = (int)System.Windows.Forms.DialogResult.No,
                            text = Properties.Resources.TASKDLG_DONT_SAVE_BUTTON + "\n" + Properties.Resources.TASKDLG_DONT_SAVE_BUTTON_TIP
                        }
                    };

                var taskDialog = new TaskDialog(Properties.Resources.IMAGE_UTILITY_TITLE, Properties.Resources.TASKDLG_ASK_TO_SAVE_TITLE,
                    Properties.Resources.TASKDLG_SELECT_ONE_MODIFIED, null, buttons, SystemIcons.Warning, dialogWidth);

                var result = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);

                switch (result.dialogResult)
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        return CheckResult.NeedSave;

                    case System.Windows.Forms.DialogResult.OK:
                        return CheckResult.NeedSaveAs;

                    case System.Windows.Forms.DialogResult.No:
                        return CheckResult.DontSave;

                    case System.Windows.Forms.DialogResult.Cancel:
                        return CheckResult.Cancel;
                }
            }

            return CheckResult.DontSave;
        }

        private CheckResult CheckToSaveBeforeOpenWith()
        {
            int dialogWidth = 260;
            if (IsNewImage)
            {
                TASKDIALOG_BUTTON[] buttons = new[]
                {
                    new TASKDIALOG_BUTTON()
                    {
                        id = (int)System.Windows.Forms.DialogResult.Yes,
                        text = Properties.Resources.TASKDLG_SAVE_OPENWITH_BUTTON + "\n" + Properties.Resources.TASKDLG_SAVE_OPENWITH_BUTTON_TIP
                    }
                };

                TaskDialog taskDialog = new TaskDialog(Properties.Resources.IMAGE_UTILITY_TITLE, Properties.Resources.TASKDLG_ASK_TO_SAVE_TITLE,
                    null, null, buttons, System.Drawing.SystemIcons.Warning, dialogWidth);

                TaskDialogResult result = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);

                switch (result.dialogResult)
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        return CheckResult.NeedSaveAs;

                    default:
                        return CheckResult.Cancel;
                }
            }
            else if (IsImageDirty)
            {
                TASKDIALOG_BUTTON[] buttons = new[]
                {
                    new TASKDIALOG_BUTTON()
                    {
                        id = (int)System.Windows.Forms.DialogResult.Yes,
                        text = Properties.Resources.TASKDLG_SAVE_OPENWITH_BUTTON + "\n" + Properties.Resources.TASKDLG_SAVE_OPENWITH_BUTTON_TIP
                    },
                    new TASKDIALOG_BUTTON()
                    {
                        id = (int)System.Windows.Forms.DialogResult.No,
                        text = Properties.Resources.TASKDLG_OPENWITH_BUTTON + "\n" + Properties.Resources.TASKDLG_OPENWITH_BUTTON_TIP
                    }
                };

                TaskDialog taskDialog = new TaskDialog(Properties.Resources.IMAGE_UTILITY_TITLE, Properties.Resources.TASKDLG_ASK_TO_SAVE_TITLE,
                    null, null, buttons, System.Drawing.SystemIcons.Warning, dialogWidth);

                TaskDialogResult result = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);

                switch (result.dialogResult)
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        return CheckResult.NeedSave;

                    case System.Windows.Forms.DialogResult.No:
                        return CheckResult.DontSave;

                    default:
                        return CheckResult.Cancel;
                }
            }
            else
            {
                return CheckResult.DontSave;
            }
        }

        private bool StartAdminProgressAndChangeIntegration()
        {
            TASKDIALOG_BUTTON[] buttons = new[]
            {
                new TASKDIALOG_BUTTON()
                {
                    id = (int)System.Windows.Forms.DialogResult.Yes,
                    text = Properties.Resources.ADMIN_REQUIRED_DLG_OK + "\n" + Properties.Resources.ADMIN_REQUIRED_DLG_OK_TIP
                },
                new TASKDIALOG_BUTTON()
                {
                    id = (int)System.Windows.Forms.DialogResult.No,
                    text = Properties.Resources.TASKDLG_CANCEL_BUTTON + "\n" + Properties.Resources.ADMIN_REQUIRED_DLG_CANCEL_TIP
                }
            };

            int taskDialogWidth = 220;
            var taskDialog = new TaskDialog(Properties.Resources.IMAGE_UTILITY_TITLE, Properties.Resources.ADMIN_REQUIRED_DLG_TITLE,
                Properties.Resources.ADMIN_REQUIRED_DLG_CONTENT, null, buttons, SystemIcons.Information, taskDialogWidth);
            TaskDialogResult taskDialogResult = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);
            if (taskDialogResult.dialogResult != System.Windows.Forms.DialogResult.Yes)
            {
                return false;
            }

            try
            {
                // To get applet stub path or real path
                var moduleFilePath = Process.GetCurrentProcess().MainModule.FileName;
                var appPath = Path.Combine(Path.GetDirectoryName(moduleFilePath), Path.GetFileNameWithoutExtension(moduleFilePath) + ".exe");

                Process shellProcess = new Process();
                shellProcess.StartInfo.FileName = appPath;
                shellProcess.StartInfo.Verb = "runas";
                shellProcess.StartInfo.Arguments = "/admincfg";
                shellProcess.Start();
                shellProcess.WaitForExit();
                shellProcess.Dispose();
            }
            // ignore exceptions
            catch
            {
            }

            Notify(nameof(AlreadySetDefault));
            return true;
        }

        private void ChangeIntegrationWithoutAdmin()
        {
            // only do remove shortcut here
            var keyNames = RegeditOperation.GetAdminConfigRegistryValueName();

            if (keyNames == null || keyNames.Length == 0)
            {
                return;
            }

            foreach (var name in keyNames)
            {
                var opt = RegeditOperation.GetAdminConfigRegistryStringValue(name);
                if (int.Parse(opt) == 0)
                {
                    if (name.Equals(RegeditOperation.WzAddDesktopIconKey))
                    {
                        RemoveLinkFile(NativeMethods.CSIDL_COMMON_DESKTOPDIRECTORY, Environment.SpecialFolder.Desktop);
                    }
                    else if (name.Equals(RegeditOperation.WzAddStartMenuKey))
                    {
                        RemoveLinkFile(NativeMethods.CSIDL_COMMON_PROGRAMS, Environment.SpecialFolder.Programs);
                    }
                }
            }
        }

        private void RemoveLinkFile(int nFolder, Environment.SpecialFolder specialFolder)
        {
            int size = 260;
            StringBuilder path = new StringBuilder(size);
            NativeMethods.SHGetSpecialFolderPath(IntPtr.Zero, path, nFolder, false);
            string folderPath = path.ToString();
            if (Directory.Exists(folderPath))
            {
                // remove shortcut in public folder
                string link = Path.Combine(folderPath, Properties.Resources.IMAGE_UTILITY_TITLE + ".lnk");
                if (File.Exists(link))
                {
                    File.Delete(link);
                }
            }

            var specialFolderPath = Environment.GetFolderPath(specialFolder);
            if (Directory.Exists(specialFolderPath))
            {
                // remove shortcut in current user's folder
                string link = Path.Combine(specialFolderPath, Properties.Resources.IMAGE_UTILITY_TITLE + ".lnk");
                if (File.Exists(link))
                {
                    File.Delete(link);
                }
            }
        }

        private void ClearCurrentImage()
        {
            _imageService.ClearPreviousImage();
            Notify(nameof(IsCurrentImageFileExist));
            Notify(nameof(CurrentPreviewImage));
            Notify(nameof(ImgUtilTitle));
        }

        #endregion
    }
}
