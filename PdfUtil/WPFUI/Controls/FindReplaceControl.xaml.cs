using Microsoft.Win32;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for FindReplaceControl.xaml
    /// </summary>
    public partial class FindReplaceControl : BaseUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isCancel = false;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public FindReplaceControl()
        {
            InitializeComponent();
        }

        public string FindWord
        {
            get
            {
                return findBox.Text;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseControl();
        }

        public void CloseControl()
        {
            findControl.Visibility = Visibility.Collapsed;
        }

        private void ResetFindHighlight()
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.ResetSearchingData();
            }
        }

        private void findBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                if (KeyboardUtil.IsShiftKeyDown)
                {
                    if (previousBtn.IsEnabled)
                    {
                        ExecuteFind(FindWord, true);
                    }
                }
                else
                {
                    if (nextBtn.IsEnabled)
                    {
                        ExecuteFind(FindWord, false);
                    }
                }
                e.Handled = true;
            }
        }

        private void ExecuteFind(string findWord, bool isPrevious)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                previousBtn.Visibility = Visibility.Visible;
                nextBtn.Visibility = Visibility.Visible;
                if (findBtn.Visibility == Visibility.Visible)
                {
                    findBtn.Visibility = Visibility.Collapsed;
                    SearchResultText.Text = string.Empty;
                    SearchResultText.Visibility = Visibility.Visible;
                }
                pdfUtilViewModel.ExecuteFindCommand(findWord, isPrevious);
            }
        }

        private void previousBtn_Click(object sender, RoutedEventArgs e)
        {
            ExecuteFind(FindWord, true);
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            ExecuteFind(FindWord, false);
        }

        private void findControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                findControl.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        public void RefreshFindBtnState(bool? isPreviousBtnEnable, bool? isNextBtnEnable)
        {
            if (isPreviousBtnEnable != null && previousBtn.IsEnabled != isPreviousBtnEnable)
            {
                previousBtn.IsEnabled = isPreviousBtnEnable == true;
            }

            if (isNextBtnEnable != null && nextBtn.IsEnabled != isNextBtnEnable)
            {
                nextBtn.IsEnabled = isNextBtnEnable == true;
            }
        }

        private void findControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (this.Visibility != Visibility.Visible)
            {
                if (pdfUtilViewModel != null)
                {
                    pdfUtilViewModel.CurrentPdfDocumentChangedEventEvent -= CloseControl;
                }
                ResetFindHighlight();
                findBox.Text = string.Empty;
                SearchResultText.Text = string.Empty;
                _isCancel = true;
            }
            else
            {
                if (pdfUtilViewModel != null)
                {
                    pdfUtilViewModel.CurrentPdfDocumentChangedEventEvent += CloseControl;
                }
                _isCancel = false;
                previousBtn.IsEnabled = true;
                nextBtn.IsEnabled = true;
            }
        }

        private void findControl_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            var model = DataContext as PdfUtilViewModel;
            if (model != null)
            {
                model.SearchingTextEvent -= Model_SearchingTextEvent;
                model.SearchingTextEvent += Model_SearchingTextEvent;
            }
        }

        private void findControl_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void Model_SearchingTextEvent(SearchingTextEventArgs e)
        {
            if (_isCancel)
            {
                e.Status = ProgressStatus.Cancel;
                return;
            }

            if (e.Status == ProgressStatus.InProgress)
            {
                SearchResultText.Text = string.Format(Properties.Resources.SEARCHING_PAGE_IN_PROGRESS, e.CurSearchingPage, e.TotalPageCount);

            }
            else if (e.Status == ProgressStatus.Completed)
            {
                if (e.IsNotFound)
                {
                    SearchResultText.Text = Properties.Resources.SEARCHING_NOT_FOUND;
                    previousBtn.IsEnabled = false;
                    nextBtn.IsEnabled = false;
                }
                else
                {
                    SearchResultText.Text = Properties.Resources.SEARCHING_COMPLETED;
                }
            }
            else
            {
                SearchResultText.Text = string.Empty;
            }
        }

        private void findBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            previousBtn.IsEnabled = true;
            nextBtn.IsEnabled = true;
            previousBtn.Visibility = Visibility.Collapsed;
            nextBtn.Visibility = Visibility.Collapsed;
            findBtn.Visibility = Visibility.Visible;
            SearchResultText.Visibility = Visibility.Hidden;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new FindReplaceControlAutomationPeer(this);
        }
    }

    public class FindReplaceControlAutomationPeer : UserControlAutomationPeer
    {
        public FindReplaceControlAutomationPeer(FindReplaceControl owner) :
            base(owner)
        {
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return nameof(FindReplaceControl);
        }
    }

    public class SearchingTextEventArgs
    {
        public int CurSearchingPage { get; set; }
        public int TotalPageCount { get; set; }
        public ProgressStatus Status { get; set; }
        public bool IsNotFound { get; set; }
    }
}
