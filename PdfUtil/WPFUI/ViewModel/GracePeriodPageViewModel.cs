using PdfUtil.WPFUI.View;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;

namespace PdfUtil.WPFUI.ViewModel
{
    public enum GracePeriodMode
    {
        BlueGracePeriod = 0,
        OrangeGracePeriod,
        RedGracePeriod,
        BlueLogin
    }

    public class GracePeriodPageViewModel : ObservableObject
    {
        private bool _isShowTopSplitLine = false;
        private GracePeriodMode _gracePeriod;
        private int _graceDaysRemaining;
        private string _userEmail;
        private Dictionary<GracePeriodMode, SolidColorBrush> _themeColorList;
        private Dictionary<GracePeriodMode, SolidColorBrush> _textColorList;

        public GracePeriodPageViewModel(GracePeriodMode gracePeriod, int graceDaysRemaining, string userEmail)
        {
            _gracePeriod = gracePeriod;
            _graceDaysRemaining = graceDaysRemaining;
            _userEmail = userEmail;

            _themeColorList = new Dictionary<GracePeriodMode, SolidColorBrush>();
            _textColorList = new Dictionary<GracePeriodMode, SolidColorBrush>();
        }

        public string UserEmail
        {
            get
            {
                return _userEmail;
            }
            set
            {
                if (value != _userEmail)
                {
                    _userEmail = value;
                    Notify(nameof(RemainDaysOrEmail));
                }
            }
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

        public GracePeriodMode GracePeriod
        {
            get
            {
                return _gracePeriod;
            }
            set
            {
                if (_gracePeriod != value)
                {
                    _gracePeriod = value;
                    Notify(nameof(PeriodThemeColor));
                    Notify(nameof(PeriodTextColor));
                    Notify(nameof(IsShowRemainDaysOrEmail));
                    Notify(nameof(GracePeriodText));
                    Notify(nameof(IsLoginSuccess));
                    Notify(nameof(GracePeriodButtonText));
                }
            }
        }

        public int GraceDaysRemaining
        {
            get
            {
                return _graceDaysRemaining;
            }
            set
            {
                if (_graceDaysRemaining != value)
                {
                    _graceDaysRemaining = value;
                    Notify(nameof(RemainDaysOrEmail));
                }
            }
        }

        public Brush PeriodThemeColor
        {
            get
            {
                if (!SystemParameters.HighContrast)
                {
                    return _themeColorList[GracePeriod];
                }
                else
                {
                    return _themeColorList[0];
                }
            }
        }

        public Brush PeriodTextColor
        {
            get
            {
                if (!SystemParameters.HighContrast)
                {
                    return _textColorList[GracePeriod];
                }
                else
                {
                    return _textColorList[0];
                }
            }
        }

        public bool IsShowGraceRemainDays
        {
            get
            {
                return _gracePeriod == GracePeriodMode.BlueGracePeriod || _gracePeriod == GracePeriodMode.OrangeGracePeriod;
            }
        }

        public bool IsShowRemainDaysOrEmail
        {
            get
            {
                return IsShowGraceRemainDays || IsLoginSuccess;
            }
        }

        public bool IsLoginSuccess
        {
            get
            {
                return _gracePeriod == GracePeriodMode.BlueLogin;
            }
        }

        public string GracePeriodText
        {
            get
            {
                if (IsShowGraceRemainDays)
                {
                    return Properties.Resources.GRACE_REMAIN_TITLE;
                }
                else if (IsLoginSuccess)
                {
                    return Properties.Resources.LOGSIGN_SUCESS;
                }
                else
                {
                    return Properties.Resources.GRACE_EXPRIED_TITLE;
                }
            }
        }

        public string RemainDaysOrEmail
        {
            get
            {
                if (IsShowGraceRemainDays)
                {
                    return _graceDaysRemaining.ToString();
                }
                else if (IsLoginSuccess)
                {
                    return UserEmail;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string GracePeriodButtonText
        {
            get
            {
                if (IsLoginSuccess)
                {
                    return Properties.Resources.DISMISS_BUTTON_TITLE;
                }
                else
                {
                    return Properties.Resources.LOG_SIGN_BUTTON_TITLE;
                }
            }
        }

        public void UpdatePeriodAndDays(GracePeriodMode mode, int graceDaysRemaining)
        {
            GracePeriod = mode;
            GraceDaysRemaining = graceDaysRemaining;
        }

        public void UpdateUserEmail(string userEmail)
        {
            UserEmail = userEmail;
        }

        public void InitThemeColorList(FrameworkElement view)
        {
            _themeColorList.Clear();
            _textColorList.Clear();
            if (SystemParameters.HighContrast)
            {
                _themeColorList.Add(GracePeriodMode.BlueGracePeriod, view.TryFindResource("Brush.Background.GraceBlue") as SolidColorBrush);
                _textColorList.Add(GracePeriodMode.BlueGracePeriod, view.TryFindResource("Brush.Text.Foreground.GraceBlue") as SolidColorBrush);
            }
            else
            {
                _themeColorList.Add(GracePeriodMode.BlueGracePeriod, view.TryFindResource("Brush.Background.GraceBlue") as SolidColorBrush);
                _themeColorList.Add(GracePeriodMode.OrangeGracePeriod, view.TryFindResource("Brush.Background.GraceOrange") as SolidColorBrush);
                _themeColorList.Add(GracePeriodMode.RedGracePeriod, view.TryFindResource("Brush.Background.GraceRed") as SolidColorBrush);
                _themeColorList.Add(GracePeriodMode.BlueLogin, view.TryFindResource("Brush.Background.Blue") as SolidColorBrush);

                _textColorList.Add(GracePeriodMode.BlueGracePeriod, view.TryFindResource("Brush.Text.Foreground.GraceBlue") as SolidColorBrush);
                _textColorList.Add(GracePeriodMode.OrangeGracePeriod, view.TryFindResource("Brush.Text.Foreground.GraceOrange") as SolidColorBrush);
                _textColorList.Add(GracePeriodMode.RedGracePeriod, view.TryFindResource("Brush.Text.Foreground.GraceRed") as SolidColorBrush);
                _textColorList.Add(GracePeriodMode.BlueLogin, view.TryFindResource("Brush.Text.Foreground.Blue") as SolidColorBrush);
            }

            Notify(nameof(PeriodThemeColor));
            Notify(nameof(PeriodTextColor));
        }
    }
}
