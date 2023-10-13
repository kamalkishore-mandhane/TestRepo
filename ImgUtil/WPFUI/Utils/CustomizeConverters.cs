using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ImgUtil.WPFUI.Utils
{
    class IndexToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is uint index && index < 10)
            {
                return index.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(bool), typeof(double))]
    public class EnabledToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object paramter, CultureInfo curture)
        {
            return System.Convert.ToBoolean(value) ? 1 : 0.5;
        }

        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityValueConverter : IValueConverter
    {
        /// <summary>
        /// If true, result of the conversion can be Visible or Hidden,
        /// otherwise Visible or Collapsed.
        /// </summary>
        public bool UseHiddenState { set; get; }

        private bool _invert = false;
        public bool Invert
        {
            set { _invert = value; }
            get { return _invert; }
        }

        //=====================================================================================
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility notVisible;
            if (UseHiddenState || parameter != null)
            {
                notVisible = Visibility.Hidden;
            }
            else
            {
                notVisible = Visibility.Collapsed;
            }
            if (Invert)
                return (bool)value ? notVisible : Visibility.Visible;
            else
                return (bool)value ? Visibility.Visible : notVisible;
        }

        //=====================================================================================
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Invert)
                return (Visibility)value != Visibility.Visible;
            else
                return (Visibility)value == Visibility.Visible;
        }
    }

    [ValueConversion(typeof(int), typeof(Visibility))]
    public class IntToVisibilityConverter : IValueConverter
    {
        private bool _invert = false;
        public bool Invert
        {
            set { _invert = value; }
            get { return _invert; }
        }

        public object Convert(object value, Type targetType, object paramter, CultureInfo curture)
        {
            if (Invert)
            {
                return System.Convert.ToInt32(value) == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return System.Convert.ToInt32(value) != 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(int), typeof(Visibility))]
    public class ContentToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object paramter, CultureInfo curture)
        {
            if (value is null)
            {
                return Visibility.Hidden;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(long), typeof(string))]
    public class FileSizeConverter : IValueConverter
    {
        public const int SFBS_FLAGS_ROUND_TO_NEAREST_DISPLAYED_DIGIT = 1;

        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern bool StrFormatByteSize(long size, StringBuilder sb, int len);

        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrFormatByteSizeEx(long size, int flag, StringBuilder sb, int len);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                long size = System.Convert.ToInt64(value);
                return StrFormatByteSize(size);
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static string StrFormatByteSize(long size)
        {
            StringBuilder sb = new StringBuilder(24);
            try
            {
                if (StrFormatByteSizeEx(size < 0 ? -size : size, SFBS_FLAGS_ROUND_TO_NEAREST_DISPLAYED_DIGIT, sb, 24) == 0)
                {
                    if (size < 0)
                    {
                        sb.Insert(0, '-');
                    }
                    return sb.ToString();
                }
            }
            catch
            {
                if (StrFormatByteSize(size, sb, 24))
                {
                    return sb.ToString();
                }
            }
            return null;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToBoolean(value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ItemSourceCountFilterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var itemSource = value as IEnumerable;
            if (itemSource == null)
                return value;

            int maxItems = 10;
            if (parameter != null)
                int.TryParse(parameter as string, out maxItems);

            if (maxItems < 1)
                return value;

            var filteredItemSource = new List<object>();
            foreach (var item in itemSource)
            {
                if (filteredItemSource.Count >= maxItems)
                    break;

                filteredItemSource.Add(item);
            }

            return filteredItemSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
