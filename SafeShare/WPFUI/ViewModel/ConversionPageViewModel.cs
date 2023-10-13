using SafeShare.Converter.ConvertWorker;
using SafeShare.Properties;
using SafeShare.Util;
using SafeShare.WPFUI.Commands;
using SafeShare.WPFUI.Model.Services;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;

namespace SafeShare.WPFUI.ViewModel
{
    public enum GenericFunctionResults
    {
        GFR_NO_RESULT,
        GFR_SUCCESS,
        GFR_CANCELED_BY_USER,
        GFR_ERROR
    }

    public class ConvertParam
    {
        public IntPtr WindowHandle { get; set; }

        public ConvertOptViewModels COptModel { get; set; }

        public FileListPageViewModel FileListModel { get; set; }

        public ExperiencePage experiencePage { get; set; }

        public EmailEncryptionPageViewModel EmailEncryptModel { get; set; }

        public Dispatcher Dispatcher { get; set; }
    }

    public class ConversionPageViewModel : ObservableObject
    {
        private const int TotalConversionCount = 5;
        private const int DefaultShowConversionCount = 3;
        private GenericFunctionResults _gfr = GenericFunctionResults.GFR_NO_RESULT;

        private bool _watermarkIsChecked = false;
        private bool _removePersonalDataIsChecked = false;
        private bool _combineIntoOnePdfIsChecked = false;
        private bool _convertToPDFIsChecked = false;
        private bool _reducePhotosIsChecked = false;
        private bool _saveCopyIsChecked = false;
        private bool _deleteFilesIsChecked = false;
        private bool _showWatermark = false;
        private bool _showRMPD = false;
        private bool _showd2p = false;
        private bool _showReduceImage = false;
        private bool _showCombinPdf = false;
        private bool _showSeeAll = false;
        private bool _showConversion = true;
        private string _deleteFileContent = string.Empty;
        private int _deleteFileDays = 30;
        private bool _seeAllClicked = false;

        private string _conversionOutputDir = string.Empty;
        private string _zipOutputDir = string.Empty;
        private string _lastErrorInfo = string.Empty;

        private FileListPage _fileListPage;
        private ConversionPage _conversionPagegView;
        private EmailEncryptionPage _emailEncryptPage;
        private ConvertOptPage _convertOptPage;

        private int _covertSetpIndex;
        private int _totalStep;
        private List<string> _allFileList = new List<string>();
        private List<string> _allOriginalFileList = new List<string>();

        private ConversionPageViewModelCommand _viewModelCommands;

        private bool _recorderShowD2P = false;
        private bool _recorderShowReduceImage = false;

        private Dictionary<ConvertType, string> _conversionSummaryDic = new Dictionary<ConvertType, string>(TotalConversionCount);

        public ConversionPageViewModel(ConversionPage view, FileListPage fileListPage, EmailEncryptionPage emailEncryptPage)
        {
            _fileListPage = fileListPage;
            _conversionPagegView = view;
            _emailEncryptPage = emailEncryptPage;

            _convertOptPage = new ConvertOptPage(ViewModel.ConvertType.WATERMARK, _conversionPagegView);
            (_convertOptPage.DataContext as ConvertOptViewModels)?.InitConvretSetting((_emailEncryptPage.DataContext as EmailEncryptionPageViewModel).ZipFileName);

            //SaveFilePath is empty when user first install and lanuch share app
            if (string.IsNullOrEmpty(Settings.Default.SaveFilePath))
            {
                IntPtr pPath;
                NativeMethods.SHGetKnownFolderPath(NativeMethods.FolderRidDownloadsGuid, (uint)0, IntPtr.Zero, out pPath);

                var DownloadsDir = System.Runtime.InteropServices.Marshal.PtrToStringUni(pPath);
                System.Runtime.InteropServices.Marshal.FreeCoTaskMem(pPath);

                Settings.Default.SaveFilePath = DownloadsDir;

                if (!Directory.Exists(Settings.Default.SaveFilePath))
                {
                    Settings.Default.SaveFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); ;
                }

                Settings.Default.Save();
            }

            UpdatePageData();
        }

        public ConversionPageViewModelCommand ViewModelCommands
        {
            get
            {
                if (_viewModelCommands == null)
                {
                    _viewModelCommands = new ConversionPageViewModelCommand(this);
                }
                return _viewModelCommands;
            }
        }

        public void ExecuteShareCommand()
        {
            ExecuteShareButtonAction();
        }

        public void UpdatePageData()
        {
            UpdateOriginalFileList();
            UpdateConvertStatus();
        }

