using Microsoft.Win32;
using PdfUtil.WPFUI.Utils;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for SignatureComboBox.xaml
    /// </summary>
    public partial class SignatureComboBox : BaseUserControl, INotifyPropertyChanged
    {
        public SignatureComboBox()
        {
            InitializeComponent();
        }

        #region Dependency Property and Routed Event

        public static DependencyProperty SignatureListProperty = DependencyProperty.Register("SignatureList", typeof(object), typeof(SignatureComboBox), new FrameworkPropertyMetadata(OnSignatureListChanged));
        public static DependencyProperty SelectedSignatureProperty = DependencyProperty.Register("SelectedSignature", typeof(SignatureItem), typeof(SignatureComboBox));

        public static RoutedEvent AddSignatureEvent = EventManager.RegisterRoutedEvent("AddSignature", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SignatureComboBox));
        public static RoutedEvent DeleteSignatureEvent = EventManager.RegisterRoutedEvent("DeleteSignature", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SignatureComboBox));
        public static RoutedEvent SignatureSelectedEvent = EventManager.RegisterRoutedEvent("SignatureSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SignatureComboBox));

        public object SignatureList
        {
            get { return GetValue(SignatureListProperty); }
            set { SetValue(SignatureListProperty, value); }
        }

        public SignatureItem SelectedSignature
        {
            get { return GetValue(SelectedSignatureProperty) as SignatureItem; }
            set { SetValue(SelectedSignatureProperty, value); }
        }

        public event RoutedEventHandler AddSignature
        {
            add { AddHandler(AddSignatureEvent, value); }
            remove { RemoveHandler(AddSignatureEvent, value); }
        }

        public event RoutedEventHandler DeleteSignature
        {
            add { AddHandler(DeleteSignatureEvent, value); }
            remove { RemoveHandler(DeleteSignatureEvent, value); }
        }

        public event RoutedEventHandler SignatureSelected
        {
            add { AddHandler(SignatureSelectedEvent, value); }
            remove { RemoveHandler(SignatureSelectedEvent, value); }
        }

        private static void OnSignatureListChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var combobox = sender as SignatureComboBox;
            if (combobox != null)
            {
                
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isSignatureSelected;

        public bool IsInitialButtonSelected
        {
            get
            {
                return SelectedSignature.IsInitialItem;
            }
        }

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SignatureComboBoxControl_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void SignatureComboBoxControl_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void InitialAddButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(AddSignatureEvent));
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var item = VisualTreeHelperUtils.FindAncestor<ComboBoxItem>((DependencyObject)e.OriginalSource);
            if (item != null && item.DataContext != null)
            {
                RaiseEvent(new RoutedEventArgs(DeleteSignatureEvent, item.DataContext));
            }
        }

        private void ContentComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            Notify(nameof(IsInitialButtonSelected));

            if (_isSignatureSelected)
            {
                _isSignatureSelected = false;
                RaiseEvent(new RoutedEventArgs(SignatureSelectedEvent));
            }
        }

        private void ComboBoxItemGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isSignatureSelected = true;
        }

        private void ContentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The selection change for adding signature will happened when combobox's dropdown is closed,
            // do raise SignatureSelectedEvent after adding signature.
            //
            // User may select the same signature for several times, this will not fire SelectionChanged event,
            // for this kind of selection, raise SignatureSelectedEvent in combobox's DropDownClosed event
            if (!ContentComboBox.IsDropDownOpen)
            {
                Notify(nameof(IsInitialButtonSelected));
                RaiseEvent(new RoutedEventArgs(SignatureSelectedEvent));
            }
        }
    }

    public sealed class SignatureItem
    {
        public SignatureItem()
        {
            IsInitialItem = true;
        }

        public SignatureItem(string path, BitmapSource image)
        {
            IsInitialItem = false;
            LocalPath = path;
            SignatureImage = image;
        }

        public SignatureItem(BitmapSource image)
        {
            IsInitialItem = false;
            SignatureImage = image;
        }

        public bool IsInitialItem { get; private set; }

        public string LocalPath { get; private set; }

        public BitmapSource SignatureImage { get; private set; }
    }

    public class SignatureComboBoxItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is SignatureItem sitem)
            {
                if (sitem.IsInitialItem == true)
                {
                    return (container as FrameworkElement)?.TryFindResource("ComboBoxInitialItemTemplate") as DataTemplate;
                }
                else
                {
                    return (container as FrameworkElement)?.TryFindResource("ComboBoxItemTemplate") as DataTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
