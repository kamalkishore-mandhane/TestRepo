using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SafeShare.WPFUI.Utils
{
    public enum ShareType
    {
        Unknown = 0,
        Attachment,
        Link
    }

    public class TrackHelper
    {
        private static TrackHelper _TrackHelper;
        private Dictionary<string, bool> _pathToTrackDic;

        public TrackHelper()
        {
            _pathToTrackDic = new Dictionary<string, bool>();

        }

        public static TrackHelper TrackHelperInstance
        {
            get
            {
                if (_TrackHelper == null)
                {
                    _TrackHelper = new TrackHelper();
                }
                return _TrackHelper;
            }
        }

        // This dictionary keeps all files added by user,
        // key is file's path and value is whether file added from picker.
        public Dictionary<string, bool> PathToTrackDic
        {
            get
            {
                return _pathToTrackDic;
            }
        }

        public ShareType ShareType { get; set; } = ShareType.Unknown;

        public bool IsEncryption { get; set; } = false;

        public bool StdPassword { get; set; } = true;

        public bool NameChanged { get; set; } = false;

        public List<WzSvcProviderIDs> ConvertIds { get; set; } = new List<WzSvcProviderIDs>();

        private static void RecurseFolder(string folder, List<string> destList)
        {
            var direcrory = new DirectoryInfo(folder);

            foreach (var file in direcrory.GetFiles())
            {
                destList.Add(file.FullName);
            }

            foreach (var dir in direcrory.GetDirectories())
            {
                RecurseFolder(dir.FullName, destList);
            }
        }

        private static void FormatProps(ref string eventProps, string newProp, string newValue)
        {
            if (string.IsNullOrEmpty(eventProps))
            {
                eventProps = string.Format("{0}={1}", newProp, newValue);
            }
            else
            {
                eventProps = string.Format("{0}|{1}={2}", eventProps, newProp, newValue);
            }
        }

        private static string ToYesOrNo(bool prop)
        {
            return prop ? "yes" : "no";
        }

        private static string ToNullOrString(string prop)
        {
            return string.IsNullOrEmpty(prop) ? "null" : prop;
        }

        #region SafeShare Feature Events Added in version 76.2

        // Tracking to record conversions that user selected.
        public static void LogShareConversionEvent()
        {
            foreach (var id in TrackHelperInstance.ConvertIds)
            {
                string eventProps = string.Empty;
                var name = WinzipMethods.GetConversionName(id);
                FormatProps(ref eventProps, "conversion-type", name);
                WinzipMethods.LogAppletFeatureEvent("feature.conversion", eventProps);
            }
        }

        // Tracking to record the user's choice before sharing.
        public static void LogShareTypeEvent()
        {
            string eventProps = string.Empty;

            FormatProps(ref eventProps, "type", TrackHelperInstance.ShareType == ShareType.Attachment ? "attachment" : "link");
            FormatProps(ref eventProps, "encryption", ToYesOrNo(TrackHelperInstance.IsEncryption));
            FormatProps(ref eventProps, "std-password", ToYesOrNo(TrackHelperInstance.IsEncryption && TrackHelperInstance.StdPassword));
            FormatProps(ref eventProps, "name-changed", ToYesOrNo(TrackHelperInstance.NameChanged));

            WinzipMethods.LogAppletFeatureEvent("feature.share-type", eventProps);
        }

        // Tracking to record files that user added.
        public static void LogFileAddEvent()
        {
            foreach (var item in TrackHelperInstance.PathToTrackDic)
            {
                if (File.Exists(item.Key))
                {
                    string eventProps = string.Empty;
                    FormatProps(ref eventProps, "type", Path.GetExtension(item.Key).ToLower());
                    FormatProps(ref eventProps, "quantity", "1"); // quantity will always be 1, for spec says "create 1 event per file"
                    FormatProps(ref eventProps, "source", item.Value ? "picker" : "dnd");

                    WinzipMethods.LogAppletFeatureEvent("feature.file-add", eventProps);
                }
                else if (Directory.Exists(item.Key))
                {
                    var destList = new List<string>();
                    RecurseFolder(item.Key, destList);
                    foreach (var filePath in destList)
                    {
                        string eventProps = string.Empty;
                        FormatProps(ref eventProps, "type", Path.GetExtension(filePath).ToLower());
                        FormatProps(ref eventProps, "quantity", "1"); // quantity will always be 1, for spec says "create 1 event per file"
                        FormatProps(ref eventProps, "source", item.Value ? "picker" : "dnd");

                        WinzipMethods.LogAppletFeatureEvent("feature.file-add", eventProps);
                    }
                }
            }
        }

        // Tracking to record how user choose to share via email.
        public static void LogAddEmailAccountEvent(string emailAddress, bool isPreset)
        {
            string eventProps = string.Empty;

            int admainIndex = emailAddress.LastIndexOf("@");
            string provider = admainIndex == -1 ? emailAddress : emailAddress.Substring(admainIndex);
            string emailSource = provider.Equals("user-email-client") ? "user-email-client" : isPreset ? "preset" : "manual";

            FormatProps(ref eventProps, "email-source", emailSource);
            FormatProps(ref eventProps, "email-provider", provider);

            WinzipMethods.LogAppletFeatureEvent("feature.email", eventProps);
        }

        // Tracking when user do the rating.
        public static void LogSafeShareRateEvent(int star)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "review-stars", star > 0 ? star.ToString() : "skipped");
            FormatProps(ref eventProps, "rating.operation", "SafeShare");

            WinzipMethods.LogAppletFeatureEvent("application.rating", eventProps);
        }

        #endregion

        #region SafeShare Feature Events Added in version 76.3

        // Tracking when user do the survey.
        public static void LogSafeShareSurveyEvent(string answer, string comment, bool isPolicyClick)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                // If comment contains carriage return(\r\n) or '|' or '=', productanalytics.dll will reject it and the feature.feedback event will not be sent. 
                // And '{' '}' are part of JSON which should not be contained in comment. So in order to successfully send the feature.feedback event,
                // replace '{' '}' '|' '=' with underscores, replace control characters with spaces and replace other
                // Format/Surrogate/Private Use/Not Assigned or Noncharacter unicode characters with underscores.
                comment = Regex.Replace(comment, @"[|={}]", "_");
                comment = Regex.Replace(comment, @"[\p{Cc}]", " ");
                comment = Regex.Replace(comment, @"[\p{C}]", "_");
            }

            string eventProps = string.Empty;
            FormatProps(ref eventProps, "feedback.operation", "SafeShare");
            FormatProps(ref eventProps, "feedback.answer", ToNullOrString(answer));
            FormatProps(ref eventProps, "feedback.comment", ToNullOrString(comment));
            FormatProps(ref eventProps, "feedback.policyclick", ToYesOrNo(isPolicyClick));

            WinzipMethods.LogAppletFeatureEvent("feature.feedback", eventProps);
        }

        #endregion

        // Tracking when user click shellext menu item
        public static void LogShellMenuEvent(string option)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "shell-menu.option", option);

            WinzipMethods.LogAppletFeatureEvent("feature.shell-menu", eventProps);
        }
    }
}
