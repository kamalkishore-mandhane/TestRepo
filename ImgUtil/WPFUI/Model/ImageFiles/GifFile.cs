using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImgUtil.WPFUI.Model.ImageFiles
{
    [DotNetSupportedImage(false)]
    class GifFile : ImageFile
    {
        private Int32Rect _croppedRect;
        private Int32Rect _imageRect;

        public GifFile(string localPath, Aspose.Imaging.FileFormat format)
            : base(localPath, format)
        {
        }

        public override ImageCapability Capabilities
        {
            get
            {
                var caps = base.Capabilities & ~ImageCapability.SupportAddToTeamsBG;
                if (ImageHelper.IsWin7)
                {
                    caps &= ~ImageCapability.SupportSetDesktopBG;
                }
                return caps;
            }
        }

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
                var imageToSave = _previewImage;

                if ((_rotation != 0) && (_rotation % 4 != 0))
                {
                    imageToSave = new TransformedBitmap(imageToSave, new RotateTransform(_rotation * 90));
                }
                _rotation = 0;

                if (_isCropped)
                {
                    imageToSave = new CroppedBitmap(imageToSave, _croppedRect);
                }
                _isCropped = false;

                using (var fs = File.Open(path, FileMode.Create))
                {
                    var encoder = new GifBitmapEncoder();
                    imageToSave = BitmapFrame.Create(imageToSave);
                    encoder.Frames.Add(imageToSave as BitmapFrame);
                    encoder.Save(fs);
                }

                _previewImage = imageToSave;
                _previewImage.Freeze();

                _imageRect = new Int32Rect(0, 0, _previewImage.PixelWidth, _previewImage.PixelHeight);
                _croppedRect = new Int32Rect(0, 0, _previewImage.PixelWidth, _previewImage.PixelHeight);
            }
        }

        public override void RotateLeft()
        {
            if (IsLoaded)
            {
                if (_isCropped)
                {
                    _croppedRect = new Int32Rect(_croppedRect.Y, _imageRect.Width - _croppedRect.X - _croppedRect.Width, _croppedRect.Height, _croppedRect.Width);
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
                    _croppedRect = new Int32Rect(_imageRect.Height - _croppedRect.Y - _croppedRect.Height, _croppedRect.X, _croppedRect.Height, _croppedRect.Width);
                }

                int temp = _imageRect.Width;
                _imageRect.Width = _imageRect.Height;
                _imageRect.Height = temp;

                _rotation += 1;
            }
        }

        public override void Load()
        {
            _previewImage = new GifBitmapDecoder(new Uri(_localPath), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
            RecreatePreviewImage();

            _imageRect = new Int32Rect(0, 0, _previewImage.PixelWidth, _previewImage.PixelHeight);
            _croppedRect = new Int32Rect(0, 0, _previewImage.PixelWidth, _previewImage.PixelHeight);

            IsLoaded = true;
        }
    }
}
