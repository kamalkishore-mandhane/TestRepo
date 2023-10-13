using ImgUtil.WPFUI.Commands;
using System.Reflection;
using System.Windows.Controls.Primitives;

namespace ImgUtil.WPFUI.Controls
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

        [Obfuscation(Exclude = true)]
        public bool IsShow
        {
            get { return true; }
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand ShowCommand => _showCommand;

        private bool CanExecuteShowCommand(object parameter)
        {
            return true;
        }

        private void ExecuteShowCommand(object parameter)
        {

        }

        [Obfuscation(Exclude = true)]
        public ModelCommand HideCommand => _hideCommand;

        public object VisualTreeHelperUtils { get; private set; }

        private bool CanExecuteHideCommand(object parameter)
        {
            return true;
        }

        private void ExecuteHideCommand(object parameter)
        {

        }
    }
}
