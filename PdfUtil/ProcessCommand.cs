using Applets.Common;
using Aspose.Pdf;
using Aspose.Pdf.Facades;
using PdfUtil.Util;
using PdfUtil.WPFUI;
using PdfUtil.WPFUI.Controls;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.View;
using PdfUtil.WPFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace PdfUtil
{
    class ProcessCommand
    {
        private string Open_PDFUtil_From_WinZip_PIPE_NAME = "openpdfutilfromwinzip_pipe_{0}";
        private string Open_PDFUtil_From_WinZip_EVENT_NAME = "openpdfutilfromwinzip_event_{0}";

        private const string WinZipFilePane = "-cfp";
        private const string WinZipZipPane = "-czp";

        private const string ConvertToPDF = "-d2p";
        private const string CombinePDF = "-mrg";
        private const string SignPDF = "-sign";
        private const string WatermarkPDF = "-wmrk";
        private const string ConverterFromPDF = "-p2d";

        private const string OpenPDFUntil = "-open";
        private const string ModifyPDFInPDFUtil = "-show";
        private const string ShowUIInSpecialOrder = "-showex?";
        private const string LockPDF = "-lock";
        private const string UnlockPDF = "-unlock";
        private const string ExtractImagesFromPDF = "-ximg";
        private const string ScanToPDF = "-scan";
        private const string PDFSettings = "-set";
        private const string Createfrom = "-create";

        private const int inBufferSize = 4096;
        private const int outBufferSize = 65536;

        private string _processID;
        private PdfUtilView _pdfUtilView;
        private IntPtr _winzipHandle;

        private List<string> _transformList = new List<string> { ConvertToPDF, CombinePDF, SignPDF, WatermarkPDF, ConverterFromPDF };
        private List<string> _operationOnPDFList = new List<string> { ModifyPDFInPDFUtil, ShowUIInSpecialOrder, LockPDF, UnlockPDF, ExtractImagesFromPDF, ScanToPDF, PDFSettings, Createfrom, OpenPDFUntil };

        public ProcessCommand(PdfUtilView view)
        {
            _pdfUtilView = view;
            _processID = string.Empty;
            _winzipHandle = IntPtr.Zero;
        }

        public void GetTransforms(ref List<WzSvcProviderIDs> spids, string command)
        {
            switch (command)
            {
                case ConvertToPDF:
                    spids.Add(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM);
                    break;
                case CombinePDF:
                    spids.Add(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM);
                    break;
                case SignPDF:
                    spids.Add(WzSvcProviderIDs.SPID_SIGN_PDF_TRANSFORM);
                    break;
                case WatermarkPDF:
                    spids.Add(WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM);
                    break;
                case ConverterFromPDF:
                    spids.Add(WzSvcProviderIDs.SPID_PDF2DOC_TRANSFORM);
                    break;
                default:
                    break;
            }
        }

        public void GetOperations(ref List<OperationPdfIDS> opids, string command)
        {
            switch (command)
            {
                case OpenPDFUntil:
                    opids.Add(OperationPdfIDS.OpID_OpenPDFUtil);
                    break;
                case ModifyPDFInPDFUtil:
                    opids.Add(OperationPdfIDS.OpID_ShowInPDFUtil);
                    break;
                case ShowUIInSpecialOrder:
                    opids.Add(OperationPdfIDS.OpID_ShowDialogInSpecialOrder);
                    break;
                case LockPDF:
                    opids.Add(OperationPdfIDS.OpID_LockPDF);
                    break;
                case UnlockPDF:
                    opids.Add(OperationPdfIDS.OpID_UnlockPDF);
                    break;
                case ExtractImagesFromPDF:
                    opids.Add(OperationPdfIDS.OpID_ExtractImages);
                    break;
                case ScanToPDF:
                    opids.Add(OperationPdfIDS.OpID_ScanToPDF);
                    break;
                case PDFSettings:
                    opids.Add(OperationPdfIDS.OpID_PDFSettings);
                    break;
                case Createfrom:
                    opids.Add(OperationPdfIDS.OpID_CreateFrom);
                    break;
                default:
                    break;
            }
        }

        public void ExecuteCommandsCalledByWinzipFilePane(List<WzSvcProviderIDs> spids, List<OperationPdfIDS> opids, List<WzCloudItem4> sourceWzCloudItems)
        {
            string pipeName = string.Format(Open_PDFUtil_From_WinZip_PIPE_NAME, _processID);

            NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough, inBufferSize, outBufferSize);

            pipeServer.BeginWaitForConnection(WaitForConnectionCallback, pipeServer);

            string eventName = string.Format(Open_PDFUtil_From_WinZip_EVENT_NAME, _processID);
            IntPtr hEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, false, eventName);

            if (hEvent != IntPtr.Zero)
            {
                NativeMethods.SetEvent(hEvent);
            }

            if (_winzipHandle != IntPtr.Zero)
            {
                _pdfUtilView.SetWindowStartupCenter(_winzipHandle);
            }

            NativeMethods.EnableWindow(_winzipHandle, false);

            if (opids.Contains(OperationPdfIDS.OpID_CreateFrom))
            {
                _pdfUtilView.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    var viewModel = _pdfUtilView.DataContext as PdfUtilViewModel;
                    viewModel.WaitLoadWinzipSharedService();

                    viewModel.FakeRibbonTabViewModel.ExecuteCreateFromCommand();
                }));

                _pdfUtilView.ShowDialog();
            }
            else if (opids.Contains(OperationPdfIDS.OpID_OpenPDFUtil))
            {
                _pdfUtilView.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    var viewModel = _pdfUtilView.DataContext as PdfUtilViewModel;
                    viewModel.WaitLoadWinzipSharedService();

                    viewModel.FakeRibbonTabViewModel.ExecuteOpenCommand();
                }));

                _pdfUtilView.ShowDialog();
            }
            else
            {
                _pdfUtilView.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    var viewModel = _pdfUtilView.DataContext as PdfUtilViewModel;
                    viewModel.WaitLoadWinzipSharedService();

                    spids.Clear();
                    bool isCloudItem = false;
                    foreach (var file in sourceWzCloudItems)
                    {
                        if (PdfHelper.IsCloudItem(file.profile.Id))
                        {
                            isCloudItem = true;
                            if (Path.GetExtension(file.name).ToLower() != PdfHelper.PdfExtension)
                            {
                                spids.Add(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM);
                                break;
                            }
                        }
                        else if (Path.GetExtension(file.itemId).ToLower() != PdfHelper.PdfExtension)
                        {
                            spids.Add(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM);
                            break;
                        }
                    }

                    if (sourceWzCloudItems.Count > 1)
                    {
                        spids.Add(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM);
                    }

                    if (spids.Count > 0)
                    {
                        var tmpSourceFiles = new List<string>(sourceWzCloudItems.Count);
                        string tmpFolder = string.Empty;

                        if (isCloudItem)
                        {
                            var sourceItems = sourceWzCloudItems.ToArray();
                            var rets = viewModel.FakeRibbonTabViewModel.DownloadCloudItems(ref sourceItems, false);
                            if (rets != CloudDownloadResult.DownloadSucceed)
                            {
                                return;
                            }

                            foreach (var item in sourceItems)
                            {
                                tmpSourceFiles.Add(item.itemId);
                            }
                        }
                        else
                        {
                            tmpFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                            bool needConvertFile = false;
                            Exception exception = null;
                            for (int i = 0; i < sourceWzCloudItems.Count; i++)
                            {
                                var newName = Path.Combine(tmpFolder, Path.GetFileName(sourceWzCloudItems[i].itemId));
                                try
                                {
                                    EDPHelper.FileCopy(sourceWzCloudItems[i].itemId, newName, true);
                                }
                                catch (Exception ex)
                                {
                                    if (exception == null)
                                    {
                                        exception = ex;
                                    }
                                    continue;
                                }

                                tmpSourceFiles.Add(newName);
                                if (Path.GetExtension(newName).ToLower() != PdfHelper.PdfExtension)
                                {
                                    needConvertFile = true;
                                }
                            }

                            // All files are skiped, handle the exception and return
                            if (tmpSourceFiles.Count == 0)
                            {
                                if (exception != null)
                                {
                                    FileOperation.HandleFileException(exception, viewModel.PdfUtilView.WindowHandle);
                                }
                                return;
                            }

                            // Because there may be files skipped, we need to re-adjust the spid list.
                            if (tmpSourceFiles.Count < 2)
                            {
                                spids.Remove(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM);
                            }

                            if (!needConvertFile)
                            {
                                spids.Remove(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM);
                            }
                        }

                        bool ret = false;
                        var resultFiles = new List<WinzipMethods.ConvertFileResultPath>();

                        if (spids.Count > 0)
                        {
                            ret = WinzipMethods.ConvertFile(_pdfUtilView.WindowHandle, spids.ToArray(), spids.Count, tmpSourceFiles.ToArray(), tmpSourceFiles.Count, null, 0, null, 
                                resultFiles, true, true, true) && (resultFiles.Count > 0);

                            if (ret && spids.Contains(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM))
                            {
                                TrackHelper.LogPdfMergeEvent(tmpSourceFiles.Count);
                            }
                        }
                        else if (tmpSourceFiles.Count == 1)
                        {
                            // There are only one pdf file in file list, just open it.
                            ret = true;
                        }

                        if (ret)
                        {
                            var newPdfPath = Path.Combine(FileOperation.GlobalTempDir, Properties.Resources.DEFAULT_PDF_TITLE_NAME);
                            var resultFilePath = resultFiles.Count > 0 ? resultFiles[0].path : tmpSourceFiles[0];
                            if (FileCopy(resultFilePath, newPdfPath, true))
                            {
                                // if you want to open the destination file in the PDFUtile, there should be only one PDF generated in the end.
                                var cloudItem = PdfHelper.InitCloudItemFromPath(newPdfPath);
                                var selectedItems = new WzCloudItem4[1] { cloudItem };

                                viewModel.Executor(() => viewModel.FakeRibbonTabViewModel.ExecuteInitDocumentByCreateFromTask(selectedItems[0]).ContinueWith(task =>
                                {
                                    if (task.Status == TaskStatus.RanToCompletion)
                                    {
                                        foreach (var file in tmpSourceFiles)
                                        {
                                            TrackHelper.LogFileImportEvent(file);
                                        }
                                    }
                                }), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                            }
                        }

                        if (Directory.Exists(tmpFolder))
                        {
                            FileOperation.ForceDeleteFolderRecursive(tmpFolder);
                        }
                    }
                    else
                    {
                        // if you want to open the destination file in the PDFUtile, there should be only one PDF generated in the end.
                        viewModel.Executor(() => viewModel.FakeRibbonTabViewModel.ExecuteInitDocumentByOpenTask(sourceWzCloudItems[0]), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                    }

                }));

                _pdfUtilView.ShowDialog();
            }
        }

        public void WaitForConnectionCallback(IAsyncResult ar)
        {
            var pipeServer = (NamedPipeServerStream)ar.AsyncState;

            pipeServer.EndWaitForConnection(ar);

            try
            {
                _pdfUtilView.PipeServer = pipeServer;
            }
            catch (IOException e)
            {
                var ex = e.InnerException;
            }

        }

        public void ExecuteCommandsCalledByWinzipZipPane(List<WzSvcProviderIDs> spids, List<OperationPdfIDS> opids, List<string> sourceFiles)
        {
            string pipeName = string.Format(Open_PDFUtil_From_WinZip_PIPE_NAME, _processID);

            NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough, inBufferSize, outBufferSize);

            pipeServer.BeginWaitForConnection(WaitForConnectionCallback, pipeServer);

            string eventName = string.Format(Open_PDFUtil_From_WinZip_EVENT_NAME, _processID);
            IntPtr hEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, false, eventName);

            if (hEvent != IntPtr.Zero)
            {
                NativeMethods.SetEvent(hEvent);
            }

            if (_winzipHandle != IntPtr.Zero)
            {
                _pdfUtilView.SetWindowStartupCenter(_winzipHandle);
            }

            NativeMethods.EnableWindow(_winzipHandle, false);
            _pdfUtilView.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                var viewModel = _pdfUtilView.DataContext as PdfUtilViewModel;
                viewModel.WaitLoadWinzipSharedService();

                spids.Clear();
                foreach (var file in sourceFiles)
                {
                    if (Path.GetExtension(file).ToLower() != PdfHelper.PdfExtension)
                    {
                        spids.Add(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM);
                        break;
                    }
                }

                if (sourceFiles.Count > 1)
                {
                    spids.Add(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM);
                }

                if (spids.Count > 0)
                {
                    var resultFiles = new List<WinzipMethods.ConvertFileResultPath>();
                    string tmpFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                    var tmpSourceFiles = new List<string>(sourceFiles.Count);

                    foreach (var file in sourceFiles)
                    {
                        string tmpPath = Path.Combine(tmpFolder, Path.GetFileName(file));
                        if (File.Exists(tmpPath))
                        {
                            int i = 1;
                            do
                            {
                                // rename file path 
                                string newPath = String.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(file), i.ToString(), Path.GetExtension(file));
                                tmpPath = Path.Combine(tmpFolder, newPath);
                                i++;
                            } while (File.Exists(tmpPath));
                        }
                        try
                        {
                            EDPHelper.FileCopy(file, tmpPath);
                        }
                        catch (Exception ex) 
                        {
                            if (sourceFiles.Count == 1) 
                            {
                                FileOperation.HandleFileException(ex, viewModel.PdfUtilView.WindowHandle);
                                return;
                            }
                            continue;
                        }
                        tmpSourceFiles.Add(tmpPath);
                    }

                    bool ret = WinzipMethods.ConvertFile(_pdfUtilView.WindowHandle, spids.ToArray(), spids.Count, tmpSourceFiles.ToArray(), tmpSourceFiles.Count, null, 0, null, resultFiles, true, true, true);
                    if (ret && resultFiles.Count > 0)
                    {
                        if (spids.Contains(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM))
                        {
                            TrackHelper.LogPdfMergeEvent(tmpSourceFiles.Count);
                        }

                        var newPdfPath = Path.Combine(FileOperation.GlobalTempDir, Properties.Resources.DEFAULT_PDF_TITLE_NAME);
                        if (FileCopy(resultFiles[0].path, newPdfPath, true))
                        {
                            // if you want to open the destination file in the PDFUtile, there should be only one PDF generated in the end.
                            var cloudItem = PdfHelper.InitCloudItemFromPath(newPdfPath);

                            if (spids.Contains(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM) || spids.Contains(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM))
                            {
                                viewModel.Executor(() => viewModel.FakeRibbonTabViewModel.ExecuteInitDocumentByCreateFromTask(cloudItem).ContinueWith(task =>
                                {
                                    if (task.Status == TaskStatus.RanToCompletion)
                                    {
                                        foreach (var file in tmpSourceFiles)
                                        {
                                            TrackHelper.LogFileImportEvent(file);
                                        }
                                    }
                                }), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                            }
                            else
                            {
                                viewModel.Executor(() => viewModel.FakeRibbonTabViewModel.ExecuteInitDocumentByOpenFromZipPaneTask(cloudItem), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                            }
                        }
                    }

                    if (Directory.Exists(tmpFolder))
                    {
                        FileOperation.ForceDeleteFolderRecursive(tmpFolder);
                    }
                }
                else
                {
                    // if you want to open the destination file in the PDFUtile, there should be only one PDF generated in the end.
                    var cloudItem = PdfHelper.InitCloudItemFromPath(sourceFiles[0]);
                    viewModel.Executor(() => viewModel.FakeRibbonTabViewModel.ExecuteInitDocumentByOpenFromZipPaneTask(cloudItem), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                }
            }));

            _pdfUtilView.ShowDialog();
        }

        public void ExecuteCommandsCalledByExplorer(List<WzSvcProviderIDs> spids, List<OperationPdfIDS> opids, List<string> sourceFiles)
        {
            // the same as Windows - double click the applet icon
            if (opids.Contains(OperationPdfIDS.OpID_OpenPDFUtil)
                && opids.Contains(OperationPdfIDS.OpID_ShowInPDFUtil))
            {
                _pdfUtilView.CalLastWindowPostion();
                _pdfUtilView.ShowDialog();
                return;
            }

            //Windows Explorer - Select MyDocument.PDF file and launch the applet via the shell extension
            if (opids.Count == 1
                && spids.Count == 0
                && opids.Contains(OperationPdfIDS.OpID_ShowInPDFUtil)
                && sourceFiles.Count == 1)
            {
                TrackHelper.LogShellMenuEvent("open-modify-pdf");
                if (Path.GetExtension(sourceFiles[0]).ToLower() != PdfHelper.PdfExtension)
                {
                    return;
                }

                _pdfUtilView.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    var viewModel = _pdfUtilView.DataContext as PdfUtilViewModel;
                    var cloudItem = PdfHelper.InitCloudItemFromPath(sourceFiles[0]);
                    viewModel.WaitLoadWinzipSharedService();
                    viewModel.Executor(() => viewModel.FakeRibbonTabViewModel.ExecuteInitDocumentByOpenTask(cloudItem), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                }));

                _pdfUtilView.CalLastWindowPostion();
                _pdfUtilView.ShowDialog();
                return;
            }

            if (!opids.Contains(OperationPdfIDS.OpID_ShowInPDFUtil))
            {
                // If user launch PdfUtil inside a msix container(with no PdfUtil main window) by Right-Click menu,
                // the operation window will not appear in the foreground. For this problem, here is a trick:
                // before showing the operation window, simulate a key release event, then the next window
                // will be displayed in the foreground.
                NativeMethods.keybd_event(NativeMethods.VK_LMENU, 0, NativeMethods.KEYEVENTF_EXTENDEDKEY | NativeMethods.KEYEVENTF_KEYUP, 0);
            }

            // transform providers selected
            if (spids.Count > 0)
            {
                TrackHelper.LogShellTransformsEvent(spids, opids.Count != 0);
                if (opids.Count == 0)
                {
                    // only transform providers selected, convert files without applet visible
                    var viewModel = _pdfUtilView.DataContext as PdfUtilViewModel;
                    if (!viewModel.WaitLoadWinzipSharedService())
                    {
                        return;
                    }

                    if (spids.Contains(WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM) || spids.Contains(WzSvcProviderIDs.SPID_SIGN_PDF_TRANSFORM))
                    {
                        if (!CheckMultiFilesReadOnly(sourceFiles, viewModel) || sourceFiles.Count == 0)
                        {
                            return;
                        }
                    }

                    var resultFiles = new List<WinzipMethods.ConvertFileResultPath>();

                    bool ret = WinzipMethods.ConvertFileForShellExt(_pdfUtilView.WindowHandle, spids.ToArray(), spids.Count, sourceFiles.ToArray(), sourceFiles.Count, null, 0, null, resultFiles, true, true, false);

                    if (ret && resultFiles.Count > 0)
                    {
                        if (spids.Contains(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM))
                        {
                            TrackHelper.LogPdfMergeEvent(sourceFiles.Count);
                        }

                        if (spids.Contains(WzSvcProviderIDs.SPID_SIGN_PDF_TRANSFORM))
                        {
                            TrackHelper.LogSignSecurityEvent();
                        }

                        if (spids.Contains(WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM))
                        {
                            TrackHelper.LogPdfWatermarkEvent();
                        }

                        FlatMessageWindows.DisplayInformationMessage(_pdfUtilView.WindowHandle, Properties.Resources.CONVERSION_COMPLETED_SUCCESSFULLY);
                    }
                }
                else
                {
                    _pdfUtilView.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        var viewModel = _pdfUtilView.DataContext as PdfUtilViewModel;
                        if (!viewModel.WaitLoadWinzipSharedService())
                        {
                            return;
                        }

                        var resultFiles = new List<WinzipMethods.ConvertFileResultPath>();
                        string tmpFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                        var tmpSourceFiles = new List<string>(sourceFiles.Count);

                        bool showPdfutil = opids.Contains(OperationPdfIDS.OpID_ShowInPDFUtil);
                        if (showPdfutil)
                        {
                            bool needConvertFile = false;
                            foreach (var file in sourceFiles)
                            {
                                string tmpPath = Path.Combine(tmpFolder, Path.GetFileName(file));
                                try
                                {
                                    EDPHelper.FileCopy(file, tmpPath);
                                }
                                catch (Exception ex)
                                {
                                    if (sourceFiles.Count == 1)
                                    {
                                        FileOperation.HandleFileException(ex, _pdfUtilView.WindowHandle);
                                        return;
                                    }
                                    continue;
                                }

                                tmpSourceFiles.Add(tmpPath);
                                if (Path.GetExtension(tmpPath).ToLower() != PdfHelper.PdfExtension)
                                {
                                    needConvertFile = true;
                                }
                            }

                            // Because there may be files skipped, we need to re-adjust the spid list.
                            if (tmpSourceFiles.Count < 2)
                            {
                                spids.Remove(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM);
                            }

                            if (!needConvertFile)
                            {
                                spids.Remove(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM);
                            }
                        }

                        bool ret = false;
                        if (spids.Count > 0)
                        {
                            ret = WinzipMethods.ConvertFileForShellExt(_pdfUtilView.WindowHandle, spids.ToArray(), spids.Count, showPdfutil ? tmpSourceFiles.ToArray() : sourceFiles.ToArray(),
                                showPdfutil ? tmpSourceFiles.Count : sourceFiles.Count, null, 0, null, resultFiles, true, true, showPdfutil) && (resultFiles.Count > 0);

                            if (ret && spids.Contains(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM))
                            {
                                TrackHelper.LogPdfMergeEvent(tmpSourceFiles.Count);
                            }
                        }
                        else if (tmpSourceFiles.Count == 1)
                        {
                            // There are only one pdf file in file list, just open it.
                            ret = true;
                        }

                        if (ret)
                        {
                            if (!showPdfutil)
                            {
                                FlatMessageWindows.DisplayInformationMessage(_pdfUtilView.WindowHandle, Properties.Resources.CONVERSION_COMPLETED_SUCCESSFULLY);
                            }
                            _pdfUtilView.Activate();

                            if (showPdfutil)
                            {
                                var newPdfPath = Path.Combine(FileOperation.GlobalTempDir, Properties.Resources.DEFAULT_PDF_TITLE_NAME);
                                var resultFilePath = resultFiles.Count > 0 ? resultFiles[0].path : tmpSourceFiles[0];
                                if (FileCopy(resultFilePath, newPdfPath, true))
                                {
                                    // if you want to open the destination file in the PDFUtile, there should be only one PDF generated in the end.
                                    var cloudItem = PdfHelper.InitCloudItemFromPath(newPdfPath);
                                    var selectedItems = new WzCloudItem4[1] { cloudItem };

                                    viewModel.Executor(() => viewModel.FakeRibbonTabViewModel.ExecuteInitDocumentByCreateFromTask(selectedItems[0]).ContinueWith(task =>
                                    {
                                        if (task.Status == TaskStatus.RanToCompletion)
                                        {
                                            foreach (var file in tmpSourceFiles)
                                            {
                                                TrackHelper.LogFileImportEvent(file);
                                            }
                                        }
                                    }), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                                }
                            }
                        }

                        if (Directory.Exists(tmpFolder))
                        {
                            FileOperation.ForceDeleteFolderRecursive(tmpFolder);
                        }
                    }));

                    _pdfUtilView.CalLastWindowPostion();
                    _pdfUtilView.ShowDialog();
                }
            }
            // none transform provider selected
            else
            {
                var viewModel = _pdfUtilView.DataContext as PdfUtilViewModel;
                if (!viewModel.WaitLoadWinzipSharedService())
                {
                    return;
                }

                if (opids.Contains(OperationPdfIDS.OpID_ScanToPDF))
                {
                    TrackHelper.LogShellMenuEvent("scan-to-pdf");
                    string folderPath = string.Empty;
                    if (sourceFiles.Count > 0)
                    {
                        folderPath = Path.GetDirectoryName(sourceFiles[0]);
                    }
                    var task = viewModel.ExecuteImportFromScannerTask(true, folderPath);

                    if (task.Result)
                    {
                        _pdfUtilView.CalLastWindowPostion();
                        _pdfUtilView.ShowDialog();
                    }
                }
                else
                {
                    if (opids.Contains(OperationPdfIDS.OpID_LockPDF))
                    {
                        TrackHelper.LogShellMenuEvent("lock-pdf");
                        if (!CheckMultiFilesReadOnly(sourceFiles, viewModel) || sourceFiles.Count == 0)
                        {
                            return;
                        }

                        viewModel.Executor(() => ExecuteLockPDFTask(_pdfUtilView, sourceFiles), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                    }

                    if (opids.Contains(OperationPdfIDS.OpID_UnlockPDF))
                    {
                        TrackHelper.LogShellMenuEvent("unlock-pdf");
                        if (!CheckMultiFilesReadOnly(sourceFiles, viewModel) || sourceFiles.Count == 0)
                        {
                            return;
                        }

                        viewModel.Executor(() => ExecuteUnLockPDFTask(_pdfUtilView, sourceFiles), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                    }

                    if (opids.Contains(OperationPdfIDS.OpID_ExtractImages))
                    {
                        TrackHelper.LogShellMenuEvent("extract-image-from-pdf");
                        viewModel.Executor(() => ExecuteExtracImagesTask(_pdfUtilView, sourceFiles), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                    }

                    if (opids.Contains(OperationPdfIDS.OpID_PDFSettings))
                    {
                        TrackHelper.LogShellMenuEvent("pdf-settings");
                        viewModel.Executor(() => ExecutePdfSettingsTask(_pdfUtilView), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                    }
                }
            }
        }

        public Document OpenAndVerifyDocumentForLockUnlock(string sourceFile, PdfUtilView view, bool doLockOperation, out bool isLocked)
        {
            isLocked = false;
            Document document = null;

            var viewModel = view.DataContext as PdfUtilViewModel;
            viewModel.CurrentPdfFileName = sourceFile;
            var fileInfo = new PdfFileInfo();
            fileInfo.BindPdf(sourceFile);
            var pdfLockStatus = new PdfLockStatus();

            // open pdf file
            if (fileInfo.HasOpenPassword)
            {
                isLocked = true;
                string password = string.Empty;
                var fileName = Path.GetFileName(sourceFile);
                while (document == null)
                {
                    try
                    {
                        if (!viewModel.FakeRibbonTabViewModel.GetPassword(fileName, true, ref password))
                        {
                            return null;
                        }

                        document = new Document(sourceFile, password);
                        fileInfo.Close();
                        fileInfo.BindPdf(document);

                        pdfLockStatus.isSetOpenPassword = true;
                        if (fileInfo.PasswordType == PasswordType.Owner)
                        {
                            pdfLockStatus.isSetPermissionPassword = true;
                            pdfLockStatus.permissionPassword = password;
                            pdfLockStatus.permissions = (Permissions)document.Permissions;
                        }
                        else
                        {
                            pdfLockStatus.openPassword = password;
                        }
                    }
                    catch
                    {
                        FlatMessageWindows.DisplayWarningMessage(view.WindowHandle, Properties.Resources.WARNING_PASSWORD_INCORRECT);
                    }
                }
            }
            else
            {
                document = new Document(sourceFile);
                fileInfo.Close();
                fileInfo.BindPdf(document);
            }

            viewModel.CurrentPdfDocument = document;
            viewModel.CurrentPdfLockStatus = pdfLockStatus;

            // check if file is signed or certified
            bool isPdfSignedOrCertified;
            try
            {
                var signature = new PdfFileSignature(document);
                isPdfSignedOrCertified = signature.IsCertified || signature.IsContainSignature();
            }
            catch
            {
                // An error may be catched when there are invalid signature in pdf file, set isPdfSignedOrCertified true;
                isPdfSignedOrCertified = true;
            }

            if (isPdfSignedOrCertified)
            {
                FlatMessageWindows.DisplayWarningMessage(viewModel.PdfUtilView.WindowHandle, Properties.Resources.PDF_SIGNED_OR_CERTIFIED_WARNING);
                return null;
            }

            // for lock a locked file, ask user to choose whether change lock info or cancel this operation
            if (doLockOperation && (fileInfo.HasOpenPassword || fileInfo.HasEditPassword))
            {
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
                var taskDialog = new TaskDialog(Properties.Resources.PDF_UTILITY_TITLE, Properties.Resources.INFORMATION_PDF_ALREADY_LOCKED, null, null, buttons, Properties.Resources.PdfUtil, taskDialogWidth);
                TaskDialogResult taskDialogResult = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);
                if (taskDialogResult.dialogResult != System.Windows.Forms.DialogResult.Yes)
                {
                    return null;
                }
            }

            // check if file has permission password
            if (fileInfo.HasEditPassword && fileInfo.PasswordType != PasswordType.Owner)
            {
                isLocked = true;
                pdfLockStatus.isSetPermissionPassword = true;
                pdfLockStatus.permissions = (Permissions)document.Permissions;

                // In fact, a PDF file can be encrypted(locked) directly without owner password using Aspose.Pdf.
                // However, for those files with owner password, it is necessary to verify the correctness of the owner password.
                while (true)
                {
                    string PermissionPassword = string.Empty;
                    if (!viewModel.FakeRibbonTabViewModel.GetPassword(Path.GetFileName(sourceFile), false, ref PermissionPassword))
                    {
                        return null;
                    }

                    if (viewModel.FakeRibbonTabViewModel.CheckPermissionPasswordCorrection(PermissionPassword))
                    {
                        pdfLockStatus.permissionPassword = PermissionPassword;
                        viewModel.CurrentPdfLockStatus = pdfLockStatus;
                        break;
                    }
                }
            }

            return document;
        }

        public Task ExecuteLockPDFTask(PdfUtilView view, List<string> sourceFiles)
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                var viewModel = view.DataContext as PdfUtilViewModel;
                foreach (var sourceFile in sourceFiles)
                {
                    bool isLocked = false;
                    Document document = null;
                    var enterpriseId = EDPAPIHelper.GetEnterpriseId(sourceFile);
                    try
                    {
                        document = OpenAndVerifyDocumentForLockUnlock(sourceFile, view, true, out isLocked);
                    }
                    catch
                    {
                        // file invalid and file in use will throw the same type of exception,
                        // so currently we use a try-catch to distinguish these two kinds of errors
                        try
                        {
                            var fileInfo = new FileInfo(sourceFile);
                            using (FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                stream.Close();
                            }
                            FlatMessageWindows.DisplayWarningMessage(viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_FILE_INVALID_WARNING, Path.GetFileName(sourceFile)));
                        }
                        catch (IOException ex)
                        {
                            if (FileOperation.FileIsLocked(ex))
                            {
                                FlatMessageWindows.DisplayInformationMessage(viewModel.PdfUtilView.WindowHandle, Properties.Resources.FILE_IN_USE_WARNING);
                            }
                        }
                        tcs.TrySetResult();
                        continue;
                    }

                    if (document == null)
                    {
                        tcs.TrySetResult();
                        continue;
                    }

                    var dialog = new LockPDFDialog(view);
                    if (dialog.Show() == System.Windows.Forms.DialogResult.OK)
                    {
                        var status = new PdfLockStatus
                        {
                            isSetOpenPassword = viewModel.LockPDFViewModel.IsSetOpenPassword,
                            isSetPermissionPassword = viewModel.LockPDFViewModel.IsSetPermissionPassword,
                            openPassword = viewModel.LockPDFViewModel.OpenPassword,
                            permissionPassword = viewModel.LockPDFViewModel.PermissionPassword,
                            permissions = viewModel.FakeRibbonTabViewModel.ParsePermissionsFromDialog()
                        };

                        try
                        {
                            if (status.isSetOpenPassword && status.isSetPermissionPassword)
                            {
                                document.Encrypt(status.openPassword, status.permissionPassword, status.permissions, CryptoAlgorithm.AESx128);
                            }
                            else if (status.isSetOpenPassword)
                            {
                                document.Encrypt(status.openPassword, null, DocumentPrivilege.AllowAll, CryptoAlgorithm.AESx128, false);
                            }
                            else if (status.isSetPermissionPassword)
                            {
                                document.Encrypt(string.Empty, status.permissionPassword, status.permissions, CryptoAlgorithm.AESx128);
                            }

                            document.Save(sourceFile);

                            if (EDPAPIHelper.IsProcessProtectedByEDP())
                            {
                                if (enterpriseId != EDPAPIHelper.GetEnterpriseId())
                                {
                                    EDPHelper.UnProtectItemDelay(sourceFile, 500);
                                }
                            }

                            TrackHelper.LogLockSecurityEvent(viewModel.LockPDFViewModel);
                            FlatMessageWindows.DisplayInformationMessage(view.WindowHandle, Properties.Resources.INFORMATION_PDF_HAS_BEEN_LOCKED);
                        }
                        catch (Exception ex)
                        {
                            if (ex is UnauthorizedAccessException)
                            {
                                FlatMessageWindows.DisplayInformationMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.WARNING_ACCESS_DENY);
                            }
                            else
                            {
                                FlatMessageWindows.DisplayInformationMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.WARNING_UNABLE_TO_LOCK_THIS_PDF);
                            }
                        }
                    }
                }

                tcs.TrySetResult();
            });
        }

        public Task ExecuteUnLockPDFTask(PdfUtilView view, List<string> sourceFiles)
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                var viewModel = view.DataContext as PdfUtilViewModel;
                foreach (var sourceFile in sourceFiles)
                {
                    bool isLocked = false;
                    Document document = null;
                    var enterpriseId = EDPAPIHelper.GetEnterpriseId(sourceFile);
                    try
                    {
                        document = OpenAndVerifyDocumentForLockUnlock(sourceFile, view, false, out isLocked);
                    }
                    catch
                    {
                        // file invalid and file in use will throw the same type of exception,
                        // so currently we use a try-catch to distinguish these two kinds of errors
                        try
                        {
                            var fileInfo = new FileInfo(sourceFile);
                            using (FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                stream.Close();
                            }
                            FlatMessageWindows.DisplayWarningMessage(viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_FILE_INVALID_WARNING, Path.GetFileName(sourceFile)));
                        }
                        catch (IOException ex)
                        {
                            if (FileOperation.FileIsLocked(ex))
                            {
                                FlatMessageWindows.DisplayInformationMessage(viewModel.PdfUtilView.WindowHandle, Properties.Resources.FILE_IN_USE_WARNING);
                            }
                        }
                        tcs.TrySetResult();
                        continue;
                    }

                    if (document == null)
                    {
                        tcs.TrySetResult();
                        continue;
                    }

                    if (isLocked)
                    {
                        try
                        {
                            document.Decrypt();
                            document.Save(sourceFile);
                            if (EDPAPIHelper.IsProcessProtectedByEDP())
                            {
                                if (enterpriseId != EDPAPIHelper.GetEnterpriseId())
                                {
                                    EDPHelper.UnProtectItemDelay(sourceFile, 500);
                                }
                            }

                            TrackHelper.LogUnlockSecurityEvent();
                            FlatMessageWindows.DisplayInformationMessage(view.WindowHandle, Properties.Resources.INFORMATION_PDF_HAS_BEEN_UNLOCKED);
                        }
                        catch (Exception ex)
                        {
                            if (ex is UnauthorizedAccessException)
                            {
                                FlatMessageWindows.DisplayInformationMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.WARNING_ACCESS_DENY);
                            }
                            else
                            {
                                FlatMessageWindows.DisplayInformationMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.WARNING_UNABLE_TO_UNLOCK_THIS_PDF);
                            }
                        }
                    }
                    else
                    {
                        FlatMessageWindows.DisplayInformationMessage(view.WindowHandle, Properties.Resources.INFORMATION_PDF_IS_NOT_LOCKED);
                    }
                }

                tcs.TrySetResult();
            });
        }

        private Document OpenDocumentInExtracImages(string file, int fileCount, PdfUtilViewModel viewModel, ref Result result)
        {
            result = Result.Ok;
            Document document = null;
            try
            {
                document = viewModel.FakeRibbonTabViewModel.OpenDocument(file, null, ref result, true, true);
            }
            catch
            {
                // file invalid and file in use will throw the same type of exception,
                // so currently we use a try-catch to distinguish these two kinds of errors
                var displayMsg = string.Empty;
                try
                {
                    var fileInfo = new FileInfo(file);
                    using (FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        stream.Close();
                    }
                    displayMsg = string.Format(Properties.Resources.PDF_FILE_INVALID_WARNING, Path.GetFileName(file));
                }
                catch (IOException ex)
                {
                    if (fileCount == 1 && FileOperation.FileIsLocked(ex))
                    {
                        // Only show file in use message when there are one file selected.
                        displayMsg = Properties.Resources.FILE_IN_USE_WARNING;
                    }
                }

                if (!string.IsNullOrEmpty(displayMsg) && !FlatMessageWindows.DisplayWarningMessage(viewModel.PdfUtilView.WindowHandle, displayMsg))
                {
                    result = Result.Cancel;
                }
            }

            if (document == null || result != Result.Ok)
            {
                return null;
            }

            if (viewModel.LockPDFViewModel.IsSetPermissionPassword && viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.AnyExceptExtracting)
            {
                if (!FlatMessageWindows.DisplayWarningMessage(viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, "\'" + Path.GetFileName(file) + "\'")))
                {
                    result = Result.Cancel;
                }
                return null;
            }
            return document;
        }

        public Task ExecuteExtracImagesTask(PdfUtilView view, List<string> srouceFiles)
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                var viewModel = view.DataContext as PdfUtilViewModel;
                bool isHasImageToExtract = false;
                bool isHasFileNotLocked = false;
                var tempImageList = new List<XImage>();
                var preloadDocuments = new List<Document>();
                foreach (var file in srouceFiles)
                {
                    Result result = Result.Ok;
                    var document = OpenDocumentInExtracImages(file, srouceFiles.Count, viewModel, ref result);
                    if (result == Result.Cancel)
                    {
                        tcs.TrySetCanceled();
                        return;
                    }
                    preloadDocuments.Add(document);
                    if (document == null)
                    {
                        continue;
                    }

                    if (isHasImageToExtract)
                    {
                        continue;
                    }

                    foreach (var page in document.Pages)
                    {
                        tempImageList.AddRange(viewModel.GetExtractImage(page));
                        if (tempImageList.Count != 0)
                        {
                            isHasImageToExtract = true;
                            break;
                        }
                    }

                    isHasFileNotLocked = true;
                }

                if (!isHasFileNotLocked)
                {
                    tcs.TrySetResult();
                    return;
                }

                if (!isHasImageToExtract)
                {
                    FlatMessageWindows.DisplayWarningMessage(viewModel.PdfUtilView.WindowHandle, Properties.Resources.NO_IMAGES_EXTRACT_WARNING);
                    tcs.TrySetResult();
                    return;
                }

                var extractImageView = new ExtractImageView(view);
                if (extractImageView.ShowWindow())
                {
                    if (extractImageView.CurDestOptions == ImageDestinationOptions.IndividualImageFiles)
                    {
                        var selectedFolder = Path.GetDirectoryName(srouceFiles[0]);

                        string suffix = viewModel.GetImageSuffix(extractImageView.CurImageFormat);
                        if (suffix == null)
                        {
                            tcs.TrySetResult();
                            return;
                        }

                        bool isCancel = false;
                        bool isExtractSuccessfully = false;
                        var ProgressView = new ProgressView(view, ProgressOperation.ExtractImage);
                        var ProcessThread = new Thread(new ThreadStart(new Action(delegate
                        {
                            foreach (var doc in preloadDocuments)
                            {
                                var imageList = new List<XImage>();
                                if (doc == null)
                                {
                                    continue;
                                }
                                var fileName = Path.GetFileName(doc.FileName);
                                if (string.IsNullOrEmpty(fileName))
                                {
                                    //This may be a bug in aspose, Document.FileName is empty
                                    fileName = Path.GetFileName(srouceFiles[preloadDocuments.IndexOf(doc)]);
                                }

                                try
                                {
                                    foreach (var page in doc.Pages)
                                    {
                                        imageList.AddRange(viewModel.GetExtractImage(page));
                                    }

                                    if (imageList.Count == 0)
                                    {
                                        continue;
                                    }

                                    int imageIndex = 1;
                                    foreach (var image in imageList)
                                    {
                                        var destStream = viewModel.GetImageSaveFileStream(selectedFolder, fileName, ref imageIndex, suffix);
                                        if (destStream != null)
                                        {
                                            if (viewModel.SaveImageWithTransform(image, destStream, extractImageView.CurImageFormat))
                                            {
                                                EDPHelper.SyncEnterpriseId(srouceFiles[preloadDocuments.IndexOf(doc)], destStream.Name, 500);
                                                destStream.Close();
                                            }
                                            else
                                            {
                                                destStream.Close();
                                                if (File.Exists(destStream.Name))
                                                {
                                                    File.Delete(destStream.Name);
                                                }
                                                imageIndex--;
                                            }

                                            viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                            {
                                                var args = new ProgressEventArgs() { CurExtractItemIndex = imageList.IndexOf(image), TotalItemsCount = imageList.Count, FileName = fileName };
                                                ProgressView.InvokeProgressEvent(args);
                                                if (viewModel.CurExtractStatus == ProgressStatus.Cancel)
                                                {
                                                    isCancel = true;
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                            {
                                                FlatMessageWindows.DisplayWarningMessage(viewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_SAVE_LOCATION_NOT_SUPPORTED);
                                                isCancel = true;
                                            }));
                                        }

                                        if (isCancel)
                                        {
                                            // break from current pdf extract
                                            break;
                                        }

                                        isExtractSuccessfully = true;
                                    }

                                    if (isCancel)
                                    {
                                        // break from all pdfs extract
                                        break;
                                    }
                                }
                                catch
                                {
                                    viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                    {
                                        if (!FlatMessageWindows.DisplayWarningMessage(viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_FILE_INVALID_WARNING, Path.GetFileName(fileName))))
                                        {
                                            isCancel = true;
                                        }
                                    }));
                                    if (isCancel)
                                    {
                                        break;
                                    }
                                    continue;
                                }
                            }

                            if (viewModel.CurExtractStatus != ProgressStatus.Cancel)
                            {
                                viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                {
                                    ProgressView.InvokeProgressEvent(new ProgressEventArgs() { Status = ProgressStatus.Completed });
                                }));
                            }
                        })));
                        ProcessThread.Start();
                        ProgressView.ShowDialog();

                        if (viewModel.CurExtractStatus != ProgressStatus.Cancel && isExtractSuccessfully)
                        {
                            TrackHelper.LogPdfExtractEvent(false, suffix);
                            FlatMessageWindows.DisplayInformationMessage(viewModel.PdfUtilView.WindowHandle, Properties.Resources.IMAGE_EXTRACTED_SUCCESSFULLY);
                        }
                        viewModel.SendProcessFinishedEvent();
                    }
                }

                tcs.TrySetResult();
            });
        }

        public static Task ExecutePdfSettingsTask(PdfUtilView view)
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                const int spidEndingSymbol = -1;
                var spids = new int[] { (int)WzSvcProviderIDs.SPID_PDF2DOC_TRANSFORM, (int)WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM, (int)WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM, (int)WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM, (int)WzSvcProviderIDs.SPID_SIGN_PDF_TRANSFORM, spidEndingSymbol };
                WinzipMethods.ShowConversionSettings(view.WindowHandle, spids);
                tcs.TrySetResult();

                TrackHelper.LogPdfSettingsEvent();
            });
        }

        private void ParseAndGetSourceFiles(string file, ref List<string> sourceFiles)
        {
            using (var reader = new StreamReader(file))
            {
                while (true)
                {
                    string line = reader.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    if (line != string.Empty)
                    {
                        sourceFiles.Add(line);
                    }
                }
            }
        }

        private void ParseJsonDataAndGetSourceFiles(string file, ref List<WzCloudItem4> sourceWzCloudItems)
        {
            var itemList = UtilsJson.AnalysisJson(file);

            foreach (var item in itemList.cloudItems)
            {
                sourceWzCloudItems.Add(itemList.ConvertToWzCloudItem4(item));
            }
        }

        public void Process(string[] commands)
        {
            var spids = new List<WzSvcProviderIDs>();
            var opids = new List<OperationPdfIDS>();
            var sourceFiles = new List<string>();
            var sourceWzCloudItems = new List<WzCloudItem4>();

            bool getProcessId = false;
            var viewModel = _pdfUtilView.DataContext as PdfUtilViewModel;
            if (viewModel != null)
            {
                viewModel.WaitProcessFinishEvent -= ViewModel_WaitProcessFinishEvent;
                viewModel.WaitProcessFinishEvent += ViewModel_WaitProcessFinishEvent;
            }

            foreach (var item in commands)
            {
                if (item.StartsWith("-")) // transforms or operation on pdf
                {
                    if (item.StartsWith("-h:"))
                    {
                        string handeLong = item.Substring(3);
                        _winzipHandle = new IntPtr(long.Parse(handeLong));
                        continue;
                    }

                    if (item.StartsWith("-cmd:", StringComparison.OrdinalIgnoreCase))
                    {
                        // The "-cmd:" must be at the end of commands
                        break;
                    }

                    if (_transformList.Contains(item))
                    {
                        GetTransforms(ref spids, item);
                    }
                    else if (_operationOnPDFList.Contains(item))
                    {
                        GetOperations(ref opids, item);
                    }
                    else if (item == WinZipFilePane)
                    {
                        _pdfUtilView.IsCalledByWinZipFilePane = true;
                    }
                    else if (item == WinZipZipPane)
                    {
                        _pdfUtilView.IsCalledByWinZipZipPane = true;
                    }
                }
                else if (item.StartsWith("&")) // source files
                {
                    string filePath = item.Substring(1);
                    if (Path.GetExtension(filePath).ToLower() == ".tmp")
                    {
                        if (_pdfUtilView.IsCalledByWinZipFilePane)
                        {
                            ParseJsonDataAndGetSourceFiles(filePath, ref sourceWzCloudItems);
                        }
                        else
                        {
                            ParseAndGetSourceFiles(filePath, ref sourceFiles);
                        }

                        // Delete the temp file created by shell extension (temp file created by winzip will be deleted in winzip).
                        if (!_pdfUtilView.IsCalledByWinZipFilePane && !_pdfUtilView.IsCalledByWinZipZipPane)
                        {
                            File.Delete(filePath);
                        }
                    }
                    else
                    {
                        sourceFiles.Add(filePath);
                    }
                }
                else if (item == "/processid")
                {
                    getProcessId = true;
                }
                else if (getProcessId)
                {
                    _processID = item;
                    getProcessId = false;
                }
                else if (item == "/position")
                {
                    ProcessNewWindowCommand(new CommandParams(commands, 1));
                    return;
                }
                else if (item == "/open")
                {
                    continue;
                }
                else if (Path.IsPathRooted(item))
                {
                    sourceFiles.Add(item);
                    GetOperations(ref opids, ModifyPDFInPDFUtil);
                }
            }

            opids = opids.Distinct().ToList();

            var waitLoadWinzip = new Action(() =>
            {
                viewModel.WaitLoadWinzipSharedService();
                NativeMethods.EnableWindow(_winzipHandle, false);
            });

            var processExitEvent = new Action(() =>
            {
                string eventName = string.Format(Open_PDFUtil_From_WinZip_EVENT_NAME, _processID);
                IntPtr hEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, false, eventName);
                if (hEvent != IntPtr.Zero)
                {
                    NativeMethods.SetEvent(hEvent);
                };
            });

            if (_pdfUtilView.IsCalledByWinZipFilePane)
            {
                if (sourceWzCloudItems.Count > 0 && !PdfHelper.IsCloudItem(sourceWzCloudItems[0].profile.Id)
                    && !opids.Contains(OperationPdfIDS.OpID_CreateFrom) && !opids.Contains(OperationPdfIDS.OpID_OpenPDFUtil))
                {
                    FileOperation.FilterUnreadableFiles(sourceWzCloudItems, IntPtr.Zero, waitLoadWinzip);
                    if (sourceWzCloudItems.Count == 0)
                    {
                        NativeMethods.EnableWindow(_winzipHandle, true);
                        processExitEvent();
                        return;
                    }
                }
                ExecuteCommandsCalledByWinzipFilePane(spids, opids, sourceWzCloudItems);
            }
            else if (_pdfUtilView.IsCalledByWinZipZipPane)
            {
                if (sourceFiles.Count > 0)
                {
                    FileOperation.FilterUnreadableFiles(sourceFiles, IntPtr.Zero, waitLoadWinzip);
                    if (sourceFiles.Count == 0)
                    {
                        NativeMethods.EnableWindow(_winzipHandle, true);
                        processExitEvent();
                        return;
                    }
                }
                ExecuteCommandsCalledByWinzipZipPane(spids, opids, sourceFiles);
            }
            else
            {
                if (sourceFiles.Count > 0 && !opids.Contains(OperationPdfIDS.OpID_PDFSettings))
                {
                    FileOperation.FilterUnreadableFiles(sourceFiles, IntPtr.Zero, waitLoadWinzip);
                    if (sourceFiles.Count == 0)
                    {
                        NativeMethods.EnableWindow(_winzipHandle, true);
                        return;
                    }
                }
                ExecuteCommandsCalledByExplorer(spids, opids, sourceFiles);
            }
        }

        private void ProcessNewWindowCommand(CommandParams args)
        {
            if (args.LeftLength != 4)
            {
                return;
            }

            _pdfUtilView.LastPdfWindowLeft = double.Parse(args.Next());
            _pdfUtilView.LastPdfWindowTop = double.Parse(args.Next());
            _pdfUtilView.ShowDialog();
        }

        private void ViewModel_WaitProcessFinishEvent()
        {
            _pdfUtilView.Close();
        }

        public static string GetAdditionalCMDParameters(in string[] commands)
        {
            var additionalCMDParameters = string.Empty;

            for (var i = 0; i < commands.Length; i++)
            {
                if (commands[i].StartsWith("-cmd:", StringComparison.OrdinalIgnoreCase))
                {
                    if (commands[i].Length > "-cmd:".Length)
                    {
                        additionalCMDParameters += commands[i].Substring("-cmd:".Length);
                    }

                    i++;
                    while (i < commands.Length)
                    {
                        var arg = commands[i].Contains(" ") ? "\"" + commands[i] + "\"" : commands[i];
                        additionalCMDParameters += " " + arg;
                        i++;
                    }
                }
            }
            return additionalCMDParameters;
        }

        public static void ChangeIntegration()
        {
            var keyNames = RegeditOperation.GetAdminConfigRegistryValueName();
            bool needNotify = false;
            foreach (var name in keyNames)
            {
                var opt = RegeditOperation.GetAdminConfigRegistryStringValue(name);
                if (int.Parse(opt) == 1)
                {
                    if (name.Equals(RegeditOperation.WzAddDesktopIconKey))
                    {
                        CreateLinkFile(NativeMethods.CSIDL_COMMON_DESKTOPDIRECTORY);
                    }
                    else if (name.Equals(RegeditOperation.WzAddStartMenuKey))
                    {
                        CreateLinkFile(NativeMethods.CSIDL_COMMON_PROGRAMS);
                    }
                    else if (name == PdfHelper.PdfExtension)
                    {
                        FileAssociation.SetAssociation(name, PdfHelper.PdfProgId);
                        var openWithSubKey = string.Format(RegeditOperation.WzFileExtOpenWithProgId, name);
                        RegeditOperation.AddCurrentUserOpenWithProgidsValue(openWithSubKey, PdfHelper.PdfProgId);
                        needNotify = true;
                    }
                }
                else
                {
                    if (name.Equals(PdfHelper.PdfExtension))
                    {
                        var openWithSubKey = string.Format(RegeditOperation.WzFileExtOpenWithProgId, name);
                        var openWithProgId = RegeditOperation.GetCurrentUserRegistryStringValue(openWithSubKey, "");
                        if (!string.IsNullOrEmpty(openWithProgId) && openWithProgId == PdfHelper.PdfProgId)
                        {
                            RegeditOperation.DeleteCurrentUserRegistryKeyValue(openWithSubKey, "");
                        }
                        RegeditOperation.DeleteCurrentUserRegistryKeyValue(openWithSubKey, PdfHelper.PdfProgId);

                        FileAssociation.RemoveAssociation(name);
                        needNotify = true;
                    }
                }
            }
            if (needNotify)
            {
                NativeMethods.SHChangeNotify(NativeMethods.SHCNE_ASSOCCHANGED, 0, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private static void CreateLinkFile(int nFolder)
        {
            // create shortcut in public folder
            int size = 1024;
            var path = new StringBuilder(size);
            NativeMethods.SHGetSpecialFolderPath(IntPtr.Zero, path, nFolder, false);
            string shortcutFolder = path.ToString();



            if (Directory.Exists(shortcutFolder))
            {
                IShellLink link = (IShellLink)new ShellLink();



                // setup shortcut information
                string AppTitle = Properties.Resources.PDF_UTILITY_TITLE;
                link.SetDescription(AppTitle);



                string slink = Path.Combine(shortcutFolder, AppTitle + ".lnk");
                link.SetPath(slink);



                // To get applet stub path or real path
                var moduleFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                var appPath = Path.Combine(Path.GetDirectoryName(moduleFilePath), Path.GetFileNameWithoutExtension(moduleFilePath) + ".exe");



                IPersistFile file = (IPersistFile)link;
                file.Save(Path.Combine(shortcutFolder, Path.GetFileNameWithoutExtension(moduleFilePath) + ".lnk"), false);
            }
        }

        private bool CheckMultiFilesReadOnly(List<string> sourceFiles, PdfUtilViewModel viewModel)
        {
            if (sourceFiles.Count == 1)
            {
                var fileInfo = new FileInfo(sourceFiles[0]);
                if (fileInfo.IsReadOnly)
                {
                    FlatMessageWindows.DisplayWarningMessage(viewModel.PdfUtilView.WindowHandle, Properties.Resources.PDF_IS_READ_ONLY);
                    return false;
                }
            }
            else
            {
                List<string> readOnlyFileList = new List<string>();
                bool verificationChecked = false;

                foreach (var file in sourceFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.IsReadOnly)
                    {
                        if (!verificationChecked)
                        {
                            var dialogWidth = 220;
                            TASKDIALOG_BUTTON[] buttons = new[]
                            {
                                new TASKDIALOG_BUTTON()
                                {
                                    id = (int)System.Windows.Forms.DialogResult.Yes,
                                    text = Properties.Resources.BUTTON_SKIP
                                },
                                new TASKDIALOG_BUTTON()
                                {
                                    id = (int)System.Windows.Forms.DialogResult.Cancel,
                                    text = Properties.Resources.BUTTON_CANCEL
                                }
                            };

                            var taskDialog = new TaskDialog(Properties.Resources.PDF_UTILITY_TITLE, string.Format(Properties.Resources.PDF_FILENAME_IS_READ_ONLY, Path.GetFileName(file)),
                                Properties.Resources.SELECT_ONE_OPTION, Properties.Resources.TASKDLG_DO_FOR_ALL_FILE, buttons, Properties.Resources.PdfUtil, dialogWidth);
                            var result = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);

                            if (result.dialogResult == System.Windows.Forms.DialogResult.Cancel)
                            {
                                return false;
                            }

                            verificationChecked = result.verificationChecked;
                        }
                        readOnlyFileList.Add(file);
                    }
                }

                foreach (var file in readOnlyFileList)
                {
                    sourceFiles.Remove(file);
                }
            }

            return true;
        }

        public bool FileCopy(string sourceFileName, string destFileName, bool overwrite)
        {
            try
            {
                EDPHelper.FileCopy(sourceFileName, destFileName, overwrite);
                return true;
            }
            catch
            {
                return false;
            }
        }

        class CommandParams
        {
            private int _currentIndex;
            private string[] _data;
            public CommandParams(string[] args, int startIndex = 0)
            {
                _currentIndex = startIndex - 1;
                _data = args;
            }

            public string Next()
            {
                _currentIndex++;
                return _data[_currentIndex].ToLower();
            }

            public int TotalLength => _data.Length;

            public int LeftLength => _data.Length - _currentIndex - 1;
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
    }
}
