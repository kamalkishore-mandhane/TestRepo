using ImgUtil.WPFUI.Utils;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace ImgUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for SimpleProgressWindow.xaml
    /// </summary>
    public partial class SimpleProgressWindow : BaseWindow
    {
        private Action _action;

        public SimpleProgressWindow(string title, int total)
        {
            InitializeComponent();
            TitleTextBlock.Text = title;
            MainProgressBar.Maximum = total;
            MainProgressBar.Value = 0;
            IsCanceled = false;
            IsProgressFinished = false;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        public bool IsCanceled { get; private set; }

        public bool IsWindowClosed { get; private set; }

        public bool IsProgressFinished { get; private set; }

        public void Run(Action action)
        {
            _action = action;
            BaseShowWindow();
        }

        public void UpdateProgress(int value)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                MainProgressBar.Value = value;

                if (value == MainProgressBar.Maximum)
                {
                    IsWindowClosed = true;
                    IsProgressFinished = true;
                    Close();
                }
            }));
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            IsWindowClosed = false;
            Task.Factory.StartNew(() => _action?.Invoke());
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsWindowClosed = true;
        }

        private void CancelButton_Clicked(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
            IsWindowClosed = true;
            Close();
        }

        private void SimpleProgressWindow_SourceInitialized(object sender, EventArgs e)
        {
            var windowHandle = new WindowInteropHelper(this).Handle;

            if (windowHandle != IntPtr.Zero)
            {
                NativeMethods.SetWindowLong(windowHandle, NativeMethods.GWL_STYLE, NativeMethods.GetWindowLong(windowHandle, NativeMethods.GWL_STYLE) & ~NativeMethods.WS_MINIMIZEBOX & ~NativeMethods.WS_MAXIMIZEBOX);
            }
        }

        private void SimpleProgressWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!IsProgressFinished) 
            {
                IsCanceled = true;
            }
        }

        private void ProgressWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void ProgressWindow_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
