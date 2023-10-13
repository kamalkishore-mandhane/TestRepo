using ImgUtil.WPFUI.Controls;
using ImgUtil.WPFUI.ViewModel;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace ImgUtil.WPFUI.View
{
    /// <summary>
    /// Interaction logic for IntegrationView.xaml
    /// </summary>
    public partial class IntegrationView : BaseWindow
    {
        private bool _canAddDesktopIcon;
        private bool _canAddStartMenu;
        private int _addFileAssociation;
        private IntPtr _windowHandle;

        public IntegrationView(bool canAddDesktopIcon, bool canAddStartMenu, int addFileAssociation)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _canAddDesktopIcon = canAddDesktopIcon;
            _canAddStartMenu = canAddStartMenu;
            _addFileAssociation = addFileAssociation;
        }

        public IntPtr WindowHandle
        {
            get
            {
                return _windowHandle;
            }
        }

        public void InitDataContext()
        {
            DataContext = new IntegrationViewModel(this);
        }

        private void IntegrationView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            (DataContext as IntegrationViewModel)?.InitIntegration(_canAddDesktopIcon, _canAddStartMenu, _addFileAssociation);
        }

        private void IntegrationView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void IntegrationView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    DialogResult = true;
                    Close();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    DialogResult = false;
                    Close();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        private void IntegrationView_SourceInitialized(object sender, EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public bool HasChanges(ref bool needAdmin)
        {
            return (DataContext as IntegrationViewModel)?.HasChanges(ref needAdmin) ?? false;
        }

        public new bool ShowDialog()
        {
            return BaseShowWindow();
        }
    }
}
