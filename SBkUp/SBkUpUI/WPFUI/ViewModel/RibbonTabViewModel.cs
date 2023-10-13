using SBkUpUI.WPFUI.Commands;
using SBkUpUI.WPFUI.Controls;
using SBkUpUI.WPFUI.Utils;
using SBkUpUI.WPFUI.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace SBkUpUI.WPFUI.ViewModel
{
    class RibbonTabViewModel
    {
        private RibbonCommand _viewModelCommands;
        private SBkUpViewModel _viewModel;

        public RibbonTabViewModel(SBkUpViewModel model)
        {
            _viewModel = model;
        }

        public RibbonCommand ViewModelCommands
        {
            get
            {
                if (_viewModelCommands == null)
                {
                    _viewModelCommands = new RibbonCommand(this, _viewModel);
                }

                return _viewModelCommands;
            }
        }

        public void ExecuteHelpCommand()
        {
            var dialog = new AboutDialog(SBkUpView.MainWindow);
            dialog.ShowDialog();
        }

        public void ExecuteCreateCommand()
        {
            ExecuteCreateCommandCore(null);
        }

        public void ExecuteCreateCommandCore(string path = null)
        {
            var view = new CreateSBkUpView();
            view.Owner = SBkUpView.MainWindow;
            view.InitCreateMode(path);
            bool showDialog = true;
            if (!string.IsNullOrEmpty(path))
            {
                view.FileName = view.GetUniqueFileName();
                showDialog = false;
                view.finishCreate();
            }

            if (!showDialog || view.ShowDialog() == true)
            {
                var filePath = Path.Combine(SBkUpViewModel.JobFolder, view.FileName + Swjf.Extension);
                for (int i = 0; i < _viewModel.Items.Count; i++)
                {
                    if (_viewModel.Items[i].FilePath == filePath)
                    {
                        try
                        {
                            File.Delete(_viewModel.Items[i].FilePath);
                            Task_Scheduler.TryDeleteTask(_viewModel.Items[i]);
                        }
                        catch (Exception e)
                        {
                            var ex = e.InnerException;
                        }
                        _viewModel.Items.RemoveAt(i);
                    }
                }

                if (view.CurrentSwjf.Save(filePath))
                {
                    var job = new JobItem(filePath);
                    if (!job.LoadFail)
                    {
                        _viewModel.Items.Add(job);
                        job.LoadBackups();
                        job.PropertyChanged += _viewModel.JobItem_PropertyChanged;

                        if (!string.IsNullOrEmpty(path))
                        {
                            job.IsEnabled = true;
                            job.IsSelected = true;
                        }

                        TrackHelper.LogCreateSBkUpEvent(job);
                    }
                }
            }
        }

        public void ExecuteRunCommand()
        {
            if (_viewModel.SelectedItems.Count == 0)
            {
                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, Properties.Resources.ERROR_SELECT_AT_LEAST_ONE_JOB);
                return;
            }

            foreach (var item in _viewModel.SelectedItems)
            {
                if (!File.Exists(item.FilePath))
                {
                    if (!item.Swjf.Save(item.FilePath))
                    {
                        continue;
                    }
                }

                Task.Factory.StartNew(() =>
                {
                    SBkUpView.MainWindow.Dispatcher.Invoke(new Action(() => { item.Running = true; }));
                    var p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = Util.GetWinZipPath();
                    p.StartInfo.Arguments = Util.RunJobArguments(item, _viewModel.Owner);
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    p.WaitForExit();
                    item.LoadBackups();
                    SBkUpView.MainWindow.Dispatcher.Invoke(new Action(() => { item.Running = false; }));

                    TrackHelper.LogRunSBkUpEvent(item.Swjf.isCanned);
                });
            }
        }

        public void ExecuteDeleteCommand()
        {
            List<JobItem> cannedJobs = new List<JobItem>();
            foreach (var item in _viewModel.SelectedItems)
            {
                if (item.Swjf.isCanned)
                {
                    cannedJobs.Add(item);
                }
            }

            foreach (var job in cannedJobs)
            {
                //we want to know if they tried
                TrackHelper.LogDeleteSBkUpEvent(job.Swjf.isCanned);
            }

            if (cannedJobs.Count == _viewModel.SelectedItems.Count)
            {
                var warning = Properties.Resources.ERROR_SELECT_AT_LEAST_ONE_JOB;
                if (cannedJobs.Count == 1)
                {
                    warning = string.Format(Properties.Resources.ERROR_DELETE_FAIL, cannedJobs[0].Name);
                }
                else if (cannedJobs.Count > 1)
                {
                    warning = Properties.Resources.ERROR_TWO_OR_MORE_DELETE_FAIL;
                }

                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, warning);
                return;
            }
            else
            {
                var question = Properties.Resources.QUESTION_SURE_TO_DELETE_JOBS;
                if (cannedJobs.Count == 1)
                {
                    question = Properties.Resources.QUESTION_SURE_TO_DELETE_JOBS + "  " + string.Format(Properties.Resources.ERROR_DELETE_FAIL, cannedJobs[0].Name);
                }
                else if (cannedJobs.Count > 1)
                {
                    question = Properties.Resources.QUESTION_SURE_TO_DELETE_JOBS + "  " + Properties.Resources.ERROR_TWO_OR_MORE_DELETE_FAIL;
                }

                if (!FlatMessageBox.ShowQuestion(SBkUpView.MainWindow, question))
                {
                    return;
                }
            }

            for (var i = _viewModel.Items.Count - 1; i >= 0; i--)
            {
                if (_viewModel.SelectedItems.Contains(_viewModel.Items[i]) && !_viewModel.Items[i].Swjf.isCanned)
                {
                    TrackHelper.LogDeleteSBkUpEvent(_viewModel.Items[i].Swjf.isCanned);

                    try
                    {
                        File.Delete(_viewModel.Items[i].FilePath);
                        Task_Scheduler.TryDeleteTask(_viewModel.Items[i]);
                    }
                    catch
                    {

                    }
                    _viewModel.Items.RemoveAt(i);
                }
            }
            _viewModel.SelectedItems.Clear();
        }

        public void ExecuteModifyCommand()
        {
            if (_viewModel.SelectedItems.Count != 1)
            {
                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, Properties.Resources.ERROR_SELECT_ONE_JOB);
                return;
            }

            if (_viewModel.SelectedItems.Count == 1)
            {
                var view = new CreateSBkUpView();
                view.Owner = SBkUpView.MainWindow;
                view.InitModifyMode(_viewModel.SelectedItems[0].Swjf);
                if (view.ShowDialog() == true)
                {
                    _viewModel.SwjfWatcher.EnableRaisingEvents = false;
                    if (view.CurrentSwjf.Save(_viewModel.SelectedItems[0].FilePath))
                    {
                        _viewModel.SelectedItems[0].Swjf = view.CurrentSwjf;
                        SBkUpView.MainWindow.ModifybottomText();

                        TrackHelper.LogModifySBkUpEvent(_viewModel.SelectedItems[0]);
                    }
                    _viewModel.SwjfWatcher.EnableRaisingEvents = true;
                }
            }
        }

        public void ExecuteAllCommand()
        {
            if (_viewModel.SelectedJob == null)
            {
                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, Properties.Resources.ERROR_SELECT_A_JOB);
                return;
            }

            if (_viewModel.Backups.Count == 0)
            {
                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, Properties.Resources.ERROR_JOB_HAS_NO_BACKUPS);
                return;
            }

            if (_viewModel.SelectedBackup == null)
            {
                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, Properties.Resources.ERROR_SELECT_ONE_BACKUP);
                return;
            }

            var handle = new WindowInteropHelper(SBkUpView.MainWindow).Handle;
            string zipPath = _viewModel.SelectedBackup.Item.path;
            if (WinZipMethods.IsCloudItem(_viewModel.SelectedBackup.Item.profile.Id))
            {
                var items = new WinZipMethods.WzCloudItem4[1] { _viewModel.SelectedBackup.Item };
                var tempFolder = Util.GetTemporaryDirectory();
                var folderItem = WinZipMethods.InitCloudItemFromPath(tempFolder);
                int downloadErrorCode = 0;
                if (!WinZipMethods.DownloadFromCloud(handle, items, 1, folderItem, false, true, ref downloadErrorCode))
                {
                    return;
                }
                zipPath = Path.Combine(tempFolder, _viewModel.SelectedBackup.Item.name);
            }

            if (!File.Exists(zipPath))
            {
                return;
            }

            var folder = new WinZipMethods.WzCloudItem4();
            if (WinZipMethods.SelectRestoreFolder(handle, in _viewModel.SelectedJob.Swjf.backupFolder, ref folder))
            {
                if (WinZipMethods.ExtractFromZip(handle, zipPath, _viewModel.OverWrite, true, folder))
                {
                    string tempStr;
                    if (WinZipMethods.IsCloudItem(folder.profile.Id))
                    {
                        var folderString = WinZipMethods.CloudItemToString(handle, folder);
                        tempStr = Path.GetTempFileName();
                        File.WriteAllText(tempStr, folderString);
                    }
                    else
                    {
                        tempStr = folder.itemId;
                    }

                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = Util.GetWinZipPath();
                    process.StartInfo.Arguments = string.Format("/showfolder -* \"{0}\"", tempStr);
                    process.Start();
                }
            }
        }

        public void ExecuteSpecifcCommand()
        {
            if (_viewModel.SelectedJob == null)
            {
                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, Properties.Resources.ERROR_SELECT_A_JOB);
                return;
            }

            if (_viewModel.Backups.Count == 0)
            {
                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, Properties.Resources.ERROR_JOB_HAS_NO_BACKUPS);
                return;
            }

            if (_viewModel.SelectedBackup == null)
            {
                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, Properties.Resources.ERROR_SELECT_ONE_BACKUP);
                return;
            }

            var argu = _viewModel.SelectedBackup.Item.itemId;
            if (WinZipMethods.IsCloudItem(_viewModel.SelectedBackup.Item.profile.Id))
            {
                var folder = Util.GetTemporaryDirectory();
                argu = WzCloud.Save(folder, _viewModel.SelectedBackup);
            }

            var handle = new WindowInteropHelper(SBkUpView.MainWindow).Handle;
            var backupFolderString = WinZipMethods.CloudItemToString(handle, _viewModel.SelectedJob.Swjf.backupFolder);
            var tempPath = Path.GetTempFileName();
            File.WriteAllText(tempPath, backupFolderString);

            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = Util.GetWinZipPath();
            process.StartInfo.Arguments = string.Format("/sbkrestore -* {0} \"{1}\" \"{2}\"", _viewModel.OverWrite ? "/overwrite" : string.Empty, tempPath, argu);
            process.Start();
        }

        public void ExecuteOpenCommand()
        {
            if (_viewModel.SelectedJob == null)
            {
                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, Properties.Resources.ERROR_SELECT_A_JOB);
                return;
            }

            if (_viewModel.Backups.Count == 0)
            {
                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, Properties.Resources.ERROR_JOB_HAS_NO_BACKUPS);
                return;
            }

            if (_viewModel.SelectedBackup == null)
            {
                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, Properties.Resources.ERROR_SELECT_ONE_BACKUP);
                return;
            }

            var argu = _viewModel.SelectedBackup.Item.itemId;
            if (WinZipMethods.IsCloudItem(_viewModel.SelectedBackup.Item.profile.Id))
            {
                var folder = Util.GetTemporaryDirectory();
                argu = WzCloud.Save(folder, _viewModel.SelectedBackup);
            }

            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = Util.GetWinZipPath();
            p.StartInfo.Arguments = "\"" + argu + "\"";
            p.Start();
        }
    }
}
