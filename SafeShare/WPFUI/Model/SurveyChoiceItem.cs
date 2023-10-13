using System.Collections.ObjectModel;

namespace SafeShare.WPFUI.Model
{
    public class SurveyOperation
    {
        public string ReviewHeader;
        public string SurveyHeader;
        public ObservableCollection<SurveyChoiceItem> Items = new ObservableCollection<SurveyChoiceItem>();
    }

    public class SurveyChoiceItem
    {
        public static readonly string[] DefaultOptionAbbrList = { "email", "cloud", "encryption", "conversions", "comment" };
        public static readonly string SkipOptionAbbr = "skip";

        private string _surveyText;
        private string _abbreviation;
        private bool _isFreeForm;

        public SurveyChoiceItem(string content, string abbreviation, bool isFreeForm)
        {
            _surveyText = content;
            _abbreviation = abbreviation;
            _isFreeForm = isFreeForm;
        }

        public string SurveyText => _surveyText;

        public string Abbreviation => _abbreviation;

        public bool IsFreeForm => _isFreeForm;
    }
}
