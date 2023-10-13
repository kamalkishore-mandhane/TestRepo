using SafeShare.Util;
using SafeShare.WPFUI.Commands;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SafeShare.WPFUI.ViewModel
{
    internal class FrontPageViewModel : ViewModelBase
    {
        private const int DefaultItemsCount = 256;
        private const int PromotionalTextDuration = 5;
        private const string winzipPromotionalUrl = "http://www.winzip.com/wzgate.cgi?url=www.winzip.com/enterprise.htm&lang={0}&prod=WNZP";
        private readonly string[] PromotionalTexts = { Properties.Resources.PROMOTIONAL_TEXT1,
                                                       Properties.Resources.PROMOTIONAL_TEXT2,
                                                       Properties.Resources.PROMOTIONAL_TEXT3,
                                                       Properties.Resources.PROMOTIONAL_TEXT4,
                                                       Properties.Resources.PROMOTIONAL_TEXT5,
                                                       Properties.Resources.PROMOTIONAL_TEXT6};

        private FrontPage _frontPageView;
        private FrontPageViewModelCommand _viewModelCommands;
        private DispatcherTimer _promotionalTextTimer;
        private Random _random = new Random();
        private int _lastTextIndex = -1;

        public FrontPageViewModel(FrontPage view, Action<bool> adjustPaneCursor) : base(IntPtr.Zero, adjustPaneCursor)
        {
            _frontPageView = view;
            _viewModelCommands = null;
        }

        public FrontPageViewModelCommand ViewModelCommands
        {
            get
            {
                if (_viewModelCommands == null)
                {
                    _viewModelCommands = new FrontPageViewModelCommand(this);
                }
                return _viewModelCommands;
            }
        }

        public string PromotionalText
        {
            get
            {
                int randomNumber = _random.Next(0, 6);
                while (_lastTextIndex != -1 && randomNumber == _lastTextIndex)
                {
                    randomNumber = _random.Next(0, 5);
                }

                _lastTextIndex = randomNumber;
                return PromotionalTexts[randomNumber];
            }
        }

        private Task ExecuteSelectFilesTask()
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                var title = Properties.Resources.SELECT_FILES;
                var defaultBtn = Properties.Resources.SELECT_BUTTON_TITLE;
                var filters = Properties.Resources.OPEN_PICKER_FILTERS;
                int count = DefaultItemsCount;
                var defaultFolder = WinZipMethodHelper.GetOpenPickerDefaultFolder();
                var preSelectedItems = new WzCloudItem4[count];

                bool ret = WinzipMethods.FileSelection(_frontPageView.MainWindow.WindowHandle, title, defaultBtn, filters, defaultFolder, preSelectedItems,
                    ref count, true, true, true, true, true, false);

                if (ret)
                {
                    // The desired behavior is to select nothing if nothing is selected.
                    if (count == 1 && preSelectedItems[0].isFolder && preSelectedItems[0].itemId.EndsWith("\\*.*"))
                    {
                        tcs.TrySetCanceled();
                        return;
                    }
                    var selectedItems = new WzCloudItem4[count];
                    Array.Copy(preSelectedItems, selectedItems, count);

                    WinZipMethodHelper.SetOpenPickerDefaultFolder(selectedItems[0]);
                    WinZipMethodHelper.SetSavePickerDefaultFolder(selectedItems[0]);

                    var isCloudItem = WinZipMethodHelper.IsCloudItem(selectedItems[0].profile.Id);
                    var isLocalPortableDeviceItem = WinZipMethodHelper.IsLocalPortableDeviceItem(selectedItems[0].profile.Id);

                    if (isCloudItem || isLocalPortableDeviceItem)
                    {
                        var res = WinZipMethodHelper.DownloadCloudItems(_frontPageView.MainWindow.WindowHandle, ref selectedItems);
                        if (!res)
                        {
                            tcs.TrySetCanceled();
                            return;
                        }
                    }
                    else
                    {
                        if (!EDPHelper.CheckProtectedFiles(selectedItems))
                        {
                            tcs.TrySetCanceled();
                            return;
                        }
                    }

                    var fileListPage = new FileListPage();
                    fileListPage.InitDataContext();
                    (fileListPage.DataContext as FileListPageViewModel).ExecuteOpenFromFilePickerTaskCommand(selectedItems);
                    _frontPageView.MainWindow.Navigate(fileListPage);

                    tcs.TrySetResult();
                }
                else
                {
                    tcs.TrySetCanceled();
                }
            });
        }

        public void ExecuteSelectFilesCommand()
        {
            _executor(() => ExecuteSelectFilesTask(), RetryStrategy.Create(false, 0));
        }

        protected override bool HandleException(Exception ex)
        {
            throw new NotImplementedException();
        }

        private void DispatcherTimerTick(object sender, EventArgs e)
        {
            Notify(nameof(PromotionalText));
        }

        public void StartTimer()
        {
            if (_promotionalTextTimer == null)
            {
                _promotionalTextTimer = new DispatcherTimer();
                _promotionalTextTimer.Tick += new EventHandler(DispatcherTimerTick);
                _promotionalTextTimer.Interval = new TimeSpan(0, 0, PromotionalTextDuration);
            }

            _promotionalTextTimer.Start();
        }

        public void StopTimer()
        {
            if (_promotionalTextTimer != null)
            {
                _promotionalTextTimer.Stop();
            }
        }

        public void ExecuteHyperlinkClickCommand()
        {
            var url = string.Format(winzipPromotionalUrl, RegeditOperation.LangIDToShortName(RegeditOperation.GetWinZipInstalledUILangID()));
            Process.Start(new ProcessStartInfo(url));
        }
    }
}