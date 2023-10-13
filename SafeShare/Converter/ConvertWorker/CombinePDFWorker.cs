using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using WXFCMBPDF;

namespace SafeShare.Converter.ConvertWorker
{
    internal class CombinePDFWorker : BaseConvertWorker
    {
        private static readonly string[] CombinPdfType = { ".pdf" };

        private List<string> _srcFileList = new List<string>();
        private List<string> _originalFielList = new List<string>();
        private List<string> _deleteOriginalFileList = new List<string>();
        private List<string> _deletelFileList = new List<string>();
        private string _destinationDir = string.Empty;
        private string _lastErrorInfo = string.Empty;
        private Func<int, bool> _progressFunc;

        private WXFCMBPDFService _wXFCMBPDFService;

        private bool _removeOriginFiles;
        private bool _useWipe;
        private string _newPdfName = string.Empty;

        private ConverWorkerManager _workMaganer;

        public CombinePDFWorker(ConverWorkerManager manager, List<string> sourceFiles, string destDir, List<string> originalFielList, Func<int, bool> progressFunc)
        {
            _workMaganer = manager;
            _srcFileList = sourceFiles;
            _originalFielList = originalFielList;
            _destinationDir = destDir;
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

        public bool RemoveOriginFiles
        {
            set
            {
                _removeOriginFiles = value;
            }
        }

        public bool UseWipe
        {
            set
            {
                _useWipe = value;
            }
        }

        public string NewPdfName
        {
            set
            {
                _newPdfName = value;
            }
        }

        public static bool IsFileTypeSupportedByTransform(string file)
        {
            if (CombinPdfType.Contains(Path.GetExtension(file).ToLower()))
            {
                return true;
            }

            return false;
        }

        public static string[] SupportFileType()
        {
            return CombinPdfType;
        }

        public void Transform()
        {
            if (_wXFCMBPDFService == null)
            {
                _wXFCMBPDFService = new WXFCMBPDFService();
                if (!_wXFCMBPDFService.InitLicense())
                {
                    return;
                }
            }

            ProgressIndex++;
            var combinfile = string.Empty;
            var skipZipFileList = new List<string>();
            string cmbDestDir = string.Empty;

            foreach (var item in _srcFileList)
            {
                if (IsFileTypeSupportedByTransform(item) && _wXFCMBPDFService.IsFileValid(item))
                {
                    combinfile += item + ";";
                    _deletelFileList.Add(item);

                    string tmpCmbDestDir = Path.GetDirectoryName(Path.Combine(_destinationDir, item));
                    if (cmbDestDir == string.Empty)
                    {
                        cmbDestDir = tmpCmbDestDir;
                    }
                    else if (tmpCmbDestDir.Length < cmbDestDir.Length && cmbDestDir.Contains(tmpCmbDestDir))
                    {
                        cmbDestDir = tmpCmbDestDir;
                    }
                }
                else if (IsFileTypeSupportedByTransform(item) && !_wXFCMBPDFService.IsFileValid(item))
                {
                    string tmpOutputFile = Path.Combine(cmbDestDir, _newPdfName);

                    if (!HandleConvertErrorResult(tmpOutputFile, skipZipFileList, item))
                    {
                        return;
                    }
                }
            }

            string outputFile = Path.Combine(cmbDestDir, _newPdfName);

            try
            {
                _wXFCMBPDFService.SetDestinationFolderName(cmbDestDir);
                _wXFCMBPDFService.SetOutputFilePath(outputFile);
                _wXFCMBPDFService.SetSourceFile(combinfile);
                _wXFCMBPDFService.SetIsDeleteOriginals(_removeOriginFiles);
                _wXFCMBPDFService.SetIsWipeFIles(_useWipe);
                _wXFCMBPDFService.CreatePDFOrderList();

                int ret = _wXFCMBPDFService.Transform(IntPtr.Zero);

                if (ret == 0)
                {
                    _srcFileList.Add(outputFile);

                    if (_removeOriginFiles)
                    {
                        foreach (var item in _originalFielList)
                        {
                            if (ConvertToPDFWorker.IsFileTypeSupportedByTransform(item) || (IsFileTypeSupportedByTransform(item) && _wXFCMBPDFService.IsFileValid(item)))
                            {
                                _deleteOriginalFileList.Add(item);
                            }
                        }

                        if (_useWipe)
                        {
                            string deleteFiles = string.Empty;
                            foreach (var item in _deleteOriginalFileList)
                            {
                                if (File.Exists(item))
                                {
                                    deleteFiles += item + ";";
                                }
                            }

                            WinzipMethods.WipeFiles(deleteFiles);
                        }
                        else
                        {
                            try
                            {
                                foreach (var item in _deleteOriginalFileList)
                                {
                                    if (File.Exists(item))
                                    {
                                        File.Delete(item);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                _workMaganer.ConvertParams.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                                {
                                    SimpleMessageWindows.DisplayWarningConfirmationMessage(e.Message);
                                }));
                            }
                        }
                    }
                }
                else
                {
                    bool res = HandleConvertErrorResult(outputFile, skipZipFileList, string.Empty);
                    if (!res)
                    {
                        return;
                    }
                }
            }
            catch
            {
                if (!HandleConvertErrorResult(outputFile, skipZipFileList, string.Empty))
                {
                    return;
                }
            }

            _progressFunc(ProgressIndex);

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

            foreach (var file in _deletelFileList)
            {
                if (_srcFileList.Contains(file))
                {
                    _srcFileList.Remove(file);
                }
            }
        }

        private bool HandleConvertErrorResult(string outputFile, List<string> skipZipFileList, string errorFile)
        {
            Func<CONVERT_ERROR_DIALOG, bool> handleErrorFunc = choiceRet =>
            {
                var file = Path.GetDirectoryName(outputFile);
                if (!string.IsNullOrEmpty(errorFile))
                {
                    file = errorFile;
                }
                else
                {
                    file = Path.Combine(file, _wXFCMBPDFService.GetErrorFiles());
                }

                var fileArr = file.Split(new string[] { ".pdf;" }, StringSplitOptions.None);
                foreach (var item in fileArr)
                {
                    var fileName = item;
                    if (!fileName.EndsWith(".pdf", true, null))
                    {
                        fileName += ".pdf";
                    }

                    if (File.Exists(fileName))
                    {
                        if (choiceRet == CONVERT_ERROR_DIALOG.Continue)
                        {
                            SkipConvertButZipFileList.Add(fileName);
                        }
                        else
                        {
                            skipZipFileList.Add(file);
                        }
                    }
                }

                return true;
            };

            if (_workMaganer.NotShowConvertErrorAgain)
            {
                handleErrorFunc(_workMaganer.ConvertErrorChoice);
            }
            else
            {
                var choice = CONVERT_ERROR_DIALOG.Cancel;
                var notShowAgain = false;

                _workMaganer.ConvertParams.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    HandleConvertError(Parent, Properties.Resources.CONVERSION_COMBINE_PDF_NAME, string.IsNullOrEmpty(errorFile) ? _wXFCMBPDFService.GetErrorFiles() : Path.GetFileName(errorFile), _wXFCMBPDFService.GetErrorCode(),
                        _wXFCMBPDFService.GetErrorDescription(), out choice, out notShowAgain);
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
            }

            return true;
        }
    }
}