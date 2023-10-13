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
    /// Interaction logic for ManageEmailPage.xaml
    /// </summary>
    public partial class ManageEmailPage : BasePage
    {
        private const double scrollSpeedFactor = 0.2;
        private bool _isLoaded = false;

        public ManageEmailPage()
        {
            InitializeComponent();
            DataContext = new ManageEmailPageViewModel(this);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;

            if (_isLoaded)
            {
                var addedItem = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
                var removedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;

                if (addedItem is EmailService addedService)
                {
                    if (addedItem is AdvancedSetup && removedItem is EmailService) // prevent Advanced Setup been in selected state
                    {
                        listView.SelectedItem = removedItem;
                        e.Handled = true;
                        (DataContext as ManageEmailPageViewModel)?.ExecuteAdvancedSetupCommand(null);
                    }
                    else if (!(removedItem is AdvancedSetup))
                    {
                        (DataContext as ManageEmailPageViewModel)?.HandleSelectionChange(addedService);
                    }
                }
            }

            var lastItem = EmailServiceListView.ItemContainerGenerator.ContainerFromItem(listView.SelectedItem) as ListViewItem;
            if (lastItem != null)
            {
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(lastItem), lastItem);
            }
        }

        private void ManageEmailPageView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            if (DataContext is ManageEmailPageViewModel viewModel)
            {
                viewModel.ClearErrorMessage();

                if (viewModel.SelectedService != null && !viewModel.IsInternalNavigation)
                {
                    if (WorkFlowManager.ShareOptionData.UseWinZipEmailer
                    && WorkFlowManager.ShareOptionData.SelectedEmail != null
                    && WorkFlowManager.ShareOptionData.SelectedEmail != viewModel.SelectedService
                    && EmailServiceListView.Items.Contains(WorkFlowManager.ShareOptionData.SelectedEmail))
                    {
                        viewModel.SelectedService = WorkFlowManager.ShareOptionData.SelectedEmail;
                    }

                    EmailServiceListView.ScrollToCenterOfView(viewModel.SelectedService);
                }
            }

            _isLoaded = true;
        }

        private void ManageEmailPageView_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void EmailServiceListView_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (sender is ListView listView)
            {
                if (e.Delta < 0 && DataContext is ManageEmailPageViewModel viewModel)
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

        public override void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ManageEmailPageViewModel viewModel)
            {
                if (e.Key == Key.Escape)
                {
                    viewModel.ExecuteBackCommand(this);
                }
            }
        }
    }

    public class EmailServiceDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is EmailService email)
            {
                if (email is AdvancedSetup)
                {
                    return (container as FrameworkElement)?.TryFindResource("AdvancedSetupDataTemplate") as DataTemplate;
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