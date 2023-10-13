using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PdfUtil.Util;

using PdfUtil.WPFUI.Controls;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.View;

namespace PdfUtil.WPFUI.ViewModel
{
    enum WzFontStyle
    {
        Regular = 1,
        Bold = 2,
        Italic = 3,
        All = 4,
    }

    class WatermarkSettingViewModel : ObservableObject
    {
        private const string WzKey = @"Software\WinZip Computing\PDFExpress\WXF\WzWXFwmrk";
        private const string WzLocationKey = "WmrkLocation";
        private const string WzAngleKey = "WmrkAngle";
        private const string WzOpacityKey = "WmrkOpacity";
        private const string WzContentKey = "WmrkContent";
        private const string WzDateStampKey = "WmrkNeedDateStamp";
        private const string WzTimeStampKey = "WmrkNeedTimeStamp";
        private const string WzFontSizeKey = "WmrkSize";
        private const string WzFontKey = "WmrkFont";
        private const string WzFontStyleKey = "WmrkStyle";
        private const string WzColorKey = "WmrkColor";
        private const string DefaultFontFamilyName = "Arial";
        private const string WzImageLocationKey = "WmrkImageLocation";
        private const string WzImageOpacityKey = "WmrkImageOpacity";
        private const string WzImageFilenameKey = "WmrkImageFilename";
        private const string WzUseTextKey = "WmrkUseText";
        private const string WzUseImageKey = "WmrkUseImage";
        private const string WzTextOverImageKey = "WmrkTextOverImage";
        private const string WzImageChangedColorKey = "WmrkImageChangedColor";
        private const int DefaultFontColor = 0xC0C0C0;
        private const int DefaultOpacity = 25;
        private const int DefaultFontSize = 36;

        private bool _textLocation0IsChecked = false;
        private bool _textLocation1IsChecked = false;
        private bool _textLocation2IsChecked = false;
        private bool _textLocation3IsChecked = false;
        private bool _textLocation4IsChecked = false;
        private bool _textLocation5IsChecked = false;
        private bool _textLocation6IsChecked = false;
        private bool _textLocation7IsChecked = false;
        private bool _textLocation8IsChecked = false;
        private bool _textAngle0IsChecked = false;
        private bool _textAngle1IsChecked = false;
        private bool _textAngle2IsChecked = false;
        private bool _textAngle3IsChecked = false;
        private bool _imageLocation0IsChecked = false;
        private bool _imageLocation1IsChecked = false;
        private bool _imageLocation2IsChecked = false;
        private bool _imageLocation3IsChecked = false;
        private bool _imageLocation4IsChecked = false;
        private bool _imageLocation5IsChecked = false;
        private bool _imageLocation6IsChecked = false;
        private bool _imageLocation7IsChecked = false;
        private bool _imageLocation8IsChecked = false;

        private bool _useImageIsChecked = false;
        private bool _useTextIsChecked = true;
        private bool _hasTransparentBackground = false;

        private bool _isEyeDropperOn = false;
        private bool _isTextOverImage = true;
        private int _textOpacity = 25;
        private int _imageOpacity = 25;
        private string _content = string.Empty;
        private bool _dateStampIsChecked = false;
        private bool _timeStampIsChecked = false;
        private string _fontFamily = string.Empty;
        private int _fontColor = 0;
        private int _fontSize = 0;
        private System.Drawing.FontStyle _fontStyle = System.Drawing.FontStyle.Regular;
        private static readonly string[] customFontStyle = new string[3] { "Bahnschrift", "Cascadia Code", "Cascadia Mono" };

        private string _sourceImagePath;
        BitmapSource _sourceImage;
        SolidColorBrush _colorBoxBrush;
        SolidColorBrush _transparentColor;

        private WatermarkSettingView _watermarkSettingView;
        private WatermarkResultElement _resultElement;
        private PdfUtilViewModel _pdfUtilViewModel;

        public WatermarkSettingViewModel(WatermarkSettingView view)
        {
            _watermarkSettingView = view;
            _pdfUtilViewModel = view.Owner.DataContext as PdfUtilViewModel;
            InitWatermarkSetting();
        }

