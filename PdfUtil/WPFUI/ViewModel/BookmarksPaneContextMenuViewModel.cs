using PdfUtil.WPFUI.Commands;
using PdfUtil.WPFUI.Utils;
using System.Reflection;

namespace PdfUtil.WPFUI.ViewModel
{
    class BookmarksPaneContextMenuViewModel
    {
        private PdfUtilViewModel _viewModel;
        private BookmarksPaneCommandModel _viewModelCommands;
        private bool _isContextMenuOpen;

        public BookmarksPaneContextMenuViewModel(PdfUtilViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModelCommands = null;
        }

        [Obfuscation(Exclude = true)]
        public BookmarksPaneCommandModel ViewModelCommands
        {
            get
            {
                if (_viewModelCommands == null)
                {
                    _viewModelCommands = new BookmarksPaneCommandModel(this);
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

        public bool CanExecuteGoToBookmarkCommand()
        {
            return true;
        }

        public void ExecuteGoToBookmarkCommand()
        {
            _viewModel.RootBookmarkTree.ExecuteGoToBookmark();
        }

        public bool CanExecuteAddBookmarkCommand()
        {
            return true;
        }

        public void ExecuteAddBookmarkCommand()
        {
            if (_viewModel.RootBookmarkTree.SelectedList.Count == 1)
            {
                var control = _viewModel.RootBookmarkTree.SelectedList[0];
                if (control.Level != 1)
                {
                    _viewModel.RootBookmarkTree.ExecuteAddBookmark(control.Parent, false);
                }
                else
                {
                    _viewModel.RootBookmarkTree.ExecuteAddBookmark(null, false);
                }
            }
            else
            {
                _viewModel.RootBookmarkTree.ExecuteAddBookmark(null, false);
            }
        }

        public bool CanExecuteAddSubBookmarkCommand()
        {
            return true;
        }

        public void ExecuteAddSubBookmarkCommand(bool isPreviewCommand = false)
        {
            if (_viewModel.RootBookmarkTree.SelectedList.Count == 0)
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.EXECUTE_ADD_SUB_BOOKMARK_TIPS);
                return;
            }

            var control = _viewModel.RootBookmarkTree.SelectedList[0];
            if (control != null)
            {
                _viewModel.RootBookmarkTree.ExecuteAddSubBookmark(control, isPreviewCommand);
            }
        }

        public bool CanExecuteRemoveBookmarkCommand()
        {
            return true;
        }

        public void ExecuteRemoveBookmarkCommand()
        {
            _viewModel.RootBookmarkTree.ExecuteDelBookmark();
        }

        public bool CanExecuteRenameBookmarkCommand()
        {
            return true;
        }

        public void ExecuteRenameBookmarkCommand()
        {
            if (_viewModel.RootBookmarkTree.SelectedList.Count == 0)
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.EXECUTE_RENAME_BOOKMARK_TIPS);
                return;
            }

            var control = _viewModel.RootBookmarkTree.SelectedList[0];
            if (control != null)
            {
                _viewModel.RootBookmarkTree.ExecuteRenameBookmark(control);
            }
        }
    }
}
