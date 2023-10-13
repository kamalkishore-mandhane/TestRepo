using SafeShare.Util;
using SafeShare.WPFUI.Commands;
using SafeShare.WPFUI.Model;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Threading;

namespace SafeShare.WPFUI.ViewModel
{
    public class ExperiencePageViewModel : ObservableObject
    {
        private const int MaxStarRating = 5;
        private const int ThanksPageExistTime = 3000;   // 3s
        private const string winzipPrivacyPolicyUrl = "https://www.corel.com/{0}/corel-privacy-policy/";

        private ExperiencePage _experiencePage;
        private int _curStarRating;
        private bool _isZipProcessFinish;
        private bool _isZipProcessSucceed;
        private bool _isSkipDoneButtonEnable;
        private bool _isShowStarExperencePage;
        private bool _isShowNormalExperiencePage;
        private bool _isShowAirPlanIcon;
        private bool _isShowPuzzleIcon;
        private bool _isShowCheckMarkIcon;
        private long _fileSharedSize;
        private string _reducePercentText;
        private string _normalExperienceStateText;
        private bool _isShowSurveyPage = false;
        private bool _isShowThanksPage = false;
        private bool _isFreeFormItemSelected = false;
        private string _freeFormThoughts;
        private ObservableCollection<SurveyChoiceItem> _surveyItemList;
        private SurveyChoiceItem _selectedSurveyItem;
        private DispatcherTimer _thanksPageTimer;
        private string _reviewHeader;
        private string _surveyHeader;
        private bool _isPolicyClick;

        private ExperiencePageViewModelCommand _viewModelCommands;

        public ExperiencePageViewModel(ExperiencePage view)
        {
            _experiencePage = view;
            _isZipProcessFinish = false;
            _curStarRating = 0;
            _fileSharedSize = 0;
            NormalExperienceStateText = Properties.Resources.NORMAL_EXPERIENCE_STATE_TEXT;
            IsShowAirPlanIcon = true;
            IsShowCheckMarkIcon = false;
            IsShowPuzzleIcon = false;
            RegeditOperation.SetSurveyCount(RegeditOperation.GetSurveyCount() + 1);
            IsShowStarExperencePage = SurveyXmlDownloadHelper.SurveyXmlAvailable && CalculateIsShowStarExperience() && LoadSurveyOptionList();
            IsShowNormalExperencePage = !IsShowStarExperencePage;
            if (IsShowStarExperencePage)
            {
                RegeditOperation.SetSurveyLastDate();
                RegeditOperation.SetSurveyCount(0);
            }
        }

        public ExperiencePageViewModelCommand ViewModelCommands
        {
            get
            {
                if (_viewModelCommands == null)
                {
                    _viewModelCommands = new ExperiencePageViewModelCommand(this);
                }
                return _viewModelCommands;
            }
        }

        public int CurStarRating
        {
            get
            {
                return _curStarRating;
            }
            set
            {
                if (_curStarRating != value)
                {
                    if (value < 0)
                    {
                        _curStarRating = 0;
                    }
                    else if (value > MaxStarRating)
                    {
                        _curStarRating = MaxStarRating;
                    }
                    else
                    {
                        _curStarRating = value;
                    }
                    Notify(nameof(CurStarRating));
                    Notify(nameof(SkipDoneButtonContent));
                }
            }
        }

        public bool IsZipProcessFinish
        {
            get => _isZipProcessFinish;
            set
            {
                if (_isZipProcessFinish != value)
                {
                    _isZipProcessFinish = value;
                    Notify(nameof(IsZipProcessFinish));
                }
            }
        }

        public bool IsZipProcessSucceed
        {
            get => _isZipProcessSucceed;
            set
            {
                if (_isZipProcessSucceed != value)
                {
                    _isZipProcessSucceed = value;
                    Notify(nameof(IsZipProcessSucceed));
                }
            }
        }

