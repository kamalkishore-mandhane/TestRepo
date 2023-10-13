using SBkUpUI.WPFUI.Controls;
using SBkUpUI.WPFUI.Utils;
using SBkUpUI.WPFUI.View;
using SBkUpUI.WPFUI.ViewModel;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SBkUpUI
{
    public enum OpenMode
    {
        Unknown = 0,
        normal = 1,
        create = 2,
        open = 3,
        checkJob = 4,
        restoreTab = 5,
        autoCreate = 6
    }

    public class Settings
    {
        public double MainWindowTop;
        public double MainWindowLeft;
        public double MainWindowWidth;
        public double MainWindowHeight;
    }

    public static class WzSBkUpUI
    {
        private static OpenMode _openMode = OpenMode.Unknown;
        private static string _cmdPath = string.Empty;
        private static IntPtr _parentHandle = IntPtr.Zero;
        private static string _additionalCMDParameters = string.Empty;
        public static Settings settings = new Settings();

        public static OpenMode OpenMode
        {
            get
            {
                return _openMode;
            }
            set
            {
                _openMode = value;
            }
        }

        public static int DllMain(string[] args, string assemblyName)
        {
            WinZipMethods.NecessaryInit();
            Application.EnableVisualStyles();

            if (!ParseCommand(args))
            {
                return 0;
            }

            if (_openMode == OpenMode.open && !Util.FileIsInJobFolder(_cmdPath))
            {
                return 0;
            }

            if (_openMode == OpenMode.checkJob)
            {
                if (Util.FileIsInJobFolder(_cmdPath))
                {
                    var job = new JobItem(_cmdPath);
                    if (job.LoadFail)
                    {
                        return 0;
                    }
                    Task_Scheduler.TryInitJobItemFromTask(job);
                    return job.IsEnabled ? 1 : 0;
                }
                return 0;
            }

            var view = new SBkUpView(_parentHandle);
            var viewModel = view.DataContext as SBkUpViewModel;
            viewModel.SetLoadWinzipSharedService(_parentHandle, _additionalCMDParameters);

            view.Closed += View_Closed;
            view.ContentRendered += View_ContentRendered;
            viewModel.CalLastWindowPostion(assemblyName);
            view.ShowDialog();

            WinZipMethods.NecessaryUnInit();
            return 0;
        }

        public static void WarningFeatureDisabled()
        {
            FlatMessageBox.ShowWarning(null, Properties.Resources.WARNING_RESTRICTED_FEATURE);
        }

        private static bool ParseCommand(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Compare(args[i], "/new", true) == 0)
                {
                    if (_openMode == OpenMode.Unknown)
                    {
                        _openMode = OpenMode.create;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (string.Compare(args[i], "/open", true) == 0 && i + 1 < args.Length)
                {
                    i += 1;
                    _cmdPath = args[i];
                    if (_openMode == OpenMode.Unknown && _cmdPath.EndsWith(Swjf.Extension, System.StringComparison.OrdinalIgnoreCase) && File.Exists(_cmdPath))
                    {
                        _openMode = OpenMode.open;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (string.Compare(args[i], "/parentid", true) == 0 && i + 1 < args.Length)
                {
                    i += 1;
                    long handle = 0;
                    if (long.TryParse(args[i], out handle))
                    {
                        _parentHandle = new IntPtr(handle);
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (string.Compare(args[i], "/check", true) == 0 && i + 1 < args.Length)
                {
                    i += 1;
                    _cmdPath = args[i];
                    if (_openMode == OpenMode.Unknown && _cmdPath.EndsWith(Swjf.Extension, System.StringComparison.OrdinalIgnoreCase) && File.Exists(_cmdPath))
                    {
                        _openMode = OpenMode.checkJob;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (args[i].StartsWith("-cmd", StringComparison.OrdinalIgnoreCase))
                {
                    if (args[i].Length > "-cmd:".Length)
                    {
                        _additionalCMDParameters = args[i].Substring("-cmd:".Length);
                    }

                    i++;
                    while (i < args.Length)
                    {
                        var arg = args[i].Contains(" ") ? "\"" + args[i] + "\"" : args[i];
                        _additionalCMDParameters += " " + arg;
                        i++;
                    }
                }
                else if (string.Compare(args[i], "/tab=restore", true) == 0)
                {
                    _openMode = OpenMode.restoreTab;
                }
                else if (string.Compare(args[i], "/autocreate", true) == 0 && i + 1 < args.Length)
                {
                    i += 1;
                    _cmdPath = args[i];
                    if (_openMode == OpenMode.Unknown && Directory.Exists(_cmdPath))
                    {
                        _openMode = OpenMode.autoCreate;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (_openMode == OpenMode.Unknown)
            {
                _openMode = OpenMode.normal;
            }

            return true;
        }

        private static void View_ContentRendered(object sender, System.EventArgs e)
        {
            var viewModel = SBkUpView.MainWindow.DataContext as SBkUpUI.WPFUI.ViewModel.SBkUpViewModel;

            var processOpenModeAction = new Action(() =>
            {
                string folderPath = _cmdPath;
                if (_openMode == OpenMode.autoCreate && Directory.Exists(folderPath))
                {
                    TrackHelper.LogBackupFolderEvent();
                    for (int i = 0; i < viewModel.Items.Count; i++)
                    {
                        if (Directory.Exists(viewModel.Items[i].Swjf.backupFolder.itemId) && Util.NormalizePath(viewModel.Items[i].Swjf.backupFolder.itemId) == Util.NormalizePath(folderPath))
                        {
                            viewModel.Items[i].IsEnabled = true;
                            viewModel.Items[i].IsSelected = true;
                            return;
                        }
                    }

                    viewModel.RibbonTabViewModel.ExecuteCreateCommandCore(folderPath);
                }
                if (_openMode == OpenMode.create)
                {
                    viewModel.RibbonTabViewModel.ExecuteCreateCommand();
                }
                if (_openMode == OpenMode.open)
                {
                    foreach (var item in viewModel.Items)
                    {
                        if (item.FilePath == _cmdPath)
                        {
                            item.IsSelected = true;
                            viewModel.RibbonTabViewModel.ExecuteModifyCommand();
                        }
                    }
                }
                if (_openMode == OpenMode.restoreTab)
                {
                    viewModel.SelectedTabIndex = 1;
                }
            });

            if (viewModel.IsWinZipLoaded)
            {
                processOpenModeAction();
            }
            else
            {
                viewModel.WinZipLoaded += (tempSender, EventArgs) => { processOpenModeAction(); };
            }
        }

        private static void View_Closed(object sender, System.EventArgs e)
        {
            CloseSBk();
        }

        private static void CloseSBk()
        {
            if (_parentHandle != IntPtr.Zero)
            {
                NativeMethods.EnableWindow(_parentHandle, true);
                NativeMethods.SetForegroundWindow(_parentHandle);
            }

            Util.CleanTempFolder();
            if (WinZipMethods.WinzipSharedServiceHandle != IntPtr.Zero)
            {
                WinZipMethods.DestroySession(WinZipMethods.WinzipSharedServiceHandle);
            }
        }
    }
}
