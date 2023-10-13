using Microsoft.Win32;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SafeShare.WPFUI.View
{
    /// <summary>
    /// Interaction logic for FileListPage.xaml
    /// </summary>
    public partial class FileListPage : BasePage
    {
        private DispatcherTimer _dispatcherTimer;
        private const int _tipDuration = 5;
        private bool _isDragging;

        public FileListPage()
        {
            InitializeComponent();
        }

        public void InitDataContext()
        {
            var viewModel = new FileListPageViewModel(this, AdjustPaneCursor);
            DataContext = viewModel;
        }

        public bool IsDragging
        {
            get => _isDragging;
            set
            {
                if (_isDragging != value)
                {
                    _isDragging = value;
                    Notify(nameof(IsDragging));
                }
            }
        }

        private void RemoveListItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = VisualTreeHelperUtils.FindAncestor<ListViewItem>(button);
            var viewModel = DataContext as FileListPageViewModel;
            var itemEntry = item.DataContext as ListViewItemEntry;

            viewModel.ListViewItems.ItemSources.Remove(itemEntry);
            viewModel.CalcutTotalItems();
            viewModel.FileTotalSize -= itemEntry.Size;
            viewModel.ItemsFullPathList.Remove(itemEntry.FullPath);
            TrackHelper.TrackHelperInstance.PathToTrackDic.Remove(itemEntry.FullPath);
            viewModel.CalcutListViewHeight();
        }

        private void SetWpfDragDropEffect(DragEventArgs e)
        {
            if (e.RoutedEvent == DragDrop.DragEnterEvent)
            {
                WpfDragDropHelper.DragEnter(e, this, this);
            }
            else if (e.RoutedEvent == DragDrop.DragOverEvent)
            {
                WpfDragDropHelper.DragOver(e, this);
            }
            else if (e.RoutedEvent == DragDrop.DragLeaveEvent)
            {
                WpfDragDropHelper.DragLeave();
            }
        }

        private void DragFileBorder_Drag(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (e.RoutedEvent == DragDrop.DragEnterEvent)
            {
                IsDragging = true;
            }
            else if (e.RoutedEvent == DragDrop.DragLeaveEvent)
            {
                IsDragging = false;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                WpfDragDropHelper.SetDropDescription(e.Data, WpfDragDropHelper.DropImageType.Copy, Properties.Resources.ADD, string.Empty);
                if (files != null && files.Length > 0)
                {
                    e.Effects = DragDropEffects.Copy;
                    SetWpfDragDropEffect(e);
                }
            }

            e.Handled = true;
        }

        private void DragFileBorder_Drop(object sender, DragEventArgs e)
        {
            IsDragging = false;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    WpfDragDropHelper.Drop(e, this);
                    var viewModel = DataContext as FileListPageViewModel;
                    viewModel.ExecuteDragFromExploreTaskCommand(files);
                    e.Handled = true;
                }
            }
        }

        private void fileListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(fileListView);
            var element = fileListView.InputHitTest(pos) as UIElement;
            var item = VisualTreeHelperUtils.FindAncestor<ListViewItem>(element);
            if (item == null)
            {
                fileListView.SelectedItems.Clear();
            }
        }

        private void FileListPageView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(DispatcherTimerTick);
            _dispatcherTimer.Interval = new TimeSpan(0, 0, _tipDuration);
            _dispatcherTimer.Start();
            NavigationCommandsManager.RemoveBrowseBackKeyGestures();
            CoutinueButton.Focus();
            MainWindow.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void DispatcherTimerTick(object sender, System.EventArgs e)
        {
            var viewModel = DataContext as FileListPageViewModel;
            if (MainWindow.IsActive)
            {
                viewModel.ShowSelectFileTips = true;
            }
        }

        private void FileListPageView_MouseMove(object sender, MouseEventArgs e)
        {
            _dispatcherTimer.Stop();
            _dispatcherTimer.Start();
        }

        private void FileListPageView_KeyDown(object sender, KeyEventArgs e)
        {
            _dispatcherTimer.Stop();
            _dispatcherTimer.Start();
        }

        private void FileListPageView_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
            _dispatcherTimer.Stop();
            _dispatcherTimer = null;
            NavigationCommandsManager.ResetBrowseBackKeyGestures();
        }

        public void AdjustPaneCursor(bool enable)
        {
            this.Cursor = enable ? null : System.Windows.Input.Cursors.Wait;

            CoutinueButton.IsEnabled = enable;
            DragFileBorder.IsEnabled = enable;
            AddFileButton.IsEnabled = enable;
            CloseButton.IsEnabled = enable;

            var frontPageViewModel = DataContext as FrontPageViewModel;
            if (enable && frontPageViewModel != null)
            {
                CoutinueButton.Focus();
            }

            Mouse.UpdateCursor();
        }
    }
}