using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DupFF.WPFUI.Utils
{
    /// <summary>
    /// MultiDPIIconSource
    /// </summary>
    public class MultiDPIIconSource : MarkupExtension
    {
        private readonly int[] IcoSizes = new[] { 40, 50, 60, 80, 100, 120, 160, 200, 256 };

        /// <summary>
        /// _dpi
        /// </summary>
        private int _dpi;

        private bool _isDisable;

        /// <summary>
        /// MultiDPIIconSource
        /// </summary>
        public MultiDPIIconSource(bool isDisable)
        {
            _dpi = UIFeature.DPI;
            _isDisable = isDisable;
        }

        /// <summary>
        /// ProvideValue
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            int icoSize = 40 * _dpi / 96;
            int select = 0;

            for (int i = 1; i < IcoSizes.Length; i++)
            {
                if (icoSize > (IcoSizes[i - 1] + IcoSizes[i]) / 2)
                {
                    select = i;
                }
            }

            var bmpi = new BitmapImage();

            return (ImageSource)bmpi;
        }
    }

    /// <summary>
    /// The base class for multi-dpi image source.
    /// </summary>
    public abstract class BaseMultiDPIImageSource : MarkupExtension
    {
        /// <summary>
        /// The image sizes.
        /// </summary>
        private readonly int[] imageSizes = new[] { 16, 20, 24, 32, 40, 48, 64, 80 };

        private int _dpi;

        protected bool _isChecked;

        public BaseMultiDPIImageSource()
        {
            _dpi = UIFeature.DPI;
            _isChecked = false;
        }

        /// <summary>
        /// Return releated image with the dpi.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            int imgSize = 16 * _dpi / 96;
            int select = 0;

            for (int i = 1; i < imageSizes.Length; i++)
            {
                if (imgSize > (imageSizes[i - 1] + imageSizes[i]) / 2)
                {
                    select = i;
                }
            }

            Image bmp = null;
            if (_isChecked)
            {
                bmp = LoadCheckedImage(imageSizes[select]);
            }
            else
            {
                bmp = LoadUnCheckedImage(imageSizes[select]);
            }

            var bmpi = new BitmapImage();
            using (var memory = new MemoryStream())
            {
                bmp.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                bmpi.BeginInit();
                bmpi.StreamSource = memory;
                bmpi.CacheOption = BitmapCacheOption.OnLoad;
                bmpi.EndInit();
            }

            return (ImageSource)bmpi;
        }

        /// <summary>
        /// Load checked image.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected abstract Image LoadCheckedImage(int select);

        /// <summary>
        /// Load unchecked image.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected abstract Image LoadUnCheckedImage(int select);
    }

    /// <summary>
    /// MultiDPIImageSource
    /// </summary>
    public class MultiDPIImageSource : BaseMultiDPIImageSource
    {
        /// <summary>
        /// MultiDPIImageSource
        /// </summary>
        /// <param name="isChecked"></param>
        public MultiDPIImageSource(bool isChecked)
            : base()
        {
            _isChecked = isChecked;
        }

        /// <summary>
        /// LoadCheckedImage
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Image LoadCheckedImage(int select)
        {
            switch (select)
            {
                case 16:
                    return Properties.Resources.CheckBoxChecked16;
                case 20:
                    return Properties.Resources.CheckBoxChecked20;
                case 24:
                    return Properties.Resources.CheckBoxChecked24;
                case 32:
                    return Properties.Resources.CheckBoxChecked32;
                case 40:
                    return Properties.Resources.CheckBoxChecked40;
                case 48:
                    return Properties.Resources.CheckBoxChecked48;
                case 64:
                    return Properties.Resources.CheckBoxChecked64;
                case 80:
                    return Properties.Resources.CheckBoxChecked80;
                default:
                    return null;
            }
        }

        /// <summary>
        /// LoadUnCheckedImage
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Image LoadUnCheckedImage(int select)
        {
            switch (select)
            {
                case 16:
                    return Properties.Resources.CheckBoxUnchecked16;
                case 20:
                    return Properties.Resources.CheckBoxUnchecked20;
                case 24:
                    return Properties.Resources.CheckBoxUnchecked24;
                case 32:
                    return Properties.Resources.CheckBoxUnchecked32;
                case 40:
                    return Properties.Resources.CheckBoxUnchecked40;
                case 48:
                    return Properties.Resources.CheckBoxUnchecked48;
                case 64:
                    return Properties.Resources.CheckBoxUnchecked64;
                case 80:
                    return Properties.Resources.CheckBoxUnchecked80;
                default:
                    return null;
            }
        }
    }
}