        [Obfuscation(Exclude = true)]
        public bool ShowConversion
        {
            get => _showConversion;
            set
            {
                if (_showConversion != value)
                {
                    _showConversion = value;
                    Notify(nameof(ShowConversion));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ShowSeeAll
        {
            get => _showSeeAll;
            set
            {
                if (_showSeeAll != value)
                {
                    _showSeeAll = value;
                    Notify(nameof(ShowSeeAll));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool WatermarkIsChecked
        {
            get => _watermarkIsChecked;
            set
            {
                if (_watermarkIsChecked != value)
                {
                    Settings.Default.ConvertOptAddWatermark = value;
                    Settings.Default.Save();

                    _watermarkIsChecked = value;
                    Notify(nameof(WatermarkIsChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool RemovePersonalDataIsChecked
        {
            get => _removePersonalDataIsChecked;
            set
            {
                if (_removePersonalDataIsChecked != value)
                {
                    Settings.Default.ConvertOptRemovePersonalData = value;
                    Settings.Default.Save();

                    _removePersonalDataIsChecked = value;
                    Notify(nameof(RemovePersonalDataIsChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool CombineIntoOnePdfIsChecked
        {
            get => _combineIntoOnePdfIsChecked;
            set
            {
                if (_combineIntoOnePdfIsChecked != value)
                {
                    Settings.Default.ConvertOptCombineIntoOnePdf = value;
                    Settings.Default.Save();

                    _combineIntoOnePdfIsChecked = value;
                    Notify(nameof(CombineIntoOnePdfIsChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ConvertToPDFIsChecked
        {
            get => _convertToPDFIsChecked;
            set
            {
                if (_convertToPDFIsChecked != value)
                {
                    Settings.Default.ConvertOptFilesToPdf = value;
                    Settings.Default.Save();

                    _convertToPDFIsChecked = value;
                    Notify(nameof(ConvertToPDFIsChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool SaveCopyIsChecked
        {
            get => _saveCopyIsChecked;
            set
            {
                if (_saveCopyIsChecked != value)
                {
                    Settings.Default.LocalDeviceChecked = value;
                    Settings.Default.Save();

                    _saveCopyIsChecked = value;
                    Notify(nameof(SaveCopyIsChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool DeleteFilesIsChecked
        {
            get => _deleteFilesIsChecked;
            set
            {
                if (_deleteFilesIsChecked != value)
                {
                    Settings.Default.DeleteFileChecked = value;
                    Settings.Default.Save();

                    _deleteFilesIsChecked = value;
                    Notify(nameof(DeleteFilesIsChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ReducePhotosIsChecked
        {
            get => _reducePhotosIsChecked;
            set
            {
                if (_reducePhotosIsChecked != value)
                {
                    Settings.Default.ReducePhotosChecked = value;
                    Settings.Default.Save();

                    _reducePhotosIsChecked = value;
                    Notify(nameof(ReducePhotosIsChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ShowWatermark
        {
            get => _showWatermark;
            set
            {
                if (_showWatermark != value)
                {
                    _showWatermark = value;
                    Notify(nameof(ShowWatermark));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ShowRMPD
        {
            get => _showRMPD;
            set
            {
                if (_showRMPD != value)
                {
                    _showRMPD = value;
                    Notify(nameof(ShowRMPD));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool Showd2p
        {
            get => _showd2p;
            set
            {
                if (_showd2p != value)
                {
                    _showd2p = value;
                    Notify(nameof(Showd2p));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ShowReduceImage
        {
            get => _showReduceImage;
            set
            {
                if (_showReduceImage != value)
                {
                    _showReduceImage = value;
                    Notify(nameof(ShowReduceImage));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ShowCombinPdf
        {
            get => _showCombinPdf;
            set
            {
                if (_showCombinPdf != value)
                {
                    _showCombinPdf = value;
                    Notify(nameof(ShowCombinPdf));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ShowScheduleFile => !RegeditOperation.IsDisableFileExpiration();

        [Obfuscation(Exclude = true)]
        public string DeleteFileContent
        {
            get => _deleteFileContent;
            set
            {
                if (_deleteFileContent != value)
                {
                    _deleteFileContent = value;
                    Notify(nameof(DeleteFileContent));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public int DeleteFileDays
        {
            get
            {
                return _deleteFileDays;
            }

            set
            {
                if (_deleteFileDays != value)
                {
                    _deleteFileDays = value;
                    Notify(nameof(DeleteFileContent));
                }
            }
        }

        public string ConversionOutputDir
        {
            get
            {
                if (string.IsNullOrEmpty(_conversionOutputDir))
                {
                    _conversionOutputDir = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                }

                return _conversionOutputDir;
            }
        }

        public string ZipOutputDir
        {
            get
            {
                if (string.IsNullOrEmpty(_zipOutputDir))
                {
                    _zipOutputDir = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                }

                return _zipOutputDir;
            }
        }

        private bool IsFileTypeSupportedByTransform(string targetFile, string[] supportType)
        {
            if (supportType.Contains(Path.GetExtension(targetFile).ToLower()))
            {
                return true;
            }

            return false;
        }

        private void RecurseFolder(string folder, List<string> destList)
        {
            var direcrory = new DirectoryInfo(folder);

            foreach (var file in direcrory.GetFiles())
            {
                destList.Add(file.FullName);
            }

            foreach (var dir in direcrory.GetDirectories())
            {
                RecurseFolder(dir.FullName, destList);
            }
        }

        private void RecurseSourceFileList(List<string> srcFileList, List<string> destList)
        {
            foreach (var item in srcFileList)
            {
                if (File.Exists(item))
                {
                    destList.Add(item);
                }
                else if (Directory.Exists(item))
                {
                    RecurseFolder(item, destList);
                }
            }
        }

        private void UpdateOriginalFileList()
        {
            _allOriginalFileList.Clear();
            var viewModel = _fileListPage.DataContext as FileListPageViewModel;
            RecurseSourceFileList(viewModel.ItemsFullPathList, _allOriginalFileList);
        }

        private void UpdateConvertStatus()
        {
            int convertCount = 0;
            int supportCombineFileCount = 0;

            ShowCombinPdf = false;
            ShowRMPD = false;
            ShowWatermark = false;
            Showd2p = false;
            ShowReduceImage = false;

            _recorderShowD2P = false;
            _recorderShowReduceImage = false;

            foreach (var item in _allOriginalFileList)
            {
                if (!ShowCombinPdf
                    && (CombinePDFWorker.IsFileTypeSupportedByTransform(item) || ConvertToPDFWorker.IsFileTypeSupportedByTransform(item)))
                {
                    supportCombineFileCount++;
                    if (supportCombineFileCount >= 2)
                    {
                        ShowCombinPdf = true;
                        convertCount++;
                    }
                }

                if (!ShowRMPD && RemovePersonalDataWorker.IsFileTypeSupportedByTransform(item))
                {
                    ShowRMPD = true;
                    convertCount++;
                }

                if (!ShowWatermark && WatermarkWorker.IsFileTypeSupportedByTransform(item))
                {
                    ShowWatermark = true;
                    convertCount++;
                }

                if (!Showd2p && ConvertToPDFWorker.IsFileTypeSupportedByTransform(item))
                {
                    Showd2p = true;
                    _recorderShowD2P = true;
                    convertCount++;
                }

                if (!ShowReduceImage && ReducePhotoWorker.IsFileTypeSupportedByTransform(item))
                {
                    ShowReduceImage = true;
                    _recorderShowReduceImage = true;
                    convertCount++;
                }

                if (convertCount == TotalConversionCount)
                {
                    break;
                }
            }

            ShowConversion = convertCount != 0;
            ShowSeeAll = convertCount > DefaultShowConversionCount && !_seeAllClicked;
            if (ShowSeeAll)
            {
                RearrangeConversionShow(convertCount);
            }

            UpdateConvertCheckStatus();
        }

        private void RearrangeConversionShow(int convertCount)
        {
            var rearrangeCount = convertCount - DefaultShowConversionCount;
            while (rearrangeCount > 0)
            {
                // check the conversion display status from the last one to the first one
                if (ShowReduceImage)
                {
                    ShowReduceImage = false;
                }
                else if (Showd2p)
                {
                    Showd2p = false;
                }
                else if (ShowCombinPdf)
                {
                    ShowCombinPdf = false;
                }
                else if (ShowRMPD)
                {
                    ShowRMPD = false;
                }
                else if (ShowWatermark)
                {
                    ShowWatermark = false;
                }
                rearrangeCount--;
            }
        }

        public void UpdateConvertCheckStatus()
        {
            WatermarkIsChecked = Settings.Default.ConvertOptAddWatermark && ShowWatermark;
            RemovePersonalDataIsChecked = Settings.Default.ConvertOptRemovePersonalData && ShowRMPD;
            ConvertToPDFIsChecked = Settings.Default.ConvertOptFilesToPdf && _recorderShowD2P;
            CombineIntoOnePdfIsChecked = Settings.Default.ConvertOptCombineIntoOnePdf && ShowCombinPdf;
            ReducePhotosIsChecked = Settings.Default.ReducePhotosChecked && _recorderShowReduceImage;
            SaveCopyIsChecked = Settings.Default.LocalDeviceChecked;
            DeleteFilesIsChecked = Settings.Default.DeleteFileChecked;
            DeleteFileContent = string.Format(Resources.DELETE_FILE_IN, Settings.Default.DeleteFileDays);

            if (ReducePhotosIsChecked || ConvertToPDFIsChecked || WatermarkIsChecked || CombineIntoOnePdfIsChecked || RemovePersonalDataIsChecked)
            {
                ClickSeeAllAction();
            }
        }

        public void ExecuteShareButtonAction()
        {
            if (WorkFlowManager.ShareOptionData.UseWinZipEmailer
                && (WorkFlowManager.ShareOptionData.SelectedEmail == null || WorkFlowManager.ShareOptionData.SelectedEmail.SelectedAccount is EmptyAccount))
            {
                WorkFlowManager.StartManageEmail(_conversionPagegView);
                return;
            }

            if (WorkFlowManager.ShareOptionData.UseEmailLink
                && (WorkFlowManager.ShareOptionData.SelectedCloud == null || WorkFlowManager.ShareOptionData.SelectedCloud.SelectedAccount is EmptyAccount))
            {
                WorkFlowManager.StartManageCloud(_conversionPagegView);
                return;
            }

            if (ShowConversion &&
                (ShowCombinPdf && CombineIntoOnePdfIsChecked ||
                Showd2p && ConvertToPDFIsChecked ||
                ShowReduceImage && ReducePhotosIsChecked ||
                ShowRMPD && RemovePersonalDataIsChecked ||
                ShowWatermark && WatermarkIsChecked) &&
                AsposeDownloadNeeded())
            {
                if (!WinzipMethods.DownloadAsposeDll(_conversionPagegView.MainWindow.WindowHandle))
                {
                    CombineIntoOnePdfIsChecked = false;
                    ConvertToPDFIsChecked = false;
                    ReducePhotosIsChecked = false;
                    RemovePersonalDataIsChecked = false;
                    WatermarkIsChecked = false;

                    return;
                }
            }

            TrackHelper.LogFileAddEvent();
            if (WorkFlowManager.ShareOptionData.SelectedEmail is UsersOwnProgram)
            {
                TrackHelper.LogAddEmailAccountEvent("user-email-client", false);
            }

            var page = new ExperiencePage();
            page.InitDataContext();
            _conversionPagegView.NavigationService.Navigate(page);

            StartConvert(page);
        }

        public void StartConvert(ExperiencePage page)
        {
            var mainWindowViewModel = _conversionPagegView.MainWindow.DataContext as SafeShareViewModel;
            ConvertParam param = new ConvertParam();
            param.Dispatcher = mainWindowViewModel.Dispatcher;
            param.WindowHandle = _conversionPagegView.MainWindow.WindowHandle;
            param.COptModel = _convertOptPage.DataContext as ConvertOptViewModels;
            param.FileListModel = _fileListPage.DataContext as FileListPageViewModel;
            param.EmailEncryptModel = _emailEncryptPage.DataContext as EmailEncryptionPageViewModel;
            param.experiencePage = page;

            Task.Factory.StartNew(() =>
            {
                param.experiencePage.Progress.Dispatcher.Invoke(new Action(delegate
                {
                    param.experiencePage.Progress.Visibility = Visibility.Visible;
                    param.experiencePage.Progress.Value = 0;
                }));

                var fileList = new List<string>();
                var tmpFileList = new List<string>();
                var fileListViewModel = param.FileListModel;
                var currentFilename = string.Empty;
                try
                {
                    foreach (var item in fileListViewModel.ItemsFullPathList)
                    {
                        currentFilename = item;
                        var fileInfo = new FileInfo(item);
                        if (fileInfo.Exists)
                        {
                            string targetFile = item;
                            string underConvertFile = Path.Combine(ConversionOutputDir, Path.GetFileName(targetFile));
                            EDPHelper.FileCopy(targetFile, underConvertFile, true);
                            fileList.Add(underConvertFile);
                        }
                        else
                        {
                            var dir = new DirectoryInfo(item);
                            if (dir.Exists)
                            {
                                string underConvertFolder = Path.Combine(ConversionOutputDir, Path.GetFileName(item));
                                FileOperation.CopyFilesRecursively(item, underConvertFolder);
                                fileList.Add(underConvertFolder);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    param.experiencePage.Dispatcher.Invoke(new Action(delegate
                    {
                        FileOperation.HandleFileException(ex, currentFilename);
                        if (param.experiencePage.DataContext is ExperiencePageViewModel expPageViewModel)
                        {
                            expPageViewModel.ZipFinishOrFailed(0);
                        }
                    }));

                    return;
                }

                RecurseSourceFileList(fileList, tmpFileList);

                if (EDPAPIHelper.IsProcessProtectedByEDP())
                {
                    WorkFlowManager.ShareOptionData.ContainsProtectedFile = EDPHelper.ContainsProtectedFile(tmpFileList.ToArray());
                }

                _allFileList = tmpFileList;
                _totalStep = CalTaskNum(_allFileList);
                _covertSetpIndex = 0;

                var manager = new ConverWorkerManager(param, _covertSetpIndex, _totalStep);
                manager.SetSourceFiles(_allFileList);
                manager.SetOrigianlFiles(_allOriginalFileList);
                manager.SetOutputDirectory(ConversionOutputDir);
                manager.SetParentHandle(_conversionPagegView.MainWindow.WindowHandle);

                if (ReducePhotosIsChecked)
                {
                    manager.SetSelectedConverter(ConvertId.ReducePhoto);
                }

                if (RemovePersonalDataIsChecked)
                {
                    manager.SetSelectedConverter(ConvertId.RemovePersonalData);
                }

                if (ConvertToPDFIsChecked || CombineIntoOnePdfIsChecked)
                {
                    manager.SetSelectedConverter(ConvertId.ConvertToPDF);
                }

                if (WatermarkIsChecked)
                {
                    manager.SetSelectedConverter(ConvertId.Watermark);
                }

                if (CombineIntoOnePdfIsChecked)
                {
                    manager.SetSelectedConverter(ConvertId.CombinePDF);
                }

                manager.StartConvert();

                if (manager.ConvertErrorChoice == CONVERT_ERROR_DIALOG.Cancel)
                {
                    param.experiencePage.Dispatcher.Invoke(new Action(delegate
                    {
                        var expPageViewModel = param.experiencePage.DataContext as ExperiencePageViewModel;
                        if (expPageViewModel != null)
                        {
                            expPageViewModel.ZipFinishOrFailed(0);
                        }
                    }));
                    return;
                }

                param.experiencePage.Progress.Dispatcher.Invoke(new Action(delegate
                {
                    param.experiencePage.Progress.Value = 50;
                }));

                DoCreateZip(param);
            }).ContinueWith(task =>
            {
                // clean the temp folder
                FileOperation.ForceDeleteFolderRecursive(ConversionOutputDir);
                _conversionOutputDir = string.Empty;
                FileOperation.ForceDeleteFolderRecursive(ZipOutputDir);
                _zipOutputDir = string.Empty;

                _conversionPagegView.MainWindow.AdjustPaneCursor(true);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void DoCreateZip(object obj)
        {
            bool ret = false;
            ConvertParam param = obj as ConvertParam;
            long sharedFileSize = 0;

            // get zip file list from ConversionOutputDir
            var fileList = GetZipSrcFileList();

            if (fileList.Count >= 1)
            {
                IntPtr handle = param.WindowHandle;
                string zipFileName = param.EmailEncryptModel.ZipFileName;
                string tempZipPath = Path.Combine(ZipOutputDir, zipFileName);
                bool isEncrypt = param.EmailEncryptModel.EncryptFileIsChecked;
                TrackHelper.TrackHelperInstance.IsEncryption = isEncrypt;
                string password = isEncrypt ? param.EmailEncryptModel.EncryptPassword : string.Empty;

                if (!isEncrypt || !string.IsNullOrEmpty(password))
                {
                    if (fileList.Count == 1 // Make sure only one file
                        && WinZipMethodHelper.ValidateZipFile(fileList[0].path) // only one zip file
                        && !param.EmailEncryptModel.EncryptFileIsChecked /* for a non-encrypted zip file*/)
                    {
                        ret = true;
                        try
                        {
                            File.Copy(fileList[0].path, tempZipPath);
                        }
                        catch (Exception)
                        {
                            ret = false;
                        }

                        param.experiencePage.Progress.Dispatcher.Invoke(new Action(delegate
                        {
                            param.experiencePage.Progress.Value = 100;
                        }));
                    }
                    else
                    {
                        WinzipMethods.CallbackDelegate callback = (ptr) =>
                        {
                            var tmp = (WinzipMethods.SaveAsZipProgressIndex)Marshal.PtrToStructure(ptr, typeof(WinzipMethods.SaveAsZipProgressIndex));

                            // update UI
                            param.experiencePage.Progress.Dispatcher.Invoke(new Action(delegate
                            {
                                double tmpProgress = Convert.ToInt32(tmp.progressIndex) / 2.0;
                                param.experiencePage.Progress.Value = 50 + tmpProgress;
                            }));
                        };

                        // create zip file in ZipPutputDir
                        ret = WinzipMethods.SaveAsZip(handle, fileList.ToArray(), (uint)fileList.Count, tempZipPath, false, false, password, callback);
                    }

                    if (SaveCopyIsChecked)
                    {
                        DoSaveCopy(tempZipPath);
                    }

                    if (WorkFlowManager.ShareOptionData.SelectedCloud != null)
                    {
                        WorkFlowManager.ShareOptionData.IsScheduledForDeletion = DeleteFilesIsChecked;
                    }
                    else
                    {
                        WorkFlowManager.ShareOptionData.IsScheduledForDeletion = false;
                    }

                    if (ret)
                    {
                        if (DoSendEmail(obj, tempZipPath))
                        {
                            // get shared file size
                            if (File.Exists(tempZipPath))
                            {
                                FileInfo fileInfo = new FileInfo(tempZipPath);
                                sharedFileSize = fileInfo.Length;
                            }
                        }
                    }
                }
            }

            // update UI
            param.experiencePage.Dispatcher.Invoke(new Action(delegate
            {
                var expPageViewModel = param.experiencePage.DataContext as ExperiencePageViewModel;
                if (expPageViewModel != null)
                {
                    expPageViewModel.ZipFinishOrFailed(sharedFileSize, param.FileListModel.FileTotalSize);
                }
            }));
        }

        private void MailItemSendedHandler(ref bool cancel)
        {
            if (cancel)
            {
                _gfr = GenericFunctionResults.GFR_ERROR;
            }
            else
            {
                _gfr = GenericFunctionResults.GFR_SUCCESS;
            }
        }

        private void MailItemCloseedHandler(ref bool cancel)
        {
            if (_gfr == GenericFunctionResults.GFR_NO_RESULT)
            {
                _gfr = GenericFunctionResults.GFR_CANCELED_BY_USER;
            }
        }

        public bool DoSendEmail(object obj, string file)
        {
            bool sendRet = false;

            ConvertParam param = obj as ConvertParam;
            IntPtr handle = param.WindowHandle;

            var emailOpt = EmailOptions.EMO_CHOOSE_AUTO;
            if (WorkFlowManager.ShareOptionData.UseEmailAttachment)
            {
                emailOpt = EmailOptions.EMO_AS_ATTACHMENT;
            }
            else if (WorkFlowManager.ShareOptionData.UseEmailLink)
            {
                emailOpt = EmailOptions.EMO_USING_CLOUD;
            }

            string htmlBody = CreateEmailHtmlBody(emailOpt, file);
            TrackHelper.LogShareTypeEvent();

            var emailBody = new WinzipMethods.ShareEmailBody()
            {
                linkUrl = string.Empty,
                subject = string.Empty,
                body = string.Empty,
                source = string.Empty,
                attachmentName = string.Empty
            };

            sendRet = WinzipMethods.SendEmail(handle, file, emailOpt, htmlBody, ref emailBody, WorkFlowManager.ShareOptionData.IsScheduledForDeletion,
                WorkFlowManager.ShareOptionData.ExpirationDays, WorkFlowManager.ShareOptionData.IsDeleteEmptyFolder);

            if (string.IsNullOrEmpty(emailBody.linkUrl)
                && string.IsNullOrEmpty(emailBody.subject)
                && string.IsNullOrEmpty(emailBody.body)
                && string.IsNullOrEmpty(emailBody.source)
                && string.IsNullOrEmpty(emailBody.attachmentName))
            {
                return sendRet;
            }

            _gfr = GenericFunctionResults.GFR_ERROR;
            if (WorkFlowManager.ShareOptionData.UseEmailLink)
            {
                htmlBody = htmlBody.Replace(@"%File_Location%", emailBody.linkUrl);
            }

            var outlookApp = new Microsoft.Office.Interop.Outlook.Application();
            var outlookNamespace = outlookApp.GetNamespace("mapi") as Microsoft.Office.Interop.Outlook.NameSpace;
            var mailItem = outlookApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem) as Microsoft.Office.Interop.Outlook.MailItem;

            try
            {
                var logOnName = outlookNamespace.CurrentUser.Name;
                if (!string.IsNullOrEmpty(logOnName))
                {
                    htmlBody = htmlBody.Replace(@"%Sender_Email%", logOnName);
                }
                else
                {
                    htmlBody = htmlBody.Replace(@"%Sender_Email%", WorkFlowManager.ShareOptionData.SelectedEmail.SelectedAccount.DisplayName);
                }
            }
            catch
            {
                htmlBody = htmlBody.Replace(@"%Sender_Email%", WorkFlowManager.ShareOptionData.SelectedEmail.SelectedAccount.DisplayName);
            }

            ((Microsoft.Office.Interop.Outlook.ItemEvents_10_Event)mailItem).Send += new Microsoft.Office.Interop.Outlook.ItemEvents_10_SendEventHandler(MailItemSendedHandler);
            ((Microsoft.Office.Interop.Outlook.ItemEvents_10_Event)mailItem).Close += new Microsoft.Office.Interop.Outlook.ItemEvents_10_CloseEventHandler(MailItemCloseedHandler);

            mailItem.BodyFormat = Microsoft.Office.Interop.Outlook.OlBodyFormat.olFormatHTML;
            mailItem.HTMLBody = htmlBody;
            mailItem.Subject = emailBody.subject;

            if (WorkFlowManager.ShareOptionData.UseEmailAttachment)
            {
                mailItem.Attachments.Add(emailBody.source, Microsoft.Office.Interop.Outlook.OlAttachmentType.olByValue, 1, emailBody.attachmentName);
            }

            mailItem.Display(true);

            if (_gfr == GenericFunctionResults.GFR_NO_RESULT)
            {
                return sendRet;
            }
            else
            {
                return _gfr == GenericFunctionResults.GFR_SUCCESS;
            }
        }

        public void ClickConvertOptAction(ConversionPage Page, ConvertType type)
        {
            if (_convertOptPage == null)
            {
                _convertOptPage = new ConvertOptPage(type, _conversionPagegView);
            }

            _convertOptPage.PageType = type;
            Page.NavigationService.Navigate(_convertOptPage);
            (_convertOptPage.DataContext as ConvertOptViewModels)?.InitConvretSetting((_emailEncryptPage.DataContext as EmailEncryptionPageViewModel).ZipFileName);
        }

        public void ClickSeeAllAction()
        {
            ShowSeeAll = false;
            Showd2p = false;
            ShowReduceImage = false;
            _seeAllClicked = true;
            Showd2p = _recorderShowD2P;
            ShowReduceImage = _recorderShowReduceImage;
        }

        private int CalTaskNum(List<string> underConvertFileList)
        {
            int taskCount = 0;

            if (ConvertToPDFIsChecked || CombineIntoOnePdfIsChecked)
            {
                taskCount += underConvertFileList.Count;
            }

            if (WatermarkIsChecked)
            {
                taskCount += underConvertFileList.Count;
            }

            if (RemovePersonalDataIsChecked)
            {
                taskCount += underConvertFileList.Count;
            }

            if (ReducePhotosIsChecked)
            {
                taskCount += underConvertFileList.Count;
            }

            if (CombineIntoOnePdfIsChecked)
            {
                taskCount++;
            }

            return taskCount;
        }

        public void DoSaveCopy(string fileName)
        {
            string desFileName = string.Empty;
            try
            {
                if (!Directory.Exists(Settings.Default.SaveFilePath))
                {
                    Directory.CreateDirectory(Settings.Default.SaveFilePath);
                }

                if (File.Exists(fileName))
                {
                    desFileName = Path.Combine(Settings.Default.SaveFilePath, Path.GetFileName(fileName));
                    while (File.Exists(desFileName))
                    {
                        string newName = FileOperation.RenameFileName(Path.GetFileName(desFileName));
                        desFileName = Path.Combine(Settings.Default.SaveFilePath, Path.GetFileName(newName));
                    }

                    EDPHelper.FileCopy(fileName, desFileName);
                }
            }
            catch (Exception e)
            {
                _convertOptPage.Dispatcher.Invoke(new Action(delegate
                {
                    FileOperation.HandleFileException(e, desFileName);
                }));
            }
        }

        public string GetLastError()
        {
            return _lastErrorInfo;
        }

        public void SetLastError(string errorInfo)
        {
            _lastErrorInfo = errorInfo;
        }

        public string GetConversionSummary(ViewModel.ConvertType PageType)
        {
            if (_conversionSummaryDic.ContainsKey(PageType))
                return _conversionSummaryDic[PageType];

            var supportType = new List<string>();
            var summaryFormatText = string.Empty;
            int fileCount = 0;

            switch (PageType)
            {
                case ConvertType.WATERMARK:
                    summaryFormatText = Properties.Resources.WATERMARK_SUMMARY_TEXT;
                    supportType = WatermarkWorker.SupportFileType().ToList();
                    break;

                case ConvertType.REMOVE_PERSONAL_DATA:
                    summaryFormatText = Properties.Resources.RMPD_SUMMARY_TEXT;
                    supportType = RemovePersonalDataWorker.SupportFileType().ToList();
                    break;

                case ConvertType.CONVERT_TO_PDF:
                    summaryFormatText = Properties.Resources.D2P_SUMMARY_PDFS_TEXT;
                    supportType = ConvertToPDFWorker.SupportFileType().ToList();
                    break;

                case ConvertType.COMBINE_INTO_ONE_PDF:
                    summaryFormatText = Properties.Resources.COMBINE_PDF_SUMMARY_TEXT;

                    string[] Combine2PdfSupportType = new string[CombinePDFWorker.SupportFileType().Length + ConvertToPDFWorker.SupportFileType().Length];
                    Array.Copy(CombinePDFWorker.SupportFileType(), 0, Combine2PdfSupportType, 0, CombinePDFWorker.SupportFileType().Length);
                    Array.Copy(ConvertToPDFWorker.SupportFileType(), 0, Combine2PdfSupportType, CombinePDFWorker.SupportFileType().Length, ConvertToPDFWorker.SupportFileType().Length);
                    supportType = Combine2PdfSupportType.ToList();
                    break;

                case ConvertType.REDUCE_PHOTOS:
                    summaryFormatText = Properties.Resources.REDUCE_IMAGE_SUMMARY_TEXT;
                    supportType = ReducePhotoWorker.SupportFileType().ToList();
                    break;

                default:
                    break;
            }

            var typeCountArr = new int[supportType.Count + 1];
            foreach (var item in _allOriginalFileList)
            {
                int idx = 0;
                foreach (var type in supportType)
                {
                    var typeList = new List<string>();
                    typeList.Add(type);
                    if (IsFileTypeSupportedByTransform(item, typeList.ToArray()))
                    {
                        typeCountArr[idx]++;
                    }
                    idx++;
                }
            }

            var summary = string.Empty;
            for (int i = 0; i < typeCountArr.Count(); ++i)
            {
                if (typeCountArr[i] > 0)
                {
                    string fileType = supportType[i].ToUpper().Substring(1);
                    summary += string.Format("{0} {1}, ", typeCountArr[i], fileType);
                    fileCount += typeCountArr[i];
                }
            }

            if (!summary.Equals(string.Empty))
            {
                summary = summary.Remove(summary.Length - 2);
            }

            if (PageType == ConvertType.CONVERT_TO_PDF && fileCount == 1)
            {
                summaryFormatText = Resources.D2P_SUMMARY_TEXT;
            }

            var finalSummary = string.Format(summaryFormatText, summary, fileCount == 1 ? Resources.FILE_TEXT : Resources.FILES_TEXT);
            _conversionSummaryDic.Add(PageType, finalSummary);
            return finalSummary;
        }

        public bool AsposeDownloadNeeded()
        {
            var installFolder = RegeditOperation.GetInstallFolder();

            var asposeDlls = new List<string>() { "Aspose.Cells.dll", "Aspose.Pdf.dll", "Aspose.Slides.dll", "Aspose.Words.dll", "Aspose.Imaging.dll", "Aspose.PSD.dll", "GroupDocs.Metadata.dll" };

            foreach (var dll in asposeDlls)
            {
                if (!File.Exists(Path.Combine(installFolder, dll)))
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateViewModelDate(FileListPage fileListPage, EmailEncryptionPage emailEncryptPage)
        {
            _fileListPage = fileListPage;
            _emailEncryptPage = emailEncryptPage;
            _conversionSummaryDic.Clear();
            UpdatePageData();
        }

        private List<WzCloudItem4> GetZipSrcFileList()
        {
            var fileList = new List<WzCloudItem4>();

            string zipSrcDir = FileOperation.CreateTempFolder(ConversionOutputDir);
            foreach (var file in _allFileList)
            {
                if (WorkFlowManager.ShareOptionData.ContainsProtectedFile == false)
                {
                    EDPAPIHelper.UnProtectItem(file);
                }

                string targetFile = file;
                string destFile = zipSrcDir + file.Replace(ConversionOutputDir, string.Empty);
                FileOperation.CopyFileWithNotExistDestDir(targetFile, destFile);
            }

            var zipSrcDirInfo = new DirectoryInfo(zipSrcDir);

            if (zipSrcDirInfo.Exists)
            {
                foreach (var file in zipSrcDirInfo.GetFiles())
                {
                    var zipItem = WinZipMethodHelper.InitCloudItemFromPath(file.FullName);
                    fileList.Add(zipItem);
                }

                foreach (var folder in zipSrcDirInfo.GetDirectories())
                {
                    var zipItem = WinZipMethodHelper.InitCloudItemFromPath(folder.FullName);
                    fileList.Add(zipItem);
                }
            }

            return fileList;
        }

        private string CreateEmailHtmlBody(EmailOptions emailOpt, string file)
        {
            string htmlBody = string.Empty;

            if (WorkFlowManager.ShareOptionData.EncryptFile)
            {
                htmlBody = Properties.Resources.email_e;
            }
            else
            {
                htmlBody = Properties.Resources.email;
            }

            if (emailOpt == EmailOptions.EMO_AS_ATTACHMENT)
            {
                // No download button
                int indexButtonBr1 = htmlBody.IndexOf(@"<tr id=""DownloadButtonBR"">");
                int indexButtonBr2 = htmlBody.IndexOf(@"<tr id=""TrtItFreeBR"">");

                htmlBody = htmlBody.Remove(indexButtonBr1, indexButtonBr2 - indexButtonBr1);
            }

            if (WorkFlowManager.ShareOptionData.UseEmailAttachment || !WorkFlowManager.ShareOptionData.IsScheduledForDeletion)
            {
                // No expired date
                int index1 = htmlBody.IndexOf(@"<table id=""ExpirationDateTable""");
                int index2 = htmlBody.IndexOf(@"<table id=""PrivacyTable""");

                htmlBody = htmlBody.Remove(index1, index2 - index1);
            }
            else
            {
                var dateExpired = DateTime.Now.AddDays(WorkFlowManager.ShareOptionData.ExpirationDays);
                string timeExpired = dateExpired.ToString("dd MMMM yyyy h:mm tt");
                string timeZoneNow = TimeZone.CurrentTimeZone.StandardName;
                timeExpired = string.Format("{0} {1}", timeExpired, timeZoneNow);
                htmlBody = htmlBody.Replace(@"%Date_Expires%", timeExpired);
            }

            string fileName = Path.GetFileName(file);
            htmlBody = htmlBody.Replace(@"%File_Name%", fileName);

            if (!IsUsingMapiEmail(WorkFlowManager.ShareOptionData.SelectedEmail))
            {
                htmlBody = htmlBody.Replace(@"%Sender_Email%", WorkFlowManager.ShareOptionData.SelectedEmail.SelectedAccount.DisplayName);
            }

            return htmlBody;
        }

        private bool IsUsingMapiEmail(EmailService emailService)
        {
            if (emailService is GmailService
                || emailService is OutlookService
                || emailService is YahooService
                || emailService is HotmailService
                || emailService is CustomEmailService)
            {
                return false;
            }

            return true;
        }
    }
}