        public bool IsSkipDoneButtonEnable
        {
            get => _isSkipDoneButtonEnable;
            set
            {
                if (_isSkipDoneButtonEnable != value)
                {
                    _isSkipDoneButtonEnable = value;
                    Notify(nameof(IsSkipDoneButtonEnable));
                }
            }
        }

        public bool IsSubmitButtonEnable
        {
            get
            {
                return (SelectedSurveyItem != null && SelectedSurveyItem.IsFreeForm == false) || (IsFreeFormItemSelected && !string.IsNullOrEmpty(FreeFormThoughts));
            }
        }

        public long FileSharedSize
        {
            get => _fileSharedSize;
            set
            {
                if (_fileSharedSize != value)
                {
                    _fileSharedSize = value;
                    Notify(nameof(FileSharedSize));
                }
            }
        }

        public string ReducePercentText
        {
            get => _reducePercentText;
            set
            {
                if (_reducePercentText != value)
                {
                    _reducePercentText = value;
                    Notify(nameof(ReducePercentText));
                }
            }
        }

        public string NormalExperienceStateText
        {
            get => _normalExperienceStateText;
            set
            {
                if (_normalExperienceStateText != value)
                {
                    _normalExperienceStateText = value;
                    Notify(nameof(NormalExperienceStateText));
                }
            }
        }

        public bool IsShowStarExperencePage
        {
            private get
            {
                return _isShowStarExperencePage;
            }
            set
            {
                if (_isShowStarExperencePage != value)
                {
                    _isShowStarExperencePage = value;
                    Notify(nameof(IsShowStarExperencePage));
                }
            }
        }

        public bool IsShowNormalExperencePage
        {
            private get
            {
                return _isShowNormalExperiencePage;
            }
            set
            {
                if (_isShowNormalExperiencePage != value)
                {
                    _isShowNormalExperiencePage = value;
                    Notify(nameof(IsShowNormalExperencePage));
                }
            }
        }

        public bool IsShowSurveyPage
        {
            get => _isShowSurveyPage;
            set
            {
                if (_isShowSurveyPage != value)
                {
                    _isShowSurveyPage = value;
                    Notify(nameof(IsShowSurveyPage));
                }
            }
        }

        public bool IsShowThanksPage
        {
            get => _isShowThanksPage;
            set
            {
                if (_isShowThanksPage != value)
                {
                    _isShowThanksPage = value;
                    Notify(nameof(IsShowThanksPage));
                }
            }
        }

        public bool IsShowAirPlanIcon
        {
            get
            {
                return _isShowAirPlanIcon;
            }
            set
            {
                if (_isShowAirPlanIcon != value)
                {
                    _isShowAirPlanIcon = value;
                    Notify(nameof(IsShowAirPlanIcon));
                }
            }
        }

        public bool IsShowPuzzleIcon
        {
            get
            {
                return _isShowPuzzleIcon;
            }
            set
            {
                if (_isShowPuzzleIcon != value)
                {
                    _isShowPuzzleIcon = value;
                    Notify(nameof(IsShowPuzzleIcon));
                }
            }
        }

        public bool IsShowCheckMarkIcon
        {
            get
            {
                return _isShowCheckMarkIcon;
            }
            set
            {
                if (_isShowCheckMarkIcon != value)
                {
                    _isShowCheckMarkIcon = value;
                    Notify(nameof(IsShowCheckMarkIcon));
                }
            }
        }

        public ObservableCollection<SurveyChoiceItem> SurveyItemList => _surveyItemList;

        public SurveyChoiceItem SelectedSurveyItem
        {
            get
            {
                return _selectedSurveyItem;
            }
            set
            {
                if (_selectedSurveyItem != value)
                {
                    _selectedSurveyItem = value;
                    if (_selectedSurveyItem != null && _selectedSurveyItem.IsFreeForm)
                    {
                        IsFreeFormItemSelected = true;
                    }

                    Notify(nameof(SelectedSurveyItem));
                    Notify(nameof(IsSubmitButtonEnable));
                }
            }
        }

