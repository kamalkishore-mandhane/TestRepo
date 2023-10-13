using StartupPaneLib.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace StartupPaneLib
{
    public enum Applet
    {
        None,
        ImageManager,
        PdfExpress,
        SecureBackup
    }


    /// <summary>
    /// Interaction logic for FileAssociationWindow.xaml
    /// </summary>
    public partial class FileAssociationWindow : Window
    {
        private string _tempFilePath;
        private string _appletFullName;
        private string _appletName;
        private IntPtr _windowHandle;
        private IntPtr _propDialogHwnd;
        private bool _windowClosed = false;
        private bool _isListening = false;

        public FileAssociationWindow(Window onwer, Applet appletType)
        {
            InitializeComponent();
            Owner = onwer;
            InitProperties(appletType);
            
        }

        public bool Associated { get; private set; }

        private void InitProperties(Applet appletType)
        {
            switch (appletType)
            {
                case Applet.PdfExpress:
                    {
                        _appletFullName = "WinZip PDF Express";
                        _appletName = "pdfutil";
                        AppletName.Text = _appletFullName;
                        MainTitleTextBlock.Text = string.Format(Properties.Resources.FILE_ASSOC_MAIN_TITLE, _appletFullName);
                        ClickOKTextBlock.Text = string.Format(Properties.Resources.FILE_ASSOC_CLICK_OK_TEXT, _appletFullName);
                        FileExtTextBlock.Text = "pdf";
                        AppletImage.Source = new BitmapImage(new Uri("pack://application:,,,/StartupPaneLib;component/Resources/PdfExpress.png", UriKind.RelativeOrAbsolute));
                        _tempFilePath = Path.Combine(Path.GetTempPath(), "WzPdfExpressFileAssoc.pdf");
                    }
                    break;

                case Applet.ImageManager:
                    {
                        // for later use
                        _appletFullName = "WinZip Image Manager";
                        _appletName = "imgutil";
                    }
                    break;

                case Applet.SecureBackup:
                    {
                        // for later use
                        _appletFullName = "WinZip Secure Backup";
                        _appletName = "sbkup";
                    }
                    break;

                case Applet.None:
                default:
                    return;
            }

            if (!string.IsNullOrEmpty(_appletFullName))
            {
                FileAssocGuideWindow.Title = _appletFullName;
            }

            try
            {
                if (!File.Exists(_tempFilePath))
                {
                    var fileStream = File.Create(_tempFilePath);
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }
            catch
            {
                // do nothing
            }
        }

        private void ShowPropertiesDialog()
        {
            if (File.Exists(_tempFilePath))
            {
                ShowPropertiesButton.Visibility = Visibility.Collapsed;

                // Open properties dialog
                var info = new NativeMethods.SHELLEXECUTEINFO();
                info.cbSize = Marshal.SizeOf(info);
                info.lpVerb = "properties";
                info.lpFile = _tempFilePath;
                info.nShow = NativeMethods.SW_SHOW;
                info.fMask = NativeMethods.SEE_MASK_INVOKEIDLIST;
                info.hwnd = _windowHandle;
                NativeMethods.ShellExecuteEx(ref info);

                _propDialogHwnd = IntPtr.Zero;
                int currentProcessId = Process.GetCurrentProcess().Id;

                // This while() and Thread.Sleep() are used to wait the properties dialog open
                while (_propDialogHwnd == IntPtr.Zero)
                {
                    NativeMethods.EnumWindows((IntPtr handle, IntPtr lParam) =>
                    {
                        int processId;
                        NativeMethods.GetWindowThreadProcessId(handle, out processId);
                        if (processId == currentProcessId && NativeMethods.IsWindowVisible(handle) && handle != _windowHandle && handle != new WindowInteropHelper(Owner).Handle)
                        {
                            var className = new StringBuilder(256);
                            NativeMethods.GetClassName(handle, className, 256);
                            if (className.ToString() != "#32770")
                            {
                                return true;
                            }

                            // Here we actually find the properties dialog's window handle
                            _propDialogHwnd = handle;
                            var rect = new NativeMethods.RECT();
                            NativeMethods.GetWindowRect(handle, out rect);
                            int propDlgW = rect.right - rect.left;
                            int propDlgH = rect.bottom - rect.top;
                            const int gap = 3;
                            int destX = 0, destY = 0;
                            var screen = System.Windows.Forms.Screen.FromHandle(_windowHandle);

                            // Set Properties dialog's position to the left of this window
                            destX = (Left - propDlgW >= 0) ? (int)(Left - gap - propDlgW) : 0;
                            destY = (int)(Top + (ActualHeight - propDlgH) / 2);

                            NativeMethods.SetWindowPos(handle, NativeMethods.HWND_TOPMOST, destX, destY, 0, 0, NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW);
                        }
                        return true;
                    }, IntPtr.Zero);

                    Thread.Sleep(500);
                }

                // Run a task to monitor if properties is close
                Task.Run(() =>
                {
                    while (NativeMethods.IsWindowVisible(_propDialogHwnd))
                    {
                        Thread.Sleep(10);
                    }

                    Dispatcher.Invoke(() => ShowPropertiesButton.Visibility = Visibility.Visible);
                });

                // Listen if file association has been changed
                // Make sure only one listener at a time
                lock (this)
                {
                    if (!_isListening)
                    {
                        _isListening = true;
                        Task.Run(() =>
                        {
                            bool associated = false;
                            try
                            {
                                while (!associated && !_windowClosed)
                                {
                                    var associateName = NativeMethods.AssocQueryString(NativeMethods.AssocStr.FriendlyAppName, ".pdf");
                                    associated = !string.IsNullOrEmpty(associateName) && (associateName.ToLower() == _appletName.ToLower() || associateName.ToLower() == _appletFullName.ToLower());
                                    Thread.Sleep(500);
                                }

                                Associated = associated;

                                // Close the properties dialog if already associated or window is closed
                                if (NativeMethods.IsWindowVisible(_propDialogHwnd))
                                {
                                    NativeMethods.SendMessage(_propDialogHwnd, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                                }

                                // Close this window if it is not closed
                                if (!_windowClosed)
                                {
                                    Dispatcher.Invoke(() => Close());
                                }
                            }
                            catch
                            {
                                _isListening = false;
                            }
                        });
                    }
                }
            }
        }

        private void ShowPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPropertiesDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;
            ShowPropertiesDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _windowClosed = true;
            if (File.Exists(_tempFilePath))
            {
                try
                {
                    File.Delete(_tempFilePath);
                }
                catch
                {
                    // do nothing
                }
            }
        }
    }
}
