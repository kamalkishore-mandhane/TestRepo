using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SafeShare.WPFUI.Controls
{
    internal class FileSizeConverter : IValueConverter
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

        public static string StrFormatByteSize(long size)
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

    internal class InvertBooleanToVisibilityConverter : IValueConverter
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

    internal class GridViewHeaderVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridView)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    internal class StringToVisibilityConverter : IValueConverter
    {
        // If true, result of the conversion is inverted just before returning, otherwise result is returned as is.
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Invert)
            {
                return string.IsNullOrEmpty(System.Convert.ToString(value)) ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return string.IsNullOrEmpty(System.Convert.ToString(value)) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class IndexToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string index && index.Length == 1)
            {
                return index;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Enum), typeof(bool))]
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object paramter, CultureInfo curture)
        {
            if (value == null || !typeof(Enum).IsAssignableFrom(value.GetType()))
            {
                return DependencyProperty.UnsetValue;
            }

            if (paramter == null || !(paramter is string))
            {
                return DependencyProperty.UnsetValue;
            }

            object p = Enum.Parse(value.GetType(), paramter as string);

            if (p.Equals(value))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            return value.Equals(true) ? paramter : Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(int), typeof(bool))]
    public class RatingToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo curture)
        {
            if (value == null || parameter == null)
            {
                return DependencyProperty.UnsetValue;
            }

            int star;
            if (int.TryParse(parameter as string, out star))
            {
                return (int)value >= star;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object paramter, CultureInfo culture)
        {
            return paramter;
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

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class Bool2VisibilityMultiConverter : IMultiValueConverter
    {
        /// <summary>
        /// if true, all bindings have to return true
        /// to make result be true too (AND operation).
        /// Otherwise (if false), at least one binding has to return true
        /// to make result be true too (OR operation).
        /// </summary>
        public bool AndOperation { get; set; }

        /// <summary>
        /// If true, result of the conversion is inverted just before returning,
        /// otherwise result is returned as is.
        /// </summary>
        public bool InvertResult { get; set; }

        /// <summary>
        /// If true, result of the conversion can be Visible or Hidden,
        /// otherwise Visible or Collapsed.
        /// </summary>
        public bool UseHiddenState { get; set; }

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility notVisibleState = UseHiddenState ? Visibility.Hidden : Visibility.Collapsed;

            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("Designed to convert to bool but not to " + targetType.Name);
            if (values == null) throw new ArgumentNullException("values");

            // Validate input
            if (values.Length > 0 && values.Contains(DependencyProperty.UnsetValue))
                return notVisibleState;

            bool result;
            if (AndOperation)
                result = values.Length > 0 && values.Cast<bool>().Contains(false) == false;
            else
                result = values.Length > 0 && values.Cast<bool>().Contains(true);

            if (InvertResult)
                result = !result;

            return result ? Visibility.Visible : notVisibleState;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion IMultiValueConverter Members
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

    [ValueConversion(typeof(double), typeof(double))]
    public class ValueMultiplication : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class ValueAddition : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value) + System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(object), typeof(Visibility))]
    internal class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(object), typeof(Visibility))]
    public class PopupCenterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.FirstOrDefault(v => v == DependencyProperty.UnsetValue) != null)
                {
                    return double.NaN;
                }
                double placementTargetWidth = (double)values[0];
                double popupWidth = (double)values[1];
                return ((placementTargetWidth / 2.0) - (popupWidth / 2.0)) * (SystemParameters.MenuDropAlignment ? -1 : 1);
            }
            catch
            {
                return double.NaN;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(object), typeof(Geometry))]
    public class BorderClipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 && values[0] is double && values[1] is double && values[2] is CornerRadius)
            {
                var width = (double)values[0];
                var height = (double)values[1];

                if (width < double.Epsilon || height < double.Epsilon)
                {
                    return Geometry.Empty;
                }

                var radius = (CornerRadius)values[2];

                List<Point> points = GetPoints(radius, width, height);
                List<PathSegment> segments = new List<PathSegment>();

                AddSegment(segments, points[0], points[1], radius.TopLeft);
                AddSegment(segments, points[2], points[3], radius.BottomLeft);
                AddSegment(segments, points[4], points[5], radius.BottomRight);
                AddSegment(segments, points[6], points[7], radius.TopRight);

                PathFigure figure = new PathFigure(points[0], segments, true);

                Geometry clip = new PathGeometry(new PathFigure[] { figure });
                clip.Freeze();

                return clip;
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private List<Point> GetPoints(CornerRadius radius, double width, double height)
        {
            List<Point> output = new List<Point> {
                new Point(radius.TopLeft, 0),
                new Point(0, radius.TopLeft),
                new Point(0, height - radius.BottomLeft),
                new Point(radius.BottomLeft, height),
                new Point(width - radius.BottomRight, height),
                new Point(width, height - radius.BottomRight),
                new Point(width, radius.TopRight),
                new Point(width - radius.TopRight, 0)
            };

            return output;
        }

        private void AddSegment(List<PathSegment> segments, Point startPoint, Point endPoint, double radius)
        {
            segments.Add(new LineSegment(startPoint, false));

            if ((startPoint - endPoint).Length > double.Epsilon)
            {
                segments.Add(new ArcSegment(endPoint, new Size(radius, radius), 0, false, SweepDirection.Counterclockwise, false));
            }
        }
    }
}