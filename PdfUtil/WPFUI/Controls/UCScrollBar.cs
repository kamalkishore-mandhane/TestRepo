using System.Windows;
using System.Reflection;
using System.Windows.Controls.Primitives;
using PdfUtil.WPFUI.Commands;
using PdfUtil.WPFUI.Utils;

namespace PdfUtil.WPFUI.Controls
{
    public class UCScrollBar : ScrollBar
    {
        private ModelCommand _showCommand;
        private ModelCommand _hideCommand;

        public UCScrollBar()
        {
            _showCommand = new ModelCommand(ExecuteShowCommand, CanExecuteShowCommand);
            _hideCommand = new ModelCommand(ExecuteHideCommand, CanExecuteHideCommand);
        }

        [ObfuscationAttribute(Exclude = true)]
        public bool IsShow
        {
            get { return true; }
        }

        [ObfuscationAttribute(Exclude = true)]
        public ModelCommand ShowCommand
        {
            get { return _showCommand; }
        }

        private bool CanExecuteShowCommand(object parameter)
        {
            return true;
        }

        private void ExecuteShowCommand(object parameter)
        {

        }

        [ObfuscationAttribute(Exclude = true)]
        public ModelCommand HideCommand
        {
            get { return _hideCommand; }
        }

        private bool CanExecuteHideCommand(object parameter)
        {
            return true;
        }

        private void ExecuteHideCommand(object parameter)
        {

        }
    }
}
