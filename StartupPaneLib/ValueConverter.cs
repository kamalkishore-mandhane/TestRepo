using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace StartupPaneLib
{
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

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToBoolean(value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
