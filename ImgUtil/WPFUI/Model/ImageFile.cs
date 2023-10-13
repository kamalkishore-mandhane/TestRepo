using Aspose.Imaging;
using ImgUtil.Util;
using ImgUtil.WPFUI.Utils;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImgUtil.WPFUI.Model
{
    [Flags]
    internal enum ImageCapability
    {
        None = 0,
        SupportConvert = 1,
        SupportRemoveData = 2,
        SupportResize = 4,
        SupportCrop = 8,
        SupportRotate = 16,
        SupportWatermark = 32,
        SupportAddToTeamsBG = 64,
        SupportSetDesktopBG = 128,
        All = 255
    }

    internal abstract class ImageFile
    {
        protected string _name;
        protected string _localPath;
        protected FileFormat _format;
        protected BitmapSource _previewImage;
        protected string _lowerExtWithDot;
        protected int _rotation;
        protected bool _isCropped;

        public ImageFile(string localPath, FileFormat format)
        {
            _localPath = localPath;
            _name = Path.GetFileName(localPath);
            _lowerExtWithDot = Path.GetExtension(_name).ToLower();
            _format = format;
            _isCropped = false;
            _rotation = 0;
            IsLoaded = false;
            IsFileExtentionMatchFormat = ImageHelper.GetFileExtensionsFromFormat(format).Contains(_lowerExtWithDot);
        }

        public string Name => _name;

        public string LocalPath => _localPath;

        public FileFormat Format => _format;

        public bool IsLoaded { get; protected set; }

        public virtual ImageCapability Capabilities => ImageCapability.All;

        public bool IsFileExtentionMatchFormat { get; private set; }

        public virtual BitmapSource PreviewImage => _previewImage;

        public abstract void Load();

        public abstract void Crop(Int32Rect rect);

        public abstract void Save(string path);

        public abstract void RotateLeft();

        public abstract void RotateRight();

        protected void RecreatePreviewImage()
        {
            if (this.GetType().IsDefined(typeof(DotNetSupportedImageAttribute), false))
            {
                bool isExifDataContainer = (Attribute.GetCustomAttribute(this.GetType(), typeof(DotNetSupportedImageAttribute)) as DotNetSupportedImageAttribute)?.ExifDataContainer ?? false;
                if (isExifDataContainer && (_previewImage.Metadata is BitmapMetadata metadata && metadata.ContainsQuery("System.Photo.Orientation")))
                {
                    // Respect EXIF orientation
                    var orientation = metadata.GetQuery("System.Photo.Orientation");
                    if (orientation != null)
                    {
                        switch ((ushort)orientation)
                        {
                            case 2: // Mirror horizontal
                                {
                                    _previewImage = new TransformedBitmap(_previewImage, new ScaleTransform(-1, 1));
                                    break;
                                }

                            case 3: // Rotate 180 degrees
                                {
                                    _previewImage = new TransformedBitmap(_previewImage, new RotateTransform(180));
                                    break;
                                }

                            case 4: // Mirror vertical
                                {
                                    _previewImage = new TransformedBitmap(new TransformedBitmap(_previewImage, new RotateTransform(180)), new ScaleTransform(-1, 1));
                                    break;
                                }

                            case 5: // Mirror horizontal and rotate 270 degrees
                                {
                                    _previewImage = new TransformedBitmap(new TransformedBitmap(_previewImage, new RotateTransform(-90)), new ScaleTransform(1, -1));
                                    break;
                                }

                            case 6: // Rotate 90 degrees
                                {
                                    _previewImage = new TransformedBitmap(_previewImage, new RotateTransform(90));
                                    break;
                                }

                            case 7: // Mirror horizontal and rotate 90 degrees
                                {
                                    _previewImage = new TransformedBitmap(new TransformedBitmap(_previewImage, new RotateTransform(90)), new ScaleTransform(1, -1));
                                    break;
                                }

                            case 8: // Rotate 270 degrees
                                {
                                    _previewImage = new TransformedBitmap(_previewImage, new RotateTransform(-90));
                                    break;
                                }

                            case 1: // Normal
                            default:
                                break;
                        }
                    }
                }
            }

            // there might be some bugs with BitmapFrame in current version's .net(3.5)
            if (_previewImage is BitmapFrame)
            {
                _previewImage = new CroppedBitmap(_previewImage, new Int32Rect(0, 0, _previewImage.PixelWidth, _previewImage.PixelHeight));
            }
            _previewImage.Freeze();
        }
    }

    internal class DotNetSupportedImageAttribute : Attribute
    {
        public DotNetSupportedImageAttribute(bool isExifDataContainer)
        {
            ExifDataContainer = isExifDataContainer;
        }

        public bool ExifDataContainer { get; private set; }

        public static bool SaveToDifferentFormat(ImageFile destImageFile, BitmapSource source)
        {
            using (var fs = File.Open(destImageFile.LocalPath, FileMode.Create))
            {
                BitmapEncoder encoder = null;
                switch (destImageFile.Format)
                {
                    case FileFormat.Bmp:
                        encoder = new BmpBitmapEncoder();
                        break;

                    case FileFormat.Gif:
                        encoder = new GifBitmapEncoder();
                        break;

                    case FileFormat.Jpeg:
                        encoder = new JpegBitmapEncoder();
                        break;

                    case FileFormat.Png:
                        encoder = new PngBitmapEncoder();
                        break;

                    case FileFormat.Tiff:
                        encoder = new TiffBitmapEncoder();
                        break;

                    default:
                        return false;
                }
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(fs);
            }
            return true;
        }
    }

    internal class AsposeImageAttribute : Attribute
    {
        public static bool SaveToDifferentFormat(ImageFile destImageFile, BitmapSource source)
        {
            string tempFolder = FileOperation.Instance.CreateTempFolder();
            string tempPath = Path.Combine(tempFolder, $"{DateTime.Now.Ticks}.png");

            using (var fs = File.Open(tempPath, FileMode.Create))
            {
                var pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(source));
                pngEncoder.Save(fs);
            }

            if (!File.Exists(tempPath))
            {
                return false;
            }

            using (var image = Aspose.Imaging.Image.Load(tempPath))
            {
                Aspose.Imaging.ImageOptionsBase option = null;
                switch (destImageFile.Format)
                {
                    case FileFormat.Jpeg2000:
                        option = new Aspose.Imaging.ImageOptions.Jpeg2000Options();
                        break;

                    case FileFormat.Psd:
                        option = new Aspose.Imaging.ImageOptions.PsdOptions();
                        break;

                    case FileFormat.Svg:
                        option = new Aspose.Imaging.ImageOptions.SvgOptions();
                        ((Aspose.Imaging.ImageOptions.SvgOptions)option).VectorRasterizationOptions = new Aspose.Imaging.ImageOptions.SvgRasterizationOptions
                        {
                            PageWidth = image.Width,
                            PageHeight = image.Height
                        };
                        break;

                    case FileFormat.Webp:
                        option = new Aspose.Imaging.ImageOptions.WebPOptions();
                        break;

                    default:
                        return false;
                }
                try
                {
                    image.Save(destImageFile.LocalPath, option);
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
                    if (e.Message == "ConvertRowsToUv assign pixel error!" || e.Message == "Can't rotate image." || e.Message == "Can't crop image.")
                    {
                        throw new AsposeBadException(e.Message, e);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return true;
        }
    }

    public interface ILazyImage
    {
        bool IsPreviewLoaded { get; }

        void LoadPreview();

        void Unload();
    }
}
