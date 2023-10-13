using Aspose.Pdf;
using Aspose.Pdf.Text;
using PdfUtil.WPFUI.Commands;
using PdfUtil.WPFUI.Controls;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace PdfUtil.WPFUI.ViewModel
{
    class ThumbnailPaneContextMenuViewModel : ObservableObject
    {
        private PdfUtilViewModel _viewModel;
        private ThumbnailPaneCommandModel _viewModelCommands;
        private bool _isContextMenuOpen;
        private const double _fillThresholdFactor = 0.01;

        private bool _isShowSetBackgroundColorMenuItem;
        private bool _isShowDeletePagesMenuItem;
        private bool _isShowDeleteBlankPagesMenuItem;
        private bool _isShowRotatePagesMenuItem;
        private bool _isShowMovePagesMenuItem;
        private bool _isShowWaterMarkPagesMenuItem;

        private bool _selectAllAfterMenuClosed;
        private IconItem _selectIconAfterMenuClosed;

        public ThumbnailPaneContextMenuViewModel(PdfUtilViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModelCommands = null;
            _selectAllAfterMenuClosed = false;
            _selectIconAfterMenuClosed = null;
        }

        [Obfuscation(Exclude = true)]
        public ThumbnailPaneCommandModel ViewModelCommands
        {
            get
            {
                if (_viewModelCommands == null)
                {
                    _viewModelCommands = new ThumbnailPaneCommandModel(this);
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
        public bool IsContextVisible
        {
            get
            {
                return _viewModel.IsContextVisible;
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowSetBackgroundColorMenuItem
        {
            get
            {
                return _isShowSetBackgroundColorMenuItem;
            }
            set
            {
                if (_isShowSetBackgroundColorMenuItem != value)
                {
                    _isShowSetBackgroundColorMenuItem = value;
                    Notify(nameof(IsShowSetBackgroundColorMenuItem));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowDeletePagesMenuItem
        {
            get
            {
                return _isShowDeletePagesMenuItem;
            }
            set
            {
                if (_isShowDeletePagesMenuItem != value)
                {
                    _isShowDeletePagesMenuItem = value;
                    Notify(nameof(IsShowDeletePagesMenuItem));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowDeleteBlankPagesMenuItem
        {
            get
            {
                return _isShowDeleteBlankPagesMenuItem;
            }
            set
            {
                if (_isShowDeleteBlankPagesMenuItem != value)
                {
                    _isShowDeleteBlankPagesMenuItem = value;
                    Notify(nameof(IsShowDeleteBlankPagesMenuItem));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowRotatePagesMenuItem
        {
            get
            {
                return _isShowRotatePagesMenuItem;
            }
            set
            {
                if (_isShowRotatePagesMenuItem != value)
                {
                    _isShowRotatePagesMenuItem = value;
                    Notify(nameof(IsShowRotatePagesMenuItem));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowMovePagesMenuItem
        {
            get
            {
                return _isShowMovePagesMenuItem;
            }
            set
            {
                if (_isShowMovePagesMenuItem != value)
                {
                    _isShowMovePagesMenuItem = value;
                    Notify(nameof(IsShowMovePagesMenuItem));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowWaterMarkPagesMenuItem
        {
            get
            {
                return _isShowWaterMarkPagesMenuItem;
            }
            set
            {
                if (_isShowWaterMarkPagesMenuItem != value)
                {
                    _isShowWaterMarkPagesMenuItem = value;
                    Notify(nameof(IsShowWaterMarkPagesMenuItem));
                }
            }
        }

        public void UpdateThumbnailContextMenuState()
        {
            var pdfUtilView = _viewModel.PdfUtilView as PdfUtilView;

            if (pdfUtilView.ThumbnailListView.SelectedItems.Count > 0)
            {
                IsShowSetBackgroundColorMenuItem = true;
                IsShowDeletePagesMenuItem = true;
                IsShowDeleteBlankPagesMenuItem = true;
                IsShowRotatePagesMenuItem = true;
                IsShowWaterMarkPagesMenuItem = true;
                IsShowMovePagesMenuItem = !(_viewModel.CurrentPdfDocument.Pages.Count < 2 || pdfUtilView.ThumbnailListView.SelectedItems.Count == _viewModel.CurrentPdfDocument.Pages.Count);

            }
            else
            {
                IsShowSetBackgroundColorMenuItem = false;
                IsShowDeletePagesMenuItem = false;
                IsShowDeleteBlankPagesMenuItem = false;
                IsShowRotatePagesMenuItem = false;
                IsShowWaterMarkPagesMenuItem = false;
                IsShowMovePagesMenuItem = false;
            }
        }

        public void ExecuteSetBackgroundColorCommand()
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            var selectedThumbnailItems = _viewModel.ThumbnailListView.SelectedItems;
            if (selectedThumbnailItems.Count != 0)
            {
                var colorDialog = new ColorDialogEx(System.Drawing.Color.White, true);
                if (colorDialog.ShowWindow() == System.Windows.Forms.DialogResult.OK)
                {
                    var newBackgroundColor = colorDialog.IsRemoveBackgroudColor ? System.Drawing.Color.White : colorDialog.Color;

                    var selectedObjs = new object[selectedThumbnailItems.Count];
                    selectedThumbnailItems.CopyTo(selectedObjs, 0);
                    var selectedIcons = Array.ConvertAll(selectedObjs, item => (IconItem)item);
                    var selectedPages = Array.ConvertAll(selectedIcons, item => item.GetPage());

                    if (PdfHelper.SetBackgroundColor(selectedPages, newBackgroundColor))
                    {
                        foreach (var icon in selectedIcons)
                        {
                            icon.UpdateThumbnailImage();
                        }
                        _viewModel.PreviewPageNeedUpdate();
                        _viewModel.NotifyDocChanged();

                        TrackHelper.LogSetBgColorEvent(ColorDialogEx.IsChoosePresetColor(colorDialog));
                    }
                }
            }
            else
            {
                FlatMessageWindows.DisplayWarningMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.EXECUTE_SET_BACKGROUND_COLOR_PDF_TIPS);
            }
        }

        public void ExecuteRotatePageCommand()
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            var selectedThumbnailItems = _viewModel.ThumbnailListView.SelectedItems;
            if (selectedThumbnailItems.Count != 0)
            {
                var rotatedialog = new RotateView(_viewModel.PdfUtilView);
                if (rotatedialog.ShowWindow())
                {
                    var selectedObjs = new object[selectedThumbnailItems.Count];
                    selectedThumbnailItems.CopyTo(selectedObjs, 0);
                    var selectedIcons = Array.ConvertAll(selectedObjs, item => (IconItem)item);
                    var selectedPages = Array.ConvertAll(selectedIcons, item => item.GetPage());

                    if (PdfHelper.DoRotatePages(selectedPages, rotatedialog.CurDegreesSelected))
                    {
                        foreach (var icon in selectedIcons)
                        {
                            icon.UpdateThumbnailImage(rotatedialog.CurDegreesSelected);
                        }
                        _viewModel.PreviewPageNeedUpdate();
                        _viewModel.NotifyDocChanged();

                        TrackHelper.LogPdfRotateEvent(selectedPages.Count(), rotatedialog.CurDegreesSelected);
                    }
                }
            }
            else
            {
                FlatMessageWindows.DisplayWarningMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.EXECUTE_ROTATE_PAGES_TIPS);
            }
        }

        public void ExecuteDeletePagesCommand()
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            var selectedThumbnailItems = _viewModel.ThumbnailListView.SelectedItems;
            if (selectedThumbnailItems.Count == 0)
            {
                FlatMessageWindows.DisplayWarningMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.SELECT_PAGES_WARNING);
            }
            else
            {
                if (FlatMessageWindows.DisplayConfirmationMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.DELETE_PAGES_CONFIRMING))
                {
                    var selectedObjs = new object[selectedThumbnailItems.Count];
                    selectedThumbnailItems.CopyTo(selectedObjs, 0);
                    var selectedIcons = Array.ConvertAll(selectedObjs, item => (IconItem)item);
                    var selectedPages = Array.ConvertAll(selectedIcons, item => item.GetPage());
                    var pageIndexs = Array.ConvertAll(selectedPages, new Converter<Aspose.Pdf.Page, int>(_viewModel.GetPageIndex));
                    var lastSelectedIcon = selectedIcons[selectedIcons.Length - 1];
                    int lastSelectedIconIndex = _viewModel.Icon.IconSources.IndexOf(lastSelectedIcon);
                    IconItem newlySelectedItem = null;
                    while (lastSelectedIconIndex >= 0 && selectedIcons.Contains(_viewModel.Icon.IconSources[lastSelectedIconIndex]))
                    {
                        lastSelectedIconIndex--;
                    }

                    if (lastSelectedIconIndex != -1)
                    {
                        newlySelectedItem = _viewModel.Icon.IconSources[lastSelectedIconIndex] as IconItem;
                    }

                    _viewModel.CurrentPdfDocument.Pages.Delete(pageIndexs);
                    _viewModel.IsIconSourcesChanging = true;
                    foreach (var icon in selectedIcons)
                    {
                        _viewModel.RemoveCommentOnPage(icon);
                        _viewModel.Icon.IconSources.Remove(icon);
                    }
                    _viewModel.IsIconSourcesChanging = false;

                    if (lastSelectedIconIndex == -1)
                    {
                        if (_viewModel.ThumbnailListView.Items.Count != 0)
                        {
                            _viewModel.ThumbnailListView.SelectedIndex = 0;
                        }
                        else
                        {
                            _viewModel.PreviewPageNeedUpdate();
                            _viewModel.PdfUtilView.findReplaceControl.CloseControl();

                            if (_viewModel.Icon.IconSources.Count == 0)
                            {
                                _viewModel.PreviewPageImage = null;
                                _viewModel.CurPreviewIconItem = null;
                                _viewModel.PdfUtilView.UpdateCommentsOnCanvas();
                            }
                        }
                    }
                    else if (newlySelectedItem != null)
                    {
                        _viewModel.ThumbnailListView.SelectedItem = newlySelectedItem;
                    }

                    if ((_viewModel.PdfUtilView.CurCaretIndex == 0 && pageIndexs.Contains(_viewModel.PdfUtilView.CurCaretIndex + 1)) || pageIndexs.Contains(_viewModel.PdfUtilView.CurCaretIndex))
                    {
                        _viewModel.PdfUtilView.CurCaretIndex = -1;
                        _viewModel.PdfUtilView.RefreshCaretWhenIconDeleted();
                    }
                    else
                    {
                        int count = 0;
                        foreach (var index in pageIndexs)
                        {
                            if (index < _viewModel.PdfUtilView.CurCaretIndex)
                            {
                                count++;
                            }
                        }
                        _viewModel.PdfUtilView.CurCaretIndex -= count;
                        _viewModel.PdfUtilView.RefreshCaretWhenIconDeleted();
                    }

                    if (_viewModel.ThumbnailListView.SelectedItem != null) 
                    {
                        var icon = _viewModel.ThumbnailListView.SelectedItem as IconItem;
                        _viewModel.ThumbnailListView.UpdateLayout();
                        _viewModel.ThumbnailListView.ScrollIntoView(icon);
                    }

                    _viewModel.RootBookmarkTree.SyncBookmarkWithDeletedPages(new List<int>(pageIndexs));

                    _viewModel.RefreshCommentCollectionView();
                    _viewModel.RefreshWhenPageChanged();
                    _viewModel.NotifyDocChanged();

                    TrackHelper.LogPdfDeletePagesEvent(selectedObjs.Length, true);
                }
            }
        }

        public bool CanExecuteDeleteBlankPagesCommand()
        {
            return true;
        }

        public void ExecuteDeleteBlankPagesCommand()
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            if (FlatMessageWindows.DisplayConfirmationMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.DELETE_BLANK_PAGES_CONFIRMING))
            {
                Task.Factory.StartNew(() =>
                {
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                    {
                        _viewModel.SetThumbnailCursorBusy(true);
                    }));

                    var pagesToDelete = new List<int>();
                    bool isDocEmpty = false;
                    if (_viewModel.CurrentPdfDocument == null)
                    {
                        isDocEmpty = true;
                    }

                    if (!isDocEmpty)
                    {
                        foreach (var page in _viewModel.CurrentPdfDocument.Pages)
                        {
                            if (page.IsBlank(_fillThresholdFactor))
                            {
                                pagesToDelete.Add(_viewModel.CurrentPdfDocument.Pages.IndexOf(page));
                            }
                        }
                    }

                    if (isDocEmpty || pagesToDelete.Count == 0)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                        {
                            FlatMessageWindows.DisplayInformationMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, Properties.Resources.NO_BLANK_PAGES_FOUND);
                        }));
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                        {
                            var selectedThumbnailItems = _viewModel.ThumbnailListView.SelectedItems;
                            IconItem newlySelectedItem = null;
                            int lastSelectedItemIndex = -1;
                            if (selectedThumbnailItems.Count != 0)
                            {
                                var lastSelectedItem = selectedThumbnailItems[selectedThumbnailItems.Count - 1] as IconItem;
                                lastSelectedItemIndex = _viewModel.Icon.IconSources.IndexOf(lastSelectedItem);
                                if (lastSelectedItem != null)
                                {
                                    if (lastSelectedItem.GetPage().IsBlank(_fillThresholdFactor))
                                    {
                                        while (lastSelectedItemIndex >= 0 && pagesToDelete.Contains(lastSelectedItemIndex + 1))
                                        {
                                            lastSelectedItemIndex--;
                                        }

                                        if (lastSelectedItemIndex != -1)
                                        {
                                            newlySelectedItem = _viewModel.Icon.IconSources[lastSelectedItemIndex] as IconItem;
                                        }
                                    }
                                }
                            }

                            _viewModel.CurrentPdfDocument.Pages.Delete(pagesToDelete.ToArray());
                            _viewModel.IsIconSourcesChanging = true;
                            var iconToDeleteList = new List<IconItem>();
                            foreach (var pageIndex in pagesToDelete)
                            {
                                var item = _viewModel.Icon.IconSources[pageIndex - 1] as IconItem;
                                if (item != null)
                                {
                                    iconToDeleteList.Add(item);
                                }
                            }

                            foreach (var icon in iconToDeleteList)
                            {
                                _viewModel.Icon.IconSources.Remove(icon);
                            }

                            _viewModel.IsIconSourcesChanging = false;

                            if (lastSelectedItemIndex == -1)
                            {
                                if (_viewModel.ThumbnailListView.Items.Count != 0)
                                {
                                    _viewModel.ThumbnailListView.SelectedIndex = 0;
                                }
                                else
                                {
                                    _viewModel.PreviewPageNeedUpdate();
                                    _viewModel.PdfUtilView.findReplaceControl.CloseControl();

                                    if (_viewModel.Icon.IconSources.Count == 0)
                                    {
                                        _viewModel.PreviewPageImage = null;
                                        _viewModel.CurPreviewIconItem = null;
                                        _viewModel.PdfUtilView.UpdateCommentsOnCanvas();
                                    }
                                }
                            }
                            else if (newlySelectedItem != null)
                            {
                                _viewModel.ThumbnailListView.SelectedItem = newlySelectedItem;
                            }

                            if ((_viewModel.PdfUtilView.CurCaretIndex == 0 && pagesToDelete.Contains(_viewModel.PdfUtilView.CurCaretIndex + 1)) || pagesToDelete.Contains(_viewModel.PdfUtilView.CurCaretIndex))
                            {
                                _viewModel.PdfUtilView.CurCaretIndex = -1;
                                _viewModel.PdfUtilView.RefreshCaretWhenIconDeleted();
                            }
                            else
                            {
                                int count = 0;
                                foreach (var index in pagesToDelete)
                                {
                                    if (index < _viewModel.PdfUtilView.CurCaretIndex)
                                    {
                                        count++;
                                    }
                                }
                                _viewModel.PdfUtilView.CurCaretIndex -= count;
                                _viewModel.PdfUtilView.RefreshCaretWhenIconDeleted();
                            }

                            if (_viewModel.ThumbnailListView.SelectedItem != null) 
                            {
                                var icon = _viewModel.ThumbnailListView.SelectedItem as IconItem;
                                _viewModel.ThumbnailListView.ScrollIntoView(icon);
                            }

                            _viewModel.RootBookmarkTree.SyncBookmarkWithDeletedPages(pagesToDelete);

                            _viewModel.RefreshCommentCollectionView();
                            _viewModel.RefreshWhenPageChanged();
                            _viewModel.NotifyDocChanged();

                            TrackHelper.LogPdfDeletePagesEvent(pagesToDelete.Count, false);
                        }));
                    }

                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                    {
                        _viewModel.SetThumbnailCursorBusy(false);
                    }));
                });
            }
        }

        public bool CanExecuteAddBlankPageCommand()
        {
            return _viewModel.ThumbnailListView.Items.Count != 0;
        }

        public void ExecuteAddBlankPageCommand(bool isFromContextMenu = true)
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            if (_viewModel.CurrentPdfDocument != null)
            {
                if (_viewModel.ThumbnailListView.Items.Count != 0)
                {
                    var selectedThumbnailItems = _viewModel.ThumbnailListView.SelectedItems;
                    var lastSelectedItem = selectedThumbnailItems.Count == 0 ? _viewModel.ThumbnailListView.Items[0] as IconItem : selectedThumbnailItems[selectedThumbnailItems.Count - 1] as IconItem;
                    if (lastSelectedItem != null)
                    {
                        int iconInsertIndex = 0;
                        if (isFromContextMenu)
                        {
                            bool isMouseDownInItemArea = false;
                            var item = _viewModel.GetClosestAndBelowItem(ref isMouseDownInItemArea);
                            if (item == null)
                            {
                                iconInsertIndex = _viewModel.Icon.IconSources.Count;
                            }
                            else
                            {
                                if (isMouseDownInItemArea)
                                {
                                    iconInsertIndex = _viewModel.Icon.IconSources.IndexOf(item) + 1;
                                }
                                else
                                {
                                    iconInsertIndex = _viewModel.Icon.IconSources.IndexOf(item);
                                }
                            }
                        }
                        else
                        {
                            if (_viewModel.PdfUtilView.CurCaretIndex != -1)
                            {
                                iconInsertIndex = _viewModel.PdfUtilView.CurCaretIndex;
                            }
                            else
                            {
                                iconInsertIndex = _viewModel.Icon.IconSources.Count;
                            }
                        }

                        var info = lastSelectedItem.GetPage().PageInfo;
                        var width = info.Width;
                        var height = info.Height;
                        var newPage = _viewModel.CurrentPdfDocument.Pages.Insert(iconInsertIndex + 1);
                        newPage.SetPageSize(width, height);
                        _selectIconAfterMenuClosed = _viewModel.InsertIconItem(newPage, iconInsertIndex, false, false);
                        _viewModel.ThumbnailListView.ScrollIntoView(_selectIconAfterMenuClosed);

                        _viewModel.RootBookmarkTree.SyncBookmarkWithAddedPages(new List<int> { iconInsertIndex + 1 });

                        _viewModel.RefreshCommentCollectionView();
                        _viewModel.RefreshWhenPageChanged();
                        _viewModel.NotifyDocChanged();

                        if (_viewModel.PdfUtilView.CurCaretIndex != -1)
                        {
                            _viewModel.PdfUtilView.CurCaretIndex++;
                        }

                        TrackHelper.TrackHelperInstance.AddBlankPageCount++;
                    }
                }
                else
                {
                    AddDefaultBlankPage();
                }
            }
            else
            {
                var defaultFileName = Path.Combine(FileOperation.CreateTempFolder(FileOperation.GlobalTempDir), Properties.Resources.DEFAULT_PDF_TITLE_NAME);
                _viewModel.CurrentPdfDocument = new Document();
                _viewModel.CurrentPdfDocument.Save(defaultFileName);
                _viewModel.CurrentPdfFileName = defaultFileName;
                _viewModel.CurrentPdfFileInfo = new Aspose.Pdf.Facades.PdfFileInfo(_viewModel.CurrentPdfDocument);
                _viewModel.LoadBookmarkSource(_viewModel.CurrentPdfDocument.Outlines, 0, new CancellationToken());
                _viewModel.CurrentPdfFilePasswordInfo = new PdfFilePasswordInfo
                {
                    HasOpenPassword = _viewModel.CurrentPdfFileInfo.HasOpenPassword,
                    HasEditPassword = _viewModel.CurrentPdfFileInfo.HasEditPassword,
                    PasswordType = _viewModel.CurrentPdfFileInfo.PasswordType
                };

                var pdfInfo = new PDFInfo
                {
                    filePath = defaultFileName,
                    fileName = Path.GetFileName(defaultFileName)
                };

                _viewModel.Icon.SetPDFFileInfo(pdfInfo);
                AddDefaultBlankPage();
            }
        }

        private void AddDefaultBlankPage()
        {
            const double width = 794; //A4 size in 96 PPI
            const double height = 1123;
            int iconDefaultIndex = 0;
            Page newPage = _viewModel.CurrentPdfDocument.Pages.Add();
            newPage.SetPageSize(width, height);
            _selectIconAfterMenuClosed = _viewModel.InsertIconItem(newPage, iconDefaultIndex, true, false);
            _viewModel.ThumbnailListView.ScrollIntoView(_selectIconAfterMenuClosed);
            _viewModel.RefreshWhenPageChanged();
            _viewModel.NotifyTotalPageCount();
            _viewModel.NotifyDocChanged();

            TrackHelper.TrackHelperInstance.AddBlankPageCount++;
        }

        public void ExecuteExtractImagesCommand()
        {
            _viewModel.ExecuteExtractImagesCommand();
        }

        public void ExecuteExtractPagesCommand()
        {
            _viewModel.ExecuteExtractPagesCommand();
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

        public void ExecuteWatermarkPagesCommand()
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyPagesPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            var selectedThumbnailItems = _viewModel.ThumbnailListView.SelectedItems;
            if (selectedThumbnailItems.Count == 0)
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.EXECUTE_WATERMARK_PAGES_TIPS);
            }
            else
            {
                _viewModel.Executor(() => ExecuteWatermarkTask(), RetryStrategy.Create(false, 0)).IgnoreExceptions();
            }
        }

        public void ExecuteSelectAllCommand()
        {
            _selectAllAfterMenuClosed = true;
        }

        public void SelectPagesAfterContextMenuClosed()
        {
            // If the page(s) selection is made during command execution, the visibility of the ContextMenuItem will be affected before ContextMenu is closed,
            // and the ContextMenu will flicker. So the page(s) selection should be made after the ContextMenu is closed.
            if (_selectAllAfterMenuClosed)
            {
                _viewModel.ThumbnailListView.SelectAll();
                _selectAllAfterMenuClosed = false;
            }
            else if (_selectIconAfterMenuClosed != null)
            {
                _viewModel.ThumbnailListView.SelectedItem = _selectIconAfterMenuClosed;
                _selectIconAfterMenuClosed = null;
            }
        }

        private Task ExecuteWatermarkTask()
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                var selectedThumbnailItems = _viewModel.ThumbnailListView.SelectedItems;
                var watermarkSettingView = new WatermarkSettingView(_viewModel.PdfUtilView);
                watermarkSettingView.InitDataContext();

                if (watermarkSettingView.ShowWindow())
                {
                    var selectedObjs = new object[selectedThumbnailItems.Count];
                    selectedThumbnailItems.CopyTo(selectedObjs, 0);
                    var selectedIcons = Array.ConvertAll(selectedObjs, item => (IconItem)item);

                    (watermarkSettingView.DataContext as WatermarkSettingViewModel)?.DoWatermark(selectedIcons);
                }

                tcs.TrySetResult();
            });
        }

        public void ExecuteMovePagesCommand(bool isFromContextMenu)
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            var selectedThumbnailItems = _viewModel.ThumbnailListView.SelectedItems;
            if (selectedThumbnailItems.Count != 0)
            {
                if (isFromContextMenu)
                {
                    var dialog = new MovePagesDialog(_viewModel.PdfUtilView, _viewModel.CurrentPdfDocument.Pages.Count);
                    if (dialog.ShowWindow())
                    {
                        DoMovePages(dialog.DestPage, dialog.IsPutAfter);
                    }
                }
                else
                {
                    _viewModel.PdfUtilView.MovePagesPopupWindow.IsOpen = true;
                }
            }
            else
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.SELECT_PAGES_MOVE_WARNING);
            }
        }

        public void DoMovePages(int destPage, bool isPutAfter)
        {
            var selectedItemArray = new IconItem[_viewModel.ThumbnailListView.SelectedItems.Count];
            _viewModel.ThumbnailListView.SelectedItems.CopyTo(selectedItemArray, 0);
            Array.Sort(selectedItemArray, _viewModel.PdfUtilView.IconCompare);

            destPage = isPutAfter ? destPage : destPage - 1;
            _viewModel.PdfUtilView.ReorderPagesAndUpdateListView(selectedItemArray, destPage);
            _viewModel.ThumbnailListView.UnselectAll();

            TrackHelper.LogPdfReorderEvent(false);
        }
    }
}
