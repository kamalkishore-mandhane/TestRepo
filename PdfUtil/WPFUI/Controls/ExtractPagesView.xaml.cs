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
    /// Interaction logic for ExtractPagesView.xaml
    /// </summary>

    public enum DocumentFormatEnum
    {
        PDF,
        DOC,
        DOCX,
        BMP,
        JPG,
        PNG,
        TIF
    }

    public enum DestinationOptions
    {
        DocumentFile,
        AddToCurrentZipFile
    }

    public partial class ExtractPagesView : BaseWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private DocumentFormatEnum _curDocFormat = DocumentFormatEnum.PDF;
        private DestinationOptions _curDestOptions = DestinationOptions.DocumentFile;
        private string _documentPath = string.Empty;

        private PdfUtilView _pdfUtilView;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ExtractPagesView(PdfUtilView view)
        {
            _pdfUtilView = view;
            InitializeComponent();
            Owner = _pdfUtilView;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public DocumentFormatEnum CurDocFormat
        {
            get { return _curDocFormat; }
            set
            {
                if (_curDocFormat != value)
                {
                    _curDocFormat = value;
                    OnPropertyChanged(nameof(CurDocFormat));
                }
            }
        }

        public DestinationOptions CurDestOptions
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

        public string DocumentPath
        {
            get { return _documentPath; }
            set
            {
                if (_documentPath !=value)
                {
                    _documentPath = value;
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
            var radioButton = sender as RadioButton;
            if (radioButton == null)
            {
                return;
            }

            if (radioButton == pdfBtn)
            {
                CurDocFormat = DocumentFormatEnum.PDF;
            }
            else if (radioButton == docBtn)
            {
                CurDocFormat = DocumentFormatEnum.DOC;
            }
            else if (radioButton == docxBtn)
            {
                CurDocFormat = DocumentFormatEnum.DOCX;
            }
            else if (radioButton == bmpBtn)
            {
                CurDocFormat = DocumentFormatEnum.BMP;
            }
            else if (radioButton == jpgBtn)
            {
                CurDocFormat = DocumentFormatEnum.JPG;
            }
            else if (radioButton == pngBtn)
            {
                CurDocFormat = DocumentFormatEnum.PNG;
            }
            else if (radioButton == tifBtn)
            {
                CurDocFormat = DocumentFormatEnum.TIF;
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
                    documentFileBtn.IsChecked = true;
                    AddToZipBtn.IsChecked = false;
                    e.Handled = true;
                }
            }

            if (bt == documentFileBtn)
            {
                CurDestOptions = DestinationOptions.DocumentFile;
            }
            else if (bt == AddToZipBtn)
            {
                CurDestOptions = DestinationOptions.AddToCurrentZipFile;
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

        private void extractPageView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
        }

        private void ExtractPagesView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void ExtractPagesView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
