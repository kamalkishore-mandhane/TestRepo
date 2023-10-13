using System;

namespace DupFF.WPFUI.Utils
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

        public static void LogFinderCreatedEvent()
        {
            WinZipMethods.LogAppletFeatureEvent("feature.finder-created", string.Empty);
        }

        public static void LogFinderModifiedEvent()
        {
            WinZipMethods.LogAppletFeatureEvent("feature.finder-modified", string.Empty);
        }

        public static void LogFinderEnabledEvent(string finder, bool enabled)
        {
            string eventProps = string.Empty;
            if (!string.IsNullOrEmpty(finder))
            {
                FormatProps(ref eventProps, "finder", finder);
            }

            WinZipMethods.LogAppletFeatureEvent(enabled ? "feature.finder-enabled" : "feature.finder-disabled", eventProps);
        }
    }
}
