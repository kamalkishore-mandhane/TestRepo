using System.Windows.Threading;

namespace ImgUtil.WPFUI.ViewModel
{
    public enum TrialPeriodMode
    {
        GreenTrialPeriod = 0,
        OrangeTrialPeriod,
        RedTrialPeriod,
        BlueSubscribe
    }

    internal class SubscribePageViewModel : ObservableObject
    {
        private bool _isShowTopSplitLine = false;

        public SubscribePageViewModel(Dispatcher dispatcher)
        {
        }

        public bool IsShowTopSplitLine
        {
            get
            {
                return _isShowTopSplitLine;
            }
            set
            {
                if (value != _isShowTopSplitLine)
                {
                    _isShowTopSplitLine = value;
                    Notify(nameof(IsShowTopSplitLine));
                }
            }
        }
    }
}
