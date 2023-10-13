using SafeShare.WPFUI.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using WXFRMPD;

namespace SafeShare.Converter.ConvertWorker
{
    internal class RemovePersonalDataWorker : BaseConvertWorker
    {
        private static readonly string[] RemovePDSupportType = { ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".pdf", ".gif", ".jpg", ".jpeg", ".jfif", ".png", ".psd", ".tiff", "tif" };

        private WXFRMPDService _rmpdService;
        private List<int> _metadataOptions;

        private List<string> _srcFileList = new List<string>();
        private string _destDir = string.Empty;
        private string _lastErrorInfo = string.Empty;
        private Func<int, bool> _progressFunc;

        private ConverWorkerManager _workMaganer;

        public RemovePersonalDataWorker(ConverWorkerManager manager, List<string> sourceFiles, string destDir, Func<int, bool> progressFunc)
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

        public static bool IsFileTypeSupportedByTransform(string file)
        {
            if (RemovePDSupportType.Contains(Path.GetExtension(file).ToLower()))
            {
                return true;
            }

            return false;
        }

        public static string[] SupportFileType()
        {
            return RemovePDSupportType;
        }

        public List<int> MetadataOptions
        {
            set
            {
                _metadataOptions = value;
            }
        }

        public void Transform()
        {
            if (_rmpdService == null)
            {
                _rmpdService = new WXFRMPDService();
                if (!_rmpdService.InitLicense())
                {
                    //SetLastError(_rmpdService.GetErrorDescription());
                    return;
                }
            }

            _rmpdService.SetRemoveOption(_metadataOptions.ToArray());
            var skipZipFileList = new List<string>();

            foreach (var item in _srcFileList)
            {
                try
                {
                    ProgressIndex++;
                    if (IsFileTypeSupportedByTransform(item))
                    {
                        _rmpdService.SetSourceFile(item);
                        bool ret = _rmpdService.Transform(item);

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
                return true;
            }

            var choice = CONVERT_ERROR_DIALOG.Cancel;
            var notShowAgain = false;

            _workMaganer.ConvertParams.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                HandleConvertError(Parent, Properties.Resources.CONVERSION_REMOVE_PERSONAL_DATA_NAME, Path.GetFileName(file), _rmpdService.GetErrorCode(),
                    _rmpdService.GetErrorDescription(), out choice, out notShowAgain);
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