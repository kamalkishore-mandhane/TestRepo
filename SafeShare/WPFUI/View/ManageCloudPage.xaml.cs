using Microsoft.Win32;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.Model.Services;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SafeShare.WPFUI.View
{
    /// <summary>
    /// Interaction logic for ManageCloudPage.xaml
    /// </summary>
    public partial class ManageCloudPage : BasePage
    {
        private const double scrollSpeedFactor = 0.2;

        public ManageCloudPage()
        {
            InitializeComponent();
            DataContext = new ManageCloudPageViewModel(this);
        }

        private void ManageCloudPageView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            if (DataContext is ManageCloudPageViewModel viewModel)
            {
                viewModel.ManageServiceErrorMessage = string.Empty;

                if (viewModel.SelectedService != null && !viewModel.IsInternalNavigation)
                {
                    if (WorkFlowManager.ShareOptionData.SelectedCloud != null
                        && WorkFlowManager.ShareOptionData.SelectedCloud != viewModel.SelectedService
                        && CloudServiceListView.Items.Contains(WorkFlowManager.ShareOptionData.SelectedCloud))
                    {
                        viewModel.SelectedService = WorkFlowManager.ShareOptionData.SelectedCloud;
                    }

                    CloudServiceListView.ScrollToCenterOfView(viewModel.SelectedService);
                }
            }

            Button_Done.Focus();
        }

        private void ManageCloudPageView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void CloudServiceListView_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (sender is ListView listView)
            {
                if (e.Delta < 0 && DataContext is ManageCloudPageViewModel viewModel)
                {
                    if (viewModel.IsSeeAllButtonVisible)
                    {
                        viewModel.ExecuteSeeAllCommand(null);
                    }
                }

                var scroller = VisualTreeHelperUtils.FindVisualChild<ScrollViewer>(listView);
                if (scroller != null)
                {
                    if (scroller.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                    {
                        scroller.ScrollToVerticalOffset(scroller.VerticalOffset - e.Delta * scrollSpeedFactor);
                        e.Handled = true;
                    }
                }
            }
        }

        private void CloudServiceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lastItem = CloudServiceListView.ItemContainerGenerator.ContainerFromIndex(CloudServiceListView.SelectedIndex) as ListViewItem;
            if (lastItem != null)
            {
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(lastItem), lastItem);
            }
        }

        public override void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ManageCloudPageViewModel viewModel)
            {
                if (e.Key == Key.Escape)
                {
                    viewModel.ExecuteBackCommand(this);
                }
            }
        }
    }

    public class CloudServiceDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is CloudService email)
            {
                if (email.DisplayName == "ZipShare Cloud(Free)")
                {
                    return (container as FrameworkElement)?.TryFindResource("ZipShareDataTemplate") as DataTemplate;
                }
                else
                {
                    return (container as FrameworkElement)?.TryFindResource("ManageServiceListViewItemTemplate") as DataTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}