        public bool IsFreeFormItemSelected
        {
            get => _isFreeFormItemSelected;
            set
            {
                if (_isFreeFormItemSelected != value)
                {
                    _isFreeFormItemSelected = value;
                    ChangeSurveyListFilter(_isFreeFormItemSelected);

                    Notify(nameof(IsFreeFormItemSelected));
                    Notify(nameof(SurveyButtonContent));
                    Notify(nameof(IsSubmitButtonEnable));
                }
            }
        }

        public string FreeFormThoughts
        {
            get => _freeFormThoughts;
            set
            {
                if (_freeFormThoughts != value)
                {
                    _freeFormThoughts = value;
                    Notify(nameof(FreeFormThoughts));
                    Notify(nameof(IsSubmitButtonEnable));
                }
            }
        }

        public string ReviewHeader
        {
            get => _reviewHeader;
            set
            {
                if (_reviewHeader != value)
                {
                    _reviewHeader = value;
                    Notify(nameof(ReviewHeader));
                }
            }
        }

        public string SurveyHeader
        {
            get => _surveyHeader;
            set
            {
                if (_surveyHeader != value)
                {
                    _surveyHeader = value;
                    Notify(nameof(SurveyHeader));
                }
            }
        }

        public string SkipDoneButtonContent
        {
            get
            {
                if (IsShowCheckMarkIcon || IsShowThanksPage)
                {
                    return Properties.Resources.DONE_BUTTON_TITLE;
                }
                else
                {
                    return CurStarRating > 0 ? Properties.Resources.SUBMIT_BUTTON_TITLE : Properties.Resources.SKIP_BUTTON_TITLE;
                }
            }
        }

        public string SurveyButtonContent => IsFreeFormItemSelected ? Properties.Resources.SURVEY_BACK_BUTTON_TITLE : Properties.Resources.SURVEY_SKIP_BUTTON_TITLE;

        private bool CalculateIsShowStarExperience()
        {
            bool isShow = false;
            var freq = RegeditOperation.GetSurveyFreq();
            if (freq > 0)
            {
                var count = RegeditOperation.GetSurveyCount();
                if (count >= freq)
                {
                    isShow = true;
                }
                else
                {
                    var days = RegeditOperation.GetSurveyDays();
                    if (days > 0)
                    {
                        var lastDate = RegeditOperation.GetSurveyLastDate();
                        var curDate = DateTime.Now;

                        var daySpan = (curDate - lastDate).Days;
                        if (daySpan >= days)
                        {
                            isShow = true;
                        }
                    }
                }
            }

            return isShow;
        }

        public void ZipFinishOrFailed(long filesize, long orgFilesize = 0)
        {
            _experiencePage.Progress.Value = 100;
            IsZipProcessFinish = true;
            IsSkipDoneButtonEnable = true;

            if (filesize > 0)
            {
                // file sharing succeeded
                IsZipProcessSucceed = true;
                FileSharedSize = filesize;

                if (orgFilesize > filesize)
                {
                    var reduce = 100 * (orgFilesize - filesize) / (double)orgFilesize;
                    ReducePercentText = string.Format(Properties.Resources.SIZE_REDUCTION_TEXT, Math.Round(reduce, reduce < 1 ? 2 : 0));
                }
                else
                {
                    ReducePercentText = string.Empty;
                }
                UpdateNormalExperienceState(true);
            }
            else
            {
                // file sharing failed
                ReducePercentText = string.Empty;
                IsZipProcessSucceed = false;
                UpdateNormalExperienceState(false);
            }
            _experiencePage.SkipDoneButton.Focus();
        }

        private void UpdateNormalExperienceState(bool success)
        {
            if (!IsShowStarExperencePage)
            {
                if (success)
                {
                    NormalExperienceStateText = Properties.Resources.NORMAL_EXPERIENCE_STATE_SUCCESS_TEXT;
                    IsShowAirPlanIcon = false;
                    IsShowPuzzleIcon = false;
                    IsShowCheckMarkIcon = true;
                    Notify(nameof(SkipDoneButtonContent));
                }
                else
                {
                    NormalExperienceStateText = Properties.Resources.NORMAL_EXPERIENCE_STATE_FAILED_TEXT;
                    IsShowAirPlanIcon = false;
                    IsShowCheckMarkIcon = false;
                    IsShowPuzzleIcon = true;
                }
            }
        }

