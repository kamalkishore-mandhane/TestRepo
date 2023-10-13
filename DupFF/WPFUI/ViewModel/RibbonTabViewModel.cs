using DupFF.Utils;
using DupFF.WPFUI.Commands;
using DupFF.WPFUI.Controls;
using DupFF.WPFUI.Utils;
using DupFF.WPFUI.View;
using System;
using System.Windows.Threading;

namespace DupFF.WPFUI.ViewModel
{
    class RibbonTabViewModel
    {
        private RibbonCommand _viewModelCommands;
        private DupFFViewModel _viewModel;

        public RibbonTabViewModel(DupFFViewModel model)
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

        public void ExecuteCreateCommand()
        {
            var str = WinZipMethods.CreateBGTool(DupFFView.MainWindow.WindowHandle);
            TrackHelper.LogFinderCreatedEvent();
            UpdateDisplayItem(str);
        }

        public void ExecuteRunCommand()
        {
            bool noAvailableItem = true;

            foreach (var item in _viewModel.SelectedItems)
            {
                if (item.Available)
                {
                    noAvailableItem = false;
                    WinZipMethods.RunBGTool(DupFFView.MainWindow.WindowHandle, item.Index);
                }
            }

            if (noAvailableItem)
            {
                FlatMessageBox.ShowWarning(DupFFView.MainWindow, Properties.Resources.ERROR_SELECT_ONE_DFF_RUN);
                return;
            }
        }

        public void ExecuteDeleteCommand()
        {
            bool noAvailableItem = true;

            foreach (var item in _viewModel.SelectedItems)
            {
                if (item.Available && !item.IsCanned)
                {
                    noAvailableItem = false;
                    WinZipMethods.DeleteBGTools(DupFFView.MainWindow.WindowHandle, item.Index);
                }
            }

            if (noAvailableItem)
            {
                FlatMessageBox.ShowWarning(DupFFView.MainWindow, Properties.Resources.ERROR_SELECT_ONE_DFF_DELETE);
                return;
            }
        }

        public void ExecuteUndoDeleteCommand()
        {
            bool noAvailableItem = true;

            foreach (var item in _viewModel.SelectedItems)
            {
                if (item.ItemStatus == ItemStatus.Delete)
                {
                    noAvailableItem = false;
                    WinZipMethods.UndoDeleteBGTools(DupFFView.MainWindow.WindowHandle, item.Index);
                }
            }

            if (noAvailableItem)
            {
                FlatMessageBox.ShowWarning(DupFFView.MainWindow, Properties.Resources.ERROR_SELECT_ONE_DFF_UNDO);
                return;
            }
        }

        public void ExecuteModifyCommand()
        {
            if (_viewModel.SelectedItems.Count != 1 || !_viewModel.SelectedItems[0].Available)
            {
                FlatMessageBox.ShowWarning(DupFFView.MainWindow, Properties.Resources.ERROR_SELECT_ONE_DFF_MODIFY);
                return;
            }

            var str = WinZipMethods.ModifyBGTool(DupFFView.MainWindow.WindowHandle, _viewModel.SelectedItems[0].Index);
            TrackHelper.LogFinderModifiedEvent();
            UpdateDisplayItem(str);
        }

        public void ExecuteActionCommand()
        {
            if (_viewModel.SelectedActionItems.Count != 1 || _viewModel.SelectedActionItems[0].Processed)
            {
                FlatMessageBox.ShowWarning(DupFFView.MainWindow, Properties.Resources.ERROR_SELECT_ONE_DFF_ACTION);
                return;
            }

            if (WinZipMethods.BGTTakeAction(DupFFView.MainWindow.WindowHandle, _viewModel.SelectedActionItems[0].Guid))
            {
                var str = WinZipMethods.GetBGTRNInfos(DupFFView.MainWindow.WindowHandle, DupFFView.MainWindow.ViewType);
                UpdateActionItem(str);
            }
        }

