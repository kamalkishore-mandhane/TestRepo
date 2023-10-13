using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using WXFD2P;

namespace SafeShare.Converter.ConvertWorker
{
    internal class ConvertToPDFWorker : BaseConvertWorker
    {
        private static readonly string[] D2pSupportType = { ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".ccitt", ".gif", ".jpg", ".jpeg", ".jpe", ".jfif", ".png", ".tiff", ".tif", ".bmp", ".dib", ".emf", ".exif", ".icon", ".ico", ".wmf", ".tex", ".txt", ".rtf", ".xps", ".htm", ".html" };

        private List<string> _srcFileList = new List<string>();
        private string _destDir = string.Empty;
        private string _lastErrorInfo = string.Empty;
        private Func<int, bool> _progressFunc;

        private WXFD2PService _wXFD2PService;

        private int _resolution;
        private int _quality;
        private bool _makePdfReadonly;
        private bool _removeComments;

        private ConverWorkerManager _workMaganer;

        public ConvertToPDFWorker(ConverWorkerManager manager, List<string> sourceFiles, string destDir, Func<int, bool> progressFunc)
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

        public int Resolution
        {
            set
            {
                _resolution = value;
            }
        }

        public int Quality
        {
            set
            {
                _quality = value;
            }
        }

        public bool MakePdfReadonly
        {
            set
            {
                _makePdfReadonly = value;
            }
        }

        public bool RemoveComments
        {
            set
            {
                _removeComments = value;
            }
        }

        public List<string> GetResultFiles()
        {
            return _srcFileList;
        }

        public static bool IsFileTypeSupportedByTransform(string file)
        {
            if (D2pSupportType.Contains(Path.GetExtension(file).ToLower()))
            {
                return true;
            }

            return false;
        }

        public static string[] SupportFileType()
        {
            return D2pSupportType;
        }

        public void Transform()
        {
            if (_wXFD2PService == null)
            {
                _wXFD2PService = new WXFD2PService();
                if (!_wXFD2PService.InitLicense())
                {
                    return;
                }
            }

            var removeFileList = new List<string>();
            var newResultFileList = new List<string>();
            var skipZipFileList = new List<string>();

            foreach (var item in _srcFileList)
            {
                try
                {
                    ProgressIndex++;
                    if (IsFileTypeSupportedByTransform(item))
                    {
                        string outputFile = Path.Combine(_destDir, item + ".pdf");

                        if (File.Exists(outputFile))
                        {
                            var name = Path.GetFileNameWithoutExtension(outputFile);
                            name += string.Format(@"{0:x4}.pdf", FileOperation.LOWORD(DateTime.Now.Ticks));
                            outputFile = Path.Combine(_destDir, name);
                        }

                        _wXFD2PService.SetSourceFile(item);
                        _wXFD2PService.SetPDFResolution((_resolution + 1) * 300);
                        _wXFD2PService.SetJPEGQuality((_quality + 1) * 50);

                        bool ret = _wXFD2PService.Transform(outputFile, _makePdfReadonly, _removeComments);

                        if (ret && File.Exists(outputFile))
                        {
                            removeFileList.Add(item);
                            newResultFileList.Add(outputFile);
                        }
                        else
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

            foreach (var file in removeFileList)
            {
                if (_srcFileList.Contains(file))
                {
                    _srcFileList.Remove(file);
                }
            }

            foreach (var file in newResultFileList)
            {
                _srcFileList.Add(file);
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
                return true;
            }

            var choice = CONVERT_ERROR_DIALOG.Cancel;
            var notShowAgain = false;

            _workMaganer.ConvertParams.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                HandleConvertError(Parent, Properties.Resources.CONVERSION_CONVERT_TO_PDF_NAME, Path.GetFileName(file), _wXFD2PService.GetErrorCode(),
                _wXFD2PService.GetErrorDescription(), out choice, out notShowAgain);
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