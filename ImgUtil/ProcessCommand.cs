using ImgUtil.Util;
using ImgUtil.WPFUI;
using ImgUtil.WPFUI.Controls;
using ImgUtil.WPFUI.Model;
using ImgUtil.WPFUI.Utils;
using ImgUtil.WPFUI.View;
using ImgUtil.WPFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Applets.Common;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace ImgUtil
{
    class ProcessCommand
    {
        private const string WinZipFilePane = "-cfp";
        private const string WinZipZipPane = "-czp";

        private const string Open = "-open";
        private const string Show = "-show";

        private const string AddToTeamsBackground = "-atb";
        private const string Crop = "-crop";
        private const string ConvertTo = "-cp";
        private const string RemovePersonalData = "-rpd";
        private const string Resize = "-resz";
        private const string RotateRight = "-rotr";
        private const string RotateLeft = "-rotl";
        private const string SetDesktopBackground = "-sdb";
        private const string Watermark = "-wmrk";
        private const string ImageSettings = "-set";

        private List<string> _actions = new List<string> { Open, Show, AddToTeamsBackground, Crop, ConvertTo, RemovePersonalData, Resize, RotateRight, RotateLeft, SetDesktopBackground, Watermark, ImageSettings };

        private ImgUtilView _imgUtilView;

        public ProcessCommand(ImgUtilView view)
        {
            _imgUtilView = view;
        }

        public void Process(string[] args)
        {
            bool getProcessId = false;
            var localSourceFiles = new List<string>();
            var actions = new List<string>();
            var sourceWzCloudItems = new List<WzCloudItem4>();
            bool isDragInFiles = false;

            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    if (arg.StartsWith("-h:"))
                    {
                        string handeLong = arg.Substring(3);
                        EnvironmentService.WinZipHandle = new IntPtr(long.Parse(handeLong));
                        continue;
                    }

                    if (arg.StartsWith("-cmd:", StringComparison.OrdinalIgnoreCase))
                    {
                        // The "-cmd:" must be at the end of commands
                        break;
                    }

                    if (_actions.Contains(arg))
                    {
                        actions.Add(arg);
                    }
                    else if (arg == WinZipFilePane)
                    {
                        EnvironmentService.IsCalledByWinZipFilePane = true;
                    }
                    else if (arg == WinZipZipPane)
                    {
                        EnvironmentService.IsCalledByWinZipZipPane = true;
                    }
                }
                else if (arg.StartsWith("&"))
                {
                    string filePath = arg.Substring(1);
                    if (Path.GetExtension(filePath).ToLower() == ".tmp")
                    {
                        if (EnvironmentService.IsCalledByWinZipFilePane)
                        {
                            sourceWzCloudItems = ParseJsonDataAndGetSourceFiles(filePath);
                        }
                        else
                        {
                            localSourceFiles = GetSourceFiles(filePath);
                        }

                        // Delete the temp file created by shell extension (temp file created by winzip will be deleted in winzip).
                        if (!EnvironmentService.IsCalledByWinZipFilePane && !EnvironmentService.IsCalledByWinZipZipPane)
                        {
                            File.Delete(filePath);
                        }
                    }
                    else
                    {
                        localSourceFiles.Add(filePath);
                    }
                }
                else if (arg == "/processid")
                {
                    getProcessId = true;
                }
                else if (getProcessId)
                {
                    EnvironmentService.WinZipProcessID = arg;
                    getProcessId = false;
                }
                else if (arg == "/position")
                {
                    ProcessNewWindowCommand(new CommandParams(args, 1));
                    return;
                }
                else if (arg == "/open")
                {
                    continue;
                }
                else if (Path.IsPathRooted(arg))
                {
                    isDragInFiles = true;
                    localSourceFiles.Add(arg);
                }
            }

            if (EnvironmentService.IsCalledByWinZip)
            {
                EnvironmentService.InitPipeServiceWhenCalledFromWinzip();
            }

            bool needShowUI = false;

            if (EnvironmentService.IsCalledByWinZipFilePane)
            {
                if (sourceWzCloudItems.Count > 0 && !ImageHelper.IsCloudItem(sourceWzCloudItems[0].profile.Id))
                {
                    FileOperation.FilterUnreadableFiles(sourceWzCloudItems, IntPtr.Zero);
                    if (sourceWzCloudItems.Count == 0)
                    {
                        return;
                    }
                }

                // Open picker from file pane
                if (actions.Contains(Show) && actions.Contains(Open) && sourceWzCloudItems.Count == 0)
                {
                    EnvironmentService.CommandLineOperation = CommandLineOperation.OpenPickerFromFilePane;
                    needShowUI = true;
                }
                // Open image from file pane
                else if (sourceWzCloudItems.Count > 0 && actions.Count == 1 && actions.Contains(Show))
                {
                    EnvironmentService.SetCommandLineFilePaneItems(sourceWzCloudItems);
                    EnvironmentService.CommandLineOperation = CommandLineOperation.OpenImageFromFilePane;
                    needShowUI = true;
                }
            }
            else
            {
                if (localSourceFiles.Count > 0)
                {
                    FileOperation.FilterUnreadableFiles(localSourceFiles, IntPtr.Zero);
                    if (localSourceFiles.Count == 0)
                    {
                        return;
                    }

                    EnvironmentService.SetCommandLineSourceFiles(localSourceFiles);

                    // Double click icon
                    if (actions.Contains(Show) && actions.Contains(Open))
                    {
                        EnvironmentService.CommandLineOperation = CommandLineOperation.None;
                        needShowUI = true;
                    }
                    // Open image from zip pane
                    else if (EnvironmentService.IsCalledByWinZipZipPane)
                    {
                        EnvironmentService.CommandLineOperation = CommandLineOperation.OpenImageFromZipPane;
                        needShowUI = true;
                    }
                    // Drag drop image on icon
                    else if (isDragInFiles)
                    {
                        EnvironmentService.CommandLineOperation = CommandLineOperation.DropImageOnIcon;
                        needShowUI = true;
                    }
                    // Right click -> Modify
                    else if (localSourceFiles.Count == 1 && actions.Count == 1 && actions.Contains(Show))
                    {
                        EnvironmentService.CommandLineOperation = CommandLineOperation.ModifyImage;
                        needShowUI = true;
                    }
                    // Right click -> Crop
                    else if (actions.Contains(Crop))
                    {
                        EnvironmentService.CommandLineOperation = CommandLineOperation.CropImage;
                        needShowUI = true;
                    }
                    // Rest of these don't need to show main UI
                    else
                    {
                        needShowUI = false;

                        if (!actions.Contains(Show) && !actions.Contains(Crop))
                        {
                            // If user launch ImgUtil inside a msix container(with no ImgUtil main window) by Right-Click menu,
                            // the operation window will not appear in the foreground. For this problem, here is a trick:
                            // before showing the operation window, simulate a key release event, then the next window
                            // will be displayed in the foreground.
                            NativeMethods.keybd_event(NativeMethods.VK_LMENU, 0, NativeMethods.KEYEVENTF_EXTENDEDKEY | NativeMethods.KEYEVENTF_KEYUP, 0);
                        }

                        // Wait for environment initialize
                        bool success = EnvironmentService.InitEnvironmentTask.Result;
                        if (!success || !EnvironmentService.InitWinzipLicense(IntPtr.Zero))
                        {
                            return;
                        }

                        var sourceFiles = localSourceFiles.ToArray();

                        // Right click -> Add to Teams Background
                        if (actions.Contains(AddToTeamsBackground))
                        {
                            TrackHelper.LogShellMenuEvent("add-to-teams");
                            ExecuteAddToTeams(sourceFiles);
                        }
                        // Right click -> Convert To
                        else if (actions.Contains(ConvertTo))
                        {
                            TrackHelper.LogShellMenuEvent("convert-to");
                            ExecuteConvertTo(sourceFiles);
                        }
                        // Right click -> Remove Presonal Data
                        else if (actions.Contains(RemovePersonalData))
                        {
                            TrackHelper.LogShellMenuEvent("remove-pers-data");
                            ExecuteRemovePersonalData(sourceFiles);
                        }
                        // Right click -> Resize
                        else if (actions.Contains(Resize))
                        {
                            TrackHelper.LogShellMenuEvent("resize");
                            ExecuteResize(sourceFiles);
                        }
                        // Right click -> Rotate Right
                        else if (actions.Contains(RotateRight))
                        {
                            TrackHelper.LogShellMenuEvent("rotate-right");
                            ExecuteRotate(sourceFiles, true);
                        }
                        // Right click -> Rotate Left
                        else if (actions.Contains(RotateLeft))
                        {
                            TrackHelper.LogShellMenuEvent("rotate-left");
                            ExecuteRotate(sourceFiles, false);
                        }
                        // Right click -> Set Desktop Background
                        else if (actions.Contains(SetDesktopBackground))
                        {
                            TrackHelper.LogShellMenuEvent("set-desktop-background");
                            ExecuteSetDesktopBackground(sourceFiles);
                        }
                        // Right click -> Watermark
                        else if (actions.Contains(Watermark))
                        {
                            TrackHelper.LogShellMenuEvent("watermark");
                            ExecuteWatermark(sourceFiles);
                        }
                        // Right click -> Image Settings
                        else if (actions.Contains(ImageSettings))
                        {
                            TrackHelper.LogShellMenuEvent("image-settings");
                            ExecuteImageSettings();
                        }
                    }
                }
            }

            if (needShowUI)
            {
                if (EnvironmentService.CommandLineOperation != CommandLineOperation.None)
                {
                    _imgUtilView.AwareCommandLineOperations();
                }

                if (EnvironmentService.IsCalledByWinZip && EnvironmentService.WinZipHandle != IntPtr.Zero)
                {
                    NativeMethods.EnableWindow(EnvironmentService.WinZipHandle, false);
                    _imgUtilView.SetWindowStartupCenter(EnvironmentService.WinZipHandle);
                }
                _imgUtilView.CalLastWindowPostion();
                _imgUtilView.ShowDialog();
            }
        }

        #region Execute without showing main UI

        private void ExecuteAddToTeams(string[] sourceFiles)
        {
            var fileList = CheckImagesCapability(sourceFiles, ImageCapability.SupportAddToTeamsBG, false);
            if (fileList.Count == 0)
            {
                return;
            }

            sourceFiles = fileList.ToArray();

            var progressWindow = new SimpleProgressWindow(string.Format(Properties.Resources.COPYING_N_FILES, sourceFiles.Length), sourceFiles.Length);
            int successCount = 0;

            progressWindow.Run(() =>
            {
                for (int i = 0; i < sourceFiles.Length; ++i)
                {
                    if (!progressWindow.IsCanceled)
                    {
                        var image = ImageFileFactory.CreateImageFromPath(sourceFiles[i]);
                        bool isIOException = false;
                        if (!ImageService.ExternalCheckImageFileValid(image, ref isIOException))
                        {
                            // The invalid file has been checked before. If there are still invalid files during the execution process, just skip it without prompt.
                            progressWindow.UpdateProgress(i + 1);
                            continue;
                        }

                        string fileName = Path.GetFileName(sourceFiles[i]);

                        if (File.Exists(Path.Combine(ImageHelper.GetTeamsBackgroundFolder(), fileName)))
                        {
                            bool isReplace = false;
                            progressWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
                            {
                                isReplace = FlatMessageWindows.DisplayConfirmationMessage(IntPtr.Zero, string.Format(Properties.Resources.TEXT_EXIST_AND_REPLACE, fileName));
                            }));
                            if (!isReplace)
                            {
                                progressWindow.UpdateProgress(i + 1);
                                continue;
                            }
                        }

                        var item = ImageHelper.InitCloudItemFromPath(sourceFiles[i]);
                        var ret = WinzipMethods.AddToTeamsBGFolder(IntPtr.Zero, item);
                        if (ret)
                        {
                            successCount++;
                        }
                        progressWindow.UpdateProgress(i + 1);
                    }
                }

                if (!progressWindow.IsWindowClosed)
                {
                    progressWindow.Close();
                }
            });

            if (successCount > 0)
            {
                FlatMessageWindows.DisplayInformationMessage(IntPtr.Zero, sourceFiles.Length > 1 ? Properties.Resources.TEXT_ADD_TO_TEAMS_BG_SUCCESS_MULTI : Properties.Resources.TEXT_ADD_TO_TEAMS_BG_SUCCESS);
                TrackHelper.LogImgToolsEvent("addToTeams", string.Empty);
            }
        }

        private void ExecuteConvertTo(string[] sourceFiles)
        {
            var fileList = CheckImagesCapability(sourceFiles, ImageCapability.SupportConvert, true);
            fileList = CheckImagesExtensions(fileList.ToArray());

            if (fileList.Count == 0)
            {
                return;
            }

            DoTransform(WzSvcProviderIDs.SPID_CONVERTPHOTOS_TRANSFORM, fileList.ToArray());
        }

        private void ExecuteRemovePersonalData(string[] sourceFiles)
        {
            var fileList = CheckImagesCapability(sourceFiles, ImageCapability.SupportRemoveData, true);
            fileList = CheckImagesReadOnly(fileList.ToArray());
            fileList = CheckImagesExtensions(fileList.ToArray());

            if (fileList.Count == 0)
            {
                return;
            }

            DoTransform(WzSvcProviderIDs.SPID_REMOVEPERSONALDATA_TRANSFORM, fileList.ToArray());
        }

        private void ExecuteResize(string[] sourceFiles)
        {
            var fileList = CheckImagesCapability(sourceFiles, ImageCapability.SupportResize, true);
            fileList = CheckImagesReadOnly(fileList.ToArray());
            fileList = CheckImagesExtensions(fileList.ToArray());

            if (fileList.Count == 0)
            {
                return;
            }

            DoTransform(WzSvcProviderIDs.SPID_ENLARGE_REDUCE_IMAGE, fileList.ToArray());
        }

        private void ExecuteWatermark(string[] sourceFiles)
        {
            var fileList = CheckImagesCapability(sourceFiles, ImageCapability.SupportWatermark, true);
            fileList = CheckImagesReadOnly(fileList.ToArray());
            fileList = CheckImagesExtensions(fileList.ToArray());

            if (fileList.Count == 0)
            {
                return;
            }

            DoTransform(WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM, fileList.ToArray());
        }

        private void DoTransform(WzSvcProviderIDs id, string[] sourceFiles)
        {
            var spids = new WzSvcProviderIDs[] { id };
            var resultFiles = new List<WinzipMethods.ConvertFileResultPath>();

            bool ret = WinzipMethods.ConvertFileForShellExt(IntPtr.Zero, spids, 1, sourceFiles, sourceFiles.Length, null, 0, null, resultFiles, true, true, false);

            if (ret && resultFiles.Count > 0)
            {
                TrackHelper.TrackImgToolsEvent(id, resultFiles[0].path);
                TrackHelper.SendImgToolsEvent();
                FlatMessageWindows.DisplayInformationMessage(IntPtr.Zero, Properties.Resources.TEXT_CONVERSION_SUCCESS);
            }
        }

        private void ExecuteRotate(string[] sourceFiles, bool isClockwise)
        {
            var fileList = CheckImagesCapability(sourceFiles, ImageCapability.SupportRotate, false);
            fileList = CheckImagesReadOnly(fileList.ToArray());

            if (fileList.Count == 0)
            {
                return;
            }

            sourceFiles = fileList.ToArray();

            var tempFolder = FileOperation.Instance.CreateTempFolder();
            var progressWindow = new SimpleProgressWindow(string.Format(Properties.Resources.ROTATING_N_FILES, sourceFiles.Length), sourceFiles.Length);
            int successCount = 0;

            progressWindow.Run(() =>
            {
                for (int i = 0; i < sourceFiles.Length; ++i)
                {
                    try
                    {
                        if (!progressWindow.IsCanceled)
                        {
                            var enterpriseId = EDPAPIHelper.GetEnterpriseId(sourceFiles[i]);
                            var tempFilePath = Path.Combine(tempFolder, Path.GetFileName(sourceFiles[i]));
                            EDPHelper.FileCopy(sourceFiles[i], tempFilePath, true);

                            var image = ImageFileFactory.CreateImageFromPath(tempFilePath);
                            bool isIOException = false;
                            if (!ImageService.ExternalCheckImageFileValid(image, ref isIOException))
                            {
                                // The invalid file has been checked before. If there are still invalid files during the execution process, just skip it without prompt.
                                progressWindow.UpdateProgress(i + 1);
                                continue;
                            }

                            if (isClockwise)
                            {
                                ImageService.ExternalRotateImageRight(image);
                            }
                            else
                            {
                                ImageService.ExternalRotateImageLeft(image);
                            }

                            ImageService.ExternalSaveImage(image, sourceFiles[i]);
                            if (EDPAPIHelper.IsProcessProtectedByEDP())
                            {
                                if (enterpriseId != EDPAPIHelper.GetEnterpriseId())
                                {
                                    EDPHelper.UnProtectItemDelay(sourceFiles[i], 500);
                                }
                            }
                            successCount++;
                            progressWindow.UpdateProgress(i + 1);
                        }
                    }
                    catch (Exception e) when (e is SaveLocationNoAccessException || e is UnauthorizedAccessException)
                    {
                        progressWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            FlatMessageWindows.DisplayWarningMessage(IntPtr.Zero, Properties.Resources.WARNING_ACCESS_DENY);
                        }));
                        break;
                    }
                    catch (IOException)
                    {
                        progressWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            FlatMessageWindows.DisplayWarningMessage(IntPtr.Zero, Properties.Resources.FILE_IN_USE_WARNING);
                        }));
                        continue;
                    }
                    catch (AsposeBadException)
                    {
                        continue;
                    }
                }

                if (!progressWindow.IsWindowClosed)
                {
                    progressWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        progressWindow.Close();
                    }));
                }
            });

            if (progressWindow.IsProgressFinished && successCount > 0)
            {
                TrackHelper.LogImgToolsEvent("rotate", isClockwise ? "right" : "left");
                TrackHelper.SendImgToolsEvent();
                FlatMessageWindows.DisplayInformationMessage(IntPtr.Zero, isClockwise ? Properties.Resources.TEXT_ROTATE_CLOCKWISE : Properties.Resources.TEXT_ROTATE_COUNTER_CLOCKWISE);
            }
        }

        private void ExecuteSetDesktopBackground(string[] sourceFiles)
        {
            if (sourceFiles.Length != 1)
            {
                return;
            }

            var fileList = CheckImagesCapability(sourceFiles, ImageCapability.SupportSetDesktopBG, false);
            if (fileList.Count == 0)
            {
                return;
            }

            if (!FlatMessageWindows.DisplayConfirmationMessage(IntPtr.Zero, Properties.Resources.TEXT_CONFIRM_SET_DESKTOP_BG))
            {
                return;
            }

            var sourceFile = fileList[0];
            var item = ImageHelper.InitCloudItemFromPath(sourceFile);
            if (WinzipMethods.SetWindowsDesktopBG(IntPtr.Zero, item))
            {
                TrackHelper.LogImgToolsEvent("setbg", string.Empty);
                TrackHelper.SendImgToolsEvent();
            }
        }

        private void ExecuteImageSettings()
        {
            var spids = new int[] { (int)WzSvcProviderIDs.SPID_CONVERTPHOTOS_TRANSFORM, (int)WzSvcProviderIDs.SPID_ENLARGE_REDUCE_IMAGE, (int)WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM, -1 };
            WinzipMethods.ShowConversionSettings(IntPtr.Zero, spids);
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
                    else
                    {
                        FileAssociation.SetAssociation(name, ImageHelper.ImageProgId);
                        FileAssociation.ChangeRootClassRegistryKey(name, ImageHelper.ImageProgId);
                        var openWithSubKey = string.Format(RegeditOperation.WzFileExtOpenWithProgId, name);
                        RegeditOperation.AddCurrentUserOpenWithProgidsValue(openWithSubKey, ImageHelper.ImageProgId);
                        needNotify = true;
                    }
                }
                else
                {
                    if (!name.Equals(RegeditOperation.WzAddDesktopIconKey) && !name.Equals(RegeditOperation.WzAddStartMenuKey))
                    {
                        var openWithSubKey = string.Format(RegeditOperation.WzFileExtOpenWithProgId, name);
                        var openWithProgId = RegeditOperation.GetCurrentUserRegistryStringValue(openWithSubKey, "");
                        if (!string.IsNullOrEmpty(openWithProgId) && openWithProgId == ImageHelper.ImageProgId)
                        {
                            RegeditOperation.DeleteCurrentUserRegistryKeyValue(openWithSubKey, "");
                        }
                        RegeditOperation.DeleteCurrentUserRegistryKeyValue(openWithSubKey, ImageHelper.ImageProgId);
                        FileAssociation.ChangeRootClassRegistryKey(name, "");
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

        #endregion

        #region Helper Functions

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

        private List<string> GetSourceFiles(string tempFile)
        {
            var sourceFiles = new List<string>();

            using (var reader = new StreamReader(tempFile))
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

            return sourceFiles;
        }

        private void ProcessNewWindowCommand(CommandParams args)
        {
            if (args.LeftLength != 4)
            {
                return;
            }

            _imgUtilView.LastWindowRect = new System.Windows.Rect(double.Parse(args.Next()), double.Parse(args.Next()), double.Parse(args.Next()), double.Parse(args.Next()));
            _imgUtilView.IsMultipleWindow = true;
            _imgUtilView.ShowDialog();
        }

        private List<string> CheckImagesReadOnly(string[] sourceFiles)
        {
            List<string> fileList = new List<string>();

            if (sourceFiles.Length == 1)
            {
                var fileInfo = new FileInfo(sourceFiles[0]);
                if (fileInfo.IsReadOnly)
                {
                    FlatMessageWindows.DisplayWarningMessage(IntPtr.Zero, Properties.Resources.IMAGE_IS_READ_ONLY);
                }
                else
                {
                    fileList.Add(sourceFiles[0]);
                }
            }
            else
            {
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
                                    id = (int)System.Windows.Forms.DialogResult.No,
                                    text = Properties.Resources.TASKDLG_SKIP_BUTTON
                                },
                                new TASKDIALOG_BUTTON()
                                {
                                    id = (int)System.Windows.Forms.DialogResult.Cancel,
                                    text = Properties.Resources.TASKDLG_CANCEL_BUTTON
                                }
                            };

                            var taskDialog = new TaskDialog(Properties.Resources.IMAGE_UTILITY_TITLE, string.Format(Properties.Resources.IMAGE_FILENAME_IS_READ_ONLY, Path.GetFileName(file)),
                                Properties.Resources.TASKDLG_SELECT_ONE, Properties.Resources.TASKDLG_DO_FOR_ALL_FILE, buttons, Properties.Resources.ImgUtil, dialogWidth);
                            var result = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);

                            if (result.dialogResult == System.Windows.Forms.DialogResult.Cancel)
                            {
                                fileList.Clear();
                                return fileList;
                            }

                            verificationChecked = result.verificationChecked;
                        }
                    }
                    else
                    {
                        fileList.Add(file);
                    }
                }
            }

            return fileList;
        }

        private List<WzCloudItem4> ParseJsonDataAndGetSourceFiles(string file)
        {
            var itemList = UtilsJson.AnalysisJson(file);

            var items = new List<WzCloudItem4>();

            foreach (var item in itemList.cloudItems)
            {
                items.Add(itemList.ConvertToWzCloudItem4(item));
            }

            return items;
        }

        private List<string> CheckImagesExtensions(string[] sourceFiles)
        {
            var fileList = new List<string>();
            foreach (var file in sourceFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    var realFormat = ImageHelper.GetImageRealFormatFromPath(file);
                    var fileExt = ImageHelper.GetFileExtensionsFromFormat(realFormat);

                    if (fileExt.Length != 0 && !fileExt.Contains(Path.GetExtension(file).ToLower()))
                    {
                        // wrong extension
                        var formatString = ImageHelper.GetFormatStringFromFormat(realFormat);
                        var result = ImageHelper.AskToModifyFileExtension(formatString, Path.GetExtension(file), true, fileInfo.IsReadOnly);

                        switch (result)
                        {
                            case System.Windows.Forms.DialogResult.Yes: // change file extension
                                var newPath = ImageHelper.ChangeFileExtension(file, fileExt[0]);
                                File.Move(file, newPath);
                                fileList.Add(newPath);
                                break;

                            case System.Windows.Forms.DialogResult.No:  // skip
                                break;

                            case System.Windows.Forms.DialogResult.Cancel:  // cancel this operation
                                fileList.Clear();
                                return fileList;
                        }
                    }
                    else
                    {
                        fileList.Add(file);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    FlatMessageWindows.DisplayWarningMessage(IntPtr.Zero, Properties.Resources.WARNING_ACCESS_DENY);
                    fileList.Clear();
                    return fileList;
                }
                catch (IOException)
                {
                    // File in use, do not prompt user the error here, continue the conversion and prompt error there.
                    fileList.Add(file);
                }
            }

            return fileList;
        }

        private List<string> CheckImagesCapability(string[] sourceFiles, ImageCapability cap, bool doConversion)
        {
            var fileList = new List<string>();
            foreach (var file in sourceFiles)
            {
                var image = ImageFileFactory.CreateImageFromPath(file);

                bool isIOException = false;
                if (!ImageService.ExternalCheckImageFileValid(image, ref isIOException))
                {
                    if (isIOException)
                    {
                        if (doConversion)
                        {
                            // In Conversion, do not prompt user file in use error here, continue the conversion and prompt error there.
                            fileList.Add(file);
                        }
                        else if (sourceFiles.Length == 1)
                        {
                            // Only show file in use message when there are one file selected.
                            FlatMessageWindows.DisplayWarningMessage(IntPtr.Zero, Properties.Resources.FILE_IN_USE_WARNING);
                        }
                    }
                    else
                    {
                        if (!FlatMessageWindows.DisplayWarningMessage(IntPtr.Zero, string.Format(Properties.Resources.WARNING_INVALID_FILE, Path.GetFileName(file))))
                        {
                            fileList.Clear();
                            return fileList;
                        }
                    }
                }
                else if (!ImageService.ExternalCheckImageSupportCapatility(image, cap))
                {
                    if (sourceFiles.Length == 1)
                    {
                        FlatMessageWindows.DisplayErrorMessage(IntPtr.Zero, Properties.Resources.ERROR_ACTION_NOT_SUPPORT);
                    }
                }
                else
                {
                    fileList.Add(file);
                }

                ImageService.ExternalUnloadImage(image);
            }

            return fileList;
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
                string AppTitle = Properties.Resources.IMAGE_UTILITY_TITLE;
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

        #endregion

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
