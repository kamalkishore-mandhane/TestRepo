using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PdfUtil.WPFUI.Controls
{
    class InvertBooleanToVisibilityConverter : IValueConverter
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

    class StringToVisibilityConverter : IValueConverter
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

    class IndexToTextConverter : IValueConverter
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
            if ((values.Length > 0) && values.Contains(DependencyProperty.UnsetValue))
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

        #endregion
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

    [ValueConversion(typeof(object), typeof(Visibility))]
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

    [ValueConversion(typeof(object), typeof(string))]
    public sealed class AccessibleTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BookmarkTreeViewItem item)
            {
                return item.Title;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
}