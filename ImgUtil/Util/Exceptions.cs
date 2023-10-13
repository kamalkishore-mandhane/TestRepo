using System;
using System.IO;

namespace ImgUtil.Util
{
    internal enum ExceptionHandlerType
    {
        Normal,
        FileOperation
    }

    internal class AsposeBadException : Exception
    {
        public AsposeBadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    internal class BadImageDimensionException : Exception
    {
        public BadImageDimensionException(int width, int height)
            : base()
        {
            Width = width;
            Height = height;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }
    }

    internal class InvalidImageException : IOException
    {
        public InvalidImageException(string filePath)
            : base()
        {
            ImagePath = filePath;
        }

        public InvalidImageException(string filePath, string message, Exception innerException)
            : base(message, innerException)
        {
            ImagePath = filePath;
        }

        public string ImagePath { get; private set; }
    }

    internal class ImageInUseException : IOException
    {
        public ImageInUseException(string message, Exception innerException)
        {
        }
    }

    internal class ImageNotFoundException : IOException
    {
        public ImageNotFoundException(string filePath)
            : base()
        {
            ImagePath = filePath;
        }

        public string ImagePath { get; private set; }
    }

    internal class ImageNotSupportException : IOException
    {
        public ImageNotSupportException(string filePath)
            : base()
        {
            ImagePath = filePath;
        }

        public string ImagePath { get; private set; }
    }

    internal class ImageReadOnlyException : IOException
    {
        public ImageReadOnlyException(string filePath)
            : base()
        {
            ImagePath = filePath;
        }

        public string ImagePath { get; private set; }
    }

    internal class NotCalledByWinZipException : Exception
    {
        public NotCalledByWinZipException()
        {

        }
    }

    internal class NoOpenedImageException : Exception
    {
        public NoOpenedImageException()
        {

        }
    }

    internal class OperationNotSupportException : Exception
    {
        public OperationNotSupportException()
        {

        }
    }

    internal class SaveLocationNoAccessException : Exception
    {
        public SaveLocationNoAccessException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