        public void ExecuteDismissCommand()
        {
            if (_viewModel.SelectedActionItems.Count == 0)
            {
                FlatMessageBox.ShowWarning(DupFFView.MainWindow, Properties.Resources.ERROR_SELECT_ONE_DFF_DISMISS);
                return;
            }

            foreach (var item in _viewModel.SelectedActionItems)
            {
                WinZipMethods.DismissBGTRN(DupFFView.MainWindow.WindowHandle, item.Guid);
            }

            var str = WinZipMethods.GetBGTRNInfos(DupFFView.MainWindow.WindowHandle, DupFFView.MainWindow.ViewType);
            UpdateActionItem(str);
        }

        public void ExecuteDismissAllCommand()
        {
            if (_viewModel.ActionItems.Count == 0)
            {
                FlatMessageBox.ShowWarning(DupFFView.MainWindow, Properties.Resources.ERROR_SELECT_ONE_DFF_DISMISSALL);
                return;
            }

            foreach (var item in _viewModel.ActionItems)
            {
                WinZipMethods.DismissBGTRN(DupFFView.MainWindow.WindowHandle, item.Guid);
            }

            var str = WinZipMethods.GetBGTRNInfos(DupFFView.MainWindow.WindowHandle, DupFFView.MainWindow.ViewType);
            UpdateActionItem(str);
        }

        public void UpdateDisplayItem(string str)
        {
            var infos = DisplayItem.ParseDisplayItem(str);
            _viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                foreach (var info in infos)
                {
                    if (!RegeditOperation.IsCannedBGTToolsEnabled() && info.IsCanned)
                    {
                        continue;
                    }
                    bool handled = false;
                    foreach (var item in _viewModel.Items)
                    {
                        if (info.Guid == item.Guid)
                        {
                            item.StopSyncBGToolInfos = true;
                            DisplayItem.CloneItem(item, info);
                            item.StopSyncBGToolInfos = false;
                            handled = true;
                            break;
                        }
                    }
                    if (!handled)
                    {
                        info.RunComplete += (sender, e) =>
                        {
                            var updateInfo = WinZipMethods.GetBGTRNInfos(DupFFView.MainWindow.WindowHandle, DupFFView.MainWindow.ViewType);
                            UpdateActionItem(updateInfo);
                        };
                        _viewModel.Items.Add(info);
                    }
                }
            }));
        }

        public void UpdateActionItem(string str)
        {
            if (str == null)
            {
                return;
            }
            var infos = ActionItem.ParseActionItem(str);
            _viewModel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                for(var i = infos.Count - 1; i >= 0; i--)
                {
                    bool finded = false;
                    foreach (var item in _viewModel.Items)
                    {
                        if (infos[i].Guid == item.Guid)
                        {
                            finded = true;
                            infos[i].Name = item.Name;
                            break;
                        }
                    }
                    if (!finded || infos[i].Processed)
                    {
                        WinZipMethods.DismissBGTRN(IntPtr.Zero, infos[i].Guid);
                        infos.RemoveAt(i);
                    }
                }

                bool fresh = infos.Count != _viewModel.ActionItems.Count;

                foreach (var info in infos)
                {
                    if (fresh)
                    {
                        break;
                    }
                    bool handled = false;
                    foreach (var item in _viewModel.ActionItems)
                    {
                        if (info.Guid == item.Guid)
                        {
                            ActionItem.CloneItem(item, info);
                            handled = true;
                            break;
                        }
                    }
                    if (!handled)
                    {
                        fresh = true;
                    }
                }

                if (fresh)
                {
                    _viewModel.SelectedActionItems.Clear();
                    _viewModel.ActionItems.Clear();
                    foreach (var item in _viewModel.Items)
                    {
                        item.ActionItem = null;
                    }

                    foreach (var item in _viewModel.Items)
                    {
                        foreach (var info in infos)
                        {
                            if (item.Guid == info.Guid)
                            {
                                info.Name = item.Name;
                                item.ActionItem = info;
                                _viewModel.ActionItems.Add(info);
                            }
                        }
                    }
                }
            }));
        }
    }
}
