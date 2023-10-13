using SafeShare.WPFUI.Commands;
using SafeShare.WPFUI.Utils;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace SafeShare.WPFUI.Controls
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
            FlatScrollViewer scrollViewer = VisualTreeHelperUtils.FindAncestor<FlatScrollViewer>(this as DependencyObject);
            if (scrollViewer != null)
            {
                scrollViewer._notifier.SetMode(true);
            }
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
            FlatScrollViewer scrollViewer = VisualTreeHelperUtils.FindAncestor<FlatScrollViewer>(this as DependencyObject);
            if (scrollViewer != null)
            {
                scrollViewer._notifier.SetMode(false);
            }
        }
    }
}