        [Obfuscation(Exclude = true)]
        public bool TextLocation0IsChecked
        {
            get => _textLocation0IsChecked;
            set
            {
                if (_textLocation0IsChecked != value)
                {
                    _textLocation0IsChecked = value;
                    Notify(nameof(TextLocation0IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TextLocation1IsChecked
        {
            get => _textLocation1IsChecked;
            set
            {
                if (_textLocation1IsChecked != value)
                {
                    _textLocation1IsChecked = value;
                    Notify(nameof(TextLocation1IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TextLocation2IsChecked
        {
            get => _textLocation2IsChecked;
            set
            {
                if (_textLocation2IsChecked != value)
                {
                    _textLocation2IsChecked = value;
                    Notify(nameof(TextLocation2IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TextLocation3IsChecked
        {
            get => _textLocation3IsChecked;
            set
            {
                if (_textLocation3IsChecked != value)
                {
                    _textLocation3IsChecked = value;
                    Notify(nameof(TextLocation3IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TextLocation4IsChecked
        {
            get => _textLocation4IsChecked;
            set
            {
                if (_textLocation4IsChecked != value)
                {
                    _textLocation4IsChecked = value;
                    Notify(nameof(TextLocation4IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TextLocation5IsChecked
        {
            get => _textLocation5IsChecked;
            set
            {
                if (_textLocation5IsChecked != value)
                {
                    _textLocation5IsChecked = value;
                    Notify(nameof(TextLocation5IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TextLocation6IsChecked
        {
            get => _textLocation6IsChecked;
            set
            {
                if (_textLocation6IsChecked != value)
                {
                    _textLocation6IsChecked = value;
                    Notify(nameof(TextLocation6IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TextLocation7IsChecked
        {
            get => _textLocation7IsChecked;
            set
            {
                if (_textLocation7IsChecked != value)
                {
                    _textLocation7IsChecked = value;
                    Notify(nameof(TextLocation7IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TextLocation8IsChecked
        {
            get => _textLocation8IsChecked;
            set
            {
                if (_textLocation8IsChecked != value)
                {
                    _textLocation8IsChecked = value;
                    Notify(nameof(TextLocation8IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TextAngle0IsChecked
        {
            get => _textAngle0IsChecked;
            set
            {
                if (_textAngle0IsChecked != value)
                {
                    _textAngle0IsChecked = value;
                    Notify(nameof(TextAngle0IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TextAngle1IsChecked
        {
            get => _textAngle1IsChecked;
            set
            {
                if (_textAngle1IsChecked != value)
                {
                    _textAngle1IsChecked = value;
                    Notify(nameof(TextAngle1IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TextAngle2IsChecked
        {
            get => _textAngle2IsChecked;
            set
            {
                if (_textAngle2IsChecked != value)
                {
                    _textAngle2IsChecked = value;
                    Notify(nameof(TextAngle2IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TextAngle3IsChecked
        {
            get => _textAngle3IsChecked;
            set
            {
                if (_textAngle3IsChecked != value)
                {
                    _textAngle3IsChecked = value;
                    Notify(nameof(TextAngle3IsChecked));
                    _resultElement?.RedrawText();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ImageLocation0IsChecked
        {
            get => _imageLocation0IsChecked;
            set
            {
                if (_imageLocation0IsChecked != value)
                {
                    _imageLocation0IsChecked = value;
                    Notify(nameof(ImageLocation0IsChecked));
                    _resultElement?.RedrawImage();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ImageLocation1IsChecked
        {
            get => _imageLocation1IsChecked;
            set
            {
                if (_imageLocation1IsChecked != value)
                {
                    _imageLocation1IsChecked = value;
                    Notify(nameof(ImageLocation1IsChecked));
                    _resultElement?.RedrawImage();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ImageLocation2IsChecked
        {
            get => _imageLocation2IsChecked;
            set
            {
                if (_imageLocation2IsChecked != value)
                {
                    _imageLocation2IsChecked = value;
                    Notify(nameof(ImageLocation2IsChecked));
                    _resultElement?.RedrawImage();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ImageLocation3IsChecked
        {
            get => _imageLocation3IsChecked;
            set
            {
                if (_imageLocation3IsChecked != value)
                {
                    _imageLocation3IsChecked = value;
                    Notify(nameof(ImageLocation3IsChecked));
                    _resultElement?.RedrawImage();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ImageLocation4IsChecked
        {
            get => _imageLocation4IsChecked;
            set
            {
                if (_imageLocation4IsChecked != value)
                {
                    _imageLocation4IsChecked = value;
                    Notify(nameof(ImageLocation4IsChecked));
                    _resultElement?.RedrawImage();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ImageLocation5IsChecked
        {
            get => _imageLocation5IsChecked;
            set
            {
                if (_imageLocation5IsChecked != value)
                {
                    _imageLocation5IsChecked = value;
                    Notify(nameof(ImageLocation5IsChecked));
                    _resultElement?.RedrawImage();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ImageLocation6IsChecked
        {
            get => _imageLocation6IsChecked;
            set
            {
                if (_imageLocation6IsChecked != value)
                {
                    _imageLocation6IsChecked = value;
                    Notify(nameof(ImageLocation6IsChecked));
                    _resultElement?.RedrawImage();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ImageLocation7IsChecked
        {
            get => _imageLocation7IsChecked;
            set
            {
                if (_imageLocation7IsChecked != value)
                {
                    _imageLocation7IsChecked = value;
                    Notify(nameof(ImageLocation7IsChecked));
                    _resultElement?.RedrawImage();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ImageLocation8IsChecked
        {
            get => _imageLocation8IsChecked;
            set
            {
                if (_imageLocation8IsChecked != value)
                {
                    _imageLocation8IsChecked = value;
                    Notify(nameof(ImageLocation8IsChecked));
                    _resultElement?.RedrawImage();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public int TextOpacity
        {
            get => _textOpacity;
            set
            {
                if (_textOpacity != value)
                {
                    _textOpacity = value;
                    _resultElement?.UpdateTextOpacity();
                    Notify(nameof(TextOpacity));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public int ImageOpacity
        {
            get => _imageOpacity;
            set
            {
                if (_imageOpacity != value)
                {
                    _imageOpacity = value;
                    Notify(nameof(ImageOpacity));
                    _resultElement?.UpdateImageOpacity();
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    _resultElement?.RedrawText();
                    Notify(nameof(Content));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool DateStampIsChecked
        {
            get => _dateStampIsChecked;
            set
            {
                if (_dateStampIsChecked != value)
                {
                    _dateStampIsChecked = value;
                    _resultElement?.RedrawText();
                    Notify(nameof(DateStampIsChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool TimeStampIsChecked
        {
            get => _timeStampIsChecked;
            set
            {
                if (_timeStampIsChecked != value)
                {
                    _timeStampIsChecked = value;
                    _resultElement?.RedrawText();
                    Notify(nameof(TimeStampIsChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public BitmapSource SourceImage
        {
            get => _sourceImage;
            private set
            {
                _sourceImage = value;
                Notify(nameof(SourceImage));
                _resultElement?.RedrawImage();
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsEyeDropperOn
        {
            get => _isEyeDropperOn;
            set
            {
                if (_isEyeDropperOn != value)
                {
                    _isEyeDropperOn = value;
                    Notify(nameof(IsEyeDropperOn));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsTextOverImage
        {
            get => _isTextOverImage;
            set
            {
                if (_isTextOverImage != value)
                {
                    _isTextOverImage = value;
                    Notify(nameof(IsTextOverImage));
                    _resultElement?.BringToFront(_isTextOverImage);
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsImageOverText => !_isTextOverImage;

        [Obfuscation(Exclude = true)]
        public bool UseImageIsChecked
        {
            get => _useImageIsChecked;
            set
            {
                if (_useImageIsChecked != value)
                {
                    _useImageIsChecked = value;
                    Notify(nameof(UseImageIsChecked));
                    _resultElement?.ShowImage(_useImageIsChecked);
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool UseTextIsChecked
        {
            get => _useTextIsChecked;
            set
            {
                if (_useTextIsChecked != value)
                {
                    _useTextIsChecked = value;
                    Notify(nameof(UseTextIsChecked));
                    _resultElement?.ShowText(_useTextIsChecked);
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool BackgroundNotTransparent
        {
            get => _hasTransparentBackground;
            set
            {
                if (value != _hasTransparentBackground)
                {
                    _hasTransparentBackground = value;
                    Notify(nameof(BackgroundNotTransparent));
                }
            }
        }

        public WzFontStyle WmrkFontStyle
        {
            get
            {
                WzFontStyle style = WzFontStyle.Regular;
                if ((_fontStyle & System.Drawing.FontStyle.Bold) != 0 && (_fontStyle & System.Drawing.FontStyle.Italic) != 0)
                {
                    style = WzFontStyle.All;
                }
                else if (_fontStyle == System.Drawing.FontStyle.Bold)
                {
                    style = WzFontStyle.Bold;
                }
                else if (_fontStyle == System.Drawing.FontStyle.Italic)
                {
                    style = WzFontStyle.Italic;
                }

                return style;
            }
        }

        public string FontFamily
        {
            get
            {
                return _fontFamily;
            }
        }

        public int FontSize
        {
            get
            {
                return _fontSize;
            }
        }

        public System.Drawing.Color FontColor
        {
            get
            {
                unchecked
                {
                    int green = _fontColor & (int)0xff00ff00;
                    int red = (_fontColor & (int)0x000000ff) << 16;
                    int blue = (_fontColor & (int)0x00ff0000) >> 16;
                    int newcolor = green | red | blue;
                    return System.Drawing.Color.FromArgb(newcolor);
                }
            }
        }

        // ^  y
        // |
        // |
        // |
        // |
        // |            x
        //-|------------>
        // this is aspose lib coordinate system, it is different from WPF by default
        public float TextAngle
        {
            get
            {
                
                if (TextAngle0IsChecked) //direction 4 (-45 degree)
                {
                    return 0;
                }
                else if (TextAngle1IsChecked) //direction 1 (0 degree)
                {
                    return -90;
                }
                else if (TextAngle2IsChecked) //direction 3 (45 degree)
                {
                    return 45;
                }
                else if (TextAngle3IsChecked) //direction 2 (-90 degree)
                {
                    return -45;
                }

                return 45;
            }
        }

        public SquaredPosition TextPosition
        {
            get
            {
                int location = -1;

                if (TextLocation0IsChecked)
                {
                    location = 0;
                }
                else if (TextLocation1IsChecked)
                {
                    location = 1;
                }
                else if (TextLocation2IsChecked)
                {
                    location = 2;
                }
                else if (TextLocation3IsChecked)
                {
                    location = 3;
                }
                else if (TextLocation4IsChecked)
                {
                    location = 4;
                }
                else if (TextLocation5IsChecked)
                {
                    location = 5;
                }
                else if (TextLocation6IsChecked)
                {
                    location = 6;
                }
                else if (TextLocation7IsChecked)
                {
                    location = 7;
                }
                else if (TextLocation8IsChecked)
                {
                    location = 8;
                }

                var position = new SquaredPosition(location / 3, location % 3);
                return position;
            }
        }

        public SquaredPosition ImagePosition
        {
            get
            {
                int location = -1;

                if (ImageLocation0IsChecked)
                {
                    location = 0;
                }
                else if (ImageLocation1IsChecked)
                {
                    location = 1;
                }
                else if (ImageLocation2IsChecked)
                {
                    location = 2;
                }
                else if (ImageLocation3IsChecked)
                {
                    location = 3;
                }
                else if (ImageLocation4IsChecked)
                {
                    location = 4;
                }
                else if (ImageLocation5IsChecked)
                {
                    location = 5;
                }
                else if (ImageLocation6IsChecked)
                {
                    location = 6;
                }
                else if (ImageLocation7IsChecked)
                {
                    location = 7;
                }
                else if (ImageLocation8IsChecked)
                {
                    location = 8;
                }

                var position = new SquaredPosition(location / 3, location % 3);
                return position;
            }
        }

        public SolidColorBrush ColorBoxBrush
        {
            get => _colorBoxBrush;
            private set
            {
                if (value != _colorBoxBrush)
                {
                    _colorBoxBrush = value;
                    Notify(nameof(ColorBoxBrush));
                }
            }
        }

        private void InitWatermarkSetting()
        {
            // init image settings
            _sourceImagePath = RegeditOperation.GetConversionSettingRegistryStringValue(WzKey, WzImageFilenameKey);
            int imageOpacity = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzImageOpacityKey);

            if (imageOpacity == -1)
            {
                imageOpacity = DefaultOpacity;
            }

            ImageOpacity = imageOpacity;

            int imageLcation = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzImageLocationKey);

            if (imageLcation == -1)
            {
                imageLcation = 4;
            }

            switch (imageLcation)
            {
                case 0:
                    ImageLocation0IsChecked = true;
                    break;
                case 1:
                    ImageLocation1IsChecked = true;
                    break;
                case 2:
                    ImageLocation2IsChecked = true;
                    break;
                case 3:
                    ImageLocation3IsChecked = true;
                    break;
                case 4:
                    ImageLocation4IsChecked = true;
                    break;
                case 5:
                    ImageLocation5IsChecked = true;
                    break;
                case 6:
                    ImageLocation6IsChecked = true;
                    break;
                case 7:
                    ImageLocation7IsChecked = true;
                    break;
                case 8:
                    ImageLocation8IsChecked = true;
                    break;
            }

            LoadImage(false);

            // init text settings
            int textLcation = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzLocationKey);

            if (textLcation == -1)
            {
                textLcation = 4;
            }

            switch (textLcation)
            {
                case 0:
                    TextLocation0IsChecked = true;
                    break;
                case 1:
                    TextLocation1IsChecked = true;
                    break;
                case 2:
                    TextLocation2IsChecked = true;
                    break;
                case 3:
                    TextLocation3IsChecked = true;
                    break;
                case 4:
                    TextLocation4IsChecked = true;
                    break;
                case 5:
                    TextLocation5IsChecked = true;
                    break;
                case 6:
                    TextLocation6IsChecked = true;
                    break;
                case 7:
                    TextLocation7IsChecked = true;
                    break;
                case 8:
                    TextLocation8IsChecked = true;
                    break;
            }

            int angle = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzAngleKey);

            if (angle == -1)
            {
                angle = 2;
            }

            switch (angle)
            {
                case 0:
                    TextAngle3IsChecked = true;
                    break;
                case 1:
                    TextAngle0IsChecked = true;
                    break;
                case 2:
                    TextAngle2IsChecked = true;
                    break;
                case 3:
                    TextAngle1IsChecked = true;
                    break;
            }

            int style = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzFontStyleKey);

            if (style == -1)
            {
                style = 1;
            }

            switch ((WzFontStyle)style)
            {
                case WzFontStyle.Regular:
                    _fontStyle = System.Drawing.FontStyle.Regular;
                    break;
                case WzFontStyle.Bold:
                    _fontStyle = System.Drawing.FontStyle.Bold;
                    break;
                case WzFontStyle.Italic:
                    _fontStyle = System.Drawing.FontStyle.Italic;
                    break;
                case WzFontStyle.All:
                    _fontStyle = System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic;
                    break;
            }

            int textOpacity = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzOpacityKey);

            if (textOpacity == -1)
            {
                textOpacity = DefaultOpacity;
            }

            TextOpacity = textOpacity;
            Content = RegeditOperation.GetConversionSettingRegistryStringValue(WzKey, WzContentKey);

            if (Content == string.Empty)
            {
                Content = Properties.Resources.WATERMARK_SETTING_DEFAULT_CONTENT;
            }

            DateStampIsChecked = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzDateStampKey) == 1;
            TimeStampIsChecked = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzTimeStampKey) == 1;
            _fontSize = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzFontSizeKey) / 10;

            if (_fontSize == 0)
            {
                _fontSize = DefaultFontSize;
            }

            _fontFamily = RegeditOperation.GetConversionSettingRegistryStringValue(WzKey, WzFontKey);

            if (_fontFamily == string.Empty)
            {
                _fontFamily = DefaultFontFamilyName;
            }

            _fontColor = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzColorKey);

            if (_fontColor == -1)
            {
                _fontColor = DefaultFontColor;
            }

            // init other settings
            UseImageIsChecked = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzUseImageKey) == 1;
            int checkText = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzUseTextKey);
            UseTextIsChecked = checkText == -1 ? true : checkText == 1;
            IsTextOverImage = RegeditOperation.GetConversionSettingRegistryIntValue(WzKey, WzTextOverImageKey) == 1;
        }

        public void ClickOpenImageButtonAction()
        {
            string title = Properties.Resources.OPEN_PICKER_TITLE;
            string defaultBtn = Properties.Resources.OPEN_PICKER_BUTTON;
            string filters = Properties.Resources.WATERMARK_FILTERS;
            var defaultFolder = PdfHelper.GetOpenPickerDefaultFolder();
            var selectedItems = new WzCloudItem4[1];
            int count = 1;

            bool ret = WinzipMethods.FileSelection(_watermarkSettingView.WindowHandle, title, defaultBtn, filters, defaultFolder, selectedItems, ref count, false, true, true, true, false, false);

            if (ret)
            {
                var selectedItem = selectedItems[0];
                string tempFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);

                PdfHelper.SetOpenPickerDefaultFolder(selectedItem);

                if (PdfHelper.IsCloudItem(selectedItem.profile.Id))
                {
                    var folderItem = PdfHelper.InitCloudItemFromPath(tempFolder);
                    int errorCode = 0;
                    ret = WinzipMethods.DownloadFromCloud(_watermarkSettingView.WindowHandle, selectedItems, count, folderItem, false, true, ref errorCode);

                    if (!ret)
                    {
                        Directory.Delete(tempFolder, true);
                        return;
                    }

                    _sourceImagePath = Path.Combine(tempFolder, selectedItem.name);
                }
                else
                {
                    if (!File.Exists(selectedItem.itemId))
                    {
                        return;
                    }
                    _sourceImagePath = Path.Combine(tempFolder, selectedItem.name);

                    File.Copy(selectedItem.itemId, _sourceImagePath, true);
                }

                LoadImage(true);
            }
        }

        public void ClickFontButtonAction()
        {
            System.Windows.Forms.FontDialog fontdialog = new WzFontDialog();
            fontdialog.Font = new System.Drawing.Font(_fontFamily, _fontSize, _fontStyle);
            fontdialog.ShowColor = true;
            fontdialog.Color = System.Drawing.Color.FromArgb(_fontColor & 0x0000ff, (_fontColor & 0x0ff00) >> 8, (_fontColor & 0xff0000) >> 16);
            if (fontdialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _fontFamily = fontdialog.Font.Name;
                _fontSize = (int)fontdialog.Font.Size;
                _fontStyle = fontdialog.Font.Style;
                _fontColor = (int)((int)fontdialog.Color.B << 16 | (int)((int)fontdialog.Color.G << 8 | (int)fontdialog.Color.R));
                _resultElement.RedrawText();
            }
        }

        public void ClickOKButtonAction()
        {
            if (UseImageIsChecked && SourceImage == null)
            {
                FlatMessageWindows.DisplayWarningMessage(new WindowInteropHelper(_watermarkSettingView).Handle, Properties.Resources.WARNING_WATERMARK_NO_IMAGE);
                return;
            }

            if (UseTextIsChecked && Content == string.Empty)
            {
                FlatMessageWindows.DisplayWarningMessage(new WindowInteropHelper(_watermarkSettingView).Handle, Properties.Resources.WARNING_WATERMARK_NO_CONTENT);
                return;
            }

            _watermarkSettingView.DialogResult = true;
        }

        public void PreviewImageMouseMoveAction(Point point)
        {
            int x = Convert.ToInt32(point.X * (SourceImage.PixelWidth / _watermarkSettingView.PreviewImage.ActualWidth));
            if (x > SourceImage.PixelWidth - 1)
            {
                x = SourceImage.PixelWidth - 1;
            }

            int y = Convert.ToInt32(point.Y * (SourceImage.PixelHeight / _watermarkSettingView.PreviewImage.ActualHeight));
            if (y > SourceImage.PixelHeight - 1)
            {
                y = SourceImage.PixelHeight - 1;
            }

            var pixels = GetPixels(new Int32Rect(x, y, 1, 1), SourceImage);
            var color = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
            ColorBoxBrush = new SolidColorBrush(color);
        }

        public void BackgroundTransparencyAction()
        {
            IsEyeDropperOn = false;
            var writeableBitmap = new WriteableBitmap(SourceImage.PixelWidth, SourceImage.PixelHeight, SourceImage.DpiX, SourceImage.DpiY, SourceImage.Format, SourceImage.Palette);
            int stride = SourceImage.PixelWidth * SourceImage.Format.BitsPerPixel / 8;
            for (int line = 0; line < SourceImage.PixelHeight; ++line)
            {
                var rect = new Int32Rect(0, line, SourceImage.PixelWidth, 1);
                var linePixels = GetPixels(rect, SourceImage);

                for (int index = 0; index < linePixels.Length; index += 4)
                {
                    if (Color.Equals(ColorBoxBrush.Color, Color.FromArgb(linePixels[index + 3], linePixels[index + 2], linePixels[index + 1], linePixels[index])))
                    {
                        linePixels[index + 3] = 0;
                    }
                }

                writeableBitmap.WritePixels(rect, linePixels, stride, 0);
            }
            _transparentColor.Color = ColorBoxBrush.Color;

            SourceImage = writeableBitmap;
        }

        public void BackgroundTransparencyCancleAction()
        {
            IsEyeDropperOn = false;
            ColorBoxBrush = _transparentColor;
        }

        public void InitResultWatermark()
        {
            _resultElement = new WatermarkResultElement(_watermarkSettingView, this, _pdfUtilViewModel.CurrentPdfDocument.Pages[1].Rect.Width, _pdfUtilViewModel.CurrentPdfDocument.Pages[1].Rect.Height);
        }

        public void DoWatermark(IconItem[] iconItems)
        {
            var progressView = new ProgressView(_pdfUtilViewModel.PdfUtilView, ProgressOperation.Watermark);

            // initialize image watermark
            Aspose.Pdf.ImageStamp imageStamp = null;
            double pixelW = 0, pixelH = 0;
            if (UseImageIsChecked && SourceImage != null)
            {
                var stream = new MemoryStream();
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(SourceImage));
                encoder.Save(stream);
                imageStamp = new Aspose.Pdf.ImageStamp(stream);
                pixelW = SourceImage.PixelWidth;
                pixelH = SourceImage.PixelHeight;
            }

            // initialize text watermark
            var waterMarkText = Content;
            Aspose.Pdf.Text.TextState textState = null;
            if (UseTextIsChecked)
            {
                var time = DateTime.Now;

                if (DateStampIsChecked)
                {
                    waterMarkText += " " + time.ToString("d");
                }

                if (TimeStampIsChecked)
                {
                    waterMarkText += " " + time.ToString("t");
                }

                bool isBold = WmrkFontStyle == WzFontStyle.Bold || WmrkFontStyle == WzFontStyle.All;
                bool isItalic = WmrkFontStyle == WzFontStyle.Italic || WmrkFontStyle == WzFontStyle.All;
                string UserFontFamily = FontFamily;
                //Some system font extern styles are not supported in Apose PDF.dll
                foreach (var frontName in customFontStyle)
                {
                    if (UserFontFamily.Contains(frontName))
                    {
                        UserFontFamily = frontName;
                        break;
                    }
                }

                try
                {
                    textState = new Aspose.Pdf.Text.TextState(UserFontFamily, isBold, isItalic);
                }
                //Look for the user's installation directory
                catch
                {
                    var fontFile = string.Format("{0}-{1}{2}{3}.ttf", UserFontFamily, isBold ? "Bold" : "", isItalic ? "Italic" : "", (!isItalic && !isBold) ? "Regular" : "");
                    fontFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Microsoft\\Windows\\Fonts\\" + fontFile;

                    Aspose.Pdf.Text.Font font;
                    if (File.Exists(fontFile))
                        font = Aspose.Pdf.Text.FontRepository.OpenFont(fontFile);
                    //can not find custom style ,user regualar
                    else
                    {
                        var oriFontFile = string.Format("{0}.ttf", UserFontFamily);
                        oriFontFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Microsoft\\Windows\\Fonts\\" + oriFontFile;
                        font = Aspose.Pdf.Text.FontRepository.OpenFont(oriFontFile);
                    }
                    textState = new Aspose.Pdf.Text.TextState("Arial", isBold, isItalic);
                    textState.Font = font;
                }

                textState.FontSize = FontSize;
                textState.ForegroundColor = Aspose.Pdf.Color.FromRgb(FontColor);
            }

            // start
            int index = 0;
            bool success = true;
            _pdfUtilViewModel.CurExtractStatus = ProgressStatus.None;
            var ProcessThread = new Thread(new ThreadStart(new Action(delegate
            {
                try
                {
                    // start progress
                    progressView.Dispatcher.Invoke(new Action(delegate
                    {
                        progressView.InvokeProgressEvent(new ProgressEventArgs() { CurExtractItemIndex = index, TotalItemsCount = iconItems.Length });
                    }));

                    foreach (var item in iconItems)
                    {
                        if (IsTextOverImage)
                        {
                            DoImageWatermark(item, imageStamp, pixelW, pixelH);
                            DoTextWatermark(item, waterMarkText, textState);
                        }
                        else
                        {
                            DoTextWatermark(item, waterMarkText, textState);
                            DoImageWatermark(item, imageStamp, pixelW, pixelH);
                        }
                        item.UpdateThumbnailImage();

                        ++index;

                        // update progress
                        progressView.Dispatcher.Invoke(new Action(delegate
                        {
                            progressView.InvokeProgressEvent(new ProgressEventArgs() { CurExtractItemIndex = index, TotalItemsCount = iconItems.Length });
                        }));

                        if (_pdfUtilViewModel.CurExtractStatus == ProgressStatus.Cancel)
                        {
                            success = false;
                            break;
                        }
                    }
                }
                catch
                {
                    success = false;
                }

                if (_pdfUtilViewModel.CurExtractStatus != ProgressStatus.Cancel)
                {
                    progressView.Dispatcher.Invoke(new Action(delegate
                    {
                        progressView.InvokeProgressEvent(new ProgressEventArgs() { Status = ProgressStatus.Completed });
                    }));
                }
            })));

            ProcessThread.Start();
            progressView.ShowDialog();

            _pdfUtilViewModel.PreviewPageNeedUpdate();
            _pdfUtilViewModel.NotifyDocChanged();

            if (success && _pdfUtilViewModel.CurExtractStatus != ProgressStatus.Cancel)
            {
                FlatMessageWindows.DisplayInformationMessage(_pdfUtilViewModel.PdfUtilView.WindowHandle, Properties.Resources.CONVERSION_COMPLETED_SUCCESSFULLY);

                TrackHelper.LogPdfWatermarkEvent(UseImageIsChecked && SourceImage != null, UseTextIsChecked);
            }
        }

        private void LoadImage(bool isNewImage)
        {
            if (File.Exists(_sourceImagePath))
            {
                try
                {
                    SourceImage = PdfHelper.LoadImageFromPath(_sourceImagePath);

                    if (!isNewImage)
                    {
                        var colorBytes = RegeditOperation.GetConversionSettingRegistryBinaryValue(WzKey, WzImageChangedColorKey);
                        var writeableBitmap = new WriteableBitmap(SourceImage.PixelWidth, SourceImage.PixelHeight, SourceImage.DpiX, SourceImage.DpiY, SourceImage.Format, SourceImage.Palette);
                        int stride = SourceImage.PixelWidth * SourceImage.Format.BitsPerPixel / 8;
                        for (int line = 0; line < SourceImage.PixelHeight; ++line)
                        {
                            var rect = new Int32Rect(0, line, SourceImage.PixelWidth, 1);
                            var linePixels = GetPixels(rect, SourceImage);

                            for (int index = 0; index < linePixels.Length; index += 4)
                            {
                                int byteIndex = 0;
                                while (true)
                                {
                                    if (colorBytes[byteIndex] == 0      // Red
                                    && colorBytes[byteIndex + 1] == 0   // Green
                                    && colorBytes[byteIndex + 2] == 0   // Blue
                                    && colorBytes[byteIndex + 3] == 0)  // Alpha
                                    {
                                        break;
                                    }

                                    if (colorBytes[byteIndex] == linePixels[index + 2]      // Red
                                    && colorBytes[byteIndex + 1] == linePixels[index + 1]   // Green
                                    && colorBytes[byteIndex + 2] == linePixels[index])      // Blue
                                    {
                                        linePixels[index + 3] = 0;                          // Alpha
                                    }

                                    byteIndex += 4;
                                }
                            }

                            writeableBitmap.WritePixels(rect, linePixels, stride, 0);
                        }
                        SourceImage = writeableBitmap;
                    }
                }
                catch
                {
                    FlatMessageWindows.DisplayErrorMessage(_watermarkSettingView.WindowHandle, Properties.Resources.ERROR_IMAGE_INVALID);
                    return;
                }

                var firstPixel = GetPixels(new Int32Rect(0, 0, 1, 1), SourceImage);
                var color = Color.FromArgb(firstPixel[3], firstPixel[2], firstPixel[1], firstPixel[0]);

                if (color.A != 0)
                {
                    ColorBoxBrush = new SolidColorBrush(color);
                    BackgroundNotTransparent = true;
                }
                else
                {
                    ColorBoxBrush = new SolidColorBrush(Colors.Transparent);
                    BackgroundNotTransparent = false;
                }
                _transparentColor = new SolidColorBrush(ColorBoxBrush.Color);
            }
        }

        private void DoImageWatermark(IconItem item, Aspose.Pdf.ImageStamp stamp, double pixelW, double pixelH)
        {
            if (!UseImageIsChecked || SourceImage == null)
            {
                return;
            }

            var page = item.GetPage();
            double pageW = page.Rect.Width;
            double pageH = page.Rect.Height;

            if (page.Rotate == Aspose.Pdf.Rotation.on90 || page.Rotate == Aspose.Pdf.Rotation.on270)
            {
                pageW = page.Rect.Height;
                pageH = page.Rect.Width;
            }

            var rect = _resultElement.GetImageRect(pixelW, pixelH, pageW, pageH);

            stamp.Opacity = 1 - ImageOpacity * 1.0 / 100;
            stamp.Width = rect.Width;
            stamp.Height = rect.Height;
            stamp.XIndent = rect.X;
            stamp.YIndent = pageH - rect.Height - rect.Y;

            page.AddStamp(stamp);
        }

        private void DoTextWatermark(IconItem item, string waterarkText, Aspose.Pdf.Text.TextState textState)
        {
            if (!UseTextIsChecked || string.IsNullOrEmpty(waterarkText))
            {
                return;
            }

            var waterMarkTextWidth = textState.MeasureString(waterarkText);
            var page = item.GetPage();
            if (page != null)
            {
                var pageRect = page.GetPageRect(false);

                var orgRotation = page.Rotate;
                page.Rotate = Aspose.Pdf.Rotation.None;

                Aspose.Pdf.WatermarkArtifact artifact = new Aspose.Pdf.WatermarkArtifact();
                artifact.SetTextAndState(waterarkText, textState);
                artifact.Opacity = 1 - TextOpacity * 1.0 / 100;
                artifact.IsBackground = false;
                artifact.Rotation = GetRotation(TextAngle, orgRotation);
                artifact.Position = GetPosition(pageRect, orgRotation, TextPosition, artifact.Rotation, waterMarkTextWidth);
                page.Artifacts.Add(artifact);

                page.Rotate = orgRotation;
                artifact.Dispose();
            }
        }

        private float GetRotation(float orgRotation, Aspose.Pdf.Rotation pageRotation)
        {
            // get watermark rotation on page rotation
            const int circumferentialAngle = 360;
            float rotation = orgRotation;

            if (pageRotation == Aspose.Pdf.Rotation.on90)
            {
                rotation = orgRotation + 90;
            }
            else if (pageRotation == Aspose.Pdf.Rotation.on180)
            {
                rotation = orgRotation + 180;
            }
            else if (pageRotation == Aspose.Pdf.Rotation.on270)
            {
                rotation = orgRotation + 270;
            }

            return ((rotation % circumferentialAngle) + circumferentialAngle) % circumferentialAngle;
        }

        private Aspose.Pdf.Point GetPosition(Aspose.Pdf.Rectangle pageRect, Aspose.Pdf.Rotation pageRotation, SquaredPosition position, double angle, double textLength)
        {
            // get selected position on page rotation
            if (pageRotation == Aspose.Pdf.Rotation.on90)
            {
                position = new SquaredPosition(2 - position.Col, position.Row);
            }
            else if (pageRotation == Aspose.Pdf.Rotation.on180)
            {
                position = new SquaredPosition(2 - position.Row, 2 - position.Col);
            }
            else if (pageRotation == Aspose.Pdf.Rotation.on270)
            {
                position = new SquaredPosition(position.Col, 2 - position.Row);
            }

            // calculate the initial width and height of the center point corresponding to selected position 
            double width, height;
            if (position.Col == 0)
            {
                width = pageRect.Width / 6;
            }
            else if (position.Col == 1)
            {
                width = pageRect.Width / 2;
            }
            else
            {
                width = pageRect.Width * 5 / 6;
            }

            if (position.Row == 0)
            {
                height = pageRect.Height * 5 / 6;
            }
            else if (position.Row == 1)
            {
                height = pageRect.Height / 2;
            }
            else
            {
                height = pageRect.Height / 6;
            }

            // adjust the width and height of the watermark according to the rotation angle
            var halfTextLength = textLength / 2;
            switch ((int)angle)
            {
                case 0:
                    // watermark position moving left
                    if (textLength > pageRect.Width || width < halfTextLength)
                    {
                        width = 0;
                    }
                    else
                    {
                        width = (pageRect.Width < width + halfTextLength) ? pageRect.Width - textLength : width - halfTextLength;
                    }
                    break;
                case 45:
                    // watermark position moving down-left
                    width = MeasureWidthOnPage(width, position, textLength, pageRect, false);
                    height = MeasureHeightOnPage(height, position, textLength, pageRect, false);
                    break;
                case 90:
                    // watermark position moving down
                    if (textLength > pageRect.Height || height < halfTextLength)
                    {
                        height = 0;
                    }
                    else
                    {
                        height = pageRect.Height < height + halfTextLength ? pageRect.Height - textLength : height - halfTextLength;
                    }
                    break;
                case 135:
                    // watermark position moving down-right
                    width = MeasureWidthOnPage(width, position, textLength, pageRect, true);
                    height = MeasureHeightOnPage(height, position, textLength, pageRect, false);
                    break;
                case 180:
                    // watermark position moving right
                    if (textLength > pageRect.Width || width + halfTextLength > pageRect.Width)
                    {
                        width = pageRect.Width;
                    }
                    else
                    {
                        width = (textLength > width + halfTextLength) ? textLength : width + halfTextLength;
                    }
                    break;
                case 225:
                    // watermark position moving up-right
                    width = MeasureWidthOnPage(width, position, textLength, pageRect, true);
                    height = MeasureHeightOnPage(height, position, textLength, pageRect, true);
                    break;
                case 270:
                    // watermark position moving up
                    if (textLength > pageRect.Height || height + halfTextLength > pageRect.Height)
                    {
                        height = pageRect.Height;
                    }
                    else
                    {
                        height = textLength > height + halfTextLength ? textLength : height + halfTextLength;
                    }
                    break;
                case 315:
                    // watermark position moving up-left
                    width = MeasureWidthOnPage(width, position, textLength, pageRect, false);
                    height = MeasureHeightOnPage(height, position, textLength, pageRect, true);
                    break;
                default:
                    break;
            }

            return new Aspose.Pdf.Point(width, height);
        }

        private double MeasureWidthOnPage(double width, SquaredPosition position, double textLength, Aspose.Pdf.Rectangle pageRect, bool isIncrease)
        {
            // adjust the width of position of the oblique watermark
            var Root2 = Math.Pow(2, 0.5);
            var halfTextLength = textLength / 2;
            var halfTextRightAngleSide = halfTextLength / Root2;
            var pageHypotenuse = pageRect.Width < pageRect.Height ? pageRect.Width * Root2 : pageRect.Height * Root2;

            if (textLength > pageHypotenuse && pageRect.Height < pageRect.Width)
            {
                // if the length of the watermark is greater than the 45-degree diagonal length of the page
                // and the height of the page is less than the width, calculate a specific width
                if (position.Col == 0)
                {
                    width = isIncrease ? pageRect.Height : 0;
                }
                else if (position.Col == 1)
                {
                    width = (pageRect.Width - pageRect.Height) / 2 + (isIncrease ? pageRect.Height : 0);
                }
                else
                {
                    width = pageRect.Width - (isIncrease ? 0 : pageRect.Height);
                }
            }
            else if (textLength > pageHypotenuse || (isIncrease ? width + halfTextRightAngleSide > pageRect.Width : width < halfTextRightAngleSide))
            {
                // if the length of the watermark is greater than the 45-degree diagonal length of the page
                // or regular adjustments will make the width exceed the page area, calculate a specific width
                width = isIncrease ? pageRect.Width : 0;
            }
            else
            {
                // other case, make regular adjustments
                if (isIncrease)
                {
                    // watermark position moving right
                    width = width < halfTextRightAngleSide ? halfTextRightAngleSide * 2 : width + halfTextRightAngleSide;
                }
                else
                {
                    // watermark position moving left
                    width = (pageRect.Width < width + halfTextRightAngleSide) ? pageRect.Width - halfTextRightAngleSide * 2 : width - halfTextRightAngleSide;
                }
            }

            return width;
        }

        private double MeasureHeightOnPage(double height, SquaredPosition position, double textLength, Aspose.Pdf.Rectangle pageRect, bool isIncrease)
        {
            // adjust the height of the position of the oblique watermark
            var Root2 = Math.Pow(2, 0.5);
            var halfTextLength = textLength / 2;
            var halfTextRightAngleSide = halfTextLength / Root2;
            var pageHypotenuse = pageRect.Width < pageRect.Height ? pageRect.Width * Root2 : pageRect.Height * Root2;

            if (textLength > pageHypotenuse && pageRect.Width < pageRect.Height)
            {
                // if the length of the watermark is greater than the 45-degree diagonal length of the page
                // and the width of the page is less than the height, calculate a specific height
                if (position.Row == 0)
                {
                    height = isIncrease ? pageRect.Height : pageRect.Height - pageRect.Width;
                }
                else if (position.Row == 1)
                {
                    height = (pageRect.Height - pageRect.Width) / 2 + (isIncrease ? pageRect.Width : 0);
                }
                else
                {
                    height = isIncrease ? pageRect.Height : 0;
                }
            }
            else if (textLength > pageHypotenuse || (isIncrease ? height + halfTextRightAngleSide > pageRect.Height : height < halfTextRightAngleSide))
            {
                // if the length of the watermark is greater than the 45-degree diagonal length of the page
                // or regular adjustments will make the height exceed the page area, calculate a specific height
                height = isIncrease ? pageRect.Height : 0;
            }
            else
            {
                // other case, make regular adjustments
                if (isIncrease)
                {
                    // watermark position moving up
                    height = height < halfTextRightAngleSide ? halfTextRightAngleSide * 2 : height + halfTextRightAngleSide;
                }
                else
                {
                    // watermark position moving down
                    height = pageRect.Height < height + halfTextRightAngleSide ? pageRect.Height - halfTextRightAngleSide * 2 : height - halfTextRightAngleSide;
                }
            }

            return height;
        }

        private byte[] GetPixels(Int32Rect rect, BitmapSource bitmap)
        {
            int pixelBytes = bitmap.Format.BitsPerPixel / 8;
            byte[] pixels = new byte[rect.Width * rect.Height * pixelBytes];
            int stride = bitmap.PixelWidth * pixelBytes;
            bitmap.CopyPixels(rect, pixels, stride, 0);
            return pixels;
        }

        class WatermarkResultElement : FrameworkElement
        {
            double _scaleX;
            double _scaleY;

            private WatermarkSettingView _view;
            private WatermarkSettingViewModel _viewModel;
            private VisualCollection _visuals;
            private DrawingVisual _textVisual;
            private DrawingVisual _imageVisual;

            public WatermarkResultElement(WatermarkSettingView view, WatermarkSettingViewModel viewModel, double pageWidth, double pageHeight)
            {
                _view = view;
                _viewModel = viewModel;
                _visuals = new VisualCollection(this);

                _scaleX = view.ResultImage.ActualWidth / pageWidth;
                _scaleY = view.ResultImage.ActualHeight / pageHeight;

                _textVisual = CreateTextVisual();
                if (_textVisual != null)
                {
                    _textVisual.Opacity = 1 - _viewModel.TextOpacity * 1.0 / 100;
                    _visuals.Add(_textVisual);
                }

                _imageVisual = CreateImageVisual();
                if (_imageVisual != null)
                {
                    _imageVisual.Opacity = 1 - _viewModel.ImageOpacity * 1.0 / 100;
                    _visuals.Add(_imageVisual);
                }

                view.ResultCanvas.Children.Add(this);

                if (view.ResultCanvas.Children.Count > 1)
                {
                    BringToFront(_viewModel.IsTextOverImage);
                }
            }

            public void RedrawText()
            {
                if (_viewModel.UseTextIsChecked)
                {
                    int index = _visuals.IndexOf(_textVisual);
                    if (index != -1)
                    {
                        _visuals.Remove(_textVisual);
                        _textVisual = CreateTextVisual();
                        _textVisual.Opacity = 1 - _viewModel.TextOpacity * 1.0 / 100;
                        _visuals.Insert(index, _textVisual);
                    }
                    else
                    {
                        ShowText(true);
                    }
                }
            }

            public void UpdateTextOpacity()
            {
                if (_viewModel.UseTextIsChecked)
                {
                    _textVisual.Opacity = 1 - _viewModel.TextOpacity * 1.0 / 100;
                }
            }

            public void ShowText(bool isShow)
            {
                if (isShow)
                {
                    if (_visuals.IndexOf(_textVisual) == -1)
                    {
                        _textVisual = CreateTextVisual();
                        if (_textVisual != null)
                        {
                            _textVisual.Opacity = 1 - _viewModel.TextOpacity * 1.0 / 100;

                            if (_viewModel.UseImageIsChecked && !_viewModel.IsTextOverImage)
                            {
                                _visuals.Insert(0, _textVisual);
                            }
                            else
                            {
                                _visuals.Add(_textVisual);
                            }
                        }
                    }
                }
                else
                {
                    if (_visuals.IndexOf(_textVisual) != -1)
                    {
                        _visuals.Remove(_textVisual);
                    }
                }
            }

            public void BringToFront(bool bringText)
            {
                if (bringText)
                {
                    _visuals.Remove(_textVisual);
                    _visuals.Add(_textVisual);
                }
                else
                {
                    _visuals.Remove(_imageVisual);
                    _visuals.Add(_imageVisual);
                }
            }

            public void RedrawImage()
            {
                if (_viewModel.UseImageIsChecked && _viewModel.SourceImage != null)
                {
                    int index = _visuals.IndexOf(_imageVisual);
                    if (index != -1)
                    {
                        _visuals.Remove(_imageVisual);
                        _imageVisual = CreateImageVisual();
                        _imageVisual.Opacity = 1 - _viewModel.ImageOpacity * 1.0 / 100;
                        _visuals.Insert(index, _imageVisual);
                    }
                    else
                    {
                        ShowImage(true);
                    }
                }
            }

            public Rect GetImageRect(double imgW, double imgH, double pageW, double pageH)
            {
                if (imgW >= pageW || imgH >= pageH)
                {
                    double zoom = Math.Min(pageW / imgW, pageH / imgH);
                    imgW *= zoom;
                    imgH *= zoom;
                }

                var position = _viewModel.ImagePosition;

                double cellW = pageW / 3;
                double cellH = pageH / 3;

                // top left
                if (position.Row == 0 && position.Col == 0)
                {
                    double midX = pageW / 6, midY = pageH / 6;
                    return new Rect(Math.Max(midX - imgW / 2, 0), Math.Max(midY - imgH / 2, 0), imgW, imgH);
                }
                // top middle
                else if (position.Row == 0 && position.Col == 1)
                {
                    double midX = pageW / 2, midY = pageH / 6;
                    return new Rect(Math.Max(midX - imgW / 2, 0), Math.Max(midY - imgH / 2, 0), imgW, imgH);
                }
                // top right
                else if (position.Row == 0 && position.Col == 2)
                {
                    double midX = pageW - pageW / 6, midY = pageH / 6;
                    return imgW <= cellW
                        ? new Rect(midX - imgW / 2, Math.Max(midY - imgH / 2, 0), imgW, imgH)
                        : new Rect(Math.Max(pageW - imgW, 0), Math.Max(midY - imgH / 2, 0), imgW, imgH);
                }
                // middle left
                else if (position.Row == 1 && position.Col == 0)
                {
                    double midX = pageW / 6, midY = pageH / 2;
                    return new Rect(Math.Max(midX - imgW / 2, 0), Math.Max(midY - imgH / 2, 0), imgW, imgH);
                }
                // center
                else if (position.Row == 1 && position.Col == 1)
                {
                    double midX = pageW / 2, midY = pageH / 2;
                    return new Rect(Math.Max(midX - imgW / 2, 0), Math.Max(midY - imgH / 2, 0), imgW, imgH);
                }
                // middle right
                else if (position.Row == 1 && position.Col == 2)
                {
                    double midX = pageW - pageW / 6, midY = pageH / 2;
                    return imgW <= cellW
                        ? new Rect(midX - imgW / 2, Math.Max(midY - imgH / 2, 0), imgW, imgH)
                        : new Rect(Math.Max(pageW - imgW, 0), Math.Max(midY - imgH / 2, 0), imgW, imgH);
                }
                // bottom left
                else if (position.Row == 2 && position.Col == 0)
                {
                    double midX = pageW / 6, midY = pageH - pageH / 6;
                    return imgH <= cellH
                        ? new Rect(Math.Max(midX - imgW / 2, 0), midY - imgH / 2, imgW, imgH)
                        : new Rect(Math.Max(midX - imgW / 2, 0), Math.Max(pageH - imgH, 0), imgW, imgH);
                }
                // bottom middle
                else if (position.Row == 2 && position.Col == 1)
                {
                    double midX = pageW / 2, midY = pageH - pageH / 6;
                    return imgH <= cellH
                        ? new Rect(Math.Max(midX - imgW / 2, 0), midY - imgH / 2, imgW, imgH)
                        : new Rect(Math.Max(midX - imgW / 2, 0), Math.Max(pageH - imgH, 0), imgW, imgH);
                }
                // bottom right
                else
                {
                    double midX = pageW - pageW / 6, midY = pageH - pageH / 6;
                    if (imgW <= cellW)
                    {
                        return imgH <= cellH
                            ? new Rect(midX - imgW / 2, midY - imgH / 2, imgW, imgH)
                            : new Rect(midX - imgW / 2, Math.Max(pageH - imgH, 0), imgW, imgH);
                    }
                    else
                    {
                        return imgH <= cellH
                            ? new Rect(Math.Max(pageW - imgW, 0), midY - imgH / 2, imgW, imgH)
                            : new Rect(Math.Max(pageW - imgW, 0), Math.Max(pageH - imgH, 0), imgW, imgH);
                    }
                }
            }

            public void UpdateImageOpacity()
            {
                if (_viewModel.UseImageIsChecked && _viewModel.SourceImage != null)
                {
                    _imageVisual.Opacity = 1 - _viewModel.ImageOpacity * 1.0 / 100;
                }
            }

            public void ShowImage(bool isShow)
            {
                if (isShow)
                {
                    if (_visuals.IndexOf(_imageVisual) == -1)
                    {
                        _imageVisual = CreateImageVisual();
                        if (_imageVisual != null)
                        {
                            _imageVisual.Opacity = 1 - _viewModel.ImageOpacity * 1.0 / 100;

                            if (_viewModel.UseTextIsChecked && _viewModel.IsTextOverImage)
                            {
                                _visuals.Insert(0, _imageVisual);
                            }
                            else
                            {
                                _visuals.Add(_imageVisual);
                            }
                        }
                    }
                }
                else
                {
                    if (_visuals.IndexOf(_imageVisual) != -1)
                    {
                        _visuals.Remove(_imageVisual);
                    }
                }
            }

            private DrawingVisual CreateTextVisual()
            {
                if (!_viewModel.UseTextIsChecked)
                {
                    return null;
                }

                var text = _viewModel.Content;
                var time = DateTime.Now;

                if (_viewModel.DateStampIsChecked)
                {
                    text += " " + time.ToString("d");
                }

                if (_viewModel.TimeStampIsChecked)
                {
                    text += " " + time.ToString("t");
                }

                if (string.IsNullOrEmpty(text))
                {
                    return null;
                }

                var visual = new DrawingVisual();
                var context = visual.RenderOpen();

                Typeface typeface;
                switch (_viewModel.WmrkFontStyle)
                {
                    case WzFontStyle.Bold:
                        typeface = new Typeface(new FontFamily(_viewModel.FontFamily), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal, new FontFamily("Arial"));
                        break;
                    case WzFontStyle.Italic:
                        typeface = new Typeface(new FontFamily(_viewModel.FontFamily), FontStyles.Italic, FontWeights.Normal, FontStretches.Normal, new FontFamily("Arial"));
                        break;
                    case WzFontStyle.All:
                        typeface = new Typeface(new FontFamily(_viewModel.FontFamily), FontStyles.Italic, FontWeights.Bold, FontStretches.Normal, new FontFamily("Arial"));
                        break;
                    default:
                        typeface = new Typeface(new FontFamily(_viewModel.FontFamily), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal, new FontFamily("Arial"));
                        break;
                }

                var fontColorBrush = new SolidColorBrush(new Color { A = 255, R = _viewModel.FontColor.R, B = _viewModel.FontColor.B, G = _viewModel.FontColor.G });

                context.PushClip(new RectangleGeometry(new Rect(0, 0, _view.ResultImage.ActualWidth, _view.ResultImage.ActualHeight)));
                context.PushTransform(new ScaleTransform(_scaleX, _scaleY));

                var formattedText = new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, _viewModel.FontSize, fontColorBrush);

                var scaledPoint = GetTextStartPoint(formattedText.Width * _scaleX, formattedText.Height * _scaleY);
                var startPoint = new Point(scaledPoint.X / _scaleX, scaledPoint.Y / _scaleY);
                // WPF coordinate system is different from aspose's
                context.PushTransform(new RotateTransform(-_viewModel.TextAngle, startPoint.X, startPoint.Y));

                context.DrawText(formattedText, startPoint);

                context.Close();

                return visual;
            }

            private Point GetTextStartPoint(double textWidth, double textHeight)
            {
                double imageWidth = _view.ResultImage.ActualWidth;
                double imageHeight = _view.ResultImage.ActualHeight;

                var position = _viewModel.TextPosition;
                // WPF coordinate system is different from aspose's
                float angle = -_viewModel.TextAngle;
                double sqrt2 = Math.Sqrt(2);

                double sqrtTextWidth = textWidth / sqrt2;
                double sqrtTextHeight = textHeight / sqrt2;

                double cellWidth = imageWidth / 3;
                double cellHeight = imageHeight / 3;

                // top left
                if (position.Col == 0 && position.Row == 0)
                {
                    double midX = imageWidth / 6;
                    double midY = imageHeight / 6;
                    if (angle == 0)
                    {
                        return new Point(Math.Max(midX - textWidth / 2, 0), midY - textHeight / 2);
                    }
                    else if (angle == 90)
                    {
                        return new Point(midX + textHeight / 2, Math.Max(midY - textWidth / 2, 0));
                    }
                    else if (angle == 45)
                    {
                        return new Point(Math.Max(midX - sqrtTextWidth, sqrtTextHeight), Math.Max(midY - sqrtTextWidth, 0));
                    }
                    else if (angle == -45)
                    {
                        return sqrtTextWidth <= cellWidth
                            ? new Point(Math.Max(midX - sqrtTextWidth / 2, 0), Math.Min(midY + sqrtTextWidth, midY))
                            : new Point(0, Math.Min(sqrtTextWidth, imageWidth));
                    }
                }
                // top middle 
                else if (position.Row == 0 && position.Col == 1)
                {
                    double midX = imageWidth / 2;
                    double midY = imageHeight / 6;
                    if (angle == 0)
                    {
                        return new Point(Math.Max(midX - textWidth / 2, 0), midY - textHeight / 2);
                    }
                    else if (angle == 90)
                    {
                        return new Point(midX + textHeight / 2, Math.Max(midY - textWidth / 2, 0));
                    }
                    else if (angle == 45)
                    {
                        return sqrtTextWidth <= imageWidth
                            ? new Point(midX - sqrtTextWidth / 2, midY - sqrtTextWidth / 2)
                            : new Point(midX - imageWidth / 2, 0);
                    }
                    else if (angle == -45)
                    {
                        return sqrtTextWidth <= imageWidth
                            ? new Point(midX - sqrtTextWidth / 2, midY + sqrtTextWidth / 2)
                            : new Point(midX - imageWidth / 2, imageWidth);
                    }
                }
                // top right
                else if (position.Row == 0 && position.Col == 2)
                {
                    double midX = imageWidth - imageWidth / 6;
                    double midY = imageHeight / 6;
                    if (angle == 0)
                    {
                        return midX + textWidth / 2 <= imageWidth
                            ? new Point(midX - textWidth / 2, midY - textHeight / 2)
                            : new Point(Math.Max(imageWidth - textWidth, 0), midY - textHeight / 2);
                    }
                    else if (angle == 90)
                    {
                        return new Point(midX + textHeight / 2, Math.Max(midY - textWidth / 2, 0));
                    }
                    else if (angle == 45)
                    {
                        return sqrtTextWidth <= cellWidth
                            ? new Point(midX - sqrtTextWidth / 2, Math.Max(midY - sqrtTextWidth / 2, 0))
                            : new Point(Math.Max(imageWidth - sqrtTextWidth, 0), 0);
                    }
                    else if (angle == -45)
                    {
                        return sqrtTextWidth <= cellWidth
                            ? new Point(midX - sqrtTextWidth / 2, midY + sqrtTextWidth / 2)
                            : new Point(Math.Max(imageWidth - sqrtTextWidth, 0), Math.Min(sqrtTextWidth, imageWidth));
                    }
                }
                // middle left
                else if (position.Row == 1 && position.Col == 0)
                {
                    double midX = imageWidth / 6;
                    double midY = imageHeight / 2;
                    if (angle == 0)
                    {
                        return new Point(Math.Max(midX - textWidth / 2, 0), midY - textHeight / 2);
                    }
                    else if (angle == 90)
                    {
                        return new Point(midX + textHeight / 2, Math.Max(midY - textWidth / 2, 0));
                    }
                    else if (angle == 45)
                    {
                        return sqrtTextWidth <= cellWidth
                            ? new Point(Math.Max(midX - sqrtTextWidth / 2, sqrtTextHeight), midY - sqrtTextWidth / 2)
                            : new Point(sqrtTextHeight, Math.Max(midY - sqrtTextWidth / 2, midY - imageWidth / 2));
                    }
                    else if (angle == -45)
                    {
                        return sqrtTextWidth <= cellWidth
                            ? new Point(midX - sqrtTextWidth / 2, midY + sqrtTextWidth / 2)
                            : new Point(0, Math.Min(midY + sqrtTextWidth / 2, midY + imageWidth / 2));
                    }
                }
                // center
                else if (position.Row == 1 && position.Col == 1)
                {
                    double midX = imageWidth / 2;
                    double midY = imageHeight / 2;
                    if (angle == 0)
                    {
                        return new Point(Math.Max(midX - textWidth / 2, 0), midY - textHeight / 2);
                    }
                    else if (angle == 90)
                    {
                        return new Point(midX + textHeight / 2, Math.Max(midY - textWidth / 2, 0));
                    }
                    else if (angle == 45)
                    {
                        return new Point(Math.Max(midX - sqrtTextWidth / 2, sqrtTextHeight), Math.Max(midY - sqrtTextWidth / 2, midY - imageWidth / 2));
                    }
                    else if (angle == -45)
                    {
                        return new Point(Math.Max(midX - sqrtTextWidth / 2, 0), Math.Min(midY + sqrtTextWidth / 2, midY + imageWidth / 2));
                    }
                }
                // middle right
                else if (position.Row == 1 && position.Col == 2)
                {
                    double midX = imageWidth - imageWidth / 6;
                    double midY = imageHeight / 2;
                    if (angle == 0)
                    {
                        return textWidth <= cellWidth
                            ? new Point(midX - textWidth / 2, midY - textHeight / 2)
                            : new Point(Math.Max(imageWidth - textWidth, 0), midY - textHeight / 2);
                    }
                    else if (angle == 90)
                    {
                        return new Point(midX + textHeight / 2, Math.Max(midY - textWidth / 2, 0));
                    }
                    else if (angle == 45)
                    {
                        return sqrtTextWidth <= cellWidth
                            ? new Point(midX - sqrtTextWidth / 2, midY - sqrtTextWidth / 2)
                            : new Point(Math.Max(imageWidth - sqrtTextWidth, sqrtTextHeight), Math.Max(midY - sqrtTextWidth / 2, midY - imageWidth / 2));
                    }
                    else if (angle == -45)
                    {
                        return sqrtTextWidth <= cellWidth
                            ? new Point(midX - sqrtTextWidth / 2, midY + sqrtTextWidth / 2)
                            : new Point(Math.Max(imageWidth - sqrtTextWidth, sqrtTextHeight), Math.Min(midY + sqrtTextWidth / 2, midY + imageWidth / 2));
                    }
                }
                // bottom left
                else if (position.Row == 2 && position.Col == 0)
                {
                    double midX = imageWidth / 6;
                    double midY = imageHeight - imageHeight / 6;
                    if (angle == 0)
                    {
                        return new Point(Math.Max(midX - sqrtTextWidth / 2, 0), midY - textHeight / 2);
                    }
                    else if (angle == 90)
                    {
                        return textWidth <= cellHeight
                            ? new Point(midX + textHeight / 2, midY - textWidth / 2)
                            : new Point(midX + textHeight / 2, Math.Max(imageHeight - textWidth, 0));
                    }
                    else if (angle == 45)
                    {
                        return sqrtTextWidth <= cellWidth
                            ? new Point(Math.Max(midX - sqrtTextWidth / 2, sqrtTextHeight), midY - sqrtTextWidth / 2)
                            : new Point(sqrtTextHeight, Math.Max(imageHeight - sqrtTextWidth, imageHeight - imageWidth));
                    }
                    else if (angle == -45)
                    {
                        return new Point(Math.Max(midX - sqrtTextWidth / 2, sqrtTextHeight), Math.Min(midY + sqrtTextWidth / 2, imageHeight - sqrtTextHeight));
                    }
                }
                // bottom middle
                else if (position.Row == 2 && position.Col == 1)
                {
                    double midX = imageWidth / 2;
                    double midY = imageHeight - imageHeight / 6;
                    if (angle == 0)
                    {
                        return new Point(midX - textWidth / 2, midY - textHeight / 2);
                    }
                    else if (angle == 90)
                    {
                        return textWidth <= cellHeight
                            ? new Point(midX + textHeight / 2, midY - textWidth / 2)
                            : new Point(midX + textHeight / 2, Math.Max(imageHeight - textWidth, 0));
                    }
                    else if (angle == 45)
                    {
                        return sqrtTextWidth <= cellWidth
                            ? new Point(midX - sqrtTextWidth / 2, midY - sqrtTextWidth / 2)
                            : new Point(midX - Math.Min(sqrtTextWidth, imageWidth) / 2 + sqrtTextHeight, Math.Max(imageHeight - sqrtTextWidth, imageHeight - imageWidth));
                    }
                    else if (angle == -45)
                    {
                        return sqrtTextWidth <= cellHeight
                            ? new Point(midX - sqrtTextWidth / 2, midY + sqrtTextWidth / 2)
                            : new Point(midX - Math.Min(sqrtTextWidth, imageWidth) / 2, imageHeight - sqrtTextHeight);
                    }
                }
                // bottom right
                else
                {
                    double midX = imageWidth - imageWidth / 6;
                    double midY = imageHeight - imageHeight / 6;
                    if (angle == 0)
                    {
                        return textWidth <= cellWidth
                            ? new Point(midX - textWidth / 2, midY - textHeight / 2)
                            : new Point(Math.Max(imageWidth - textWidth, 0), midY - textHeight / 2);
                    }
                    else if (angle == 90)
                    {
                        return textWidth <= cellHeight
                            ? new Point(midX + textHeight / 2, midY - textWidth / 2)
                            : new Point(midX + textHeight / 2, Math.Max(imageHeight - textWidth, 0));
                    }
                    else if (angle == 45)
                    {
                        return sqrtTextWidth <= cellWidth
                            ? new Point(midX - sqrtTextWidth / 2, midY - sqrtTextWidth / 2)
                            : new Point(Math.Max(imageWidth - sqrtTextWidth, 0), imageHeight - Math.Min(sqrtTextWidth, imageWidth));
                    }
                    else if (angle == -45)
                    {
                        return sqrtTextWidth <= cellWidth
                            ? new Point(midX - sqrtTextWidth / 2, midY + sqrtTextWidth / 2)
                            : new Point(Math.Max(imageWidth - sqrtTextWidth, 0), imageHeight - sqrtTextHeight);
                    }
                }

                return new Point(0, 0);
            }

            private DrawingVisual CreateImageVisual()
            {
                if (!_viewModel.UseImageIsChecked || _viewModel.SourceImage == null)
                {
                    return null;
                }

                var visual = new DrawingVisual();
                var context = visual.RenderOpen();
                context.PushClip(new RectangleGeometry(new Rect(0, 0, _view.ResultImage.ActualWidth, _view.ResultImage.ActualHeight)));
                context.PushTransform(new ScaleTransform(_scaleX, _scaleY));

                double imgW = _viewModel.SourceImage.Width;
                double imgH = _viewModel.SourceImage.Height;

                double pageW = _view.ResultCanvas.ActualWidth / _scaleX;
                double pageH = _view.ResultCanvas.ActualHeight / _scaleY;

                var rect = GetImageRect(imgW, imgH, pageW, pageH);
                context.DrawImage(_viewModel.SourceImage, rect);
                context.Close();
                return visual;
            }

            protected override Visual GetVisualChild(int index)
            {
                return _visuals[index];
            }

            protected override int VisualChildrenCount => _visuals.Count;
        }
    }
}
