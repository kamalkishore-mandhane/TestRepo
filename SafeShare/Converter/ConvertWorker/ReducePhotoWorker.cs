using SafeShare.Converter.ConvertUtil;
using SafeShare.WPFUI.View;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace SafeShare.Converter.ConvertWorker
{
    internal class ReducePhotoWorker : BaseConvertWorker
    {
        private static readonly string[] ReduceImageType = { ".bmp", ".dib", ".exif", ".gif", ".jpeg", ".jpg", ".jfif", ".jpe", ".png", ".tif", ".tiff" };

        private int _width;
        private int _height;
        private List<string> _srcFileList = new List<string>();
        private string _destDir = string.Empty;
        private Func<int, bool> _progressFunc;

        private ConverWorkerManager _workMaganer;

        public ReducePhotoWorker(ConverWorkerManager manager, List<string> sourceFiles, string destDir, Func<int, bool> progressFunc)
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

        public int Width
        {
            private get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        public int Height
        {
            private get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }

        public int ProgressIndex
        {
            get;
            set;
        }

        public void Transform()
        {
            var skipZipFileList = new List<string>();

            foreach (var item in _srcFileList)
            {
                try
                {
                    ProgressIndex++;
                    if (IsFileTypeSupportedByTransform(item))
                    {
                        var bResize = false;
                        long errorCode = 0;
                        byte[] byteOutput = new byte[512];

                        IMGVWRService.ResizeImage(item, item, Width, Height, ref bResize, ref errorCode, ref byteOutput[0]);

                        string lastErrorInfo = Encoding.Unicode.GetString(byteOutput).TrimEnd('\0');

                        if (!string.IsNullOrEmpty(lastErrorInfo))
                        {
                            bool res = HandleConvertErrorResult(item, skipZipFileList, lastErrorInfo);

                            if (!res)
                            {
                                return;
                            }
                        }
                    }
                }
                catch
                {
                    if (!HandleConvertErrorResult(item, skipZipFileList, string.Empty))
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

        public static bool IsFileTypeSupportedByTransform(string file)
        {
            if (ReduceImageType.Contains(Path.GetExtension(file).ToLower()))
            {
                return true;
            }

            return false;
        }

        public static string[] SupportFileType()
        {
            return ReduceImageType;
        }

        public Image DoResizeImage(string OriginalFileLocation, int height, int width)
        {
            var FullsizeImage = Image.FromFile(OriginalFileLocation);

            if (height >= FullsizeImage.Height && width >= FullsizeImage.Width)
            {
                return null;
            }

            FullsizeImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(RotateFlipType.Rotate180FlipNone);

            double newWidth = width;
            double newHeight = height;
            if (FullsizeImage.Width > FullsizeImage.Height)
            {
                double ratio = (double)width / (double)FullsizeImage.Width;
                newHeight = FullsizeImage.Height * ratio;
                if (newHeight > height)
                {
                    newHeight = height;
                    ratio = height / (double)FullsizeImage.Height;
                    newWidth = FullsizeImage.Width * ratio;
                }
            }
            else
            {
                double ratio = (double)height / (double)FullsizeImage.Height;
                newWidth = FullsizeImage.Width * ratio;
                if (newWidth > width)
                {
                    newWidth = width;
                    ratio = width / (double)FullsizeImage.Width;
                    newHeight = FullsizeImage.Height * ratio;
                }
            }

            return FullsizeImage.GetThumbnailImage((int)newWidth, (int)newHeight, null, IntPtr.Zero);
        }

        public Image ResizeImage(string OriginalFileLocation, int height, int width)
        {
            return DoResizeImage(OriginalFileLocation, height, width);
        }

        private bool HandleConvertErrorResult(string file, List<string> skipZipFileList, string lastErrorInfo)
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
                HandleConvertError(Parent, Properties.Resources.CONVERSION_REDUCE_PHOTO_NAME, Path.GetFileName(file), 0,
                    lastErrorInfo, out choice, out notShowAgain);
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