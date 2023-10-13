using Aspose.Imaging;
using ImgUtil.WPFUI.Model.ImageFiles;

namespace ImgUtil.WPFUI.Model
{
    static class ImageFileFactory
    {
        public static ImageFile CreateImageFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var format = FileFormat.Undefined;
            try
            {
                format = ImageHelper.GetImageRealFormatFromPath(path);
                if (format == FileFormat.Undefined)
                {
                    format = ImageHelper.GetImageFormatFromPath(path);
                }
            }
            catch
            {
                format = ImageHelper.GetImageFormatFromPath(path);
            }

            switch (format)
            {
                case FileFormat.Bmp:
                    return new BmpFile(path, format);

                case FileFormat.Gif:
                    return new GifFile(path, format);

                case FileFormat.Jpeg:
                    return new JpegFile(path, format);

                case FileFormat.Jpeg2000:
                    return new Jpeg2000File(path, format);

                case FileFormat.Png:
                    return new PngFile(path, format);

                case FileFormat.Psd:
                    return new PsdFile(path, format);

                case FileFormat.Svg:
                    return new SvgFile(path, format);

                case FileFormat.Tiff:
                    return new TiffFile(path, format);

                case FileFormat.Webp:
                    return new WebpFile(path, format);

                default:
                    return null;
            }
        }
    }
}
