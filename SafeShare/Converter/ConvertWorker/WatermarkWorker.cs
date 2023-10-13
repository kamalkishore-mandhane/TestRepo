using SafeShare.WPFUI.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using WXFWMRK;

namespace SafeShare.Converter.ConvertWorker
{
    internal class WatermarkWorker : BaseConvertWorker
    {
        private static readonly string[] WatermarkSupportType = { ".bmp", ".dib", ".pdf", ".jpg", ".jpeg", ".jfif", ".gif", ".png", ".tiff", ".tif" };

        private List<string> _srcFileList = new List<string>();
        private string _destDir = string.Empty;
        private string _lastErrorInfo = string.Empty;
        private Func<int, bool> _progressFunc;

        private WXFWMRKService _watermarkService;

        private bool _timeStamp;
        private bool _dateStamp;
        private string _watermarkContent;
        private int _textAngle;
        private int _textPosition;
        private int _textColor = 0xC0C0C0;
        private int _textOpacity = 25;

        private ConverWorkerManager _workMaganer;

        public WatermarkWorker(ConverWorkerManager manager, List<string> sourceFiles, string destDir, Func<int, bool> progressFunc)
        {
            _workMaganer = manager;
            _srcFileList = sourceFiles;
            _destDir = destDir;
            _progressFunc = progressFunc;
        }

        public IntPtr Parent
        {
            get;
            set;
        }

        public int ProgressIndex
        {
            get;
            set;
        }

        public bool TimeStamp
        {
            set
            {
                _timeStamp = value;
            }
        }

        public bool DateStamp
        {
            set
            {
                _dateStamp = value;
            }
        }

        public string WatermarkContent
        {
            set
            {
                _watermarkContent = value;
            }
        }

        public int TextAngle
        {
            set
            {
                _textAngle = value;
            }
        }

        public int TextPosition
        {
            set
            {
                _textPosition = value;
            }
        }

        public int TextColor
        {
            set
            {
                _textColor = value;
            }
        }

        public int TextOpacity
        {
            set
            {
                _textOpacity = value;
            }
        }

        public static bool IsFileTypeSupportedByTransform(string file)
        {
            if (WatermarkSupportType.Contains(Path.GetExtension(file).ToLower()))
            {
                return true;
            }

            return false;
        }

        public static string[] SupportFileType()
        {
            return WatermarkSupportType;
        }

        public void Transform()
        {
            if (_watermarkService == null)
            {
                _watermarkService = new WXFWMRKService();

                if (!_watermarkService.InitLicense())
                {
                    return;
                }
            }

            var skipZipFileList = new List<string>();

            foreach (var item in _srcFileList)
            {
                try
                {
                    ProgressIndex++;
                    if (IsFileTypeSupportedByTransform(item))
                    {
                        _watermarkService.SetTimeStamp(_timeStamp);
                        _watermarkService.SetDateStamp(_dateStamp);
                        _watermarkService.SetText(_watermarkContent);
                        _watermarkService.SetDirection(_textAngle);
                        _watermarkService.SetLocation(_textPosition);
                        _watermarkService.SetColor(_textColor);
                        _watermarkService.SetOpacity(_textOpacity);

                        bool ret = _watermarkService.AddWatermark(item);

                        if (!ret)
                        {
                            bool res = HandleConvertErrorResult(item, skipZipFileList);
                            if (!res)
                            {
                                return;
                            }
                        }
                    }
                }
                catch
                {
                    if (!HandleConvertErrorResult(item, skipZipFileList))
                    {
                        return;
                    }
                }

                _progressFunc(ProgressIndex);
            }

            foreach (var file in skipZipFileList)
            {
                if (_srcFileList.Contains(file))
                {
                    _srcFileList.Remove(file);
                }
            }

            foreach (var file in SkipConvertButZipFileList)
            {
                if (_srcFileList.Contains(file))
                {
                    _srcFileList.Remove(file);
                }
            }
        }

        private bool HandleConvertErrorResult(string file, List<string> skipZipFileList)
        {
            Func<CONVERT_ERROR_DIALOG, bool> handleErrorFunc = choiceRet =>
            {
                if (choiceRet == CONVERT_ERROR_DIALOG.Continue)
                {
                    SkipConvertButZipFileList.Add(file);
                }
                else
                {
                    skipZipFileList.Add(file);
                }

                return true;
            };

            if (_workMaganer.NotShowConvertErrorAgain)
            {
                handleErrorFunc(_workMaganer.ConvertErrorChoice);
                return true; ;
            }

            var choice = CONVERT_ERROR_DIALOG.Cancel;
            var notShowAgain = false;

            _workMaganer.ConvertParams.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                HandleConvertError(Parent, Properties.Resources.CONVERSION_WATERMARK_NAME, Path.GetFileName(file), _watermarkService.GetErrorCode(),
                    _watermarkService.GetErrorDescription(), out choice, out notShowAgain);
            }));

            if (choice == CONVERT_ERROR_DIALOG.Cancel)
            {
                _workMaganer.ConvertErrorChoice = CONVERT_ERROR_DIALOG.Cancel;
                return false;
            }

            if (notShowAgain)
            {
                _workMaganer.ConvertErrorChoice = choice;
                _workMaganer.NotShowConvertErrorAgain = notShowAgain;
            }

            handleErrorFunc(choice);
            return true;
        }
    }
}