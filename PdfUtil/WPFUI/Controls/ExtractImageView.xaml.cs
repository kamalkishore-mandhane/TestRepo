using Microsoft.Win32;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.View;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for ExtractImageView.xaml
    /// </summary>

    public enum ImageFormatEnum
    {
        BMP,
        GIF,
        JPG,
        JP2,
        PNG,
        PSD,
        TIF,
        WEBP,
        SVG
    }

    public enum ImageDestinationOptions
    {
        IndividualImageFiles,
        AddToCurrentZipFile
    }

    public partial class ExtractImageView : BaseWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ImageFormatEnum _curImageFormat = ImageFormatEnum.BMP;
        private ImageDestinationOptions _curDestOptions = ImageDestinationOptions.IndividualImageFiles;
        private string _initialFolderPath = string.Empty;
        private string _selectedFolderPath = string.Empty;

        private PdfUtilView _pdfUtilView;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ExtractImageView(PdfUtilView view)
        {
            _pdfUtilView = view;
            InitializeComponent();
            if (_pdfUtilView.IsLoaded)
            {
                Owner = _pdfUtilView;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        public ImageFormatEnum CurImageFormat
        {
            get { return _curImageFormat; }
            set
            {
                if (_curImageFormat != value)
                {
                    _curImageFormat = value;
                    OnPropertyChanged(nameof(CurImageFormat));
                }
            }
        }

        public ImageDestinationOptions CurDestOptions
        {
            get { return _curDestOptions; }
            set
            {
                if (_curDestOptions != value)
                {
                    _curDestOptions = value;
                    OnPropertyChanged(nameof(CurDestOptions));
                }
            }
        }

        public string SelectedFolderPath
        {
            get { return _selectedFolderPath; }
            set
            {
                if (_selectedFolderPath != value)
                {
                    _selectedFolderPath = value;
                }
            }
        }

        public string InitialFolderPath
        {
            get { return _initialFolderPath; }
            set
            {
                if (_initialFolderPath != value)
                {
                    _initialFolderPath = value;
                }
            }
        }

        public bool IsCalledByWinZip
        {
            get
            {
                return _pdfUtilView.IsCalledByWinZip;
            }
        }

        private void FormatRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var bt = sender as RadioButton;
            if (bt == null)
            {
                return;
            }

            if (bt == BitmapBtn)
            {
                CurImageFormat = ImageFormatEnum.BMP;
            }
            else if (bt == GIFBtn)
            {
                CurImageFormat = ImageFormatEnum.GIF;
            }
            else if (bt == JPGBtn)
            {
                CurImageFormat = ImageFormatEnum.JPG;
            }
            else if (bt == JP2Btn)
            {
                CurImageFormat = ImageFormatEnum.JP2;
            }
            else if (bt == PNGBtn)
            {
                CurImageFormat = ImageFormatEnum.PNG;
            }
            else if (bt == PSDBtn)
            {
                CurImageFormat = ImageFormatEnum.PSD;
            }
            else if (bt == TIFBtn)
            {
                CurImageFormat = ImageFormatEnum.TIF;
            }
            else if (bt == WEBPBtn)
            {
                CurImageFormat = ImageFormatEnum.WEBP;
            }
            else if (bt == SVGBtn)
            {
                CurImageFormat = ImageFormatEnum.SVG;
            }

        }

        private void DestRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var bt = sender as RadioButton;
            if (bt == null)
            {
                return;
            }

            if (bt == AddToZipBtn)
            {
                if (!_pdfUtilView.IsCalledByWinZip)
                {
                    FlatMessageWindows.DisplayWarningMessage(_pdfUtilView.WindowHandle, Properties.Resources.EXECUTE_SAVE_TO_ZIP_INDEPENDENTLY_TIPS);
                    individualImageBtn.IsChecked = true;
                    AddToZipBtn.IsChecked = false;
                    e.Handled = true;
                }
            }

            if (bt == individualImageBtn)
            {
                CurDestOptions = ImageDestinationOptions.IndividualImageFiles;
            }
            else if (bt == AddToZipBtn)
            {
                CurDestOptions = ImageDestinationOptions.AddToCurrentZipFile;
            }
        }

        private void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public bool ShowWindow()
        {
            return BaseShowWindow();
        }

        private void extractImageView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
        }

        private void ExtractImageView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void ExtractImageView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
