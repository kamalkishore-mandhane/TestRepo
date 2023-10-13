using Aspose.Imaging.FileFormats.Svg;
using Aspose.Imaging.ImageOptions;
using Aspose.Imaging;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImgUtil.WPFUI.Model.ImageFiles
{
    [AsposeImage]
    class SvgFile : ImageFile , ILazyImage
    {
        private SvgImage _image;

        public SvgFile(string localPath, Aspose.Imaging.FileFormat format)
            : base(localPath, format)
        {
            IsPreviewLoaded = false;
        }

        public override ImageCapability Capabilities => base.Capabilities & ~(ImageCapability.SupportSetDesktopBG | ImageCapability.SupportAddToTeamsBG |
            ImageCapability.SupportRemoveData | ImageCapability.SupportResize | ImageCapability.SupportCrop | ImageCapability.SupportRotate | ImageCapability.SupportWatermark);

        public bool IsPreviewLoaded { get; private set; }

        public override void Crop(Int32Rect rect)
        {
            throw new NotSupportedException();
        }

        public override void Save(string path)
        {
            if (IsLoaded)
            {
                _image.Save(path);
            }
        }

        public override void RotateLeft()
        {
            throw new NotSupportedException();
        }

        public override void RotateRight()
        {
            throw new NotSupportedException();
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
                _image = Image.Load(_localPath) as SvgImage;
            }
            catch (System.UnauthorizedAccessException)
            {
                using (FileStream stream = File.Open(_localPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _image = Image.Load(stream) as SvgImage;
                }
            }
            IsLoaded = true;
        }
    }
}
