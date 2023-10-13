using Microsoft.Win32;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.View;
using PdfUtil.WPFUI.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for PrintView.xaml
    /// </summary>
    public partial class PrintView : BaseWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _curPageRadioBtnContent = string.Empty;
        private string _selectedPageRadioBtnContent = string.Empty;
        private string _allPagesRadioBtnContent = string.Empty;
        private string _pageRageRadioBtnContent = string.Empty;
        private PageSelectionEnum _curPageSelection = PageSelectionEnum.CurrentPage;
        private PdfUtilViewModel _viewModel;
        private bool _isSelectedPageRadioBtnVisible = false;
        private int _curDocumentPagesCount = 0;
        private int _pageRangeFrom = 0;
        private int _pageRangeTo = 0;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PrintView(PdfUtilView view)
        {
            InitializeComponent();
            Owner = view;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (Owner != null && Owner.DataContext as PdfUtilViewModel != null)
            {
                _viewModel = Owner.DataContext as PdfUtilViewModel;
            }

            InitRadtioBtnContent();
        }

        public string CurPageRadioBtnContent
        {
            get
            {
                return _curPageRadioBtnContent;
            }
            set
            {
                if (_curPageRadioBtnContent != value)
                {
                    _curPageRadioBtnContent = value;
                    OnPropertyChanged(nameof(CurPageRadioBtnContent));
                }
            }
        }

        public string SelectedPageRadioBtnContent
        {
            get
            {
                return _selectedPageRadioBtnContent;
            }
            set
            {
                if (_selectedPageRadioBtnContent != value)
                {
                    _selectedPageRadioBtnContent = value;
                    OnPropertyChanged(nameof(SelectedPageRadioBtnContent));
                }
            }
        }

        public string AllPagesRadioBtnContent
        {
            get
            {
                return _allPagesRadioBtnContent;
            }
            set
            {
                if (_allPagesRadioBtnContent != value)
                {
                    _allPagesRadioBtnContent = value;
                    OnPropertyChanged(nameof(AllPagesRadioBtnContent));
                }
            }
        }

        private string PageRageRadioBtnContent
        {
            get
            {
                return _pageRageRadioBtnContent;
            }
            set
            {
                if (_pageRageRadioBtnContent != value)
                {
                    _pageRageRadioBtnContent = value;
                    OnPropertyChanged(nameof(PageRageRadioBtnContent));
                }
            }
        }

        public bool IsSelectedPageRadioBtnVisible
        {
            get
            {
                return _isSelectedPageRadioBtnVisible;
            }
            set
            {
                if (_isSelectedPageRadioBtnVisible != value)
                {
                    _isSelectedPageRadioBtnVisible = value;
                    OnPropertyChanged(nameof(IsSelectedPageRadioBtnVisible));
                }
            }
        }

        public PageSelectionEnum CurPageSelection
        {
            get
            {
                return _curPageSelection;
            }
            set
            {
                if (_curPageSelection != value)
                {
                    _curPageSelection = value;
                    OnPropertyChanged(nameof(CurPageSelection));
                }
            }
        }

        public int PageRangeFrom
        {
            get
            {
                return _pageRangeFrom;
            }
        }

        public int PageRangeTo
        {
            get
            {
                return _pageRangeTo;
            }
        }

        private void InitRadtioBtnContent()
        {
            if (_viewModel == null || _viewModel.CurrentPdfDocument == null || _viewModel.CurPreviewIconItem == null)
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_NO_OPEN_PDF);
                return;
            }

            int curPageIndex = _viewModel.CurrentPdfDocument.Pages.IndexOf(_viewModel.CurPreviewIconItem.GetPage());
            int curSelectedPageCount = _viewModel.PdfUtilView.ThumbnailListView.SelectedItems.Count;
            if (curPageIndex < 0)
            {
                return;
            }

            CurPageRadioBtnContent = string.Format(Properties.Resources.PRINT_CURRENT_PAGE, curPageIndex);

            if (curSelectedPageCount != 0)
            {
                IsSelectedPageRadioBtnVisible = true;
                SelectedPageRadioBtnContent = string.Format(Properties.Resources.PRING_SELECTED_PAGE, curSelectedPageCount);
                CurPageSelection = PageSelectionEnum.SelectedPages;
                selectedPageRadioBtn.Focus();
            }

            _curDocumentPagesCount = _viewModel.PdfUtilView.ThumbnailListView.Items.Count;
            AllPagesRadioBtnContent = string.Format(Properties.Resources.PRINT_ALL_PAGES, _curDocumentPagesCount);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckPageSelection())
            {
                return;
            }

            DialogResult = true;
            Close();
        }

        public bool ShowWindow()
        {
            if (_viewModel == null || _viewModel.CurrentPdfDocument == null || _viewModel.CurPreviewIconItem == null)
                return false;

            return BaseShowWindow();
        }

        private bool CheckPageSelection()
        {
            if (CurPageSelection == PageSelectionEnum.PageRange)
            {
                if (_pageRangeTo > _curDocumentPagesCount || _pageRangeFrom > _curDocumentPagesCount)
                {
                    FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PRINT_PAGE_OUT_OF_RANGE, _curDocumentPagesCount));
                    return false;
                }
                else if (_pageRangeFrom < 1 || _pageRangeTo < 1)
                {
                    FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.PRINT_INVALID_PAGE_NUMBER);
                    if (_pageRangeFrom < 1)
                    {
                        PageRangeFromTextBox.Focus();
                    }
                    else
                    {
                        PageRangeToTextBox.Focus();
                    }
                    return false;
                }
                else if (_pageRangeFrom >= _pageRangeTo)
                {
                    FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.PRINT_MINIMUMPAGE_LESS_MAXIMUM);
                    return false;
                }
            }

            return true;
        }

        private void PageRangeFromTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int value = -1;
            if (int.TryParse(e.Text, out value))
            {
                return;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void PageRangeToTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int value = -1;
            if (int.TryParse(e.Text, out value))
            {
                return;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void PageRangeFromTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(PageRangeFromTextBox.Text, out _pageRangeFrom);
        }

        private void PageRangeToTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(PageRangeToTextBox.Text, out _pageRangeTo);
        }

        private void printView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void currentPageRadioBtn_GotFocus(object sender, RoutedEventArgs e)
        {
            currentPageRadioBtn.IsChecked = true;
        }

        private void selectedPageRadioBtn_GotFocus(object sender, RoutedEventArgs e)
        {
            selectedPageRadioBtn.IsChecked = true;
        }

        private void allPagesRadioBtn_GotFocus(object sender, RoutedEventArgs e)
        {
            allPagesRadioBtn.IsChecked = true;
        }

        private void pageRangeRadioBtn_GotFocus(object sender, RoutedEventArgs e)
        {
            pageRangeRadioBtn.IsChecked = true;
        }

        private void printView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void printView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }

    public enum PageSelectionEnum
    {
        CurrentPage,
        SelectedPages,
        AllPages,
        PageRange
    }
}