        public void ExecuteSkipDoneCommand()
        {
            if (IsShowStarExperencePage) // user in rating page
            {
                TrackHelper.LogSafeShareRateEvent(CurStarRating);

                if (CurStarRating > 0 && CurStarRating < 4) // user rating is less than 4 stars, show survey page
                {
                    IsShowStarExperencePage = false;
                    IsShowSurveyPage = true;
                }
                else // user skip rating or rate 4 or 5 star, go to first page
                {
                    _experiencePage.SkipToNextPage();
                }
            }
            else if (IsShowThanksPage) // user click Done button in thank you page
            {
                _thanksPageTimer.Stop();
                IsShowThanksPage = false;
                _experiencePage.SkipToNextPage();
            }
            else // the rating page is not displayed
            {
                _experiencePage.SkipToNextPage();
            }
        }

        public void ExecuteSurveyCommand()
        {
            if (IsFreeFormItemSelected) // user click back button
            {
                IsFreeFormItemSelected = false;
                _experiencePage.SurveyOptionList.UnselectAll();
            }
            else // user click skip survey button
            {
                TrackHelper.LogSafeShareSurveyEvent(SurveyChoiceItem.SkipOptionAbbr, string.Empty, _isPolicyClick);
                _experiencePage.SkipToNextPage();
            }
        }

        public void ExecuteSubmitCommand()
        {
            TrackHelper.LogSafeShareSurveyEvent(SelectedSurveyItem.Abbreviation, SelectedSurveyItem.IsFreeForm ? FreeFormThoughts : string.Empty, _isPolicyClick);
            IsShowSurveyPage = false;
            IsShowThanksPage = true;
            Notify(nameof(SkipDoneButtonContent));
            InitThanksPageTimer();
        }

        public void ExecutePolicyLinkCommand()
        {
            _isPolicyClick = true;
            var url = string.Format(winzipPrivacyPolicyUrl, RegeditOperation.LangIDToShortName(RegeditOperation.GetWinZipInstalledUILangID()));
            Process.Start(new ProcessStartInfo(url));
        }

        private void InitThanksPageTimer()
        {
            // thank you page will automatically switch to next page in 3 seconds
            if (_thanksPageTimer == null)
            {
                _thanksPageTimer = new DispatcherTimer(DispatcherPriority.Normal);
                _thanksPageTimer.Interval = TimeSpan.FromMilliseconds(ThanksPageExistTime);
                _thanksPageTimer.Tick += delegate (object obj, EventArgs arg)
                {
                    _thanksPageTimer.Stop();
                    if (IsShowThanksPage)
                    {
                        _experiencePage.SkipToNextPage();
                    }
                };
            }

            _thanksPageTimer.Start();
        }

        private void ChangeSurveyListFilter(bool doFilter)
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(_experiencePage.SurveyOptionList.ItemsSource);
            if (view != null)
            {
                if (doFilter)
                {
                    view.Filter = SurveyListFilter;
                }
                else
                {
                    view.Filter = null;
                }
            }
        }

        private bool SurveyListFilter(object item)
        {
            var option = item as SurveyChoiceItem;
            return option.IsFreeForm;
        }

        private bool LoadSurveyOptionList()
        {
            if (SurveyXmlDownloadHelper.SurveyXmlAvailable)
            {
                var surveyOperation = SurveyXmlDownloadHelper.LoadSurveyXmlFromLocal();
                if (surveyOperation != null && surveyOperation.Items.Count != 0)
                {
                    ReviewHeader = surveyOperation.ReviewHeader;
                    SurveyHeader = surveyOperation.SurveyHeader;

                    _surveyItemList = surveyOperation.Items;
                    return true;
                }
            }
            return false;
        }
    }
}