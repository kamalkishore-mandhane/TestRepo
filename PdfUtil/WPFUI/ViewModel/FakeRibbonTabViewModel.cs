using Aspose.Pdf;
using Aspose.Pdf.Devices;
using Aspose.Pdf.Facades;
using PdfUtil.Util;
using PdfUtil.WPFUI.Commands;
using PdfUtil.WPFUI.Controls;
using PdfUtil.WPFUI.Model;
using PdfUtil.WPFUI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Xps.Packaging;

namespace PdfUtil.WPFUI.ViewModel
{
    public enum Result
    {
        Ok = 1,
        Error,
        Cancel
    }

    public enum SaveChangesCheckEnum
    {
        Save,
        SaveAs,
        Cancel,
        DoNotSave
    }

    public enum CloudDownloadResult
    {
        DownloadSucceed,
        CanceledByUser,
        CloudUnauthorized,
        DownloadFailed
    };


    class FakeRibbonTabViewModel
    {
        private const int DefaultItemsCount = 256;
        private const int MaxRecentListCount = 15;

        private PdfUtilViewModel _viewModel;
        private RibbonCommandModel _viewModelCommands;

        private bool _isContextMenuOpen;
        private bool _isFirstTimeOpenIntegration;

        private const string ShareAppName32 = "SafeShare32.exe";
        private const string ShareAppName64 = "SafeShare64.exe";

        public FakeRibbonTabViewModel(PdfUtilViewModel viewModel)
        {
            _viewModel = viewModel;
            _isFirstTimeOpenIntegration = true;
        }

        public string ShareAppletName
        {
            get
            {
                if (WinzipMethods.Is32Bit())
                {
                    return ShareAppName32;
                }
                else
                {
                    return ShareAppName64;
                }
            }
        }

        public RibbonCommandModel ViewModelCommands
        {
            get
            {
                if (_viewModelCommands == null)
                {
                    _viewModelCommands = new RibbonCommandModel(this);
                }

                return _viewModelCommands;
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsContextMenuOpen
        {
            get
            {
                return _isContextMenuOpen;
            }
            set
            {
                _isContextMenuOpen = value;
            }
        }

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

        public void ExecuteCreateFromCommand()
        {
            _viewModel.Executor(() => ExecuteCreateFromTask(), RetryStrategy.Create(false, 0)).IgnoreExceptions();
        }

        private Task ExecuteCreateFromTask()
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                bool isCancel = false;
                _viewModel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    if (DoSaveChangesCheck() == SaveChangesCheckEnum.Cancel)
                    {
                        isCancel = true;
                    }
                }));

                if (isCancel)
                {
                    tcs.TrySetCanceled();
                    return;
                }

                _viewModel.PdfUtilView.SetWindowLoadingStatus(true);
                var resultFilePath = ImportFilesByConvert();

