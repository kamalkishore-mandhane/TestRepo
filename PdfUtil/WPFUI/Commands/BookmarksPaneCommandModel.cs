using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PdfUtil.WPFUI.ViewModel;

namespace PdfUtil.WPFUI.Commands
{
    class BookmarksPaneCommandModel
    {
        private BookmarksPaneContextMenuViewModel _viewModel;

        private ModelCommand _goToBookmarkCommand;
        private ModelCommand _addBookMarkCommand;
        private ModelCommand _addSubBookMarkCommand;
        private ModelCommand _removeBookmarkCommand;
        private ModelCommand _renameBookmarkCommand;

        public BookmarksPaneCommandModel(BookmarksPaneContextMenuViewModel viewModel)
        {
            _viewModel = viewModel;

            _goToBookmarkCommand = new ModelCommand(ExecuteGoToBookmarkCommand, CanExecuteGoToBookmarkCommand);
            _addBookMarkCommand = new ModelCommand(ExecuteAddBookMarkCommand, CanExecuteAddBookMarkCommand);
            _addSubBookMarkCommand = new ModelCommand(ExecuteAddSubBookMarkCommand, CanExecuteAddSubBookMarkCommand);
            _removeBookmarkCommand = new ModelCommand(ExecuteRemoveBookmarkCommand, CanExecuteRemoveBookmarkCommand);
            _renameBookmarkCommand = new ModelCommand(ExecuteRenameBookmarkCommand, CanExecuteRenameBookmarkCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand GoToBookmarkCommand
        {
            get { return _goToBookmarkCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand AddBookMarkCommand
        {
            get { return _addBookMarkCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand AddSubBookMarkCommand
        {
            get { return _addSubBookMarkCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand RemoveBookmarkCommand
        {
            get { return _removeBookmarkCommand; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand RenameBookmarkCommand
        {
            get { return _renameBookmarkCommand; }
        }

        private bool CanExecuteGoToBookmarkCommand(object parameter)
        {
            return true;
        }

        private void ExecuteGoToBookmarkCommand(object parameter)
        {
            _viewModel.ExecuteGoToBookmarkCommand();
        }


        private bool CanExecuteAddBookMarkCommand(object parameter)
        {
            return true;
        }

        private void ExecuteAddBookMarkCommand(object parameter)
        {
            _viewModel.ExecuteAddBookmarkCommand();

        }
        private bool CanExecuteAddSubBookMarkCommand(object parameter)
        {
            return true;
        }

        private void ExecuteAddSubBookMarkCommand(object parameter)
        {
            _viewModel.ExecuteAddSubBookmarkCommand();
        }

        private bool CanExecuteRemoveBookmarkCommand(object parameter)
        {
            return true;
        }

        private void ExecuteRemoveBookmarkCommand(object parameter)
        {
            _viewModel.ExecuteRemoveBookmarkCommand();
        }

        private bool CanExecuteRenameBookmarkCommand(object parameter)
        {
            return true;
        }

        private void ExecuteRenameBookmarkCommand(object parameter)
        {
            _viewModel.ExecuteRenameBookmarkCommand();
        }
    }
}
