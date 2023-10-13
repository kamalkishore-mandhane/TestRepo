using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StartupPaneLib
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class StartupPane : UserControl, INotifyPropertyChanged
    {
        public StartupPane()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region High Contrast Theme

        protected void UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Accessibility)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        protected void SetHighContrastTheme(bool highContrast)
        {
            this.Resources.MergedDictionaries[0].MergedDictionaries.Clear();
            if (highContrast)
            {
                this.Resources.MergedDictionaries[0].MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/StartupPaneLib;component/Themes/HighContrastTheme.xaml", UriKind.RelativeOrAbsolute) });
            }
            else
            {
                this.Resources.MergedDictionaries[0].MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/StartupPaneLib;component/Themes/WinZipColorTheme.xaml", UriKind.RelativeOrAbsolute) });
            }
        }

        #endregion

        #region Dependency Property and Routed Event

        public static DependencyProperty AppletIconProperty = DependencyProperty.Register("AppletIcon", typeof(ImageSource), typeof(StartupPane));

        public static DependencyProperty AppletNameProperty = DependencyProperty.Register("AppletName", typeof(string), typeof(StartupPane));

        public static DependencyProperty DragInstructionTextProperty = DependencyProperty.Register("DragInstructionText", typeof(string), typeof(StartupPane));

        public static DependencyProperty DefaultAppFileTypeProperty = DependencyProperty.Register("DefaultAppFileType", typeof(string), typeof(StartupPane));

        public static DependencyProperty DefaultAlreadyProperty = DependencyProperty.Register("DefaultAlready", typeof(bool), typeof(StartupPane), new FrameworkPropertyMetadata(OnDefaultAlreadyChanged)); //change

        public static DependencyProperty RecentFilesListProperty = DependencyProperty.Register("RecentFilesList", typeof(object), typeof(StartupPane), new FrameworkPropertyMetadata(OnRecentFilesListChanged));

        public static DependencyProperty SelectRecentFileProperty = DependencyProperty.Register("SelectRecentFile", typeof(object), typeof(StartupPane));

        public static RoutedEvent RecentFileOpenEvent = EventManager.RegisterRoutedEvent("RecentFileOpen", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StartupPane));

        public static RoutedEvent ChooseFileEvent = EventManager.RegisterRoutedEvent("ChooseFile", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StartupPane));

        public static RoutedEvent SetAsDefaultOpenEvent = EventManager.RegisterRoutedEvent("SetAsDefault", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StartupPane));

        public ImageSource AppletIcon
        {
            get { return GetValue(AppletIconProperty) as ImageSource; }
            set { SetValue(AppletIconProperty, value); }
        }

        public string AppletName
        {
            get { return (string)GetValue(AppletNameProperty); }
            set { SetValue(AppletNameProperty, value);}
        }

        public string DragInstructionText
        {
            get { return (string)GetValue(DragInstructionTextProperty); }
            set { SetValue(DragInstructionTextProperty, value); }
        }

        public string DefaultAppFileType
        {
            get { return (string)GetValue(DefaultAppFileTypeProperty); }
            set { SetValue(DefaultAppFileTypeProperty, value); }
        }

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
                    return string.IsNullOrEmpty(AppletName) ? string.Empty : string.Format(Properties.Resources.ALREADY_DEFAULT_APP_TEXT, AppletName, DefaultAppFileType);
                }
                else
                {
                    return string.IsNullOrEmpty(AppletName) ? string.Empty : string.Format(Properties.Resources.MAKE_DEFAULT_APP_TEXT, AppletName);
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

        public bool IsDraging
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

        private void AppletStartupPane_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            IsDraging = false;
            Notify(nameof(IsDraging));
            Notify(nameof(SetDefaultText));
            Notify(nameof(DefaultAlready));
        }

        private void AppletStartupPane_Unloaded(object sender, RoutedEventArgs e)
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
                IsDraging = true;
            }
            else if (e.RoutedEvent == DragDrop.DropEvent || e.RoutedEvent == DragDrop.DragLeaveEvent)
            {
                IsDraging = false;
            }

            Notify(nameof(IsDraging));
        }
    }
}
