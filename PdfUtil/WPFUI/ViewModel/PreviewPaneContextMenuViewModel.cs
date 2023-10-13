using System.Reflection;
using PdfUtil.WPFUI.Commands;
using PdfUtil.WPFUI.Controls;
using PdfUtil.WPFUI.Utils;

namespace PdfUtil.WPFUI.ViewModel
{
    class PreviewPaneContextMenuViewModel : ObservableObject
    {
        private PdfUtilViewModel _viewModel;
        private PreviewPaneCommandModel _viewModelCommands;
        private bool _isContextMenuOpen;

        private bool _isShowAddCommentItem;
        private bool _isShowDeleteCommentItem;
        private bool _isShowDeleteCommentsItem;
        private bool _isShowAddHighlightItem;
        private bool _isShowChangeHighlightItem;
        private bool _isShowDeleteHighlightsItem;
        private bool _isShowDeleteSignatureItem;
        private bool _isShowDeleteSignaturesItem;

        public PreviewPaneContextMenuViewModel(PdfUtilViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        [Obfuscation(Exclude = true)]
        public PreviewPaneCommandModel ViewModelCommands
        {
            get
            {
                if (_viewModelCommands == null)
                {
                    _viewModelCommands = new PreviewPaneCommandModel(this);
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
        public bool IsShowAddCommentItem
        {
            get
            {
                return _isShowAddCommentItem;
            }
            set
            {
                if (_isShowAddCommentItem != value)
                {
                    _isShowAddCommentItem = value;
                    Notify(nameof(IsShowAddCommentItem));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowDeleteCommentItem
        {
            get
            {
                return _isShowDeleteCommentItem;
            }
            set
            {
                if (_isShowDeleteCommentItem != value)
                {
                    _isShowDeleteCommentItem = value;
                    Notify(nameof(IsShowDeleteCommentItem));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowDeleteCommentsItem
        {
            get
            {
                return _isShowDeleteCommentsItem;
            }
            set
            {
                if (_isShowDeleteCommentsItem != value)
                {
                    _isShowDeleteCommentsItem = value;
                    Notify(nameof(IsShowDeleteCommentsItem));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowAddHighlightItem
        {
            get
            {
                return _isShowAddHighlightItem;
            }
            set
            {
                if (_isShowAddHighlightItem != value)
                {
                    _isShowAddHighlightItem = value;
                    Notify(nameof(IsShowAddHighlightItem));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowChangeHighlightItem
        {
            get
            {
                return _isShowChangeHighlightItem;
            }
            set
            {
                if (_isShowChangeHighlightItem != value)
                {
                    _isShowChangeHighlightItem = value;
                    Notify(nameof(IsShowChangeHighlightItem));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowDeleteHighlightsItem
        {
            get
            {
                return _isShowDeleteHighlightsItem;
            }
            set
            {
                if (_isShowDeleteHighlightsItem != value)
                {
                    _isShowDeleteHighlightsItem = value;
                    Notify(nameof(IsShowDeleteHighlightsItem));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowDeleteSignatureItem
        {
            get
            {
                return _isShowDeleteSignatureItem;
            }
            set
            {
                if (_isShowDeleteSignatureItem != value)
                {
                    _isShowDeleteSignatureItem = value;
                    Notify(nameof(IsShowDeleteSignatureItem));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsShowDeleteSignaturesItem
        {
            get
            {
                return _isShowDeleteSignaturesItem;
            }
            set
            {
                if (_isShowDeleteSignaturesItem != value)
                {
                    _isShowDeleteSignaturesItem = value;
                    Notify(nameof(IsShowDeleteSignaturesItem));
                }
            }
        }

        public void InitPreviewPaneMenuItem()
        {
            IsShowAddCommentItem = _viewModel.IsCommentOptionEnable;
            IsShowDeleteCommentItem = _viewModel.IsCommentOptionEnable && IsCommentSelected(false);
            IsShowAddHighlightItem = _viewModel.IsRectSelected && IsTextSelectedByRectangle();
            IsShowChangeHighlightItem = IsHighlightSelected(false);
            IsShowDeleteSignatureItem = IsSignatureSelected(false);
        }

        public bool InitPreviewPaneSelectedRectMenuItem()
        {
            IsShowDeleteCommentsItem = _viewModel.IsCommentOptionEnable && IsCommentSelected(true);
            IsShowAddHighlightItem = _viewModel.IsRectSelected && IsTextSelectedByRectangle();
            IsShowDeleteHighlightsItem = IsHighlightSelected(true);
            IsShowDeleteSignaturesItem = IsSignatureSelected(true);

            return IsShowDeleteCommentsItem || IsShowAddHighlightItem || IsShowDeleteHighlightsItem || IsShowDeleteSignaturesItem;
        }

        private bool IsCommentSelected(bool isInRectangle)
        {
            var previewPage = GetPreviewPageFromThumbnail();
            if (previewPage != null)
            {
                if (isInRectangle)
                {
                    var leftTopPointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.SelectRectLeftTopPoint, previewPage);
                    var rightBottomPointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.SelectRectRightBottomPoint, previewPage);
                    return PdfHelper.FindSelectedComment(previewPage, leftTopPointOnPage, rightBottomPointOnPage);
                }
                else
                {
                    string currentComment = string.Empty;
                    var pointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.MouseButtonChoosedPoint, previewPage);
                    return PdfHelper.FindSelectedComment(previewPage, pointOnPage);
                }
            }
            return false;
        }

        private bool IsHighlightSelected(bool isInRectangle)
        {
            var previewPage = GetPreviewPageFromThumbnail();
            if (previewPage != null)
            {
                if (isInRectangle)
                {
                    var leftTopPointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.SelectRectLeftTopPoint, previewPage);
                    var rightBottomPointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.SelectRectRightBottomPoint, previewPage);
                    return PdfHelper.FindSelectedHighlight(previewPage, leftTopPointOnPage, rightBottomPointOnPage);
                }
                else
                {
                    var pointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.MouseButtonChoosedPoint, previewPage);
                    return PdfHelper.FindSelectedHighlight(previewPage, pointOnPage);
                }
            }
            return false;
        }

        private bool IsTextSelectedByRectangle()
        {
            var previewPage = GetPreviewPageFromThumbnail();
            if (previewPage != null)
            {
                var leftTopPointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.SelectRectLeftTopPoint, previewPage);
                var rightBottomPointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.SelectRectRightBottomPoint, previewPage);
                return PdfHelper.FindSelectedText(previewPage, leftTopPointOnPage, rightBottomPointOnPage);
            }
            return false;
        }

        private bool IsSignatureSelected(bool isInRectangle)
        {
            var previewPage = GetPreviewPageFromThumbnail();
            if (previewPage != null)
            {
                if (isInRectangle)
                {
                    var leftTopPointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.SelectRectLeftTopPoint, previewPage);
                    var rightBottomPointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.SelectRectRightBottomPoint, previewPage);
                    return PdfHelper.FindSelectedSignature(previewPage, leftTopPointOnPage, rightBottomPointOnPage);
                }
                else
                {
                    var pointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.MouseButtonChoosedPoint, previewPage);
                    return PdfHelper.FindSelectedSignature(previewPage, pointOnPage);
                }
            }
            return false;
        }

        public bool CanExecuteAddBookmarkCommand()
        {
            return _viewModel.RootBookmarkTree.SelectedList.Count <= 1;
        }

        public void ExecuteAddBookmarkCommand()
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            if (_viewModel.RootBookmarkTree.SelectedList.Count == 1)
            {
                var control = _viewModel.RootBookmarkTree.SelectedList[0];
                if (control.Level != 1)
                {
                    _viewModel.RootBookmarkTree.ExecuteAddBookmark(control.Parent, true);
                }
                else
                {
                    _viewModel.RootBookmarkTree.ExecuteAddBookmark(null, true);
                }
            }
            else
            {
                _viewModel.RootBookmarkTree.ExecuteAddBookmark(null, true);
            }
        }

        public bool CanExecuteRemoveBookmarkCommand()
        {
            return _viewModel.RootBookmarkTree.SelectedList.Count != 0;
        }

        public void ExecuteRemoveBookmarkCommand()
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            _viewModel.RootBookmarkTree.ExecuteDelBookmark();
        }

        public void ExecuteAddCommentCommand()
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None 
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyPagesPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            if (_viewModel.IsCommentOptionEnable)
            {
                _viewModel.IsCommentPaneShow = true;
                _viewModel.ExcutePrepareAddComment();
            }
        }

        public void ExecuteDeleteCommentCommand(bool isInRectangle)
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyPagesPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            if (_viewModel.IsCommentOptionEnable)
            {
                _viewModel.ExcuteDeleteComment(isInRectangle);
            }
        }

        public void ExecuteAddChangeHighlightCommand()
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyPagesPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            var previewPage = GetPreviewPageFromThumbnail();
            if (previewPage != null)
            {
                _viewModel.PdfUtilView.HighlightColorPopupWindow.IsOpen = true;
            }
        }

        public void ChangeHighlight(System.Drawing.Color color, bool isInRectangle, bool isChangeLastHighlight = false)
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyPagesPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            var previewPage = GetPreviewPageFromThumbnail();
            if (previewPage != null)
            {
                if (isInRectangle)
                {
                    var leftTopPointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.SelectRectLeftTopPoint, previewPage);
                    var rightBottomPointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.SelectRectRightBottomPoint, previewPage);

                    if (PdfHelper.AddHighlight(previewPage, color, leftTopPointOnPage, rightBottomPointOnPage))
                    {
                        _viewModel.CurPreviewPageEdited();
                    }
                }
                else
                {
                    var pointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.MouseButtonChoosedPoint, previewPage);

                    if (PdfHelper.ChangeHighlight(previewPage, color, pointOnPage, isChangeLastHighlight))
                    {
                        _viewModel.CurPreviewPageEdited();
                    }
                }
                _viewModel.ClearAllSelectionOnPage();
            }
        }

        public void ExecuteDeleteHighlightsCommand()
        {
            if (FileOperation.FileIsReadOnly(_viewModel.PdfUtilView.WindowHandle, _viewModel.CurrentPdfFileName))
            {
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyPagesPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            var previewPage = GetPreviewPageFromThumbnail();
            if (previewPage != null)
            {
                var leftTopPointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.SelectRectLeftTopPoint, previewPage);
                var rightBottomPointOnPage = _viewModel.CalculateSelectedAreaOnPage(_viewModel.SelectRectRightBottomPoint, previewPage);

                if (PdfHelper.RemoveHighlight(previewPage, leftTopPointOnPage, rightBottomPointOnPage))
                {
                    _viewModel.CurPreviewPageEdited();
                }
                _viewModel.ClearAllSelectionOnPage();
            }
        }

        public Aspose.Pdf.Page GetPreviewPageFromThumbnail()
        {
            if (_viewModel.CurPreviewIconItem != null)
            {
                return _viewModel.CurPreviewIconItem.GetPage();
            }

            var selectedThumbnailItems = _viewModel.ThumbnailListView.SelectedItems;
            if (selectedThumbnailItems.Count != 0)
            {
                var lastSelectedThumbnail = (IconItem)selectedThumbnailItems[selectedThumbnailItems.Count - 1];
                return lastSelectedThumbnail.GetPage();
            }
            return null;
        }

        private IconItem GetPreviewPageThumbnail()
        {
            if (_viewModel.CurPreviewIconItem != null)
            {
                return _viewModel.CurPreviewIconItem;
            }

            var selectedThumbnailItems = _viewModel.ThumbnailListView.SelectedItems;
            if (selectedThumbnailItems.Count != 0)
            {
                return (IconItem)selectedThumbnailItems[selectedThumbnailItems.Count - 1];
            }
            return null;
        }

        public void ExecuteExtractImagesCommand()
        {
            _viewModel.ExecuteExtractImagesCommand(true);
        }

        public void ExecuteExtractPagesCommand()
        {
            _viewModel.ExecuteExtractPagesCommand(true);
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

        public void ExecuteDeleteSignatureCommand(bool isInRectangle)
        {
            _viewModel.ExecuteDeleteSignatureFromPage(isInRectangle);
        }
    }
}
