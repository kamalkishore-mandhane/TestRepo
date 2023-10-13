using SafeShare.Properties;
using System.IO;
using System.Reflection;

namespace SafeShare.WPFUI.ViewModel
{
    internal enum WzFontStyle
    {
        Regular = 1,
        Bold = 2,
        Italic = 3,
        All = 4,
    }

    public enum ConvertType
    {
        WATERMARK,
        REMOVE_PERSONAL_DATA,
        COMBINE_INTO_ONE_PDF,
        CONVERT_TO_PDF,
        REDUCE_PHOTOS = 4
    }

    public class ConvertOptViewModels : ObservableObject
    {
        private string _convertTypeText;
        //Watermark settings
        private const string DefaultFontFamilyName = "Arial";

        private const int DefaultOpacity = 25;
        private const int DefaultFontSize = 36;
        private string _convertSummary = string.Empty;

        private bool _addWatermark = false;
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
        private int _textOpacity = 25;
        private string _content = string.Empty;
        private bool _dateStampIsChecked = false;
        private bool _timeStampIsChecked = false;
        private string _fontFamily = string.Empty;
        private int _fontSize = 0;

        //Convert to PDF settings
        private bool _converttoPDF = false;

        private bool _removeCommentsIsChecked = false;
        private bool _removeMarkupIsChecked = false;
        private bool _makePdfReadonly = false;
        private int _resolution = 1;
        private int _quality = 8;

        //Combine into one PDF settings
        private bool _combineIntoOnePDF = false;

        private bool _deleteOriginalFiles = false;
        private bool _isUseWipe = false;
        private string _newPdfName = string.Empty;
        private bool _showFileNameErrorTips = false;
        private string _fileNameErrorTips = string.Empty;

        //Remove personal data settings
        private bool _removePersonalSetting = false;

        //Reduce photos settings
        private int _reducePhotos = 0;
        private bool _reduce640x480isChecked = false;
        private bool _reduce800x600isChecked = false;
        private bool _reduce1024x768isChecked = false;
        private bool _reduce1280x1024isChecked = false;
        private bool _reduce1600x1200isChecked = false;
        private bool _reduce1920x1080isChecked = false;

        public ConvertOptViewModels()
        {
            OldPdfName = string.Empty;
            IsCustomName = false;
        }

        public string ConvertTypeText
        {
            get
            {
                return _convertTypeText;
            }
            set
            {
                if (_convertTypeText != value)
                {
                    _convertTypeText = value;
                    Notify(nameof(ConvertTypeText));
                }
            }
        }

        public string OldPdfName { get; set; }

        public bool IsCustomName { get; set; }

        public int TextColor = 0xC0C0C0;

