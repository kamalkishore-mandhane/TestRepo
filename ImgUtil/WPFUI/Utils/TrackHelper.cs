using System.Collections.Generic;
using System.IO;

namespace ImgUtil.WPFUI.Utils
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

        public static void LogFileSaveAsEvent(string ext)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "file-save-as.type", ext.TrimStart('.').ToLower());

            WinzipMethods.LogAppletFeatureEvent("feature.file-save-as", eventProps);
        }

        public enum ImportWay
        {
            ImageWay,
            CameraWay,
            ScannerWay
        }

        public static void LogFileImportEvent(ImportWay importWay, string path = "")
        {
            string eventProps = string.Empty;
            switch (importWay)
            {
                case ImportWay.ImageWay:
                    var ext = Path.GetExtension(path).ToLower().TrimStart('.');
                    FormatProps(ref eventProps, "imgimport.source", ext);
                    break;
                case ImportWay.CameraWay:
                    FormatProps(ref eventProps, "imgimport.source", "cam");
                    break;
                case ImportWay.ScannerWay:
                    FormatProps(ref eventProps, "imgimport.source", "scan");
                    break;
            }

            WinzipMethods.LogAppletFeatureEvent("feature.imgimport", eventProps);
        }

        public static void LogFileOpenEvent(string file)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "file-open.type", Path.GetExtension(file).TrimStart('.').ToLower());

            WinzipMethods.LogAppletFeatureEvent("feature.file-open", eventProps);
        }

        public static void LogFileShareEvent(string file)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "file-share.type", Path.GetExtension(file).TrimStart('.').ToLower());

            WinzipMethods.LogAppletFeatureEvent("feature.file-share", eventProps);
        }

        public static void LogFileNewEvent(string file)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "type", Path.GetExtension(file).TrimStart('.').ToLower());

            WinzipMethods.LogAppletFeatureEvent("feature.file-new", eventProps);
        }

        private static string _ImgViewerEventProps;
        public static void LogImgViewerEvent(string zoomRatio)
        {
            string eventProps = string.Empty;
            if (zoomRatio == Properties.Resources.INFO_VIEW_BY_ACTUAL_SIZE)
            {
                FormatProps(ref eventProps, "imgviewer.zoom", "actual");
            }
            else if (zoomRatio == Properties.Resources.INFO_VIEW_BY_SIZE_TO_FIT)
            {
                FormatProps(ref eventProps, "imgviewer.zoom", "fit");
            }
            else
            {
                FormatProps(ref eventProps, "imgviewer.zoom", zoomRatio);
            }

            _ImgViewerEventProps = eventProps;
        }

        public static void SendImgViewerEvent()
        {
            if (!string.IsNullOrEmpty(_ImgViewerEventProps))
            {
                WinzipMethods.LogAppletFeatureEvent("feature.imgviewer", _ImgViewerEventProps);
            }
            _ImgViewerEventProps = string.Empty;
        }

        private static Dictionary<string, string> _CacheLog = new Dictionary<string, string>();
        public static void LogImgToolsEvent(string operation, string value)
        {
            string eventProps = string.Empty;
            switch (operation)
            {
                case "addToTeams":
                    {
                        FormatProps(ref eventProps, "imgtools.toolname", "teams");
                        break;
                    }
                case "copy":
                    {
                        FormatProps(ref eventProps, "imgtools.toolname", "copy");
                        break;
                    }
                case "convert":
                    {
                        FormatProps(ref eventProps, "imgtools.toolname", "convert");
                        FormatProps(ref eventProps, "imgtools.convert-type", value);
                        break;
                    }
                case "remexif":
                    {
                        FormatProps(ref eventProps, "imgtools.toolname", "rmvdata");
                        break;
                    }
                case "crop":
                    {
                        FormatProps(ref eventProps, "imgtools.toolname", "crop");
                        FormatProps(ref eventProps, "imgtools.crop-ratio", value);
                        break;
                    }
                case "resize":
                    {
                        FormatProps(ref eventProps, "imgtools.toolname", "resize");
                        FormatProps(ref eventProps, "imgtools.resize", value);
                        break;
                    }
                case "rotate":
                    {
                        FormatProps(ref eventProps, "imgtools.toolname", "rotate");
                        FormatProps(ref eventProps, "imgtools.rotate", value);
                        break;
                    }
                case "setbg":
                    {
                        FormatProps(ref eventProps, "imgtools.toolname", "setbg");
                        break;
                    }
                default:
                    break;
            }

            _CacheLog[operation] = eventProps;
        }

        public static void SendImgToolsEvent()
        {
            foreach(var eventProps in _CacheLog.Values)
            {
                WinzipMethods.LogAppletFeatureEvent("feature.imgtools", eventProps);
            }
            _CacheLog.Clear();
        }

        public static void TrackImgToolsEvent(WzSvcProviderIDs id, string file)
        {
            switch (id)
            {
                case WzSvcProviderIDs.SPID_CONVERTPHOTOS_TRANSFORM:
                    TrackHelper.LogImgToolsEvent("convert", Path.GetExtension(file).TrimStart('.').ToLower());
                    break;
                case WzSvcProviderIDs.SPID_REMOVEPERSONALDATA_TRANSFORM:
                    TrackHelper.LogImgToolsEvent("remexif", string.Empty);
                    break;
                default:
                    break;
            }
        }

        // Tracking when user click shellext menu item
        public static void LogShellMenuEvent(string option)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "shell-menu.option", option);

            WinzipMethods.LogAppletFeatureEvent("feature.shell-menu", eventProps);
        }
    }
}
