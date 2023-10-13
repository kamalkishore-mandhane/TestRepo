using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Security.Cryptography;
using SBkUpUI.WPFUI.Controls;

namespace SBkUpUI.WPFUI.Utils
{
    public class TrackHelper
    {
        private static TrackHelper _TrackHelper;

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

        #region Secure Backup Feature Events Added in version 76.4

        public static void LogCreateSBkUpEvent(JobItem job)
        {
            string keep = job.Swjf.limitMaxBackupNumber ? job.Swjf.maxBackupNumber.ToString() : "no";
            string time = string.Format("{0:hh:mm:ss tt}", job.Time);
            string amOrPm = time.Contains("PM") ? "PM" : "AM";
            string localOrCloud = WinZipMethods.IsCloudItem(job.Swjf.storeFolder.profile.Id) ? WinZipMethods.CloudServiceNameMap[job.Swjf.storeFolder.profile.Id] : "local";

            string eventProps = string.Empty;
            FormatProps(ref eventProps, "createsb.destination", localOrCloud);
            FormatProps(ref eventProps, "createsb.encrypt", ToYesOrNo(job.Swjf.encrypt));
            FormatProps(ref eventProps, "createsb.keep", keep);
            FormatProps(ref eventProps, "createsb.freq", job.Frequency.ToString());
            FormatProps(ref eventProps, "createsb.time", amOrPm);

            WinZipMethods.LogAppletFeatureEvent("feature.createsb", eventProps);
        }

        public static void LogRunSBkUpEvent(bool isCanned)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "runsb.type", isCanned ? "canned" : "user");

            WinZipMethods.LogAppletFeatureEvent("feature.runsb", eventProps);
        }

        public static void LogModifySBkUpEvent(JobItem job)
        {
            string localOrCloud = WinZipMethods.IsCloudItem(job.Swjf.storeFolder.profile.Id) ? WinZipMethods.CloudServiceNameMap[job.Swjf.storeFolder.profile.Id] : "local";
            string keep = job.Swjf.limitMaxBackupNumber ? job.Swjf.maxBackupNumber.ToString() : "no";

            string eventProps = string.Empty;
            FormatProps(ref eventProps, "modifysb.type", job.Swjf.isCanned ? "canned" : "user");
            FormatProps(ref eventProps, "modifysb.destination", localOrCloud);
            FormatProps(ref eventProps, "modifysb.encrypt", ToYesOrNo(job.Swjf.encrypt));
            FormatProps(ref eventProps, "modifysb.keep", keep);

            WinZipMethods.LogAppletFeatureEvent("feature.modifysb", eventProps);
        }

        public static void LogEnableSBkUpEvent(JobItem job, bool enabled)
        {
            string time = string.Format("{0:hh:mm:ss tt}", job.Time);
            string amOrPm = time.Contains("PM") ? "PM" : "AM";

            string eventProps = string.Empty;
            FormatProps(ref eventProps, "enablesb.type", job.Swjf.isCanned ? "canned" : "user");
            FormatProps(ref eventProps, "enablesb.selection", enabled ? "enabled" : "disabled");
            FormatProps(ref eventProps, "enablesb.freq", job.Frequency.ToString());
            FormatProps(ref eventProps, "enablesb.time", amOrPm);

            WinZipMethods.LogAppletFeatureEvent("feature.enablesb", eventProps);
        }

        public static void LogDeleteSBkUpEvent(bool isCanned)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "deletesb.type", isCanned ? "canned" : "user");

            WinZipMethods.LogAppletFeatureEvent("feature.deletesb", eventProps);
        }

        public static void LogBackupFolderEvent()
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "shell-menu.option", "back-up-this-folder");

            WinZipMethods.LogAppletFeatureEvent("feature.shell-menu", eventProps);
        }

        #endregion
    }
}
