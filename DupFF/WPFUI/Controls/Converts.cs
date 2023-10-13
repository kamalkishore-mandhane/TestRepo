using System;
using System.Collections;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;
using DupFF.WPFUI.View;
using DupFF.WPFUI.ViewModel;

namespace DupFF.WPFUI.Controls
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Boolean && (bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility && (Visibility)value == Visibility.Hidden)
            {
                return false;
            }
            return true;
        }
    }

    public class BooleanToReverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Boolean && (bool)value)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility && (Visibility)value == Visibility.Collapsed)
            {
                return true;
            }
            return false;
        }
    }

    public class EnabledToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object paramter, System.Globalization.CultureInfo curture)
        {
            return System.Convert.ToBoolean(value) ? 1 : 0.5;
        }

        public object ConvertBack(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    class GridViewHeaderVisibilityConverter : IValueConverter
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

    class ListToReverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as IList).Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ListToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as IList).Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TrimmedTextBlockVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length > 1 && values[1] is string des && !string.IsNullOrEmpty(des))
            {
                return Visibility.Visible;
            }
            if (values.Length > 0 && values[0] is TextBlock textBlock)
            {
                var formattedText = new FormattedText(
                textBlock.Text,
                CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black);
                if (formattedText.Width > (textBlock as FrameworkElement).ActualWidth)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return (string.IsNullOrEmpty(str)) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ItemStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var status = (ItemStatusUI)value;
            switch (status)
            {
                case ItemStatusUI.Running:
                    return Properties.Resources.BGT_BUSY;
                case ItemStatusUI.Deleted:
                    return Properties.Resources.BGT_DEL_PENDING;
                case ItemStatusUI.ExtSched:
                    return Properties.Resources.BGT_EXT_SCHED;
                case ItemStatusUI.Complete:
                    return Properties.Resources.BGT_COMPLETE;
                case ItemStatusUI.ReadyToRun:
                    return Properties.Resources.BGT_READY_TO_RUN;
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
}
