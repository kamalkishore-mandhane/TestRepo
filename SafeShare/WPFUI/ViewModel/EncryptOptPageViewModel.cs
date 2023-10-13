using SafeShare.Util;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System.Windows;

namespace SafeShare.WPFUI.ViewModel
{
    internal class EncryptOptPageViewModel : ObservableObject
    {
        private EncryptOptPage _encryptOptPage;
        private bool _suggestedPasswordIsChecked;
        private bool _customPasswordIsChecked;
        private bool _copiedSuggestPwdVisible;
        private bool _copiedCustomPwdVisible;
        private bool _suggestedPwdErrorMsgVisible;
        private string _suggestedPwdErrorMsg;
        private bool _customPwdErrorMsgVisible;
        private string _customPwdErrorMsg;
        private string _suggestedPassword = string.Empty;
        private string _customPassword = string.Empty;
        private string _customVerifyPassword = string.Empty;

        public EncryptOptPageViewModel(EncryptOptPage page, bool isPasswordCustom, string password)
        {
            _encryptOptPage = page;
            _suggestedPassword = password;
            _suggestedPasswordIsChecked = !isPasswordCustom;
            _customPasswordIsChecked = isPasswordCustom;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.CustomEncryptPassword))
            {
                _customPassword = Properties.Settings.Default.CustomEncryptPassword;
                _customVerifyPassword = Properties.Settings.Default.CustomEncryptPassword;
            }
        }

        public bool SuggestedPasswordIsChecked
        {
            get => _suggestedPasswordIsChecked;
            set
            {
                if (_suggestedPasswordIsChecked != value)
                {
                    _suggestedPasswordIsChecked = value;
                    if (value == true)
                    {
                        CopiedCustomPwdVisible = false;
                        CustomPwdErrorMsgVisible = false;
                    }
                    Notify(nameof(SuggestedPasswordIsChecked));
                }
            }
        }

        public bool CustomPasswordIsChecked
        {
            get => _customPasswordIsChecked;
            set
            {
                if (_customPasswordIsChecked != value)
                {
                    _customPasswordIsChecked = value;
                    if (value == true)
                    {
                        CopiedSuggestPwdVisible = false;
                        SuggestedPwdErrorMsgVisible = false;
                    }
                    Notify(nameof(CustomPasswordIsChecked));
                }
            }
        }

        public bool CopiedSuggestPwdVisible
        {
            get => _copiedSuggestPwdVisible;
            set
            {
                if (_copiedSuggestPwdVisible != value)
                {
                    _copiedSuggestPwdVisible = value;
                    Notify(nameof(CopiedSuggestPwdVisible));
                }
            }
        }

        public bool CopiedCustomPwdVisible
        {
            get => _copiedCustomPwdVisible;
            set
            {
                if (_copiedCustomPwdVisible != value)
                {
                    _copiedCustomPwdVisible = value;
                    Notify(nameof(CopiedCustomPwdVisible));
                }
            }
        }

        public bool SuggestedPwdErrorMsgVisible
        {
            get => _suggestedPwdErrorMsgVisible;
            set
            {
                if (_suggestedPwdErrorMsgVisible != value)
                {
                    _suggestedPwdErrorMsgVisible = value;
                    Notify(nameof(SuggestedPwdErrorMsgVisible));
                }
            }
        }

        public string SuggestedPwdErrorMsg
        {
            get => _suggestedPwdErrorMsg;
            set
            {
                if (_suggestedPwdErrorMsg != value)
                {
                    _suggestedPwdErrorMsg = value;
                    Notify(nameof(SuggestedPwdErrorMsg));
                }
            }
        }

        public bool CustomPwdErrorMsgVisible
        {
            get => _customPwdErrorMsgVisible;
            set
            {
                if (_customPwdErrorMsgVisible != value)
                {
                    _customPwdErrorMsgVisible = value;
                    Notify(nameof(CustomPwdErrorMsgVisible));
                }
            }
        }

        public string CustomPwdErrorMsg
        {
            get => _customPwdErrorMsg;
            set
            {
                if (_customPwdErrorMsg != value)
                {
                    _customPwdErrorMsg = value;
                    Notify(nameof(CustomPwdErrorMsg));
                }
            }
        }

        public string SuggestedPassword
        {
            get => _suggestedPassword;
            set
            {
                if (_suggestedPassword != value)
                {
                    _suggestedPassword = value;
                    Notify(nameof(SuggestedPassword));
                }
            }
        }

        public string CustomPassword
        {
            get => _customPassword;
            set
            {
                if (_customPassword != value)
                {
                    _customPassword = value;
                    Notify(nameof(CustomPassword));
                }
            }
        }

        public string CustomVerifyPassword
        {
            get => _customVerifyPassword;
            set
            {
                if (_customVerifyPassword != value)
                {
                    _customVerifyPassword = value;
                    Notify(nameof(CustomVerifyPassword));
                }
            }
        }

        public bool CanGoBack(EmailEncryptionPageViewModel viewModel)
        {
            if (VerifyPasswordFormat())
            {
                if (SuggestedPasswordIsChecked)
                {
                    viewModel.IsPasswordCustom = false;
                    viewModel.IsPasswordCopied = false;
                    viewModel.SuggestedPassword = SuggestedPassword;
                }
                else
                {
                    viewModel.IsPasswordCustom = true;
                    viewModel.IsPasswordCopied = false;
                    viewModel.CustomPassword = CustomPassword;

                    Properties.Settings.Default.CustomEncryptPassword = CustomPassword;
                    Properties.Settings.Default.Save();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public void ResetCopyErrorText(bool resetSuggested = true, bool resetCustom = true)
        {
            if (resetSuggested)
            {
                CopiedSuggestPwdVisible = false;
                SuggestedPwdErrorMsg = string.Empty;
                SuggestedPwdErrorMsgVisible = false;
            }

            if (resetCustom)
            {
                CopiedCustomPwdVisible = false;
                CustomPwdErrorMsg = string.Empty;
                CustomPwdErrorMsgVisible = false;
            }
        }

        public void ClickCopySuggestedButtonAction()
        {
            if (VerifyPasswordFormat())
            {
                Clipboard.SetText(SuggestedPassword);
                CopiedSuggestPwdVisible = true;
            }
            else
            {
                CopiedSuggestPwdVisible = false;
            }
        }

        public void ClickCopyCustomButtonAction()
        {
            if (VerifyPasswordFormat())
            {
                Clipboard.SetText(CustomPassword);
                CopiedCustomPwdVisible = true;
            }
            else
            {
                CopiedCustomPwdVisible = false;
            }
        }

        public bool VerifyPasswordFormat()
        {
            var policy = RegeditOperation.GetPasswordPolicy();
            if (CustomPasswordIsChecked)
            {
                if (string.IsNullOrEmpty(_customPassword))
                {
                    CustomPwdErrorMsg = Properties.Resources.ERROR_PASSWORD_EMPTY;
                    CustomPwdErrorMsgVisible = true;
                    return false;
                }
                else if (string.IsNullOrEmpty(_customVerifyPassword))
                {
                    CustomPwdErrorMsg = Properties.Resources.ERROR_PASSWORD_EMPTY;
                    CustomPwdErrorMsgVisible = true;
                    return false;
                }
                else if (!_customPassword.Equals(_customVerifyPassword))
                {
                    CustomPwdErrorMsg = Properties.Resources.ERROR_PASSWORD_NOT_MATCH;
                    CustomPwdErrorMsgVisible = true;
                    return false;
                }
                else if (_customPassword.Length < policy.MinLength)
                {
                    CustomPwdErrorMsg = string.Format(Properties.Resources.ERROR_PASSWORD_TOO_SHORT, policy.MinLength);
                    CustomPwdErrorMsgVisible = true;
                    return false;
                }
                else if (!IsPasswordValidate(_customPassword))
                {
                    CustomPwdErrorMsg = Properties.Resources.ERROR_PASSWORD_NOT_MEET_POLICY;
                    CustomPwdErrorMsgVisible = true;
                    return false;
                }
                else
                {
                    CustomPwdErrorMsg = string.Empty;
                    CustomPwdErrorMsgVisible = false;
                    return true;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_suggestedPassword))
                {
                    SuggestedPwdErrorMsg = Properties.Resources.ERROR_PASSWORD_EMPTY;
                    SuggestedPwdErrorMsgVisible = true;
                    return false;
                }
                else if (_suggestedPassword.Length < policy.MinLength)
                {
                    SuggestedPwdErrorMsg = string.Format(Properties.Resources.ERROR_PASSWORD_TOO_SHORT, policy.MinLength);
                    SuggestedPwdErrorMsgVisible = true;
                    return false;
                }
                else if (!IsPasswordValidate(_suggestedPassword))
                {
                    SuggestedPwdErrorMsg = Properties.Resources.ERROR_PASSWORD_NOT_MEET_POLICY;
                    SuggestedPwdErrorMsgVisible = true;
                    return false;
                }
                else
                {
                    SuggestedPwdErrorMsg = string.Empty;
                    SuggestedPwdErrorMsgVisible = false;
                    return true;
                }
            }
        }

        private bool IsPasswordValidate(string password)
        {
            return WinzipMethods.CheckPasswordPolicyCompliance(_encryptOptPage.MainWindow.WindowHandle, password, false);
        }
    }
}