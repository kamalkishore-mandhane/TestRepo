using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImgUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for StartupPane.xaml
    /// </summary>
    public partial class StartupPane : BaseControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public StartupPane()
        {
            InitializeComponent();

#if WZ_APPX
            FileAssocGrid.Visibility = Visibility.Collapsed;
            DradDropGrid.SetValue(Grid.ColumnSpanProperty, 2);
#endif
        }

        #region Dependency Property and Routed Event

        public static DependencyProperty DefaultAlreadyProperty = DependencyProperty.Register("DefaultAlready", typeof(bool), typeof(StartupPane), new FrameworkPropertyMetadata(OnDefaultAlreadyChanged)); //change

        public static DependencyProperty RecentFilesListProperty = DependencyProperty.Register("RecentFilesList", typeof(object), typeof(StartupPane), new FrameworkPropertyMetadata(OnRecentFilesListChanged));

        public static DependencyProperty SelectRecentFileProperty = DependencyProperty.Register("SelectRecentFile", typeof(object), typeof(StartupPane));

        public static RoutedEvent RecentFileOpenEvent = EventManager.RegisterRoutedEvent("RecentFileOpen", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StartupPane));

        public static RoutedEvent ChooseFileEvent = EventManager.RegisterRoutedEvent("ChooseFile", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StartupPane));

        public static RoutedEvent SetAsDefaultOpenEvent = EventManager.RegisterRoutedEvent("SetAsDefault", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StartupPane));

        public bool DefaultAlready
        {
            get { return (bool)GetValue(DefaultAlreadyProperty); }
            set { SetValue(DefaultAlreadyProperty, value); }
        }

        public object RecentFilesList
        {
            get { return GetValue(RecentFilesListProperty); }
            set { SetValue(RecentFilesListProperty, value); }
        }

        public object SelectRecentFile
        {
            get { return GetValue(SelectRecentFileProperty); }
            set { SetValue(SelectRecentFileProperty, value); }
        }

        public event RoutedEventHandler RecentFileOpen
        {
            add { AddHandler(RecentFileOpenEvent, value); }
            remove { RemoveHandler(RecentFileOpenEvent, value); }
        }

        public event RoutedEventHandler ChooseFile
        {
            add { AddHandler(ChooseFileEvent, value); }
            remove { RemoveHandler(ChooseFileEvent, value); }
        }

        public event RoutedEventHandler SetAsDefault
        {
            add { AddHandler(SetAsDefaultOpenEvent, value); }
            remove { RemoveHandler(SetAsDefaultOpenEvent, value); }
        }

        private static void OnDefaultAlreadyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var panel = sender as StartupPane;
            if (panel != null)
            {
                panel.Notify(nameof(SetDefaultText));
            }
        }

        private static void OnRecentFilesListChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var panel = sender as StartupPane;
            if (panel != null)
            {
                panel.Notify(nameof(HasRecentFiles));
            }
        }

        #endregion


        private double _fileNameRealWidth;
        private double _fileDateRealWidth;
        private double _fileSizeRealWidth;

        public bool HasRecentFiles
        {
            get { return RecentFileListView.Items.Count != 0; }
        }

        public string SetDefaultText
        {
            get
            {
                if (DefaultAlready)
                {
                    return Properties.Resources.SUP_ALREADY_DEFAULT_APP_TEXT;
                }
                else
                {
                    return Properties.Resources.SUP_MAKE_DEFAULT_APP_TEXT;
                }
            }
        }

        public double FileNameRealWidth
        {
            get
            {
                return _fileNameRealWidth;
            }
            set
            {
                _fileNameRealWidth = value;
                Notify(nameof(FileNameRealWidth));
            }
        }

        public double FileDateRealWidth
        {
            get
            {
                return _fileDateRealWidth;
            }
            set
            {
                _fileDateRealWidth = value;
                Notify(nameof(FileDateRealWidth));
            }
        }

        public double FileSizeRealWidth
        {
            get
            {
                return _fileSizeRealWidth;
            }
            set
            {
                _fileSizeRealWidth = value;
                Notify(nameof(FileSizeRealWidth));
            }
        }

        public bool IsDragging
        {
            get;
            set;
        }

        private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
        {
            FileNameRealWidth = FileNameColumn.ActualWidth;
            FileDateRealWidth = FileDateColumn.ActualWidth;
            FileSizeRealWidth = FileSizeColumn.ActualWidth;
        }

        private void ImageStartupPane_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            IsDragging = false;
            Notify(nameof(IsDragging));
            Notify(nameof(SetDefaultText));
            Notify(nameof(DefaultAlready));
        }

        private void ImageStartupPane_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void RecentFileListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var list = sender as ListView;
                if (list != null)
                {
                    var item = list.SelectedItem;
                    if (item != null)
                    {
                        RaiseEvent(new RoutedEventArgs(RecentFileOpenEvent, item));
                    }
                }
            }
        }

        private void RecentFileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = sender as ListView;
            if (list != null)
            {
                var item = list.SelectedItem;
                if (item != null)
                {
                    RaiseEvent(new RoutedEventArgs(RecentFileOpenEvent, item));
                }
            }
        }

        private void RecentFileListView_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list != null)
            {
                var item = e.NewFocus as ListViewItem;
                if (item != null)
                {
                    list.SelectedItem = item.Content;
                }
            }
        }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ChooseFileEvent));
        }

        private void SetDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SetAsDefaultOpenEvent));
        }

        private void DragFileBorder_DragAndDrop(object sender, DragEventArgs e)
        {
            if (e.RoutedEvent == DragDrop.DragEnterEvent)
            {
                IsDragging = true;
            }
            else if (e.RoutedEvent == DragDrop.DropEvent || e.RoutedEvent == DragDrop.DragLeaveEvent)
            {
                IsDragging = false;
            }

            Notify(nameof(IsDragging));
        }
    }
}
