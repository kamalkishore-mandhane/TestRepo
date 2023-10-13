using Aspose.Imaging.FileFormats.Jpeg2000;
using Aspose.Imaging.ImageOptions;
using Aspose.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ImgUtil.WPFUI.Model.ImageFiles
{
    [AsposeImage]
    class Jpeg2000File : ImageFile , ILazyImage
    {
        private Jpeg2000Image _image;
        private Rectangle _croppedRect;
        private Int32Rect _imageRect;

        public Jpeg2000File(string localPath, Aspose.Imaging.FileFormat format)
            : base(localPath, format)
        {
            IsPreviewLoaded = false;
        }

        public override ImageCapability Capabilities => base.Capabilities & ~(ImageCapability.SupportSetDesktopBG | ImageCapability.SupportAddToTeamsBG | ImageCapability.SupportRemoveData | ImageCapability.SupportResize | ImageCapability.SupportWatermark);

        public bool IsPreviewLoaded { get; private set; }

        public override void Crop(Int32Rect rect)
        {
            if (IsLoaded)
            {
                _croppedRect.X += rect.X;
                _croppedRect.Y += rect.Y;
                _croppedRect.Width = rect.Width;
                _croppedRect.Height = rect.Height;
                _isCropped = true;
            }
        }

        public override void Save(string path)
        {
            if (IsLoaded)
            {
                if ((_rotation != 0) && (_rotation % 4 != 0))
                {
                    _image.Rotate(_rotation * 90);
                    if (IsPreviewLoaded)
                    {
                        _previewImage = new TransformedBitmap(_previewImage, new RotateTransform(_rotation * 90));
                    }
                }
                _rotation = 0;

                if (_isCropped)
                {
                    _image.Crop(_croppedRect);
                    if (IsPreviewLoaded)
                    {
                        _previewImage = new CroppedBitmap(_previewImage, new Int32Rect(_croppedRect.X, _croppedRect.Y, _croppedRect.Width, _croppedRect.Height));
                    }
                }

                _croppedRect = new Rectangle(0, 0, _image.Width, _image.Height);
                _imageRect = new Int32Rect(0, 0, _image.Width, _image.Height);
                _isCropped = false;

                if (IsPreviewLoaded)
                {
                    _previewImage = BitmapFrame.Create(_previewImage);
                    _previewImage.Freeze();
                }
                
                _image.Save(path);
            }
        }

        public override void RotateLeft()
        {
            if (IsLoaded)
            {
                if (_isCropped)
                {
                    _croppedRect = new Rectangle(_croppedRect.Y, _imageRect.Width - _croppedRect.X - _croppedRect.Width, _croppedRect.Height, _croppedRect.Width);
                }

                int temp = _imageRect.Width;
                _imageRect.Width = _imageRect.Height;
                _imageRect.Height = temp;

                _rotation -= 1;
            }
        }

        public override void RotateRight()
        {
            if (IsLoaded)
            {
                if (_isCropped)
                {
                    _croppedRect = new Rectangle(_imageRect.Height - _croppedRect.Y - _croppedRect.Height, _croppedRect.X, _croppedRect.Height, _croppedRect.Width);
                }

                int temp = _imageRect.Width;
                _imageRect.Width = _imageRect.Height;
                _imageRect.Height = temp;

                _rotation += 1;
            }
        }

        public void LoadPreview()
        {
            var stream = new MemoryStream();
            _image.Save(stream, new PngOptions { ResolutionSettings = new ResolutionSetting(ImageHelper.CurrentDpi_X, ImageHelper.CurrentDpi_Y) });
            _previewImage = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
            RecreatePreviewImage();
            IsPreviewLoaded = true;
        }

        public void Unload()
        {
            if (!_image?.Disposed ?? false)
            {
                _image.Dispose();
            }
        }

        public override void Load()
        {
            try
            {
                _image = Image.Load(_localPath) as Jpeg2000Image;
            }
            catch (System.UnauthorizedAccessException)
            {
                using (FileStream stream = File.Open(_localPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _image = Image.Load(stream) as Jpeg2000Image;
                }
            }

            _croppedRect = new Rectangle(0, 0, _image.Width, _image.Height);
            _imageRect = new Int32Rect(0, 0, _image.Width, _image.Height);

            IsLoaded = true;
        }
    }
}
