using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Threading;

namespace SBkUpUI.WPFUI.ViewModel
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
