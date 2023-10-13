using PdfUtil.Util;
using PdfUtil.WPFUI.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfUtil.WPFUI.Utils
{
    public class TrackHelper
    {
        private const string WzPdfP2dKey = @"Software\WinZip Computing\PDFExpress\WXF\WzWXFp2d";
        private const string WzPdfP2dConvertTypeKey = "PDFConvertType";

        private const string WzPdfD2pKey = @"Software\WinZip Computing\PDFExpress\WXF\WzWXFd2p";
        private const string WzPdfD2pRemoveCommentsKey = "d2pNeedRemoveWordComments";
        private const string WzPdfD2pReadOnlyKey = "d2pIsPDFReadonly";
        private const string WzPdfD2pRemoveMarkupKey = "d2pAcceptAllRevisions";
        private const string WzPdfD2pResolutionKey = "d2pPDFResolution";
        private const string WzPdfD2pJPGQualityKey = "d2pJpgQuality";

        private const string WzPdfWmrkKey = @"Software\WinZip Computing\PDFExpress\WXF\WzWXFwmrk";
        private const string WzPdfWmrkUseTextKey = "WmrkUseText";
        private const string WzPdfWmrkUseImageKey = "WmrkUseImage";

        private const string WzPdfCmbpdfKey = @"Software\WinZip Computing\PDFExpress\WXF\WzWXFcmbpdf";
        private const string WzPdfCmbpdfDeleteOriginalsKey = "CmbPdfDeleteOriginals";
        private const string WzPdfCmbpdfWipeFilesKey = "CmbPdfWipeFiles";

        private static string[] ConvertTypeList = { "docx", "doc", "bmp", "jpg", "png", "tif"};
        private static string[] SecurityAllowPrintingList = { "none", "lowres", "highres"};
        private static string[] SecurityAllowChangesList = { "none", "insert", "fillform", "comment", "any"};
        private static string[] PrintPageSelectionList = { "current", "selected", "all", "range"};
        private static string[] RotateAngle = { "90", "180", "270"};

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

        public int AddBlankPageCount = 0;

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

        #region PDF Express Feature Events Added in version 76.1

        // Tracking when user import pdf, including import from Ribbon->Create From, Insert Pages From > Files, and some operations of the file explorer context menu
        public static void LogFileImportEvent(string file)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "type", Path.GetExtension(file).TrimStart('.').ToLower());

            WinzipMethods.LogAppletFeatureEvent("feature.file-import", eventProps);
        }

        // Tracking when user drag in a file or open a file from Ribbon->Open or recent list
        public static void LogFileOpenEvent(string file)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "type", Path.GetExtension(file).TrimStart('.').ToLower());

            WinzipMethods.LogAppletFeatureEvent("feature.file-open", eventProps);
        }

        // Tracking when user create a new file by SaveAs operation
        public static void LogFileNewEvent(string file)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "type", Path.GetExtension(file).TrimStart('.').ToLower());

            WinzipMethods.LogAppletFeatureEvent("feature.file-new", eventProps);
        }

        #endregion

        #region PDF Express Feature Events Added in version 76.3

        // Tracking when user insert pages from camera
        public static void LogInsertFromCameraEvent(string file)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "insertpage.source", "camera");
            FormatProps(ref eventProps, "insertpage.filetype", Path.GetExtension(file).TrimStart('.').ToLower());

            WinzipMethods.LogAppletFeatureEvent("feature.insertpage", eventProps);
        }

        // Tracking when user insert pages from scanner
        public static void LogInsertFromScannerEvent(string file)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "insertpage.source", "scanner");
            FormatProps(ref eventProps, "insertpage.filetype", Path.GetExtension(file).TrimStart('.').ToLower());

            WinzipMethods.LogAppletFeatureEvent("feature.insertpage", eventProps);
        }

        // Tracking when user insert pages from file
        public static void LogInsertFromFileEvent(string file)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "insertpage.source", "file");
            FormatProps(ref eventProps, "insertpage.filetype", Path.GetExtension(file).TrimStart('.').ToLower());

            WinzipMethods.LogAppletFeatureEvent("feature.insertpage", eventProps);
        }

        // Tracking when user perform the combine pdfs operation
        public static void LogPdfMergeEvent(int quantity)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "merge.filesquantity", quantity.ToString());

            WinzipMethods.LogAppletFeatureEvent("feature.merge", eventProps);
        }

        // Tracking when user extract image or pages from the current pdf
        public static void LogPdfExtractEvent(bool isPages, string type)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "extract.type", isPages ? "pages" : "image");
            FormatProps(ref eventProps, "extract.pages", Path.GetExtension(type).TrimStart('.').ToUpper());

            WinzipMethods.LogAppletFeatureEvent("feature.extract", eventProps);
        }

        // Tracking when user lock the pdf
        public static void LogLockSecurityEvent(LockPDFViewModel lockStatus)
        {
            string eventProps = string.Empty;

            FormatProps(ref eventProps, "pdfsecurity.lockopen", ToYesOrNo(lockStatus.IsSetOpenPassword));
            FormatProps(ref eventProps, "pdfsecurity.lockpermissions", ToYesOrNo(lockStatus.IsSetPermissionPassword));

            if (lockStatus.IsSetPermissionPassword)
            {
                // if set permission password, tracking the current permission status
                int allowPrintingInt = (int)lockStatus.CurAllowPrinting;
                string lockPrint = allowPrintingInt >= 0 && allowPrintingInt < SecurityAllowPrintingList.Count() ?
                                   SecurityAllowPrintingList[allowPrintingInt] : SecurityAllowPrintingList[0];

                int allowChangesInt = (int)lockStatus.CurAllowChanges;
                string lockChange = allowChangesInt >= 0 && allowChangesInt < SecurityAllowChangesList.Count() ?
                                   SecurityAllowChangesList[allowChangesInt] : SecurityAllowChangesList[0];

                FormatProps(ref eventProps, "pdfsecurity.lockprint", lockPrint);
                FormatProps(ref eventProps, "pdfsecurity.lockchange", lockChange);
                FormatProps(ref eventProps, "pdfsecurity.allowcopy", ToYesOrNo(lockStatus.IsAllowCopyingChecked));
                FormatProps(ref eventProps, "pdfsecurity.allowtxtaccess", ToYesOrNo(lockStatus.IsAllowScreenReaderChecked));
            }
            else
            {
                // no permission password
                FormatProps(ref eventProps, "pdfsecurity.lockprint", "highres");
                FormatProps(ref eventProps, "pdfsecurity.lockchange", "any");
                FormatProps(ref eventProps, "pdfsecurity.allowcopy", "yes");
                FormatProps(ref eventProps, "pdfsecurity.allowtxtaccess", "yes");
            }

            FormatProps(ref eventProps, "pdfsecurity.unlock", "no");

            WinzipMethods.LogAppletFeatureEvent("feature.pdfsecurity", eventProps);
        }

        // Tracking when user unlock the pdf
        public static void LogUnlockSecurityEvent()
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "pdfsecurity.lockopen", "no");
            FormatProps(ref eventProps, "pdfsecurity.lockpermissions", "no");
            FormatProps(ref eventProps, "pdfsecurity.lockprint", "highres");
            FormatProps(ref eventProps, "pdfsecurity.lockchange", "any");
            FormatProps(ref eventProps, "pdfsecurity.allowcopy", "yes");
            FormatProps(ref eventProps, "pdfsecurity.allowtxtaccess", "yes");
            FormatProps(ref eventProps, "pdfsecurity.unlock", "yes");

            WinzipMethods.LogAppletFeatureEvent("feature.pdfsecurity", eventProps);
        }

        // Tracking when user sign the pdf
        public static void LogSignSecurityEvent()
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "pdfsecurity.sign", "yes");

            WinzipMethods.LogAppletFeatureEvent("feature.pdfsecurity", eventProps);
        }

        // Tracking when user perform the save to zip operation
        public static void LogPdfSaveToZipEvent()
        {
            WinzipMethods.LogAppletFeatureEvent("feature.savetozip", string.Empty);
        }

        // Tracking when user save current pdf as another file
        public static void LogPdfSaveAsEvent(string type)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "saveas.type", Path.GetExtension(type).TrimStart('.').ToLower());

            WinzipMethods.LogAppletFeatureEvent("feature.saveas", eventProps);
        }

        // Tracking when user print the current pdf
        public static void LogPdfPrintEvent(PageSelectionEnum pageSelection)
        {
            string eventProps = string.Empty;
            int pageSelectionInt = (int)pageSelection;
            string printOption = pageSelectionInt >= 0 && pageSelectionInt < PrintPageSelectionList.Count() ?
                               PrintPageSelectionList[pageSelectionInt] : PrintPageSelectionList[0];

            FormatProps(ref eventProps, "printpdf.option", printOption);
            WinzipMethods.LogAppletFeatureEvent("feature.printpdf", eventProps);
        }

        // Tracking when user change the pdf settings
        public static void LogPdfSettingsEvent()
        {
            string eventProps = string.Empty;

            // get convert from pdf settings for PdfExpress from registry
            int p2dConvertTypeInt = RegeditOperation.GetConversionSettingRegistryIntValue(WzPdfP2dKey, WzPdfP2dConvertTypeKey);
            string p2dConvertType = p2dConvertTypeInt >= 0 && p2dConvertTypeInt < ConvertTypeList.Count() ? ConvertTypeList[p2dConvertTypeInt] : ConvertTypeList[0];

            // get convert to pdf settings for PdfExpress from registry
            bool d2pRemoveComments = RegeditOperation.GetConversionSettingRegistryIntValue(WzPdfD2pKey, WzPdfD2pRemoveCommentsKey) == 1;
            bool d2pReadOnly = RegeditOperation.GetConversionSettingRegistryIntValue(WzPdfD2pKey, WzPdfD2pReadOnlyKey) == 1;
            bool d2pRemoveMarkup = RegeditOperation.GetConversionSettingRegistryIntValue(WzPdfD2pKey, WzPdfD2pRemoveMarkupKey) == 1;
            int d2pResolution = RegeditOperation.GetConversionSettingRegistryIntValue(WzPdfD2pKey, WzPdfD2pResolutionKey);
            int d2pJPGQuality = RegeditOperation.GetConversionSettingRegistryIntValue(WzPdfD2pKey, WzPdfD2pJPGQualityKey);

            // get combine pdf settings for PdfExpress from registry
            bool cmbpdfDeleteOrig = RegeditOperation.GetConversionSettingRegistryIntValue(WzPdfCmbpdfKey, WzPdfCmbpdfDeleteOriginalsKey) == 1;
            bool cmbpdfWipe = RegeditOperation.GetConversionSettingRegistryIntValue(WzPdfCmbpdfKey, WzPdfCmbpdfWipeFilesKey) == 1;

            // get watermark settings for PdfExpress from registry
            bool WmrkUseImage = RegeditOperation.GetConversionSettingRegistryIntValue(WzPdfWmrkKey, WzPdfWmrkUseImageKey) == 1;
            int checkText = RegeditOperation.GetConversionSettingRegistryIntValue(WzPdfWmrkKey, WzPdfWmrkUseTextKey);
            bool WmrkUseText = checkText == -1 ? true : checkText == 1;

            // format needed properties for feature.pdfsettings event
            FormatProps(ref eventProps, "pdfsettings.convertfrompdf", p2dConvertType);
            FormatProps(ref eventProps, "pdfsettings.converttopdfRC", ToYesOrNo(d2pRemoveComments));
            FormatProps(ref eventProps, "pdfsettings.converttopdfRO", ToYesOrNo(d2pReadOnly));
            FormatProps(ref eventProps, "pdfsettings.converttopdfRM", ToYesOrNo(d2pRemoveMarkup));
            FormatProps(ref eventProps, "pdfsettings.resolution", d2pResolution == 2 ? "low" : "high");
            FormatProps(ref eventProps, "pdfsettings.jpgquality", d2pJPGQuality.ToString());
            FormatProps(ref eventProps, "pdfsettings.combinedelete", cmbpdfDeleteOrig ? (cmbpdfWipe ? "wipe" : "yes") : "no");
            FormatProps(ref eventProps, "pdfsettings.watermark", WmrkUseImage && WmrkUseText ? "both" : WmrkUseImage ? "image" : "text");

            WinzipMethods.LogAppletFeatureEvent("feature.pdfsettings", eventProps);
        }

        // Tracking when user add watermark to the current pdf
        public static void LogPdfWatermarkEvent(bool isImageSelected, bool isTextSelected)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "pdfwatermark", isImageSelected && isTextSelected ? "both" : isImageSelected ? "image" : "text");

            WinzipMethods.LogAppletFeatureEvent("feature.pdfwatermark", eventProps);
        }

        // Tracking when user add watermark to the current pdf
        public static void LogPdfWatermarkEvent()
        {
            WinzipMethods.LogAppletFeatureEvent("feature.pdfwatermark", string.Empty);
        }

        // Tracking when user rotate the pages in the current pdf
        public static void LogPdfRotateEvent(int quantity, DegreesSelected degrees)
        {
            string eventProps = string.Empty;
            int degreeInt = (int)degrees;
            string rotateAngle = degreeInt >= 0 && degreeInt < RotateAngle.Count() ? RotateAngle[degreeInt] : RotateAngle[0];

            FormatProps(ref eventProps, "pdfrotate.pages", quantity.ToString());
            FormatProps(ref eventProps, "pdfrotate.angle", rotateAngle);

            WinzipMethods.LogAppletFeatureEvent("feature.pdfrotate", eventProps);
        }

        // Tracking when user reorder the pages in the current pdf
        public static void LogPdfReorderEvent(bool isDrag)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "pdfreorder.type", isDrag ? "drag" : "context");

            WinzipMethods.LogAppletFeatureEvent("feature.pdfreorder", eventProps);
        }

        // Tracking when user delete the pages from the current pdf
        public static void LogPdfDeletePagesEvent(int quantity, bool isSelected)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "pdfdeletepages.quantity", quantity > 1 ? "multiple" : "single");
            FormatProps(ref eventProps, "pdfdeletepages.type", isSelected ? "selected" : "blank");

            WinzipMethods.LogAppletFeatureEvent("feature.pdfdeletepages", eventProps);
        }

        // Tracking the blank pages user added when user close the current pdf
        public static void LogAddBlankPageEvent(int blankPageCount)
        {
            if (blankPageCount > 0)
            {
                string eventProps = string.Empty;
                FormatProps(ref eventProps, "addblankpages.quantity", blankPageCount.ToString());

                WinzipMethods.LogAppletFeatureEvent("feature.addblankpage", eventProps);
            }
        }

        // Tracking when user change the page's background color in current pdf
        public static void LogSetBgColorEvent(bool isColorPreset)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "pdfsetbgcolor.type", isColorPreset ? "preset" : "custom");

            WinzipMethods.LogAppletFeatureEvent("feature.pdfsetbgcolor", eventProps);
        }

        #endregion

        #region PDF Express Feature Events Added in version 76.4

        // Tracking when user click shellext menu item
        public static void LogShellMenuEvent(string option)
        {
            string eventProps = string.Empty;
            FormatProps(ref eventProps, "shell-menu.option", option);

            WinzipMethods.LogAppletFeatureEvent("feature.shell-menu", eventProps);
        }

        // Tracking when user select transform in shellext menu
        public static void LogShellTransformsEvent(List<WzSvcProviderIDs> spids, bool isShow)
        {
            if (isShow)
            {
                if (spids.Contains(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM) && spids.Contains(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM))
                {
                    LogShellMenuEvent("convert-combine-modify-pdf");
                }
                else if (spids.Contains(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM))
                {
                    LogShellMenuEvent("combine-pdfs-and-modify");
                }
                else if (spids.Contains(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM))
                {
                    LogShellMenuEvent("convert-and-modify-pdf");
                }
            }
            else
            {
                if (spids.Contains(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM) && spids.Contains(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM))
                {
                    LogShellMenuEvent("convert-combine-to-pdf");
                }
                else if (spids.Contains(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM))
                {
                    LogShellMenuEvent("combine-files-to-pdf");
                }
                else if (spids.Contains(WzSvcProviderIDs.SPID_SIGN_PDF_TRANSFORM))
                {
                    LogShellMenuEvent("sign-pdf");
                }
                else if (spids.Contains(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM))
                {
                    LogShellMenuEvent("convert-to-pdf");
                }
                else if (spids.Contains(WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM))
                {
                    LogShellMenuEvent("watermark-pdf");
                }
                else if (spids.Contains(WzSvcProviderIDs.SPID_PDF2DOC_TRANSFORM))
                {
                    LogShellMenuEvent("convert-from-pdf");
                }
            }
        }

        #endregion
    }
}
