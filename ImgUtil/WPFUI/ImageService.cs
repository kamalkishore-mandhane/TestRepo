using ImgUtil.Util;
using ImgUtil.WPFUI.Model;
using ImgUtil.WPFUI.Utils;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImgUtil.WPFUI
{
    internal class ImageService
    {
        #region Fields

        private Dispatcher _uiDispatcher;
        public ImageFile _currentImageFile;

        #endregion

        #region Contructors

        public ImageService(Dispatcher dispatcher) => _uiDispatcher = dispatcher;

        #endregion

        #region Public Properties used for ImageService object

        public bool HasImageOpened => _currentImageFile != null;

        public string CurrentImageName => _currentImageFile?.Name;

        public string CurrentImageLocalPath => _currentImageFile?.LocalPath;

        public bool IsCurrentImageExtensionMatchFormat => _currentImageFile?.IsFileExtentionMatchFormat ?? false;

        public Aspose.Imaging.FileFormat CurrentImageFormat => _currentImageFile?.Format ?? Aspose.Imaging.FileFormat.Undefined;

        public BitmapSource CurrentPreviewImage { get; private set; } = null;

        #endregion

        #region Public Functions used for ImageService object

        public void LoadImage(string path)
        {
            _currentImageFile = ImageFileFactory.CreateImageFromPath(path);
            if (_currentImageFile != null)
            {

                _currentImageFile.Load();
                if (_currentImageFile is ILazyImage lazyImage)
                {
                    lazyImage.LoadPreview();
                }

                // Check Dimension
                var preview = _currentImageFile.PreviewImage;
                const int maxRenderPixelSize = 16000;
                if (preview != null && (preview.PixelHeight > maxRenderPixelSize || preview.PixelWidth > maxRenderPixelSize))
                {
                    throw new BadImageDimensionException(preview.PixelWidth, preview.PixelHeight);
                }

                ReloadPreviewForWPF();
            }
        }

        public void UnloadCurrentImage()
        {
            (_currentImageFile as ILazyImage)?.Unload();
        }

        public void ClearPreviousImage()
        {
            _currentImageFile = null;
            _uiDispatcher.Invoke(() => CurrentPreviewImage = null);
        }

        public void SaveCurrentImage(string path)
        {
            try
            {
                string destPath = path;
                if (!IsCurrentImageExtensionMatchFormat)
                {
                    string tempFolder = FileOperation.Instance.CreateTempFolder();
                    destPath = Path.Combine(tempFolder, Path.GetFileNameWithoutExtension(path) + ImageHelper.GetFileExtensionsFromFormat(CurrentImageFormat)[0]);
                }

                _currentImageFile.Save(destPath);
                ReloadPreviewForWPF();

                if (IsCurrentImageExtensionMatchFormat)
                {
                    EDPHelper.SyncEnterpriseId(CurrentImageLocalPath, destPath);
                }
                else
                {
                    string tempFile = Path.ChangeExtension(destPath, Path.GetExtension(path));
                    File.Move(destPath, tempFile);
                    EDPHelper.FileCopy(tempFile, path, true);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                throw new SaveLocationNoAccessException(e.Message, e);
            }
            catch (Exception e)
            {
                // Aspose.Imaging.dll v20.3 save webp file after cropped into 1*1, it will thrwo exception.
                // It may be a bug in Aspose.Imaging.dll v20.3, other format does not have this issue.
                // Also, aspose cannot rotate or crop low resolution image, like 1*13, 2*7....
                // we only cancel operation when this exception occurs make Image Manager a little more stable, however, save is not successful.
                var isAsposeImg = _currentImageFile.GetType().IsDefined(typeof(AsposeImageAttribute), false);
                if (isAsposeImg && (e.Message == "ConvertRowsToUv assign pixel error!" || e.Message == "Can't rotate image." || e.Message == "Can't crop image."))
                {
                    throw new AsposeBadException(e.Message, e);
                }
                else
                {
                    throw;
                }
            }
        }

        public void RotateCurrentImageRight()
        {
            _uiDispatcher.Invoke(() => CurrentPreviewImage = new TransformedBitmap(CurrentPreviewImage, new RotateTransform(90)));
            _currentImageFile.RotateRight();
        }

        public void RotateCurrentImageLeft()
        {
            _uiDispatcher.Invoke(() => CurrentPreviewImage = new TransformedBitmap(CurrentPreviewImage, new RotateTransform(-90)));
            _currentImageFile.RotateLeft();
        }

        public void CropCurrentImage(Int32Rect rect)
        {
            _uiDispatcher.Invoke(() => CurrentPreviewImage = new CroppedBitmap(CurrentPreviewImage, rect));
            _currentImageFile.Crop(rect);
        }

        public bool CheckImageSupportCapatility(ImageCapability cap)
        {
            return (_currentImageFile.Capabilities & cap) != 0;
        }

        public string SaveCurrentImageToTempFolder(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                string tempFolder = FileOperation.Instance.CreateTempFolder();
                string tempPath = Path.Combine(tempFolder, CurrentImageName);
                SaveCurrentImage(tempPath);
                return tempPath;
            }
            else
            {
                SaveCurrentImage(path);
                return path;
            }
        }

        public bool SaveCurrentImageToDifferentFormat(string sourceFilePath)
        {
            var tempImage = ImageFileFactory.CreateImageFromPath(sourceFilePath);
            if (tempImage == null)
            {
                return false;
            }

            var type = tempImage.GetType();
            if (type.IsDefined(typeof(DotNetSupportedImageAttribute), false))
            {
                return _uiDispatcher.Invoke(() => DotNetSupportedImageAttribute.SaveToDifferentFormat(tempImage, CurrentPreviewImage));
            }
            else if (type.IsDefined(typeof(AsposeImageAttribute), false))
            {
                return _uiDispatcher.Invoke(() => AsposeImageAttribute.SaveToDifferentFormat(tempImage, CurrentPreviewImage));
            }

            return false;
        }

        #endregion

        #region Public External Static Functions

        public static bool ExternalCheckImageSupportCapatility(ImageFile imageFile, ImageCapability cap)
        {
            return (imageFile.Capabilities & cap) != 0;
        }

        public static bool ExternalCheckImageFileValid(ImageFile imageFile, ref bool isIOException)
        {
            try
            {
                imageFile.Load();
            }
            catch (Exception e)
            {
                isIOException = e is IOException;
                return false;
            }
            return true;
        }

        public static void ExternalSaveImage(ImageFile imageFile, string path)
        {
            try
            {
                if (!imageFile.IsLoaded)
                {
                    imageFile.Load();
                }

                string destPath = path;
                if (!imageFile.IsFileExtentionMatchFormat)
                {
                    string tempFolder = FileOperation.Instance.CreateTempFolder();
                    destPath = Path.Combine(tempFolder, Path.GetFileNameWithoutExtension(path) + ImageHelper.GetFileExtensionsFromFormat(imageFile.Format)[0]);
                }

                imageFile.Save(destPath);

                if (imageFile.IsFileExtentionMatchFormat)
                {
                    EDPHelper.SyncEnterpriseId(imageFile.LocalPath, destPath);
                }
                else
                {
                    string tempFile = Path.ChangeExtension(destPath, Path.GetExtension(path));
                    File.Move(destPath, tempFile);
                    EDPHelper.FileCopy(tempFile, path, true);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                throw new SaveLocationNoAccessException(e.Message, e);
            }
            catch (Exception e)
            {
                // Aspose.Imaging.dll v20.3 save webp file after cropped into 1*1, it will thrwo exception.
                // It may be a bug in Aspose.Imaging.dll v20.3, other format does not have this issue.
                // Also, aspose cannot rotate or crop low resolution image, like 1*13, 2*7....
                // we only cancel operation when this exception occurs make Image Manager a little more stable, however, save is not successful.
                var isAsposeImg = imageFile.GetType().IsDefined(typeof(AsposeImageAttribute), false);
                if (isAsposeImg && (e.Message == "ConvertRowsToUv assign pixel error!" || e.Message == "Can't rotate image." || e.Message == "Can't crop image."))
                {
                    throw new AsposeBadException(e.Message, e);
                }
                else
                {
                    throw;
                }
            }
        }

        public static void ExternalRotateImageRight(ImageFile imageFile)
        {
            if (!imageFile.IsLoaded)
            {
                imageFile.Load();
            }
                
            imageFile.RotateRight();
        }

        public static void ExternalRotateImageLeft(ImageFile imageFile)
        {
            if (!imageFile.IsLoaded)
            {
                imageFile.Load();
            }

            imageFile.RotateLeft();
        }

        public static BitmapSource ExternalGetPreviewImageFromPath(string path)
        {
            var temp = ImageFileFactory.CreateImageFromPath(path);
            if (temp != null)
            {

                temp.Load();
                if (temp is ILazyImage lazyImage)
                {
                    lazyImage.LoadPreview();
                    lazyImage.Unload();
                }

                var previewImage = new CroppedBitmap(temp.PreviewImage, new Int32Rect(0, 0, temp.PreviewImage.PixelWidth, temp.PreviewImage.PixelHeight));
                previewImage.Freeze();
                return previewImage;
            }

            return null;
        }

        public static void ExternalUnloadImage(ImageFile imageFile)
        {
            (imageFile as ILazyImage)?.Unload();
        }

        #endregion

        #region Private Helper Functions

        private void ReloadPreviewForWPF()
        {
            _uiDispatcher.Invoke(() =>
            {
                CurrentPreviewImage = new CroppedBitmap(_currentImageFile.PreviewImage, new Int32Rect(0, 0, _currentImageFile.PreviewImage.PixelWidth, _currentImageFile.PreviewImage.PixelHeight));
            });
        }

        #endregion
    }
}
