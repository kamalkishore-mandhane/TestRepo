using Microsoft.Win32;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace SafeShare.WPFUI.View
{
    /// <summary>
    /// Interaction logic for ConvertOptPage.xaml
    /// </summary>
    public partial class ConvertOptPage : BasePage
    {
        private const int FleNameMaxLen = 251;

        private enum TextLocation
        {
            TOP_LEFT = 1,
            TOP_CENTER,
            TOP_RIGHT,
            MID_LEFT,
            MID_CENTER,
            MID_RIGHT,
            BOTTOM_LEFT,
            BOTTOM_CENTER,
            BOTTOM_RIGHT,
        }

        private enum TextAngle
        {
            DEGREES_0 = 0,
            DEGREES_90 = 90,
            DEGREES_B45 = -45,
            DEGREES_45 = 45
        }

        public ViewModel.ConvertType PageType;

        private ConversionPage _conversionPage;

        public ConvertOptPage(ViewModel.ConvertType type, ConversionPage conversionPage)
        {
            InitializeComponent();
            InitDataContext();

            var viewModel = DataContext as ConvertOptViewModels;
            _conversionPage = conversionPage;
            switch (type)
            {
                case ViewModel.ConvertType.WATERMARK:
                    viewModel.ConvertTypeText = Properties.Resources.WARKMARK_SETTING;
                    break;

                case ViewModel.ConvertType.REMOVE_PERSONAL_DATA:
                    viewModel.ConvertTypeText = Properties.Resources.REMOVE_PERSONAL_DATA_SETTING;
                    break;

                case ViewModel.ConvertType.CONVERT_TO_PDF:
                    viewModel.ConvertTypeText = Properties.Resources.CONVERT_TO_PDF_SETTING;
                    break;

                case ViewModel.ConvertType.COMBINE_INTO_ONE_PDF:
                    viewModel.ConvertTypeText = Properties.Resources.COMBINE_INTO_ONE_PDF_SETTING;
                    break;

                case ViewModel.ConvertType.REDUCE_PHOTOS:
                    viewModel.ConvertTypeText = Properties.Resources.REDUCE_PHOTOS_SETTING;
                    break;

                default:
                    break;
            }
            TabMain.SelectedIndex = (int)type;
        }

        public void InitDataContext()
        {
            var viewModel = new ViewModel.ConvertOptViewModels();
            this.DataContext = viewModel;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ConvertOptViewModels;
            viewModel.ShowFileNameErrorTips = false;
            if (PageType == ConvertType.COMBINE_INTO_ONE_PDF && viewModel.CombineIntoOnePDF)
            {
                if (viewModel.NewPdfName.Equals(string.Empty))
                {
                    viewModel.FileNameErrorTips = Properties.Resources.FILE_NAME_EMPTY;
                    viewModel.ShowFileNameErrorTips = true;
                    TextBox_PdfName.TextBox.Focus();
                    TextBox_PdfName.TextBox.Select(viewModel.NewPdfName.Length, 0);
                    return;
                }
                else if (viewModel.NewPdfName.Length > FleNameMaxLen)
                {
                    viewModel.ShowFileNameErrorTips = true;
                    viewModel.FileNameErrorTips = Properties.Resources.FILE_NAME_TOO_LONG;
                    TextBox_PdfName.TextBox.Focus();
                    TextBox_PdfName.TextBox.Select(viewModel.NewPdfName.Length, 0);
                    return;
                }
                else
                {
                    viewModel.ShowFileNameErrorTips = false;
                }

                if (!viewModel.NewPdfName.EndsWith(".pdf", true, null))
                {
                    viewModel.NewPdfName += ".pdf";
                }

                if (!viewModel.NewPdfName.Equals(viewModel.OldPdfName))
                {
                    viewModel.IsCustomName = true;
                }
            }

            viewModel.SaveConvertOpeSetting();
            NavigationService.GoBack();
        }

        private void ConvertOptPageView_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ConvertOptViewModels;
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            switch (PageType)
            {
                case ViewModel.ConvertType.WATERMARK:
                    viewModel.ConvertTypeText = Properties.Resources.WARKMARK_SETTING;
                    break;

                case ViewModel.ConvertType.REMOVE_PERSONAL_DATA:
                    viewModel.ConvertTypeText = Properties.Resources.REMOVE_PERSONAL_DATA_SETTING;
                    break;

                case ViewModel.ConvertType.CONVERT_TO_PDF:
                    viewModel.ConvertTypeText = Properties.Resources.CONVERT_TO_PDF_SETTING;
                    break;

                case ViewModel.ConvertType.COMBINE_INTO_ONE_PDF:
                    viewModel.ConvertTypeText = Properties.Resources.COMBINE_INTO_ONE_PDF_SETTING;
                    break;

                case ViewModel.ConvertType.REDUCE_PHOTOS:
                    viewModel.ConvertTypeText = Properties.Resources.REDUCE_PHOTOS_SETTING;
                    break;

                default:
                    break;
            }

            TabMain.SelectedIndex = (int)PageType;

            (this.DataContext as ViewModel.ConvertOptViewModels).ConvertSummary = (_conversionPage.DataContext as ViewModel.ConversionPageViewModel).GetConversionSummary(PageType);
        }

        private void ConvertOptPageView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void Panel_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Panel panel && panel.IsKeyboardFocusWithin)
            {
                foreach (var child in panel.Children)
                {
                    if (child is RadioButton button && button.IsChecked == true)
                    {
                        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(button), button);
                    }
                }
            }
        }

        private void PlacementGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is Panel panel && panel.IsKeyboardFocusWithin)
            {
                RadioButton focusElement = FocusManager.GetFocusedElement(FocusManager.GetFocusScope(panel)) as RadioButton;
                int moveStep = 1;
                var edgeTagList = new List<int>();
                bool isArrowKey = false;

                switch (e.Key)
                {
                    case Key.Down:
                        isArrowKey = true;
                        moveStep = 3;
                        edgeTagList = new List<int> { (int)TextLocation.BOTTOM_LEFT, (int)TextLocation.BOTTOM_CENTER, (int)TextLocation.BOTTOM_RIGHT };
                        break;

                    case Key.Up:
                        isArrowKey = true;
                        moveStep = -3;
                        edgeTagList = new List<int> { (int)TextLocation.TOP_LEFT, (int)TextLocation.TOP_CENTER, (int)TextLocation.TOP_RIGHT };
                        break;

                    case Key.Left:
                        isArrowKey = true;
                        moveStep = -1;
                        edgeTagList = new List<int> { (int)TextLocation.TOP_LEFT, (int)TextLocation.MID_LEFT, (int)TextLocation.BOTTOM_LEFT };
                        break;

                    case Key.Right:
                        isArrowKey = true;
                        moveStep = 1;
                        edgeTagList = new List<int> { (int)TextLocation.TOP_RIGHT, (int)TextLocation.MID_RIGHT, (int)TextLocation.BOTTOM_CENTER };
                        break;
                }

                if (isArrowKey)
                {
                    foreach (var child in panel.Children)
                    {
                        if (child is RadioButton button)
                        {
                            if (Convert.ToInt32(button.Tag) == Convert.ToInt32(focusElement.Tag) + moveStep)
                            {
                                button.IsChecked = true;
                                button.Focus();
                            }

                            if (edgeTagList.Contains(Convert.ToInt32(button.Tag)))
                            {
                                e.Handled = true;
                            }
                        }
                    }
                }
            }
        }

        private void DirectGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is Panel panel && panel.IsKeyboardFocusWithin)
            {
                RadioButton focusElement = FocusManager.GetFocusedElement(FocusManager.GetFocusScope(panel)) as RadioButton;
                int moveStep = 1;
                int edgeTag = 1;
                bool isArrowKey = false;

                Dictionary<int, int> tagIndex = new Dictionary<int, int>();
                tagIndex.Add((int)TextAngle.DEGREES_0, 1);
                tagIndex.Add((int)TextAngle.DEGREES_90, 2);
                tagIndex.Add((int)TextAngle.DEGREES_B45, 3);
                tagIndex.Add((int)TextAngle.DEGREES_45, 4);

                switch (e.Key)
                {
                    case Key.Right:
                    case Key.Down:
                        moveStep = 1;
                        edgeTag = 4;
                        isArrowKey = true;
                        break;

                    case Key.Up:
                    case Key.Left:
                        moveStep = -1;
                        edgeTag = 1;
                        isArrowKey = true;
                        break;
                }

                if (isArrowKey)
                {
                    foreach (var child in panel.Children)
                    {
                        if (child is RadioButton button)
                        {
                            if (tagIndex[Convert.ToInt32(button.Tag)] == tagIndex[Convert.ToInt32(focusElement.Tag)] + moveStep)
                            {
                                button.IsChecked = true;
                                button.Focus();
                            }

                            if (tagIndex[Convert.ToInt32(button.Tag)] == edgeTag)
                            {
                                e.Handled = true;
                            }
                        }
                    }
                }
            }
        }

        public override void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                BackButton_Click(null, null);
            }
        }
    }

    public class CustomTickBar : System.Windows.Controls.Primitives.TickBar
    {
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            var size = new Size(base.ActualWidth, base.ActualHeight);
            int tickCount = (int)((this.Maximum - this.Minimum) / this.TickFrequency);
            if ((this.Maximum - this.Minimum) % this.TickFrequency == 0)
            {
                tickCount -= 1;
            }

            Double tickFrequencySize;
            tickFrequencySize = (size.Width * this.TickFrequency / (this.Maximum - this.Minimum));
            int offSet = 7;
            for (int i = 0; i <= tickCount; i++)
            {
                double x = offSet + tickFrequencySize * i;
                double y = 5;
                var pen = new Pen(Brushes.Gray, 1);
                dc.DrawLine(pen, new Point(x, 0), new Point(x, y));
            }
        }
    }
}
