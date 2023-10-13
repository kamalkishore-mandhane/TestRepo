using Microsoft.Win32;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for AddSignatureDialog.xaml
    /// </summary>
    /// 

    public enum SignatureDialogTab
    {
        TypeTab,
        DrawTab,
        UploadTab
    }

    public partial class AddSignatureDialog : BaseWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private const int MaxSignImageHeight = 256;
        private const string SignatureNameFormat = "signature-{0:x4}.png";
        private BitmapSource _sourceImage;
        private string _typeText;
        private int _signTabSelectedIndex;

        // Segoe MDL2 Assets and Segoe Fluent Icons are fonts used for icons, when applying we replace it with Segoe UI.
        private readonly Dictionary<string, string> FontReplaceDict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "Segoe MDL2 Assets", "Segoe UI"},
            { "Segoe Fluent Icons", "Segoe UI"},
        };

        public AddSignatureDialog(PdfUtilView view)
        {
            InitializeComponent();
            Owner = view;
        }

        private void Notify(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public SignatureItem Item
        {
            get;
            private set;
        }

        public string TypeText
        {
            get => _typeText;
            set
            {
                if (_typeText != value)
                {
                    _typeText = value;
                    Notify(nameof(TypeText));
                    Notify(nameof(IsButtonEnable));
                }
            }
        }

        public Brush TypeTextForeground
        {
            get => new SolidColorBrush(PdfUtilSettings.Instance.SignatureForeground);
        }

        public float TypeTextFontSize
        {
            get => PdfUtilSettings.Instance.SignatureFontSize;
        }

        public FontFamily TypeTextFontFamily
        {
            get
            {
                if (FontReplaceDict.ContainsKey(PdfUtilSettings.Instance.SignatureFontFamily))
                {
                    return new FontFamily(FontReplaceDict[PdfUtilSettings.Instance.SignatureFontFamily]);
                }
                else
                {
                    return new FontFamily(PdfUtilSettings.Instance.SignatureFontFamily);
                }
            }
        }

        public FontStyle TypeTextFontStyle
        {
            get => ((PdfUtilSettings.Instance.SignatureFontStyle & System.Drawing.FontStyle.Italic) != 0) ? FontStyles.Italic : FontStyles.Normal;
        }

        public FontWeight TypeTextFontWeight
        {
            get => ((PdfUtilSettings.Instance.SignatureFontStyle & System.Drawing.FontStyle.Bold) != 0) ? FontWeights.Bold : FontWeights.Normal;
        }

        public BitmapSource SourceImage
        {
            get => _sourceImage;
            set
            {
                if (_sourceImage != value)
                {
                    _sourceImage = value;
                    Notify(nameof(SourceImage));
                    Notify(nameof(IsShowUploadImage));
                    Notify(nameof(IsButtonEnable));
                }
            }
        }

        public bool IsShowUploadImage
        {
            get { return SourceImage != null; }
        }

        public int SignTabSelectedIndex
        {
            get => _signTabSelectedIndex;
            set
            {
                if (_signTabSelectedIndex != value)
                {
                    _signTabSelectedIndex = value;
                    Notify(nameof(IsButtonEnable));
                }
            }
        }

        public bool IsButtonEnable
        {
            get
            {
                switch ((SignatureDialogTab)SignTabSelectedIndex)
                {
                    case SignatureDialogTab.TypeTab:
                        return !string.IsNullOrEmpty(TypeText);
                    case SignatureDialogTab.DrawTab:
                        return SignCanvas.Strokes.Count != 0;
                    case SignatureDialogTab.UploadTab:
                        return IsShowUploadImage;
                    default:
                        return false;
                }
            }
        }

        public bool ShowWindow()
        {
            return BaseShowWindow();
        }

        private void AddSignatureDialogView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
            else if (e.Key == Key.Enter)
            {
                if (IsButtonEnable)
                {
                    AddSignatureToList();
                    DialogResult = true;
                }
            }
        }

        private void AddSignatureDialogView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            SignCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
        }

        private void AddSignatureDialogView_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void ChangeStyleButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FontDialog fontdialog = new WzFontDialog();
            fontdialog.Font = new System.Drawing.Font(PdfUtilSettings.Instance.SignatureFontFamily, PdfUtilSettings.Instance.SignatureFontSize, PdfUtilSettings.Instance.SignatureFontStyle);
            fontdialog.ShowColor = true;
            fontdialog.Color = System.Drawing.Color.FromArgb(PdfUtilSettings.Instance.SignatureForeground.A,
                                                             PdfUtilSettings.Instance.SignatureForeground.R,
                                                             PdfUtilSettings.Instance.SignatureForeground.G,
                                                             PdfUtilSettings.Instance.SignatureForeground.B);

            if (fontdialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PdfUtilSettings.Instance.SignatureFontFamily = fontdialog.Font.Name;
                PdfUtilSettings.Instance.SignatureFontSize = fontdialog.Font.Size;
                PdfUtilSettings.Instance.SignatureFontStyle = fontdialog.Font.Style;
                PdfUtilSettings.Instance.SignatureForeground = Color.FromArgb(fontdialog.Color.A, fontdialog.Color.R, fontdialog.Color.G, fontdialog.Color.B);

                Notify(nameof(TypeTextFontFamily));
                Notify(nameof(TypeTextFontSize));
                Notify(nameof(TypeTextForeground));
                Notify(nameof(TypeTextFontStyle));
                Notify(nameof(TypeTextFontWeight));
            }
        }

        private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            Notify(nameof(IsButtonEnable));
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            SelectImageButtonAction();
        }

        private void UploadGrid_Drag(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                WpfDragDropHelper.SetDropDescription(e.Data, WpfDragDropHelper.DropImageType.Copy, Properties.Resources.OPEN_PICKER_TITLE, string.Empty);
                if (files != null && files.Length > 0)
                {
                    e.Effects = DragDropEffects.Copy;

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
            }

            e.Handled = true;
        }

        private void UploadGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    WpfDragDropHelper.Drop(e, this);
                    LoadImage(CopyTempImage(files[0]));
                    e.Handled = true;
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // clear current signature
            switch ((SignatureDialogTab)SignTabControl.SelectedIndex)
            {
                case SignatureDialogTab.TypeTab:
                    TypeText = string.Empty;
                    break;
                case SignatureDialogTab.DrawTab:
                    SignCanvas.Strokes.Clear();
                    break;
                case SignatureDialogTab.UploadTab:
                    SourceImage = null;
                    break;
                default:
                    break;
            }

            e.Handled = true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // add current signature to signature list
            AddSignatureToList();
            DialogResult = true;
        }

        private void AddSignatureToList()
        {
            UIElement element = null;
            switch ((SignatureDialogTab)SignTabControl.SelectedIndex)
            {
                case SignatureDialogTab.TypeTab:
                    element = TypeNameGrid;
                    break;
                case SignatureDialogTab.DrawTab:
                    element = SignCanvas;
                    break;
                case SignatureDialogTab.UploadTab:
                    element = PreviewImage;
                    break;
                default:
                    break;
            }

            if (element != null)
            {
                BitmapSource bitmap;
                if (element is Image imageElement)
                {
                    var orgBitmap = PdfHelper.BitmapSourceFromImage(imageElement);
                    double ratio = MaxSignImageHeight / element.RenderSize.Height;
                    bitmap = new TransformedBitmap(orgBitmap, new ScaleTransform(ratio, ratio));
                }
                else
                {
                    var sourceBrush = new VisualBrush(element);
                    bitmap = PdfHelper.BitmapSourceFromBrush(sourceBrush, (int)element.RenderSize.Width, (int)element.RenderSize.Height);
                }
                string signaturePath = SaveSignature(bitmap);
                Item = new SignatureItem(signaturePath, bitmap);
            }
        }

        private string SaveSignature(BitmapSource bitmap)
        {
            var folder = ApplicationHelper.DefaultLocalUserPdfUtilSignature;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string filePath = FileOperation.GenerateUniqueFileName(folder, SignatureNameFormat);

            try
            {
                var encoder = new PngBitmapEncoder();
                var outputFrame = BitmapFrame.Create(bitmap);
                encoder.Frames.Add(outputFrame);
                using (var stream = File.Create(filePath))
                {
                    encoder.Save(stream);
                }

                return filePath;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private void SelectImageButtonAction()
        {
            string title = Properties.Resources.OPEN_PICKER_TITLE;
            string defaultBtn = Properties.Resources.OPEN_PICKER_BUTTON;
            string filters = Properties.Resources.WATERMARK_FILTERS;
            var defaultFolder = PdfHelper.GetOpenPickerDefaultFolder();
            var selectedItems = new WzCloudItem4[1];
            int count = 1;

            bool ret = WinzipMethods.FileSelection(new WindowInteropHelper(this).Handle, title, defaultBtn, filters, defaultFolder, selectedItems, ref count, false, true, true, true, false, false);

            if (ret)
            {
                var selectedItem = selectedItems[0];
                string tempFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                string filePath = string.Empty;

                PdfHelper.SetOpenPickerDefaultFolder(selectedItem);

                if (PdfHelper.IsCloudItem(selectedItem.profile.Id))
                {
                    var folderItem = PdfHelper.InitCloudItemFromPath(tempFolder);
                    int errorCode = 0;
                    ret = WinzipMethods.DownloadFromCloud(new WindowInteropHelper(this).Handle, selectedItems, count, folderItem, false, true, ref errorCode);

                    if (!ret)
                    {
                        Directory.Delete(tempFolder, true);
                        return;
                    }

                    filePath = Path.Combine(tempFolder, selectedItem.name);
                }
                else
                {
                    filePath = CopyTempImage(selectedItem.itemId);
                }

                LoadImage(filePath);
            }
        }

        private string CopyTempImage(string orgFilePath)
        {
            if (File.Exists(orgFilePath))
            {
                string tempFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                string filePath = Path.Combine(tempFolder, Path.GetFileName(orgFilePath));

                File.Copy(orgFilePath, filePath, true);
                return filePath;
            }

            return string.Empty;
        }

        private void LoadImage(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    SourceImage = PdfHelper.LoadImageFromPath(filePath);
                    Notify(nameof(IsShowUploadImage));
                }
                catch
                {
                    FlatMessageWindows.DisplayErrorMessage(new WindowInteropHelper(this).Handle, Properties.Resources.ERROR_IMAGE_INVALID);
                }
            }
        }
    }
}
