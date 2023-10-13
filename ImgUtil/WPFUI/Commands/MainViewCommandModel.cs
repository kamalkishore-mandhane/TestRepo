using ImgUtil.WPFUI.ViewModel;

namespace ImgUtil.WPFUI.Commands
{
    class MainViewCommandModel
    {
        #region Fields

        private ImgUtilViewModel _viewModel;

        private ModelCommand _dragFromExplorerCommand;

        #endregion

        #region Constructors

        public MainViewCommandModel(ImgUtilViewModel viewModel) => _viewModel = viewModel;

        #endregion

        #region Commands

        public ModelCommand DragFromExplorerCommand => _dragFromExplorerCommand ?? (_dragFromExplorerCommand = new ModelCommand(ExecuteDragFromExplorerCommand, p => true));

        private async void ExecuteDragFromExplorerCommand(object parameter)
        {
            await _viewModel.ExecuteAsync(() => _viewModel.ExecuteDragFromExplorerAsync(parameter));
        }

        #endregion
    }
}