                // For Lead review #61, pdf name should be New.pdf in Create from workflow
                try
                {
                    if (!string.IsNullOrEmpty(resultFilePath) && File.Exists(resultFilePath))
                    {
                        var newPdfPath = Path.Combine(Path.GetDirectoryName(resultFilePath), Properties.Resources.DEFAULT_PDF_TITLE_NAME);

                        // For create from twice case, we should delete the first created temp New.pdf first.
                        if (File.Exists(newPdfPath) && resultFilePath != newPdfPath)
                        {
                            ClearDocument(null, string.Empty, true);
                            File.Delete(newPdfPath);
                        }

                        File.Move(resultFilePath, newPdfPath);
                        resultFilePath = newPdfPath;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    _viewModel.PdfUtilView.SetWindowLoadingStatus(false);
                    tcs.SetCanceled();
                    return;
                }

                if (!string.IsNullOrEmpty(resultFilePath) && Path.GetExtension(resultFilePath.ToLower()) == PdfHelper.PdfExtension)
                {
                    _viewModel.OpenNewPdfAfterConverted(resultFilePath);
                }

                _viewModel.PdfUtilView.SetWindowLoadingStatus(false);
                tcs.TrySetResult();
            }).ContinueWith(task =>
            {
                _viewModel.PdfUtilView.Focus();
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public string ImportFilesByConvert(bool isFromImportFiles = false)
        {
            string title = isFromImportFiles ? Properties.Resources.IMPORT_FILE_PICKER_TITLE : Properties.Resources.CREATE_FROM_PICKER_TITLE;
            string defaultBtn = isFromImportFiles ? Properties.Resources.IMPORT_FILE_PICKER_BUTTON : Properties.Resources.OPEN_PICKER_BUTTON;

            string filters = Properties.Resources.OPEN_PICKER_FILTERS; ;
            var defaultFolder = PdfHelper.GetOpenPickerDefaultFolder();
            var selectedItem = PdfHelper.InitWzCloudItem();
            selectedItem.profile.Id = WzSvcProviderIDs.SPID_UNKNOWN;

            int count = DefaultItemsCount;
            var preSelectedItems = new WzCloudItem4[count];

            if (!WinzipMethods.IsProviderSupport(_viewModel.PdfUtilView.WindowHandle, WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM))
            {
                filters = Properties.Resources.CREATE_FROM_FILTERS;
            }

            bool ret = WinzipMethods.FileSelection(_viewModel.PdfUtilView.WindowHandle, title, defaultBtn, filters, defaultFolder, preSelectedItems, ref count, true, true, true, true, false, false);

            if (ret)
            {
                var selectedItems = new WzCloudItem4[count];
                Array.Copy(preSelectedItems, selectedItems, count);

                bool isCloudItem = PdfHelper.IsCloudItem(selectedItems[0].profile.Id);
                bool isLocalPortableDeviceItem = PdfHelper.IsLocalPortableDeviceItem(selectedItems[0].profile.Id);

                if (!isCloudItem && !isLocalPortableDeviceItem)
                {
                    selectedItems = FileOperation.FilterUnreadableFiles(selectedItems, ref count, _viewModel.PdfUtilView.WindowHandle);
                    if (selectedItems.Length == 0)
                    {
                        return string.Empty;
                    }
                }

                PdfHelper.SetOpenPickerDefaultFolder(selectedItems[0]);
                PdfHelper.SetSavePickerDefaultFolder(selectedItems[0]);

                var str = ConvertSelectedFilesToPdf(selectedItems);
                if (!string.IsNullOrEmpty(str))
                {
                    foreach (var file in selectedItems)
                    {
                        TrackHelper.LogFileImportEvent(file.name);
                        if (_viewModel.CurrentPdfDocument != null && isFromImportFiles)
                        {
                            TrackHelper.LogInsertFromFileEvent(file.name);
                        }
                    }
                }
                return str;
            }

            return string.Empty;
        }

        public string ConvertSelectedFilesToPdf(WzCloudItem4[] selectedItems)
        {
            bool ret;
            int count = selectedItems.Count();
            var selectedItem = PdfHelper.InitWzCloudItem();
            selectedItem.profile.Id = WzSvcProviderIDs.SPID_UNKNOWN;

            bool isCloudItem = PdfHelper.IsCloudItem(selectedItems[0].profile.Id);
            bool isLocalPortableDeviceItem = PdfHelper.IsLocalPortableDeviceItem(selectedItems[0].profile.Id);

            if (isCloudItem || isLocalPortableDeviceItem)
            {
                if (DownloadCloudItems(ref selectedItems, false) != CloudDownloadResult.DownloadSucceed)
                {
                    return string.Empty;
                }
            }
            else
            {
                string tmpFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                for (int i = 0; i < count; i++)
                {
                    var newName = Path.Combine(tmpFolder, Path.GetFileName(selectedItems[i].itemId));
                    try
                    {
                        EDPHelper.FileCopy(selectedItems[i].itemId, newName, true);
                        File.SetAttributes(newName, File.GetAttributes(newName) & ~FileAttributes.ReadOnly); // remove the readonly flag when create or import files
                    }
                    catch (Exception ex)
                    {
                        if (count == 1)
                        {
                            // prompt error message when there are only one file selected
                            FileOperation.HandleFileException(ex, _viewModel.PdfUtilView.WindowHandle);
                            return string.Empty;
                        }
                        else
                        {
                            // skip this error file if there are more than one file selected
                            selectedItems[i].itemId = string.Empty;
                            continue;
                        }
                    }
                    selectedItems[i].itemId = newName;
                }
            }

            // add files that are not skipped to the file list.
            bool EDPProtected = false;
            var selectedItemsPath = new List<string>();
            for (int i = 0; i < count; i++)
            {
                if (!string.IsNullOrEmpty(selectedItems[i].itemId))
                {
                    selectedItemsPath.Add(selectedItems[i].itemId);
                    if (EDPAPIHelper.IsProcessProtectedByEDP())
                    {
                        EDPProtected |= EDPAPIHelper.GetEnterpriseId(selectedItems[i].itemId) == EDPAPIHelper.GetEnterpriseId();
                    }
                }
            }

            // return if there are no file left or there are one pdf file left.
            if (selectedItemsPath.Count == 0)
            {
                return string.Empty;
            }
            else if (selectedItemsPath.Count == 1 && Path.GetExtension(selectedItemsPath[0]).ToLower() == PdfHelper.PdfExtension)
            {
                return selectedItemsPath[0];
            }

            // set spidList for files
            var spidList = new List<WzSvcProviderIDs>();
            if (selectedItemsPath.Count > 1)
            {
                spidList.Add(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM);
            }

            foreach (var path in selectedItemsPath)
            {
                if (Path.GetExtension(path).ToLower() != PdfHelper.PdfExtension)
                {
                    spidList.Add(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM);
                    break;
                }
            }

            var resultFiles = new List<WinzipMethods.ConvertFileResultPath>();
            ret = WinzipMethods.ConvertFile(_viewModel.PdfUtilView.WindowHandle, spidList.ToArray(), spidList.Count, selectedItemsPath.ToArray(), selectedItemsPath.Count, null, 0, null, resultFiles, true, true, true);
            if (ret && resultFiles.Count > 0)
            {
                if (spidList.Contains(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM))
                {
                    TrackHelper.LogPdfMergeEvent(selectedItemsPath.Count);
                }

                if (EDPProtected)
                {
                    EDPAPIHelper.ProtectNewItem(resultFiles[0].path);
                }
                selectedItem.itemId = resultFiles[0].path;
            }
            else
            {
                selectedItem.itemId = string.Empty;
            }

            return selectedItem.itemId;
        }

        public void ExecuteOpenCommand()
        {
            _viewModel.Executor(() => ExecuteOpenTask(true), RetryStrategy.Create(false, 0)).IgnoreExceptions();
        }

        public Task ExecuteOpenTask(bool fromOpenCommand)
        {
            if (!fromOpenCommand && _viewModel.CurrentPdfDocument != null)
            {
                return Task.Factory.CompletedTask();
            }

            return Task.Factory.StartNewTCS(tcs =>
            {
                bool isCancel = false;
                _viewModel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    if (DoSaveChangesCheck() == SaveChangesCheckEnum.Cancel)
                    {
                        isCancel = true;
                    }
                }));

                if (isCancel)
                {
                    tcs.TrySetCanceled();
                    return;
                }

                _viewModel.PdfUtilView.SetWindowLoadingStatus(true);

                string title = Properties.Resources.OPEN_PICKER_TITLE;
                string defaultBtn = Properties.Resources.OPEN_PICKER_BUTTON;
                string filters = Properties.Resources.PDF_FILTER;
                var defaultFolder = PdfHelper.GetOpenPickerDefaultFolder();
                var selectedItems = new WzCloudItem4[1];
                int count = 1;

                // Select one PDF file in open command.
                bool ret = WinzipMethods.FileSelection(_viewModel.PdfUtilView.WindowHandle, title, defaultBtn, filters, defaultFolder, selectedItems, ref count, false, true, true, true, false, false);

                if (ret)
                {
                    var selectedItem = selectedItems[0];
                    var isCloudItem = PdfHelper.IsCloudItem(selectedItem.profile.Id);
                    var isLocalPortableDeviceItem = PdfHelper.IsLocalPortableDeviceItem(selectedItem.profile.Id);

                    if (!isCloudItem && !isLocalPortableDeviceItem)
                    {
                        selectedItems = FileOperation.FilterUnreadableFiles(selectedItems, ref count, _viewModel.PdfUtilView.WindowHandle);
                        if (selectedItems.Length == 0)
                        {
                            _viewModel.PdfUtilView.SetWindowLoadingStatus(false);
                            tcs.TrySetCanceled();
                            return;
                        }
                    }

                    PdfHelper.SetOpenPickerDefaultFolder(selectedItem);
                    PdfHelper.SetSavePickerDefaultFolder(selectedItem);

                    if ((!isCloudItem && !isLocalPortableDeviceItem) || DownloadCloudItems(ref selectedItems, true) == CloudDownloadResult.DownloadSucceed)
                    {
                        if (InitDocumentByOpen(selectedItems[0], isCloudItem, isLocalPortableDeviceItem) == Result.Ok)
                        {
                            TrackHelper.LogFileOpenEvent(_viewModel.CurrentPdfFileName);
                            _viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                RefreshRecentList(selectedItem);
                            }));
                        }
                    }
                }

                _viewModel.PdfUtilView.SetWindowLoadingStatus(false);
                if (ret)
                {
                    tcs.TrySetResult();
                }
                else
                {
                    tcs.TrySetCanceled();
                }
            }).ContinueWith(task =>
            {
                _viewModel.PdfUtilView.Focus();
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public Task ExecuteInitDocumentByOpenTask(WzCloudItem4 selectedItem)
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                var selectedItems = new WzCloudItem4[] { selectedItem };
                var isCloudItem = PdfHelper.IsCloudItem(selectedItem.profile.Id);
                var isLocalPortableDeviceItem = PdfHelper.IsLocalPortableDeviceItem(selectedItem.profile.Id);

                if ((!isCloudItem && !isLocalPortableDeviceItem) || DownloadCloudItems(ref selectedItems, true) == CloudDownloadResult.DownloadSucceed)
                {
                    if (InitDocumentByOpen(selectedItems[0], isCloudItem, isLocalPortableDeviceItem) == Result.Ok)
                    {
                        RefreshRecentList(selectedItem);
                        TrackHelper.LogFileOpenEvent(_viewModel.CurrentPdfFileName);
                    }
                }
                tcs.TrySetResult();
            });
        }

        public Task ExecuteInitDocumentByOpenFromZipPaneTask(WzCloudItem4 selectedItem)
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                if (InitDocumentByOpen(selectedItem, false, false) == Result.Ok)
                {
                    TrackHelper.LogFileOpenEvent(_viewModel.CurrentPdfFileName);
                }
                tcs.TrySetResult();
            });
        }

        public Task ExecuteInitDocumentByCreateFromTask(WzCloudItem4 item)
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                _viewModel.PdfUtilView.SetWindowLoadingStatus(true);

                string itemPath = !string.IsNullOrEmpty(item.itemId) ? item.itemId : Path.Combine(item.parentId, item.name);

                if (!string.IsNullOrEmpty(itemPath) && Path.GetExtension(itemPath.ToLower()) == PdfHelper.PdfExtension)
                {
                    var result = Result.Ok;
                    var doc = OpenDocument(itemPath, null, ref result, true, false);
                    if (doc == null || result != Result.Ok)
                    {
                        _viewModel.PdfUtilView.SetWindowLoadingStatus(false);
                        ClearDocument(doc, itemPath);
                        tcs.TrySetCanceled();
                        return;
                    }

                    ClearDocument(doc, itemPath);
                    _viewModel.InitForTheFirstDocument();

                    var pdfInfo = new PdfFileInfo(doc);
                    _viewModel.CurrentPdfFileInfo = pdfInfo;
                    _viewModel.CurrentOpenedItem = PdfHelper.InitWzCloudItem();
                    _viewModel.CurrentPdfFilePasswordInfo = new PdfFilePasswordInfo
                    {
                        HasOpenPassword = pdfInfo.HasOpenPassword,
                        HasEditPassword = pdfInfo.HasEditPassword,
                        PasswordType = pdfInfo.PasswordType
                    };

                    var pdfFileInfo = new PDFInfo
                    {
                        filePath = itemPath,
                        fileName = Path.GetFileName(itemPath)
                    };

                    _viewModel.Icon.SetPDFFileInfo(pdfFileInfo);
                    _viewModel.LoadThumbnailAndBookmark(pdfFileInfo, true, true);
                }

                _viewModel.PdfUtilView.SetWindowLoadingStatus(false);
                tcs.TrySetResult();
            }).ContinueWith(task =>
            {
                _viewModel.PdfUtilView.Focus();
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public Task ExecuteOpenRecentItemTask(WzCloudItem4 recentFile)
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                bool isCancel = false;
                _viewModel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    if (DoSaveChangesCheck() == SaveChangesCheckEnum.Cancel)
                    {
                        isCancel = true;
                    }
                }));

                if (isCancel)
                {
                    tcs.TrySetCanceled();
                    return;
                }

                _viewModel.PdfUtilView.SetWindowLoadingStatus(true);

                var downloadResult = CloudDownloadResult.DownloadSucceed;
                var recentFiles = new WzCloudItem4[] { recentFile };
                var isCloudItem = PdfHelper.IsCloudItem(recentFile.profile.Id);
                var isLocalPortableDeviceItem = PdfHelper.IsLocalPortableDeviceItem(recentFile.profile.Id);

                if ((!isCloudItem && !isLocalPortableDeviceItem) || (downloadResult = DownloadCloudItems(ref recentFiles, true)) == CloudDownloadResult.DownloadSucceed)
                {
                    RefreshRecentList(recentFile);
                    // File is in local or file download successful.
                    var result = InitDocumentByOpen(recentFiles[0], isCloudItem, isLocalPortableDeviceItem);
                    bool doRemove = result == Result.Error;
                    RefreshRecentList(recentFile, doRemove);
                    if (doRemove)
                    {
                        var itemName = !string.IsNullOrEmpty(recentFile.name) ? recentFile.name : Path.GetFileName(recentFile.itemId);
                        FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.FILE_NOT_FOUND_WARNING, itemName));
                    }
                    else if (result == Result.Ok)
                    {
                        TrackHelper.LogFileOpenEvent(_viewModel.CurrentPdfFileName);
                    }
                }
                else
                {
                    // Failed for download file from cloud.
                    // If download is canceled by user, do nothing.
                    // If the cloud is authorized, try to login cloud again.
                    // if not above, remove file from the recent list and show warning message.
                    if (downloadResult != CloudDownloadResult.CanceledByUser)
                    {
                        if (downloadResult == CloudDownloadResult.CloudUnauthorized)
                        {
                            AuthorizeCloudAndDownload(recentFile);
                        }
                        else
                        {
                            RefreshRecentList(recentFile, true);
                            // TODO: Display warning message to tell the user that the download failed
                        }
                    }
                }

                _viewModel.PdfUtilView.SetWindowLoadingStatus(false);
                tcs.TrySetResult();
            }).ContinueWith(task =>
            {
                _viewModel.PdfUtilView.Focus();
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void RefreshRecentList(WzCloudItem4 selectedItem, bool doRemove = false)
        {
            var recentFile = new RecentFile();
            recentFile.IsCloudItem = PdfHelper.IsCloudItem(selectedItem.profile.Id);
            recentFile.IsLocalPortableDeviceItem = PdfHelper.IsLocalPortableDeviceItem(selectedItem.profile.Id);
            recentFile.RecentOpenFileCloudItem = selectedItem;
            recentFile.UpdateFileInfo();

            RecentFile duplicateItem = null;

            if (_viewModel != null)
            {
                _viewModel.LoadRecentFilesXML();
            }

            foreach (var item in _viewModel.RecentList)
            {
                if (item.RecentFileFullName.Equals(recentFile.RecentFileFullName))
                {
                    duplicateItem = item;
                    break;
                }
            }

            bool usedToSelected = usedToSelected = _viewModel.SelectFileInRecentList != null;

            if (_viewModel.RecentList.Contains(duplicateItem))
            {
                _viewModel.RecentList.Remove(duplicateItem);
            }

            if (_viewModel.RecentList.Count >= MaxRecentListCount)
            {
                _viewModel.RecentList.RemoveAt(_viewModel.RecentList.Count - 1);
            }

            if (!doRemove)
            {
                _viewModel.RecentList.Insert(0, recentFile);

                // if there is a file selected in startup pane, but lost selection in Remove operation, do reselect this file again
                if (usedToSelected && _viewModel.SelectFileInRecentList == null)
                {
                    _viewModel.SelectFileInRecentList = recentFile;
                }
            }

            // Refresh recent item index.
            foreach (var item in _viewModel.RecentList)
            {
                const int startIndex = 1;
                item.RecentFileIndex = (_viewModel.RecentList.IndexOf(item) + startIndex).ToString();
            }
            _viewModel.SaveRecentFilesXML();
        }

        private void AuthorizeCloudAndDownload(WzCloudItem4 selectedItem)
        {
            // Try to login the authorized cloud and download the file and open it.
            WzProfile2[] profile = new WzProfile2[1];
            WzProfile2 itemProfile = selectedItem.profile;
            if (WinzipMethods.AuthorizeCloud(_viewModel.PdfUtilView.WindowHandle, itemProfile.Id, itemProfile.authId, itemProfile.name, profile))
            {
                if (profile[0].authId == itemProfile.authId && profile[0].Id == itemProfile.Id)
                {
                    RefreshRecentList(selectedItem, true);
                    selectedItem.profile.name = profile[0].name;
                    var recentFiles = new WzCloudItem4[] { selectedItem };

                    if (DownloadCloudItems(ref recentFiles, true) == CloudDownloadResult.DownloadSucceed
                        && InitDocumentByOpen(recentFiles[0], true, false) == Result.Ok)
                    {
                        RefreshRecentList(selectedItem);
                    }
                }
            }
        }

        public CloudDownloadResult DownloadCloudItems(ref WzCloudItem4[] selectedItems, bool forOpenCommand)
        {
            var downloadResult = CloudDownloadResult.DownloadSucceed;
            var downloadFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
            var folderItem = PdfHelper.InitCloudItemFromPath(downloadFolder);
            int count = selectedItems.Count();
            int downloadErrorCode = 0;

            var ret = WinzipMethods.DownloadFromCloud(_viewModel.PdfUtilView.WindowHandle, selectedItems, count, folderItem, false, false, ref downloadErrorCode);
            if (!ret)
            {
                // Download file failed, return the error code
                Directory.Delete(downloadFolder, true);
                return (CloudDownloadResult)downloadErrorCode;
            }

            for (int i = 0; i < count; i++)
            {
                string downloadPath = Path.Combine(downloadFolder, selectedItems[i].name);
                if (!File.Exists(downloadPath))
                {
                    // Download file return no error but there is no corresponding file in the download folder.
                    // It may be because the cloud file was deleted or renamed.
                    Directory.Delete(downloadFolder, true);
                    return CloudDownloadResult.DownloadFailed;
                }

                if (forOpenCommand)
                {
                    // If download file for open a PDF, then set CurrentOpenedItem.
                    _viewModel.CurrentOpenedItem = selectedItems[i];
                }

                selectedItems[i] = PdfHelper.InitCloudItemFromPath(downloadPath);
            }

            return downloadResult;
        }

        public Result InitDocumentByOpen(WzCloudItem4 selectedItem, bool isCloudItem, bool isLocalPortableDeviceItem)
        {
            if (!isCloudItem && !isLocalPortableDeviceItem)
            {
                _viewModel.CurrentOpenedItem = selectedItem;
                string itemPath = !string.IsNullOrEmpty(selectedItem.itemId) ? selectedItem.itemId : Path.Combine(selectedItem.parentId, selectedItem.name);

                if (!File.Exists(itemPath))
                {
                    return Result.Error;
                }

                var newName = Path.Combine(FileOperation.GlobalTempDir, Path.GetFileName(itemPath));
                try
                {
                    EDPHelper.FileCopy(itemPath, newName, true);
                }
                catch (Exception ex)
                {
                    FileOperation.HandleFileException(ex, _viewModel.PdfUtilView.WindowHandle);
                    return Result.Cancel;
                }
                selectedItem.itemId = newName;
            }

            if (Path.GetExtension(selectedItem.itemId.ToLower()) != PdfHelper.PdfExtension)
            {
                return Result.Error;
            }

            Document doc = null;
            var result = Result.Ok;
            _viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                try
                {
                    doc = OpenDocument(selectedItem.itemId, null, ref result, true, false);
                }
                catch
                {
                    FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_FILE_INVALID_WARNING, Path.GetFileName(selectedItem.itemId)));
                }
            }));

            if (doc == null || result != Result.Ok)
            {
                return result;
            }

            _viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                ClearDocument(doc, selectedItem.itemId);
                _viewModel.InitForTheFirstDocument();
            }));

            var pdfInfo = new PdfFileInfo(doc);
            _viewModel.CurrentPdfFileInfo = pdfInfo;
            _viewModel.CurrentPdfFilePasswordInfo = new PdfFilePasswordInfo
            {
                HasOpenPassword = pdfInfo.HasOpenPassword,
                HasEditPassword = pdfInfo.HasEditPassword,
                PasswordType = pdfInfo.PasswordType
            };

            var pdfFileInfo = new PDFInfo
            {
                filePath = selectedItem.itemId,
                fileName = Path.GetFileName(selectedItem.itemId)
            };
            _viewModel.Icon.SetPDFFileInfo(pdfFileInfo);
            _viewModel.LoadThumbnailAndBookmark(pdfFileInfo, false, false);

            return Result.Ok;
        }

        public void ExecuteShareCommand()
        {
            Task.Factory.StartNewTCS(tcs =>
            {
                ExecuteOpenTask(false).ContinueWithTCSTaskInContext(tcs, task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        if (_viewModel.CurrentPdfDocument == null)
                        {
                            return;
                        }

                        _viewModel.Executor(() => ExecuteShareTask(), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                    }
                    tcs.TrySetResult();
                });
            }); 
        }

        private Task ExecuteShareTask()
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                var curPdfFileName = Path.GetFileName(_viewModel.CurrentPdfFileName);
                if (_viewModel.IsNewPDF && _viewModel.IsDocChanged)
                {
                    curPdfFileName = System.IO.Path.GetFileNameWithoutExtension(curPdfFileName);
                    curPdfFileName = new NameNewPdfDialog(_viewModel.PdfUtilView).RunDialog(curPdfFileName);
                    if (string.IsNullOrEmpty(curPdfFileName))
                    {
                        tcs.TrySetResult();
                        return;
                    }
                }

                var tempFolderName = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                curPdfFileName = Path.Combine(tempFolderName, curPdfFileName);

                using (var stream = File.Create(curPdfFileName))
                {
                    lock (IconSouceImage.LockLoadPdfThumbnail)
                    {
                        _viewModel.CurrentPdfDocument.Save(stream);
                        EDPHelper.SyncEnterpriseId(_viewModel.CurrentPdfFileName, curPdfFileName);
                    }
                }

                if (File.Exists(curPdfFileName))
                {
                    string callfrom = "-cpdfutil";
                    Process current = Process.GetCurrentProcess();
                    string filepath = string.Format("&\"{0}\"", curPdfFileName);
                    string cmd = string.Format("-show {0} -h:{1} /processid {2} {3} -cmd:/notrk", callfrom, _viewModel.PdfUtilView.WindowHandle.ToInt32(), current.Id, filepath);

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

                    try
                    {
                        Process.Start(shareAppletFilePath, cmd);
                    }
                    catch
                    {
                        tcs.TrySetCanceled();
                        return;
                    }
                }

                tcs.TrySetResult();
            });
        }

        public Result ExecuteSaveCommand()
        {
            if (_viewModel.PdfUtilView.IsCalledByWinZipZipPane)
            {
                bool ret = _viewModel.ExecuteSaveToZipCommand(true);

                return ret ? Result.Ok : Result.Cancel;
            }

            if (_viewModel.CurrentPdfDocument == null)
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_NO_OPEN_PDF_SAVE);
                return Result.Error;
            }

            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return Result.Cancel;
            }

            if (!_viewModel.IsDocChanged)
            {
                return Result.Cancel;
            }

            Result result = Result.Ok;
            if (!string.IsNullOrEmpty(_viewModel.CurrentOpenedItem.itemId)
                || (!string.IsNullOrEmpty(_viewModel.CurrentOpenedItem.parentId) && !string.IsNullOrEmpty(_viewModel.CurrentOpenedItem.name)))
            {
                Task task = null;
                _viewModel.Executor(() => task = ExecuteSaveTask(), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                        result = Result.Cancel;
                        break;
                    case TaskStatus.RanToCompletion:
                        result = Result.Ok;
                        break;
                    default:
                        result = Result.Error;
                        break;
                }
            }
            else
            {
                result = ExecuteSaveAsCommand();
            }
            return result;
        }

        public Result ExecuteSaveAsCommand()
        {
            if (_viewModel.CurrentPdfDocument == null)
            {
                Task.Factory.StartNewTCS(tcs =>
                {
                    ExecuteOpenTask(false).ContinueWithTCSTaskInContext(tcs, tsk =>
                    {
                        if (tsk.Status == TaskStatus.RanToCompletion)
                        {
                            if (_viewModel.CurrentPdfDocument == null)
                            {
                                return;
                            }
                        }
                        tcs.TrySetResult();
                    });
                });

                if (_viewModel.CurrentPdfDocument == null)
                {
                    return Result.Error;
                }
            }

            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return Result.Cancel;
            }

            var result = Result.Ok;
            Task task = null;

            _viewModel.Executor(() => task = ExecuteSaveAsTask(), RetryStrategy.Create(false, 0)).IgnoreExceptions();
            switch (task.Status)
            {
                case TaskStatus.Canceled:
                    result = Result.Cancel;
                    break;
                case TaskStatus.RanToCompletion:
                    result = Result.Ok;
                    break;
                default:
                    result = Result.Error;
                    break;
            }
            return result;
        }

        private Task ExecuteSaveAsTask()
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                string title = Properties.Resources.SAVE_PICKER_TITLE;
                string defaultBtn = Properties.Resources.SAVE_PICKER_BUTTON;

                var defaultFolder = PdfHelper.GetSavePickerDefaultFolder();
                var selectedItem = PdfHelper.InitWzCloudItem();
                selectedItem.profile.Id = WzSvcProviderIDs.SPID_UNKNOWN;
                string filters = Properties.Resources.SAVE_PICKER_FILTERS; ;
                var imageName = new List<string>();

                bool ret = WinzipMethods.SaveAsDialog(_viewModel.PdfUtilView.WindowHandle, title, defaultBtn, Path.GetFileName(_viewModel.CurrentPdfFileName), filters, defaultFolder, ref selectedItem);
                if (ret)
                {
                    try
                    {
                        PdfHelper.SetSavePickerDefaultFolder(selectedItem);

                        var fileExt = Path.GetExtension(selectedItem.name).ToLower();
                        if (PdfHelper.IsCloudItem(selectedItem.profile.Id))
                        {
                            ret = DoUploadCloudItem(selectedItem);
                        }
                        else
                        {
                            string itemPath = Path.Combine(selectedItem.parentId, selectedItem.name);
                            bool lockSuccess = false;

                            if (fileExt != PdfHelper.PdfExtension)
                            {
                                if (fileExt == ".zip" || fileExt == ".zipx") // Compress
                                {
                                    var dialog = new EncryptZipFileDialog(_viewModel.PdfUtilView);
                                    if (dialog.Show() != System.Windows.Forms.DialogResult.OK)
                                    {
                                        ret = false;
                                        tcs.TrySetCanceled();
                                        return;
                                    }

                                    string password = dialog.GetPassword();

                                    // Add to zip
                                    lockSuccess = DoLockPdfOperation();
                                    _viewModel.CurrentPdfDocument.Save(_viewModel.CurrentPdfFileName);

                                    // Create zip(x) file in a temp folder
                                    var tempFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                                    string tempZipPath = Path.Combine(tempFolder, selectedItem.name);
                                    var zipItem = PdfHelper.InitCloudItemFromPath(_viewModel.CurrentPdfFileName);
                                    var zipItems = new WzCloudItem4[] { zipItem };
                                    ret = WinzipMethods.SaveToZip(_viewModel.PdfUtilView.WindowHandle, zipItems, 1, tempZipPath, true, true, password);
                                    EDPHelper.SyncEnterpriseId(_viewModel.CurrentPdfFileName, tempZipPath);

                                    if (ret)
                                    {
                                        try
                                        {
                                            // Copy file from temp path to selected place.
                                            EDPHelper.FileCopy(tempZipPath, itemPath, true);
                                        }
                                        catch (UnauthorizedAccessException)
                                        {
                                            FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_SAVE_LOCATION_NOT_SUPPORTED);
                                            ret = false;
                                        }
                                    }

                                    if (Directory.Exists(tempFolder))
                                    {
                                        Directory.Delete(tempFolder, true);
                                    }
                                }
                                else
                                {
                                    // No need to lock and save files when save as another format.
                                    ret = SaveAsOtherFormat(itemPath, imageName);
                                }
                            }
                            else
                            {
                                // Before save to the temp folder, we need to check if the selected path is allowed to be written.
                                // But so far, no good method has been found to perfectly detect the write permission, so we just copy it first,
                                // if the copy can be successful, it means we have write permission.
                                var enterpriseId = EDPAPIHelper.GetEnterpriseId(_viewModel.CurrentPdfFileName);
                                if (CopyToRealSourceFile(itemPath, true))
                                {
                                    lockSuccess = DoLockPdfOperation();

                                    if (Path.GetFileName(itemPath).ToLower() != Path.GetFileName(_viewModel.CurrentPdfFileName).ToLower())
                                    {
                                        string newPath = Path.GetDirectoryName(_viewModel.CurrentPdfFileName);
                                        newPath = Path.Combine(newPath, Path.GetFileName(itemPath));
                                        _viewModel.CurrentPdfDocument.Save(newPath);
                                        EDPHelper.SyncEnterpriseId(_viewModel.CurrentPdfFileName, newPath);

                                        if (File.Exists(_viewModel.CurrentPdfFileName))
                                        {
                                            File.Delete(_viewModel.CurrentPdfFileName);
                                        }

                                        _viewModel.CurrentPdfFileName = newPath;
                                    }
                                    else
                                    {
                                        _viewModel.CurrentPdfDocument.Save(_viewModel.CurrentPdfFileName);
                                    }

                                    bool res = CopyToRealSourceFile(itemPath, true);

                                    if (EDPAPIHelper.IsProcessProtectedByEDP())
                                    {
                                        if (enterpriseId != EDPAPIHelper.GetEnterpriseId())
                                        {
                                            EDPHelper.UnProtectItemDelay(_viewModel.CurrentPdfFileName, 500);
                                            EDPHelper.UnProtectItemDelay(itemPath, 500);
                                        }
                                    }

                                    if (res)
                                    {
                                        RefreshRecentList(selectedItem);
                                        _viewModel.CurrentOpenedItem = selectedItem;
                                    }
                                    else
                                    {
                                        ret = false;
                                    }
                                }
                                else
                                {
                                    ret = false;
                                }
                            }

                            if (lockSuccess)
                            {
                                UpdateCurrentPdfPasswordInfo();
                            }
                        }

                        // If we save CurrentPdfDocument as pdf, then we need to switch Current Pdf 
                        if (ret && fileExt == PdfHelper.PdfExtension)
                        {
                            _viewModel.ResetDocChangedState();
                        }
                    }
                    catch
                    {
                        ret = false;
                    }
                }

                if (ret)
                {
                    TrackHelper.LogPdfSaveAsEvent(selectedItem.name);
                    TrackHelper.LogFileNewEvent(selectedItem.name);
                    tcs.TrySetResult();
                }
                else
                {
                    tcs.TrySetCanceled();
                }
            });
        }

        public void ExecuteSaveToZipCommand()
        {
            _viewModel.ExecuteSaveToZipCommand(false);
        }

        private bool DoUploadCloudItem(WzCloudItem4 selectedItem)
        {
            var fileExt = Path.GetExtension(selectedItem.name).ToLower();
            var tempFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
            var uploadFilePath = Path.Combine(tempFolder, selectedItem.name);
            var imageNameList = new List<string>();
            bool lockSuccess = false;

            if (fileExt != PdfHelper.PdfExtension)
            {
                if (fileExt == ".zip" || fileExt == ".zipx") // Compress
                {
                    var dialog = new EncryptZipFileDialog(_viewModel.PdfUtilView);
                    if (dialog.Show() != System.Windows.Forms.DialogResult.OK)
                    {
                        return false;
                    }

                    string password = dialog.GetPassword();

                    // Add to zip
                    lockSuccess = DoLockPdfOperation();
                    _viewModel.CurrentPdfDocument.Save(_viewModel.CurrentPdfFileName);

                    var zipItem = PdfHelper.InitCloudItemFromPath(_viewModel.CurrentPdfFileName);
                    var zipItems = new WzCloudItem4[] { zipItem };
                    bool res = WinzipMethods.SaveToZip(_viewModel.PdfUtilView.WindowHandle, zipItems, 1, uploadFilePath, true, true, password);
                    EDPHelper.SyncEnterpriseId(_viewModel.CurrentPdfFileName, uploadFilePath);

                    if (res)
                    {
                        var uploadItem = PdfHelper.InitCloudItemFromPath(uploadFilePath);
                        var uploadItems = new WzCloudItem4[] { uploadItem };
                        int count = 1;
                        res = WinzipMethods.UploadToCloud(_viewModel.PdfUtilView.WindowHandle, uploadItems.ToArray(), ref count, selectedItem, false, false);
                    }

                    if (Directory.Exists(tempFolder))
                    {
                        Directory.Delete(tempFolder, true);
                    }

                    return res;
                }
                else
                {
                    // No need to lock and save files when save as another format.
                    if (!SaveAsOtherFormat(uploadFilePath, imageNameList))
                    {
                        Directory.Delete(tempFolder, true);
                        return false;
                    }
                }
            }
            else
            {
                lockSuccess = DoLockPdfOperation();
                var enterpriseId = EDPAPIHelper.GetEnterpriseId(_viewModel.CurrentPdfFileName);
                if (selectedItem.name.ToLower() != Path.GetFileName(_viewModel.CurrentPdfFileName).ToLower())
                {
                    string newPath = Path.GetDirectoryName(_viewModel.CurrentPdfFileName);
                    newPath = Path.Combine(newPath, selectedItem.name);
                    _viewModel.CurrentPdfDocument.Save(newPath);

                    if (File.Exists(_viewModel.CurrentPdfFileName))
                    {
                        File.Delete(_viewModel.CurrentPdfFileName);
                    }

                    _viewModel.CurrentPdfFileName = newPath;
                }
                else
                {
                    _viewModel.CurrentPdfDocument.Save(_viewModel.CurrentPdfFileName);
                }

                if (EDPAPIHelper.IsProcessProtectedByEDP())
                {
                    if (enterpriseId != EDPAPIHelper.GetEnterpriseId())
                    {
                        Thread.Sleep(500);
                        EDPAPIHelper.UnProtectItem(_viewModel.CurrentPdfFileName);
                    }
                }

                var uploadItem = PdfHelper.InitCloudItemFromPath(_viewModel.CurrentPdfFileName);
                var uploadItems = new WzCloudItem4[] { uploadItem };
                int count = 1;
                bool res = WinzipMethods.UploadToCloud(_viewModel.PdfUtilView.WindowHandle, uploadItems, ref count, selectedItem, false, false);

                if (res && count > 0)
                {
                    _viewModel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        RefreshRecentList(uploadItems[0]);
                        _viewModel.CurrentOpenedItem = selectedItem;
                    }));
                }

                Directory.Delete(tempFolder, true);
                return res;
            }

            bool ret = true;
            if (fileExt == ".bmp" || fileExt == ".jpg" || fileExt == ".png" || fileExt == ".tif")
            {
                if (imageNameList.Count != 0)
                {
                    List<WzCloudItem4> uploadItems = new List<WzCloudItem4>();
                    foreach (var name in imageNameList)
                    {
                        var uploadItem = PdfHelper.InitCloudItemFromPath(name);
                        uploadItems.Add(uploadItem);
                    }
                    int count = uploadItems.Count;
                    ret = WinzipMethods.UploadToCloud(_viewModel.PdfUtilView.WindowHandle, uploadItems.ToArray(), ref count, selectedItem, false, false);
                }
            }
            else
            {
                if (File.Exists(uploadFilePath))
                {
                    var uploadItem = PdfHelper.InitCloudItemFromPath(uploadFilePath);
                    var uploadItems = new WzCloudItem4[] { uploadItem };
                    int count = 1;
                    ret = WinzipMethods.UploadToCloud(_viewModel.PdfUtilView.WindowHandle, uploadItems, ref count, selectedItem, false, false);
                }
            }

            if (lockSuccess)
            {
                UpdateCurrentPdfPasswordInfo();
            }

            Directory.Delete(tempFolder, true);
            return ret;
        }

        private Task ExecuteSaveTask()
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                bool ret = true;
                try
                {
                    if (PdfHelper.IsCloudItem(_viewModel.CurrentOpenedItem.profile.Id))
                    {
                        ret = DoUploadCloudItem(_viewModel.CurrentOpenedItem);
                    }
                    else
                    {
                        bool lockSuccess = false;
                        string itemPath = _viewModel.CurrentOpenedItem.itemId;
                        if (string.IsNullOrEmpty(itemPath))
                        {
                            if (!string.IsNullOrEmpty(_viewModel.CurrentOpenedItem.parentId) && !string.IsNullOrEmpty(_viewModel.CurrentOpenedItem.name))
                            {
                                itemPath = Path.Combine(_viewModel.CurrentOpenedItem.parentId, _viewModel.CurrentOpenedItem.name);
                            }
                        }

                        // Before save to the temp folder, we need to check if the selected path is allowed to be written.
                        // But so far, no good method has been found to perfectly detect the write permission, so we just copy it first,
                        // if the copy can be successful, it means we have write permission.
                        var enterpriseId = EDPAPIHelper.GetEnterpriseId(_viewModel.CurrentPdfFileName);
                        if (CopyToRealSourceFile(itemPath, false))
                        {
                            lockSuccess = DoLockPdfOperation();

                            using (var pdfStream = File.Create(_viewModel.CurrentPdfFileName))
                            {
                                _viewModel.CurrentPdfDocument.Save(pdfStream);
                                pdfStream.Flush();
                                pdfStream.Close();
                            }

                            if (!CopyToRealSourceFile(itemPath, false))
                            {
                                ret = false;
                            }
                        }
                        else
                        {
                            ret = false;
                        }

                        if (EDPAPIHelper.IsProcessProtectedByEDP())
                        {
                            if (enterpriseId != EDPAPIHelper.GetEnterpriseId())
                            {
                                EDPHelper.UnProtectItemDelay(_viewModel.CurrentPdfFileName, 500);
                                EDPHelper.UnProtectItemDelay(itemPath, 500);
                            }
                        }

                        if (lockSuccess)
                        {
                            UpdateCurrentPdfPasswordInfo();
                        }
                    }
                }
                catch
                {
                    ret = false;
                }

                if (ret)
                {
                    // update recent files date info
                    foreach (var item in _viewModel.RecentList)
                    {
                        item.UpdateFileInfo();
                    }

                    _viewModel.ResetDocChangedState();
                    tcs.TrySetResult();
                }
                else
                {
                    tcs.TrySetCanceled();
                }
            });
        }

        private void UpdateCurrentPdfPasswordInfo()
        {
            // after saving pdf with lock changed, update the CurrentPdfFilePasswordInfo.
            if (_viewModel.CurrentPdfFilePasswordInfo.HasEditPassword != _viewModel.CurrentPdfLockStatus.isSetPermissionPassword
            || _viewModel.CurrentPdfFilePasswordInfo.HasOpenPassword != _viewModel.CurrentPdfLockStatus.isSetOpenPassword)
            {
                PdfFileInfo pdfFileInfo;
                if (_viewModel.CurrentPdfLockStatus.isSetOpenPassword)
                {
                    pdfFileInfo = new PdfFileInfo(_viewModel.CurrentPdfDocument.FileName, _viewModel.CurrentPdfLockStatus.openPassword);
                }
                else
                {
                    pdfFileInfo = new PdfFileInfo(_viewModel.CurrentPdfDocument.FileName);
                }

                _viewModel.CurrentPdfFilePasswordInfo = new PdfFilePasswordInfo
                {
                    HasOpenPassword = pdfFileInfo.HasOpenPassword,
                    HasEditPassword = pdfFileInfo.HasEditPassword,
                    PasswordType = pdfFileInfo.PasswordType
                };
            }
        }

        public void ExecuteImportFilesCommand()
        {
            _viewModel.Executor(() => _viewModel.ExecuteImportFilesTask(), RetryStrategy.Create(false, 0)).IgnoreExceptions();
        }

        public void ExecuteImportFromCameraCommand()
        {
            _viewModel.Executor(() => _viewModel.ExecuteImportFromCameraTask(), RetryStrategy.Create(false, 0)).IgnoreExceptions();
        }

        public void ExecuteImportFromScannerCommand()
        {
            _viewModel.Executor(() => _viewModel.ExecuteImportFromScannerTask(), RetryStrategy.Create(false, 0)).IgnoreExceptions();
        }

        public bool CanExecuteExtractImagesCommand()
        {
            return true;
        }

        public void ExecuteExtractImagesCommand()
        {
            _viewModel.ExecuteExtractImagesCommand();
        }

        public bool CanExecuteExtractPagesCommand()
        {
            return true;
        }

        public void ExecuteExtractPagesCommand()
        {
            _viewModel.ExecuteExtractPagesCommand();
        }

        // Set or change passwords and permissions for a PDF file.
        public void ExecuteLockPdfCommand()
        {
            Task.Factory.StartNewTCS(tcs =>
            {
                ExecuteOpenTask(false).ContinueWithTCSTaskInContext(tcs, task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        if (_viewModel.CurrentPdfDocument == null)
                        {
                            return;
                        }

                        if (_viewModel.IsPdfSignedOrCertified)
                        {
                            FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.PDF_SIGNED_OR_CERTIFIED_WARNING);
                            return;
                        }

                        if (_viewModel.CurrentPdfLockStatus.isSetOpenPassword || _viewModel.CurrentPdfLockStatus.isSetPermissionPassword)
                        {
                            // If the PDF file is set locked permission and not save, prompt user.
                            TASKDIALOG_BUTTON[] buttons = new[]
                            {
                                new TASKDIALOG_BUTTON()
                                {
                                    id = (int)System.Windows.Forms.DialogResult.Yes,
                                    text = Properties.Resources.BUTTON_CHANGE_PERMISSIONS + "\n" + Properties.Resources.BUTTON_CHANGE_PERMISSIONS_TIP
                                },
                                new TASKDIALOG_BUTTON()
                                {
                                    id = (int)System.Windows.Forms.DialogResult.No,
                                    text = Properties.Resources.BUTTON_CANCEL + "\n" + Properties.Resources.BUTTON_CANCEL_PERMISSION_CHANGE_TIP
                                }
                            };

                            int taskDialogWidth = 260;
                            var taskDialog = new TaskDialog(Properties.Resources.PDF_UTILITY_TITLE, Properties.Resources.INFORMATION_PDF_ALREADY_LOCKED, null, null, buttons, SystemIcons.Information, taskDialogWidth);
                            TaskDialogResult taskDialogResult = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);
                            if (taskDialogResult.dialogResult != System.Windows.Forms.DialogResult.Yes)
                            {
                                return;
                            }
                        }

                        if (_viewModel.CurrentPdfFilePasswordInfo.HasEditPassword && _viewModel.CurrentPdfFilePasswordInfo.PasswordType != PasswordType.Owner)
                        {
                            // In fact, a PDF file can be encrypted(locked) directly without owner password using Aspose.Pdf.
                            // However, for those files with owner password, it is necessary to verify the correctness of the owner password.
                            string PermissionPassword = string.Empty;
                            if (!GetPassword(Path.GetFileName(_viewModel.CurrentPdfFileName), false, ref PermissionPassword))
                            {
                                return;
                            }
                            if (!CheckPermissionPasswordCorrection(PermissionPassword))
                            {
                                return;
                            }
                            else
                            {
                                var pdfLockStatus = _viewModel.CurrentPdfLockStatus;
                                pdfLockStatus.permissionPassword = PermissionPassword;
                                _viewModel.CurrentPdfLockStatus = pdfLockStatus;
                            }
                        }

                        var dialog = new LockPDFDialog(_viewModel.PdfUtilView);
                        if (dialog.Show() == System.Windows.Forms.DialogResult.OK)
                        {
                            // Keep the lock status, and the lock will take effect after save.
                            _viewModel.CurrentPdfLockStatus = new PdfLockStatus
                            {
                                isSetOpenPassword = _viewModel.LockPDFViewModel.IsSetOpenPassword,
                                isSetPermissionPassword = _viewModel.LockPDFViewModel.IsSetPermissionPassword,
                                openPassword = _viewModel.LockPDFViewModel.OpenPassword,
                                permissionPassword = _viewModel.LockPDFViewModel.PermissionPassword,
                                permissions = ParsePermissionsFromDialog()
                            };
                            _viewModel.IsCurrentPdfLockChanged = true;
                            FlatMessageWindows.DisplayInformationMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.INFORMATION_PDF_HAS_BEEN_LOCKED);
                            _viewModel.NotifyDocChanged();

                            TrackHelper.LogLockSecurityEvent(_viewModel.LockPDFViewModel);
                        }
                    }
                    tcs.TrySetResult();
                });
            });
        }

        // Remove all the security settings of the PDF file, including those that have taken effect and those that have not taken effect.
        public void ExecuteUnlockPdfCommand()
        {
            Task.Factory.StartNewTCS(tcs =>
            {
                ExecuteOpenTask(false).ContinueWithTCSTaskInContext(tcs, task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        if (_viewModel.CurrentPdfDocument == null)
                        {
                            return;
                        }

                        if (_viewModel.IsPdfSignedOrCertified)
                        {
                            FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.PDF_SIGNED_OR_CERTIFIED_WARNING);
                            return;
                        }

                        if (!_viewModel.CurrentPdfFilePasswordInfo.HasOpenPassword && !_viewModel.CurrentPdfFilePasswordInfo.HasEditPassword && !_viewModel.CurrentPdfLockStatus.isSetOpenPassword && !_viewModel.CurrentPdfLockStatus.isSetPermissionPassword)
                        {
                            FlatMessageWindows.DisplayInformationMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.INFORMATION_PDF_IS_NOT_LOCKED);
                            return;
                        }
                        try
                        {
                            if (_viewModel.CurrentPdfFilePasswordInfo.HasEditPassword && _viewModel.CurrentPdfFilePasswordInfo.PasswordType != PasswordType.Owner)
                            {
                                // In fact, a PDF file can be decrypted directly without owner password using Aspose.Pdf.
                                // However, for those files with owner password, it is necessary to verify the correctness of the owner password.
                                var dialog = new UnlockPDFDialog(_viewModel.PdfUtilView);
                                if (dialog.Show() == System.Windows.Forms.DialogResult.OK)
                                {
                                    if (!CheckPermissionPasswordCorrection(dialog.Password))
                                    {
                                        return;
                                    }
                                    _viewModel.CurrentPdfDocument.Decrypt();
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else
                            {
                                _viewModel.CurrentPdfDocument.Decrypt();
                            }

                            if (_viewModel.CurrentPdfLockStatus.isSetOpenPassword || _viewModel.CurrentPdfLockStatus.isSetPermissionPassword)
                            {
                                _viewModel.CurrentPdfLockStatus = new PdfLockStatus();
                                _viewModel.CurrentPdfFilePasswordInfo = new PdfFilePasswordInfo
                                {
                                    HasOpenPassword = _viewModel.CurrentPdfFileInfo.HasOpenPassword,
                                    HasEditPassword = _viewModel.CurrentPdfFileInfo.HasEditPassword,
                                    PasswordType = _viewModel.CurrentPdfFileInfo.PasswordType
                                };
                            }
                        }
                        catch (Exception)
                        {
                            FlatMessageWindows.DisplayInformationMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.WARNING_UNABLE_TO_UNLOCK_THIS_PDF);
                            return;
                        }

                        FlatMessageWindows.DisplayInformationMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.INFORMATION_PDF_HAS_BEEN_UNLOCKED);
                        _viewModel.NotifyDocChanged();

                        TrackHelper.LogUnlockSecurityEvent();
                    }
                    tcs.TrySetResult();
                });
            });
        }

        public void ExecuteOpenAdoutCommand()
        {
            var dialog = new AboutDialog(_viewModel.PdfUtilView);
            dialog.ShowDialog();
        }

        public void ExecuteSignPdfCommand()
        {
            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyPagesPermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            Task.Factory.StartNewTCS(tcs =>
            {
                ExecuteOpenTask(false).ContinueWithTCSTaskInContext(tcs, task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        if (_viewModel.CurrentPdfDocument == null)
                        {
                            return;
                        }

                        if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
                        {
                            return;
                        }

                        _viewModel.Executor(() => ExecuteSignPdfTask(), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                    }
                    tcs.TrySetResult();
                });
            });
        }

        private Task ExecuteSignPdfTask()
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                var spids = new WzSvcProviderIDs[] { WzSvcProviderIDs.SPID_SIGN_PDF_TRANSFORM };

                var selectedPath = new string[] { _viewModel.CurrentPdfFileName };

                var resultFiles = new List<WinzipMethods.ConvertFileResultPath>();
                _viewModel.CurrentPdfDocument.Save(_viewModel.CurrentPdfFileName);

                bool ret = WinzipMethods.ConvertFile(_viewModel.PdfUtilView.WindowHandle, spids, 1, selectedPath, 1, null, 0, null, resultFiles, true, true, true);

                if (ret && resultFiles.Count > 0)
                {
                    TrackHelper.LogSignSecurityEvent();

                    string resultFile = resultFiles[0].path;
                    var doc = OpenCurrentGeneratedDocument(resultFile);
                    _viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        ClearDocument(doc, resultFile);
                        _viewModel.InitForTheFirstDocument();
                        var pdfInfo = new PdfFileInfo(doc);
                        _viewModel.CurrentPdfFileInfo = pdfInfo;

                        var pdfFileInfo = new PDFInfo
                        {
                            filePath = resultFile,
                            fileName = Path.GetFileName(resultFile)
                        };

                        _viewModel.Icon.SetPDFFileInfo(pdfFileInfo);
                        _viewModel.LoadThumbnailAndBookmark(pdfFileInfo, false, true);
                    }));
                }

                tcs.TrySetResult();
            });
        }

        public void ExecutePdfSettingsCommand()
        {
            _viewModel.Executor(() => ExecutePdfSettingsTask(), RetryStrategy.Create(false, 0)).IgnoreExceptions();
        }

        private Task ExecutePdfSettingsTask()
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                const int spidEndingSymbol = -1;
                var spids = new int[] { (int)WzSvcProviderIDs.SPID_PDF2DOC_TRANSFORM, (int)WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM, (int)WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM, (int)WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM, (int)WzSvcProviderIDs.SPID_SIGN_PDF_TRANSFORM, spidEndingSymbol };
                WinzipMethods.ShowConversionSettings(_viewModel.PdfUtilView.WindowHandle, spids);
                tcs.TrySetResult();

                TrackHelper.LogPdfSettingsEvent();
            });
        }

        public bool StartAdminProgressAndChangeIntegration()
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
                    text = Properties.Resources.ADMIN_REQUIRED_DLG_CANCEL + "\n" + Properties.Resources.ADMIN_REQUIRED_DLG_CANCEL_TIP
                }
            };

            int taskDialogWidth = 220;
            var taskDialog = new TaskDialog(Properties.Resources.PDF_UTILITY_TITLE, Properties.Resources.ADMIN_REQUIRED_DLG_TITLE,
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

            _viewModel.NotifyDefaultSet();
            return true;
        }

        public void ChangeIntegrationWithoutAdmin()
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

        public void RemoveLinkFile(int nFolder, Environment.SpecialFolder specialFolder)
        {
            int size = 260;
            StringBuilder path = new StringBuilder(size);
            NativeMethods.SHGetSpecialFolderPath(IntPtr.Zero, path, nFolder, false);
            string folderPath = path.ToString();
            if (Directory.Exists(folderPath))
            {
                // remove shortcut in public folder
                string link = Path.Combine(folderPath, Properties.Resources.PDF_UTILITY_TITLE + ".lnk");
                if (File.Exists(link))
                {
                    File.Delete(link);
                }
            }

            var specialFolderPath = Environment.GetFolderPath(specialFolder);
            if (Directory.Exists(specialFolderPath))
            {
                // remove shortcut in current user's folder
                string link = Path.Combine(specialFolderPath, Properties.Resources.PDF_UTILITY_TITLE + ".lnk");
                if (File.Exists(link))
                {
                    File.Delete(link);
                }
            }
        }

        public void ExecuteHelpCommand()
        {
            if (_viewModel != null)
            {
                _viewModel.FakeRibbonTabViewModel.ExecuteOpenAdoutCommand();
            }
        }

        public void ExecuteWindowsIntegrationCommand()
        {
            bool canAddDesktopIcon = false;
            bool canAddStartMenu = false;
            int addFileAssociation = 0;
            RegeditOperation.GetWinzipPolicyIntegrationRegistryValue(ref canAddDesktopIcon, ref canAddStartMenu, ref addFileAssociation);

            if (!canAddDesktopIcon && !canAddStartMenu && addFileAssociation == 0)
            {
                // integration change is disabled by administrator
                FlatMessageWindows.DisplayInformationMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.INTEGRATION_DISABLE_BY_ADMIN);
            }
            else
            {
                var dialog = new IntegrationDialog(_viewModel.PdfUtilView, canAddDesktopIcon, canAddStartMenu, addFileAssociation, _viewModel.PdfUtilView.WindowHandle, _isFirstTimeOpenIntegration);
                if (dialog.Show() == System.Windows.Forms.DialogResult.OK)
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
                        _isFirstTimeOpenIntegration = false;
                        RegeditOperation.DeleteAdminConfigRegistryStringValue();
                    }
                }
            }
        }

        public void ExecuteSignatureCommand()
        {
            Task.Factory.StartNewTCS(tcs =>
            {
                ExecuteOpenTask(false).ContinueWithTCSTaskInContext(tcs, task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        if (_viewModel.CurrentPdfDocument == null)
                        {
                            return;
                        }

                        if (!_viewModel.IsSignatureBarShow)
                        {
                            FlatMessageWindows.DisplaySpecifiedFieldsDetectedDialog(_viewModel.PdfUtilView.WindowHandle);
                        }

                        _viewModel.IsSignatureBarShow = !_viewModel.IsSignatureBarShow;
                    }
                    tcs.TrySetResult();
                });
            });
        }

        public void ExecuteCommentPdfCommand()
        {
            Task.Factory.StartNewTCS(tcs =>
            {
                ExecuteOpenTask(false).ContinueWithTCSTaskInContext(tcs, task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        if (_viewModel.CurrentPdfDocument == null)
                        {
                            return;
                        }

                        _viewModel.IsCommentPaneShow = !_viewModel.IsCommentPaneShow;
                    }
                    tcs.TrySetResult();
                });
            });


        }

        public Document OpenDocument(string name, string originalName, ref Result result, bool updateLockInfo, bool fromShellExtension)
        {
            if (!File.Exists(name))
            {
                result = Result.Error;
                return null;
            }

            var fileInfo = new PdfFileInfo();
            fileInfo.BindPdf(name);
            var pdfLockStatus = new PdfLockStatus();

            try
            {
                Document document = null;
                if (fileInfo.HasOpenPassword)
                {
                    string password = string.Empty;
                    var fileName = string.IsNullOrEmpty(originalName) ? Path.GetFileName(name) : Path.GetFileName(originalName);
                    while (document == null)
                    {
                        try
                        {
                            if (!GetPassword(fileName, true, ref password))
                            {
                                result = Result.Cancel;
                                break;
                            }
                            document = new Document(name, password);
                            fileInfo.Close();
                            fileInfo.BindPdf(document);

                            pdfLockStatus.isSetOpenPassword = true;
                            pdfLockStatus.permissions = (Permissions)document.Permissions;
                            if (fileInfo.PasswordType == PasswordType.Owner)
                            {
                                pdfLockStatus.isSetPermissionPassword = true;
                                pdfLockStatus.permissionPassword = password;
                            }
                            else
                            {
                                pdfLockStatus.openPassword = password;
                                if (fileInfo.HasEditPassword)
                                {
                                    pdfLockStatus.isSetPermissionPassword = true;
                                }
                            }
                            if (updateLockInfo)
                            {
                                _viewModel.CurrentPdfLockStatus = pdfLockStatus;
                                _viewModel.CurrentPdfSourceLockStatus = pdfLockStatus;
                            }
                            result = Result.Ok;
                        }
                        catch
                        {
                            if (FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_PASSWORD_INCORRECT))
                            {
                                result = Result.Error;
                            }
                            else
                            {
                                result = Result.Cancel;
                                break;
                            }
                        }
                    }
                    return document;
                }
                else
                {
                    document = new Document(name);
                    pdfLockStatus.permissions = (Permissions)document.Permissions;

                    if (fileInfo.HasEditPassword)
                    {
                        pdfLockStatus.isSetPermissionPassword = true;
                    }

                    if (updateLockInfo)
                    {
                        _viewModel.CurrentPdfLockStatus = pdfLockStatus;
                        _viewModel.CurrentPdfSourceLockStatus = pdfLockStatus;
                    }
                    result = Result.Ok;
                    return document;
                }
            }
            catch (NullReferenceException ex)
            {
                if (fromShellExtension)
                {
                    throw ex;
                }
                else
                {
                    if (FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_FILE_INVALID_WARNING, Path.GetFileName(name))))
                    {
                        result = Result.Error;
                    }
                    else
                    {
                        result = Result.Cancel;
                    }
                    return null;
                }
            }
        }

        public bool GetPassword(string fileName, bool isOpen, ref string password)
        {
            var dialog = new PasswordAcquireDialog(_viewModel.PdfUtilView, fileName, isOpen);
            if (dialog.Show())
            {
                password = dialog.Password;
                return true;
            }
            return false;
        }

        // Do lock PDF when save this PDF file.
        private bool DoLockPdfOperation()
        {
            var currentPdf = _viewModel.CurrentPdfDocument;
            var lockStatus = _viewModel.CurrentPdfLockStatus;

            if (_viewModel.IsCurrentPdfLockChanged)
            {
                try
                {
                    if (lockStatus.isSetOpenPassword && lockStatus.isSetPermissionPassword)
                    {
                        currentPdf.Encrypt(lockStatus.openPassword, lockStatus.permissionPassword, lockStatus.permissions, CryptoAlgorithm.AESx128);
                        _viewModel.IsCurrentPdfLockChanged = false;
                    }
                    else if (lockStatus.isSetOpenPassword)
                    {
                        currentPdf.Encrypt(lockStatus.openPassword, null, DocumentPrivilege.AllowAll, CryptoAlgorithm.AESx128, false);
                        _viewModel.IsCurrentPdfLockChanged = false;
                    }
                    else if (lockStatus.isSetPermissionPassword)
                    {
                        currentPdf.Encrypt(string.Empty, lockStatus.permissionPassword, lockStatus.permissions, CryptoAlgorithm.AESx128);
                        _viewModel.IsCurrentPdfLockChanged = false;
                    }

                    _viewModel.CurrentPdfSourceLockStatus = _viewModel.CurrentPdfLockStatus;
                }
                catch (Exception)
                {
                    FlatMessageWindows.DisplayInformationMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.WARNING_UNABLE_TO_LOCK_THIS_PDF);
                    return false;
                }
            }
            return true;
        }

        public bool CheckPermissionPasswordCorrection(string password)
        {
            try
            {
                var document = new Document(_viewModel.CurrentPdfFileName, password);
                PdfFileInfo fileInfo = new PdfFileInfo(document);
                if (fileInfo.PasswordType != PasswordType.Owner)
                {
                    throw new FileLoadException();
                }
            }
            catch
            {
                bool isCryptoAlgorithm256 = false;
                try
                {
                    // "Lock PDF" will use CryptoAlgorithm.AESx128, but the value of CurrentPdfDocument.CryptoAlgorithm
                    // will become unreadable after saving the locked file, so we try-catch this error.
                    if (_viewModel.CurrentPdfDocument.CryptoAlgorithm == CryptoAlgorithm.AESx256)
                    {
                        isCryptoAlgorithm256 = true;
                    }
                }
                catch
                {
                    isCryptoAlgorithm256 = false;
                }

                if (isCryptoAlgorithm256)
                {
                    FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_NOT_SUPPORTED_ENCRYPTION);
                }
                else
                {
                    FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_UNABLE_TO_UNLOCK_PDF);
                }
                return false;
            }

            return true;
        }

        public Permissions ParsePermissionsFromDialog()
        {
            Permissions permission = 0;
            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword)
            {
                if (_viewModel.LockPDFViewModel.IsAllowScreenReaderChecked)
                {
                    permission = permission | Permissions.ExtractContentWithDisabilities;
                }

                if (_viewModel.LockPDFViewModel.IsAllowCopyingChecked)
                {
                    permission = permission | Permissions.ExtractContent | Permissions.ExtractContentWithDisabilities;
                }

                switch (_viewModel.LockPDFViewModel.CurAllowPrinting)
                {
                    case AllowPrinting.LowResolution:
                        permission = permission | Permissions.PrintDocument;
                        break;
                    case AllowPrinting.HighResolution:
                        permission = permission | Permissions.PrintingQuality | Permissions.PrintDocument;
                        break;
                    default:
                        break;
                }

                switch (_viewModel.LockPDFViewModel.CurAllowChanges)
                {
                    case AllowChanges.ModifyPagesPermission:
                        permission = permission | Permissions.AssembleDocument;
                        break;
                    case AllowChanges.ModifySignaturePermission:
                        permission = permission | Permissions.FillForm;
                        break;
                    case AllowChanges.ModifyCommentsPermission:
                        permission = permission | Permissions.ModifyTextAnnotations | Permissions.FillForm;
                        break;
                    case AllowChanges.AnyExceptExtracting:
                        permission = permission | Permissions.ModifyContent | Permissions.ModifyTextAnnotations | Permissions.FillForm;
                        break;
                    default:
                        break;
                }
            }
            return permission;
        }

        public void ExecutePrintCommand()
        {
            Task.Factory.StartNewTCS(tcs =>
            {
                ExecuteOpenTask(false).ContinueWithTCSTaskInContext(tcs, task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        if (_viewModel.CurrentPdfDocument == null)
                        {
                            return;
                        }

                        if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && _viewModel.LockPDFViewModel.CurAllowPrinting == AllowPrinting.None)
                        {
                            FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                            return;
                        }

                        var printView = new PrintView(_viewModel.PdfUtilView);
                        if (printView.ShowWindow())
                        {
                            var printDlg = new System.Windows.Controls.PrintDialog();
                            var ticket = new System.Printing.PrintTicket();
                            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && _viewModel.LockPDFViewModel.CurAllowPrinting == AllowPrinting.LowResolution)
                            {
                                ticket.PageResolution = new System.Printing.PageResolution(System.Printing.PageQualitativeResolution.Default);
                            }
                            else
                            {
                                ticket.PageResolution = new System.Printing.PageResolution(System.Printing.PageQualitativeResolution.High);
                            }

                            printDlg.PrintTicket = ticket;
                            if (printDlg.ShowDialog() == true)
                            {
                                string docFilePath = GetPrintDocumentFilePath(printView.CurPageSelection, printView.PageRangeFrom, printView.PageRangeTo);
                                if (docFilePath != string.Empty)
                                {
                                    string printDescription = Path.GetFileName(_viewModel.CurrentPdfFileName);
                                    var xpsDocument = new XpsDocument(docFilePath, FileAccess.ReadWrite);
                                    var fixedDDocSequence = xpsDocument.GetFixedDocumentSequence();
                                    printDlg.PrintDocument(fixedDDocSequence.DocumentPaginator, printDescription);
                                    xpsDocument.Close();
                                    File.Delete(docFilePath);
                                    TrackHelper.LogPdfPrintEvent(printView.CurPageSelection);
                                }
                            }
                        }
                    }
                });
            });
        }

        private string GetPrintDocumentFilePath(PageSelectionEnum pageSelection, int pageRangeFrom = 0, int pageRangeTo = 0)
        {
            string tempPdfPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(_viewModel.Icon.FileName) + "-temp.pdf");
            if (_viewModel.CurrentPdfSourceLockStatus.isSetOpenPassword)
            {
                // PdfFileEditor cannot extract files with open password, so we decrypt and save it to a temp file and do extract
                var password = string.IsNullOrEmpty(_viewModel.CurrentPdfSourceLockStatus.openPassword) ?
                    _viewModel.CurrentPdfSourceLockStatus.permissionPassword : _viewModel.CurrentPdfSourceLockStatus.openPassword;
                var tempDoc = new Document(_viewModel.CurrentPdfFileName, password);
                if (tempDoc == null)
                {
                    return string.Empty;
                }

                tempDoc.Decrypt();
                tempDoc.Save(tempPdfPath);
            }
            else
            {
                _viewModel.CurrentPdfDocument.Save(tempPdfPath);
            }

            if (!File.Exists(tempPdfPath))
            {
                return string.Empty;
            }

            if (pageSelection == PageSelectionEnum.CurrentPage)
            {
                int curPageIndex = _viewModel.CurrentPdfDocument.Pages.IndexOf(_viewModel.CurPreviewIconItem.GetPage());
                var pdfEditor = new PdfFileEditor();
                var selectedPageList = new List<int>() { curPageIndex };
                pdfEditor.Extract(tempPdfPath, selectedPageList.ToArray(), tempPdfPath);
                var curXpsFileName = GetTempXpsFileName();
                var doc = new Document(tempPdfPath);

                if (doc != null)
                {
                    var saveOption = new XpsSaveOptions();
                    doc.Save(curXpsFileName, saveOption);
                    if (File.Exists(curXpsFileName))
                    {
                        File.Delete(tempPdfPath);
                        return curXpsFileName;
                    }

                    doc.Dispose();
                }

            }
            else if (pageSelection == PageSelectionEnum.SelectedPages)
            {
                var selectedPageList = new List<int>();
                foreach (var item in _viewModel.ThumbnailListView.SelectedItems)
                {
                    var icon = item as IconItem;
                    if (icon != null)
                    {
                        selectedPageList.Add(_viewModel.GetPageIndex(icon.GetPage()));
                    }
                }

                selectedPageList.Sort();
                var pdfEditor = new PdfFileEditor();
                pdfEditor.Extract(tempPdfPath, selectedPageList.ToArray(), tempPdfPath);
                var curXpsFileName = GetTempXpsFileName();
                var doc = new Document(tempPdfPath);

                if (doc != null)
                {
                    var saveOption = new XpsSaveOptions();
                    doc.Save(curXpsFileName, saveOption);
                    if (File.Exists(curXpsFileName))
                    {
                        File.Delete(tempPdfPath);
                        return curXpsFileName;
                    }

                    doc.Dispose();
                }
            }
            else if (pageSelection == PageSelectionEnum.AllPages)
            {
                var curXpsFileName = GetTempXpsFileName();
                var saveOption = new XpsSaveOptions();
                _viewModel.CurrentPdfDocument.Save(curXpsFileName, saveOption);
                if (File.Exists(curXpsFileName))
                {
                    File.Delete(tempPdfPath);
                    return curXpsFileName;
                }
            }
            else if (pageSelection == PageSelectionEnum.PageRange)
            {
                if (pageRangeFrom == 0 || pageRangeTo == 0)
                {
                    File.Delete(tempPdfPath);
                    return string.Empty;
                }

                List<int> selectedPageList;
                if (pageRangeFrom < pageRangeTo)
                {
                    selectedPageList = Enumerable.Range(pageRangeFrom, pageRangeTo - pageRangeFrom + 1).ToList();
                }
                else
                {
                    selectedPageList = Enumerable.Range(pageRangeTo, pageRangeFrom - pageRangeTo + 1).ToList();
                }

                var pdfEditor = new PdfFileEditor();
                pdfEditor.Extract(tempPdfPath, selectedPageList.ToArray(), tempPdfPath);
                var curXpsFileName = GetTempXpsFileName();
                var doc = new Document(tempPdfPath);

                if (doc != null)
                {
                    var saveOption = new XpsSaveOptions();
                    doc.Save(curXpsFileName, saveOption);
                    if (File.Exists(curXpsFileName))
                    {
                        File.Delete(tempPdfPath);
                        return curXpsFileName;
                    }

                    doc.Dispose();
                }
            }

            File.Delete(tempPdfPath);
            return string.Empty;
        }

        private string GetTempXpsFileName()
        {
            const string xpsSuffix = ".xps";
            var curPdfFileName = Path.GetFileName(_viewModel.CurrentPdfFileName) + xpsSuffix;
            curPdfFileName = Path.Combine(ApplicationHelper.LocalUserDataPath, curPdfFileName);
            return curPdfFileName;
        }

        public void ClearDocument(Document newDoc, string newPdfName, bool isCloseDoc = false)
        {
            _viewModel.Icon.IconSources.Clear();
            _viewModel.Icon.SetPDFFileInfo(new PDFInfo());
            _viewModel.RootBookmarkTree.BookmarkItems.Clear();
            _viewModel.CurrentPdfDocument = newDoc;
            _viewModel.CurrentPdfFileName = newPdfName;
            _viewModel.CancelLoadingExecuteAction();
            _viewModel.PdfUtilView.AdjustBookmarkTabCursor(true);
            _viewModel.IsBookmarkOptionEnable = true;
            IconThumbnailManager.ClearBackgroundRender();
            _viewModel.CommentItems.Clear();
            _viewModel.PdfUtilView.ClearCommentSearch();
            _viewModel.NotifyPageCommentChange();
            PdfHelper.UserColorList.Clear();
            TrackHelper.LogAddBlankPageEvent(TrackHelper.TrackHelperInstance.AddBlankPageCount);
            TrackHelper.TrackHelperInstance.AddBlankPageCount = 0;

            if (isCloseDoc)
            {
                _viewModel.CurrentOpenedItem = PdfHelper.InitWzCloudItem();
                _viewModel.NotifyTotalPageCount();
                _viewModel.RefreshPDFTitle();
                _viewModel.IsSignatureBarShow = false;
                _viewModel.IsCommentPaneShow = false;

                // reset lock status and password info
                var pdfLockStatus = new PdfLockStatus();
                _viewModel.CurrentPdfLockStatus = pdfLockStatus;
                _viewModel.CurrentPdfSourceLockStatus = pdfLockStatus;
                _viewModel.CurrentPdfFileInfo = null;
                _viewModel.CurrentPdfFilePasswordInfo = new PdfFilePasswordInfo();
            }
        }

        public void ExecuteCloseCommand()
        {
            if (_viewModel.CurrentPdfDocument == null)
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.EXECUTE_CLOSE_TIPS);
                return;
            }

            if (DoSaveChangesCheck() == SaveChangesCheckEnum.Cancel)
            {
                return;
            }

            ClearDocument(null, string.Empty, true);
            _viewModel.ResetDocChangedState();
        }

        public void ExecuteExitCommand()
        {
            _viewModel.PdfUtilView.Close();
        }

        public void ExecuteNewWindowCommand()
        {
            // To get applet stub path or real path
            var moduleFilePath = Process.GetCurrentProcess().MainModule.FileName;
            var appPath = Path.Combine(Path.GetDirectoryName(moduleFilePath), Path.GetFileNameWithoutExtension(moduleFilePath) + ".exe");

            try
            {
                PdfUtilSettings.Instance.WindowPosLeft = _viewModel.PdfUtilView.Left;
                PdfUtilSettings.Instance.WindowPosTop = _viewModel.PdfUtilView.Top;
                PdfUtilSettings.SavePDFUtilSettingsXML();

                string argu = "/position " + GetViewWindowsRectAsCommand();
                Process.Start(appPath, argu);
            }
            catch (Win32Exception)
            {

            }
        }

        public SaveChangesCheckEnum DoSaveChangesCheck()
        {
            if (!_viewModel.IsDocChanged || _viewModel.CurrentPdfFileName == null)
            {
                return SaveChangesCheckEnum.DoNotSave;
            }

            if ((string.IsNullOrEmpty(_viewModel.CurrentOpenedItem.parentId) || string.IsNullOrEmpty(_viewModel.CurrentOpenedItem.name))
                && string.IsNullOrEmpty(_viewModel.CurrentOpenedItem.itemId))
            {
                var buttons = new[]
                {
                    new TASKDIALOG_BUTTON()
                    {
                        id = (int)SaveChangesCheckEnum.Save,
                        text = Properties.Resources.WINZIP_UNSAVE_CHECK_SAVE
                    },
                    new TASKDIALOG_BUTTON()
                    {
                        id = (int)SaveChangesCheckEnum.DoNotSave,
                        text = Properties.Resources.WINZIP_UNSAVE_CHECK_DO_NOT_SAVE + "\n" + Properties.Resources.WINZIP_UNSAVE_CHECK_DO_NOT_SAVE_CHANGES
                    }
                };

                int taskDialogWidth = 250;
                var taskDialog = new TaskDialog(Properties.Resources.PDF_UTILITY_TITLE, Properties.Resources.SAVE_CHANGES_WARNING, Properties.Resources.SELECT_ONE_OPTION, null, buttons, SystemIcons.Warning, taskDialogWidth);
                TaskDialogResult taskDialogResult = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);
                if (taskDialogResult.dialogResult == (System.Windows.Forms.DialogResult)SaveChangesCheckEnum.Save)
                {
                    if (ExecuteSaveCommand() == Result.Ok)
                    {
                        return SaveChangesCheckEnum.Save;
                    }
                    else
                    {
                        return SaveChangesCheckEnum.Cancel;
                    }
                }
                else if (taskDialogResult.dialogResult == (System.Windows.Forms.DialogResult)SaveChangesCheckEnum.DoNotSave)
                {
                    return SaveChangesCheckEnum.DoNotSave;
                }
                else if (taskDialogResult.dialogResult == (System.Windows.Forms.DialogResult)SaveChangesCheckEnum.Cancel)
                {
                    return SaveChangesCheckEnum.Cancel;
                }
            }
            else
            {
                var buttons = new[]
                {
                    new TASKDIALOG_BUTTON()
                    {
                        id = (int)SaveChangesCheckEnum.Save,
                        text = Properties.Resources.WINZIP_UNSAVE_CHECK_SAVE + "\n" + Properties.Resources.WINZIP_UNSAVE_CHECK_SAVE_CHANGES
                    },
                    new TASKDIALOG_BUTTON()
                    {
                        id = (int)SaveChangesCheckEnum.SaveAs,
                        text = Properties.Resources.WINZIP_UNSAVE_CHECK_SAVE_AS + "\n" + Properties.Resources.WINZIP_UNSAVE_CHECK_SAVE_AS_CHANGES
                    },
                    new TASKDIALOG_BUTTON()
                    {
                        id = (int)SaveChangesCheckEnum.DoNotSave,
                        text = Properties.Resources.WINZIP_UNSAVE_CHECK_DO_NOT_SAVE + "\n" + Properties.Resources.WINZIP_UNSAVE_CHECK_DO_NOT_SAVE_CHANGES
                    }
                };

                int taskDialogWidth = 250;
                var taskDialog = new TaskDialog(Properties.Resources.PDF_UTILITY_TITLE, Properties.Resources.SAVE_CHANGES_WARNING, Properties.Resources.PDF_MODIFYED_PLEASE_SELECT_ONE_OPTION, null, buttons, SystemIcons.Warning, taskDialogWidth);
                TaskDialogResult taskDialogResult = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);
                if (taskDialogResult.dialogResult == (System.Windows.Forms.DialogResult)SaveChangesCheckEnum.Save)
                {
                    if (ExecuteSaveCommand() == Result.Ok)
                    {
                        return SaveChangesCheckEnum.Save;
                    }
                    else
                    {
                        return SaveChangesCheckEnum.Cancel;
                    }
                }
                else if (taskDialogResult.dialogResult == (System.Windows.Forms.DialogResult)SaveChangesCheckEnum.SaveAs)
                {
                    if (ExecuteSaveAsCommand() == Result.Ok)
                    {
                        return SaveChangesCheckEnum.SaveAs;
                    }
                    else
                    {
                        return SaveChangesCheckEnum.Cancel;
                    }
                }
                else if (taskDialogResult.dialogResult == (System.Windows.Forms.DialogResult)SaveChangesCheckEnum.Cancel)
                {
                    return SaveChangesCheckEnum.Cancel;
                }
                else if (taskDialogResult.dialogResult == (System.Windows.Forms.DialogResult)SaveChangesCheckEnum.DoNotSave)
                {
                    return SaveChangesCheckEnum.DoNotSave;
                }
            }
            return SaveChangesCheckEnum.DoNotSave;
        }

        private string GetViewWindowsRectAsCommand()
        {
            var view = _viewModel.PdfUtilView;
            return $"{view.Left} {view.Top} {view.Width} {view.Height}";
        }

        private bool SaveAsOtherFormat(string newFileName, List<string> imageNameList)
        {
            var fileExt = Path.GetExtension(newFileName).ToLower();
            bool ret = true;

            bool disableCancel = fileExt.Equals(".doc") || fileExt.Equals(".docx");
            var view = new ProgressView(_viewModel.PdfUtilView, ProgressOperation.SaveAs, disableCancel);
            _viewModel.CurExtractStatus = ProgressStatus.None;

            var ProcessThread = new Thread(new ThreadStart(new Action(delegate
            {
                try
                {
                    switch (fileExt)
                    {
                        case ".doc":
                            ConvertToWord(SaveFormat.Doc, newFileName, view);
                            break;
                        case ".docx":
                            ConvertToWord(SaveFormat.DocX, newFileName, view);
                            break;
                        case ".bmp":
                            ConvertToImage(new BmpDevice(), newFileName, imageNameList, view);
                            break;
                        case ".jpg":
                            ConvertToImage(new JpegDevice(), newFileName, imageNameList, view);
                            break;
                        case ".png":
                            ConvertToImage(new PngDevice(), newFileName, imageNameList, view);
                            break;
                        case ".tif":
                            ConvertToImage(new TiffDevice(), newFileName, imageNameList, view);
                            break;
                        default:
                            break;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    _viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_SAVE_LOCATION_NOT_SUPPORTED);
                    }));
                    ret = false;
                }
                catch
                {
                    ret = false;
                }

                if (ret)
                {
                    EDPHelper.SyncEnterpriseId(_viewModel.CurrentPdfFileName, newFileName);
                }

                if (_viewModel.CurExtractStatus != ProgressStatus.Cancel)
                {
                    _viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        view.InvokeProgressEvent(new ProgressEventArgs() { Status = ProgressStatus.Completed });
                    }));
                }
            })));

            ProcessThread.Start();
            view.ShowWindow();

            if (ret && _viewModel.CurExtractStatus == ProgressStatus.Cancel)
            {
                ret = false;
            }

            return ret;
        }

        public void ConvertToWord(SaveFormat format, string newFileName, ProgressView view)
        {
            _viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                view.InvokeProgressEvent(new ProgressEventArgs() { TotalItemsCount = 1, FileName = Path.GetFileName(newFileName) });
            }));

            _viewModel.CurrentPdfDocument.Save(newFileName, format);

            if (_viewModel.CurExtractStatus == ProgressStatus.Cancel && File.Exists(newFileName))
            {
                File.Delete(newFileName);
            }
        }

        public void ConvertToImage(Device imageDevice, string outputName, List<string> imageNameList, ProgressView view)
        {
            bool isCancel = false;
            string imageExt = Path.GetExtension(outputName);
            for (int pageCount = 1; pageCount <= _viewModel.CurrentPdfDocument.Pages.Count; pageCount++)
            {
                if (isCancel)
                {
                    break;
                }

                string imageName = outputName;
                if (_viewModel.CurrentPdfDocument.Pages.Count > 1)
                {
                    // If the document has more than one page, add the page number to the image name
                    imageName = imageName.Insert(outputName.IndexOf(imageExt), "-" + pageCount.ToString());
                }

                using (FileStream imageStream = new FileStream(imageName, FileMode.Create))
                {
                    if (imageExt.ToLower().Equals(".tif"))
                    {
                        (imageDevice as TiffDevice).Process(_viewModel.CurrentPdfDocument, pageCount, pageCount, imageStream);
                    }
                    else
                    {
                        (imageDevice as ImageDevice).Process(_viewModel.CurrentPdfDocument.Pages[pageCount], imageStream);
                    }
                    imageStream.Close();
                }
                imageNameList.Add(imageName);

                _viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    var args = new ProgressEventArgs() { CurExtractItemIndex = pageCount, TotalItemsCount = _viewModel.CurrentPdfDocument.Pages.Count, FileName = Path.GetFileName(imageName) };
                    view.InvokeProgressEvent(args);

                    if (_viewModel.CurExtractStatus == ProgressStatus.Cancel)
                    {
                        isCancel = true;
                    }
                }));
            }
        }

        private bool CopyToRealSourceFile(string realSourcePath, bool isSaveAs)
        {
            try
            {
                EDPHelper.FileStreamCopy(_viewModel.CurrentPdfFileName, realSourcePath);
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException)
                {
                    if (isSaveAs)
                    {
                        FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_SAVE_LOCATION_NOT_SUPPORTED);
                    }
                    else
                    {
                        FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_ACCESS_DENY);
                    }
                }
                else if (ex is IOException ioex && FileOperation.FileIsLocked(ioex))
                {
                    FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.FILE_IN_USE_WARNING);
                }
                return false;
            }

            return true;
        }

        public Document OpenCurrentGeneratedDocument(string name)
        {
            if (!File.Exists(name))
            {
                return null;
            }

            var fileInfo = new PdfFileInfo();
            fileInfo.BindPdf(name);

            if (fileInfo.HasOpenPassword)
            {
                var password = string.IsNullOrEmpty(_viewModel.CurrentPdfSourceLockStatus.openPassword) ?
                    _viewModel.CurrentPdfSourceLockStatus.permissionPassword : _viewModel.CurrentPdfSourceLockStatus.openPassword;
                var document = new Document(name, password);

                fileInfo.Close();
                fileInfo.BindPdf(document);
                return document;
            }
            else
            {
                var document = new Document(name);
                return document;
            }
        }
    }
}