        public int TextAngle
        {
            get
            {
                if (TextAngle0IsChecked) //direction 4 (-45 degree)
                {
                    return 0;
                }
                else if (TextAngle1IsChecked) //direction 1 (0 degree)
                {
                    return 90;
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

        public int Resolution
        {
            get => _resolution;
            set
            {
                if (_resolution != value)
                {
                    _resolution = value;
                    Notify(nameof(Resolution));
                }
            }
        }

        public int ReducePhotosHeight
        {
            get
            {
                if (Reduce1600x1200isChecked)
                {
                    return 1200;
                }

                if (Reduce1024x768isChecked)
                {
                    return 768;
                }

                if (Reduce1280x1024isChecked)
                {
                    return 1024;
                }

                if (Reduce1920x1080isChecked)
                {
                    return 1080;
                }

                if (Reduce800x600isChecked)
                {
                    return 600;
                }

                return 480;
            }
        }

        public int ReducePhotoWidth
        {
            get
            {
                if (Reduce1600x1200isChecked)
                {
                    return 1600;
                }

                if (Reduce1024x768isChecked)
                {
                    return 1024;
                }

                if (Reduce1280x1024isChecked)
                {
                    return 1280;
                }

                if (Reduce1920x1080isChecked)
                {
                    return 1920;
                }

                if (Reduce800x600isChecked)
                {
                    return 800;
                }

                return 640;
            }
        }

        public int TextPosition
        {
            get
            {
                if (TextLocation0IsChecked)
                {
                    return 0;
                }
                else if (TextLocation1IsChecked)
                {
                    return 1;
                }
                else if (TextLocation2IsChecked)
                {
                    return 2;
                }
                else if (TextLocation3IsChecked)
                {
                    return 3;
                }
                else if (TextLocation4IsChecked)
                {
                    return 4;
                }
                else if (TextLocation5IsChecked)
                {
                    return 5;
                }
                else if (TextLocation6IsChecked)
                {
                    return 6;
                }
                else if (TextLocation7IsChecked)
                {
                    return 7;
                }
                else if (TextLocation8IsChecked)
                {
                    return 8;
                }

                return -1;
            }
        }

        [Obfuscation(Exclude = true)]
        public bool AddWatermark
        {
            get => _addWatermark;
            set
            {
                if (_addWatermark != value)
                {
                    _addWatermark = value;
                    Notify(nameof(AddWatermark));
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
                    Notify(nameof(TextOpacity));
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
                    Notify(nameof(TimeStampIsChecked));
                }
            }
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
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ConvertToPDF
        {
            get => _converttoPDF;
            set
            {
                if (_converttoPDF != value)
                {
                    _converttoPDF = value;
                    Notify(nameof(ConvertToPDF));
                    Notify(nameof(ConvertToPdfOpt));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool RemoveCommentsIsChecked
        {
            get => _removeCommentsIsChecked;
            set
            {
                if (_removeCommentsIsChecked != value)
                {
                    _removeCommentsIsChecked = value;
                    Notify(nameof(RemoveCommentsIsChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool RemoveMarkupIsChecked
        {
            get => _removeMarkupIsChecked;
            set
            {
                if (_removeMarkupIsChecked != value)
                {
                    _removeMarkupIsChecked = value;
                    Notify(nameof(RemoveMarkupIsChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool CombineIntoOnePDF
        {
            get => _combineIntoOnePDF;
            set
            {
                if (_combineIntoOnePDF != value)
                {
                    _combineIntoOnePDF = value;
                    Notify(nameof(CombineIntoOnePDF));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public int ReducePhotosResolution
        {
            get
            {
                if (Reduce640x480isChecked)
                {
                    return 0;
                }
                else if (Reduce800x600isChecked)
                {
                    return 1;
                }
                else if (Reduce1024x768isChecked)
                {
                    return 2;
                }
                else if (Reduce1280x1024isChecked)
                {
                    return 3;
                }
                else if (Reduce1600x1200isChecked)
                {
                    return 4;
                }
                else if (Reduce1920x1080isChecked)
                {
                    return 5;
                }
                return 0;
            }
        }

        [Obfuscation(Exclude = true)]
        public int Quality
        {
            get => _quality;
            set
            {
                if (_quality != value)
                {
                    _quality = value;
                    Notify(nameof(Quality));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool MakePdfReadonly
        {
            get => _makePdfReadonly;
            set
            {
                if (_makePdfReadonly != value)
                {
                    _makePdfReadonly = value;
                    Notify(nameof(MakePdfReadonly));
                    Notify(nameof(ConvertToPdfOpt));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ConvertToPdfOpt
        {
            get => _makePdfReadonly && _converttoPDF;
        }

        [Obfuscation(Exclude = true)]
        public bool DeleteOriginalFiles
        {
            get => _deleteOriginalFiles;
            set
            {
                if (_deleteOriginalFiles != value)
                {
                    _deleteOriginalFiles = value;
                    Notify(nameof(DeleteOriginalFiles));
                }

                if (!value)
                {
                    IsUseWipe = false;
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsUseWipe
        {
            get => _isUseWipe;
            set
            {
                if (_isUseWipe != value)
                {
                    _isUseWipe = value;
                    Notify(nameof(IsUseWipe));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public string NewPdfName
        {
            get => _newPdfName;
            set
            {
                if (_newPdfName != value)
                {
                    _newPdfName = value;
                    Notify(nameof(NewPdfName));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool RemovePersonalSetting
        {
            get => _removePersonalSetting;
            set
            {
                if (_removePersonalSetting != value)
                {
                    _removePersonalSetting = value;
                    Notify(nameof(RemovePersonalSetting));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public int ReducePhotos
        {
            get => _reducePhotos;
            set
            {
                if (_reducePhotos != value)
                {
                    _reducePhotos = value;
                    Notify(nameof(ReducePhotos));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool Reduce640x480isChecked
        {
            get => _reduce640x480isChecked;
            set
            {
                if (_reduce640x480isChecked != value)
                {
                    _reduce640x480isChecked = value;
                    Notify(nameof(Reduce640x480isChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool Reduce800x600isChecked
        {
            get => _reduce800x600isChecked;
            set
            {
                if (_reduce800x600isChecked != value)
                {
                    _reduce800x600isChecked = value;
                    Notify(nameof(Reduce800x600isChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool Reduce1024x768isChecked
        {
            get => _reduce1024x768isChecked;
            set
            {
                if (_reduce1024x768isChecked != value)
                {
                    _reduce1024x768isChecked = value;
                    Notify(nameof(Reduce1024x768isChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool Reduce1280x1024isChecked
        {
            get => _reduce1280x1024isChecked;
            set
            {
                if (_reduce1280x1024isChecked != value)
                {
                    _reduce1280x1024isChecked = value;
                    Notify(nameof(Reduce1280x1024isChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool Reduce1600x1200isChecked
        {
            get => _reduce1600x1200isChecked;
            set
            {
                if (_reduce1600x1200isChecked != value)
                {
                    _reduce1600x1200isChecked = value;
                    Notify(nameof(Reduce1600x1200isChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool Reduce1920x1080isChecked
        {
            get => _reduce1920x1080isChecked;
            set
            {
                if (_reduce1920x1080isChecked != value)
                {
                    _reduce1920x1080isChecked = value;
                    Notify(nameof(Reduce1920x1080isChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public string ConvertSummary
        {
            get => _convertSummary;
            set
            {
                if (_convertSummary != value)
                {
                    _convertSummary = value;
                    Notify(nameof(ConvertSummary));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public string FileNameErrorTips
        {
            get => _fileNameErrorTips;
            set
            {
                if (_fileNameErrorTips != value)
                {
                    _fileNameErrorTips = value;
                    Notify(nameof(FileNameErrorTips));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ShowFileNameErrorTips
        {
            get => _showFileNameErrorTips;
            set
            {
                if (_showFileNameErrorTips != value)
                {
                    _showFileNameErrorTips = value;
                    Notify(nameof(ShowFileNameErrorTips));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public int RealTextOpacity => 100 - TextOpacity;

        public void InitConvretSetting(string newPdefName)
        {
            int textLcation = Settings.Default.ConvertOptWatermarkTextPosition;

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

            int angle = Settings.Default.ConvertOptWatermarkAngle;

            switch (angle)
            {
                case 0:
                    TextAngle0IsChecked = true;
                    break;

                case 45:
                    TextAngle2IsChecked = true;
                    break;

                case 90:
                    TextAngle1IsChecked = true;
                    break;

                case -45:
                    TextAngle3IsChecked = true;
                    break;
            }

            int textOpacity = Settings.Default.ConvertOptWatermarkOpacity;

            if (textOpacity == -1)
            {
                textOpacity = DefaultOpacity;
            }

            TextOpacity = textOpacity;
            Content = Settings.Default.ConvertOptWatermarkContent;

            if (string.IsNullOrEmpty(Content))
            {
                Content = Resources.WATERMARK_CONTENT;
            }

            DateStampIsChecked = Settings.Default.ConvertOptWatermarkDateStampIsChecked;
            TimeStampIsChecked = Settings.Default.ConvertOptWatermarkTimeStampIsChecked;
            _fontSize = 12;

            if (_fontSize == 0)
            {
                _fontSize = DefaultFontSize;
            }

            _fontFamily = DefaultFontFamilyName;

            if (_fontFamily == string.Empty)
            {
                _fontFamily = DefaultFontFamilyName;
            }

            switch (Settings.Default.ConvertOptReducePhoteResolution)
            {
                case 0:
                    Reduce640x480isChecked = true;
                    break;

                case 1:
                    Reduce800x600isChecked = true;
                    break;

                case 2:
                    Reduce1024x768isChecked = true;
                    break;

                case 3:
                    Reduce1280x1024isChecked = true;
                    break;

                case 4:
                    Reduce1600x1200isChecked = true;
                    break;

                case 5:
                    Reduce1920x1080isChecked = true;
                    break;

                default:
                    Reduce640x480isChecked = true;
                    break;

            }

            if (!string.IsNullOrEmpty(newPdefName) && !IsCustomName)
            {
                NewPdfName = Path.GetFileNameWithoutExtension(newPdefName) + ".pdf";
                OldPdfName = NewPdfName;
            }

            AddWatermark = Settings.Default.ConvertOptAddWatermark;
            ConvertToPDF = Settings.Default.ConvertOptFilesToPdf;
            RemovePersonalSetting = Settings.Default.ConvertOptRemovePersonalData;
            CombineIntoOnePDF = Settings.Default.ConvertOptCombineIntoOnePdf;
            RemoveCommentsIsChecked = Settings.Default.ConvertOptRemoveComments && ConvertToPDF;
            RemoveMarkupIsChecked = Settings.Default.ConvertOptRemovemarkup && ConvertToPDF; ;
            MakePdfReadonly = Settings.Default.ConvertOptMakePdfReadonly && ConvertToPDF;
            Resolution = Settings.Default.ConvertOptResolution = Resolution;
            Quality = Settings.Default.ConvertOptQuality;
            DeleteOriginalFiles = Settings.Default.ConvertOptDeleteOriginalFiles && CombineIntoOnePDF;
            IsUseWipe = Settings.Default.ConvertOptUseWipeToDeleteFiles && CombineIntoOnePDF;
        }

        public bool SaveConvertOpeSetting()
        {
            //save watermark settings
            Settings.Default.ConvertOptWatermarkContent = Content;
            Settings.Default.ConvertOptWatermarkDateStampIsChecked = DateStampIsChecked;
            Settings.Default.ConvertOptWatermarkTimeStampIsChecked = TimeStampIsChecked;
            Settings.Default.ConvertOptWatermarkTextPosition = TextPosition;
            Settings.Default.ConvertOptWatermarkAngle = TextAngle;
            Settings.Default.ConvertOptWatermarkOpacity = TextOpacity;
            Settings.Default.ConvertOptAddWatermark = AddWatermark;

            // save convert to pdf settings
            Settings.Default.ConvertOptFilesToPdf = ConvertToPDF;
            Settings.Default.ConvertOptRemoveComments = RemoveCommentsIsChecked;
            Settings.Default.ConvertOptRemovemarkup = RemoveMarkupIsChecked;
            Settings.Default.ConvertOptMakePdfReadonly = MakePdfReadonly;
            Settings.Default.ConvertOptResolution = Resolution;
            Settings.Default.ConvertOptQuality = Quality;

            //save rmpd settings
            Settings.Default.ConvertOptRemovePersonalData = RemovePersonalSetting;

            //save combine pdf settings
            Settings.Default.ConvertOptCombineIntoOnePdf = CombineIntoOnePDF;
            Settings.Default.ConvertOptDeleteOriginalFiles = DeleteOriginalFiles;
            Settings.Default.ConvertOptUseWipeToDeleteFiles = IsUseWipe;

            //save reduce photo settings
            Settings.Default.ConvertOptReducePhoteResolution = ReducePhotosResolution;
            Settings.Default.Save();
            return true;
        }
    }
}