using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Text;
using PdfUtil.WPFUI.Controls;
using PdfUtil.WPFUI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Color = Aspose.Pdf.Color;
using WindowsPoint = System.Windows.Point;

namespace PdfUtil.WPFUI
{
    public struct PDFInfo
    {
        public string filePath;
        public string fileName;
        public string password;
        public bool isSign;
    }

    static class PdfHelper
    {
        private static HighlightAnnotation _lastHighlightAnnotation = null;

        private static string[] FileTypeSupportConvert = {".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".ccitt", ".dib", ".gif", ".jpg",
            ".jpeg", ".jpe", ".jfif", ".png", ".tiff", ".tif", ".bmp", ".emf", ".exif", ".icon", ".ico", ".wmf", ".tex", ".txt", ".rtf", ".xps", ".htm", ".html"};

        public static string[] ColorsList = { "#FF1C99C0", "#FFFFD700", "#FFFFB6C1", "#FF9ACD32", "#FF87CEEB", "#FFFFA500", "#FFFF6347", "#FF9370DB", "#FF228B22", "#FF4682B4" };
        public static Dictionary<string, string> UserColorList = new Dictionary<string, string>();

        public const string PdfExtension = ".pdf";
        public const string PdfProgId = "WinZip.PdfExpress";
        public const string AppUserModeId = "AppUserModelID";
        public const double DefaultCommentSize = 18;
        public const double ScrollLeaveDistance = 50;

        // Set page background color for the selected pages in ThumbnailPane
        public static bool SetBackgroundColor(Page[] pages, System.Drawing.Color drawingColor)
        {
            bool isColorChanged = false;
            Color newBackgroundColor = Color.FromRgb(drawingColor);

            try
            {
                foreach (var page in pages)
                {
                    if (page != null)
                    {
                        if (page.Background != newBackgroundColor)
                        {
                            isColorChanged = true;
                            page.Background = newBackgroundColor;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return isColorChanged;
        }

        // Find if there are highlight text selected by selectedPoint
        public static bool FindSelectedHighlight(Page page, WindowsPoint selectedPoint)
        {
            try
            {
                if (page != null)
                {
                    return page.Annotations.ToList().Exists(annotation => (annotation is HighlightAnnotation highlightAnnotation)
                        && IsSelectedPointInsideItemRect(highlightAnnotation.GetRectangle(true), ConvertFromSystemPoint(selectedPoint)));
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        // Find if there are highlighted text intersect with the selected rectangle
        public static bool FindSelectedHighlight(Page page, WindowsPoint leftTopPoint, WindowsPoint rightBottomPoint)
        {
            try
            {
                if (page != null)
                {
                    var tempRotate = page.Rotate;
                    page.Rotate = Aspose.Pdf.Rotation.None;

                    var rect = new Rectangle(leftTopPoint.X, leftTopPoint.Y, rightBottomPoint.X, rightBottomPoint.Y);
                    var result = page.Annotations.ToList().Exists(annotation => (annotation is HighlightAnnotation highlightAnnotation)
                        && IsRectIntersect(highlightAnnotation.GetRectangle(true), rect));
                    page.Rotate = tempRotate;

                    return result;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        //Find if there are text in the selected rectangle
        public static bool FindSelectedText(Page page, WindowsPoint leftTopPoint, WindowsPoint rightBottomPoint)
        {
            bool hasText = false;
            try
            {
                if (page != null)
                {
                    var tempRotate = page.Rotate;
                    page.Rotate = Aspose.Pdf.Rotation.None;

                    var rect = new Rectangle(leftTopPoint.X, leftTopPoint.Y, rightBottomPoint.X, rightBottomPoint.Y);
                    var textFragmentAbsorber = new TextFragmentAbsorber();
                    textFragmentAbsorber.TextSearchOptions = new TextSearchOptions(rect);
                    textFragmentAbsorber.Visit(page);
                    page.Rotate = tempRotate;

                    hasText = textFragmentAbsorber.TextFragments.Count > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return hasText;
        }

        // Add highlight for all text in a selected rectangle
        public static bool AddHighlight(Page page, System.Drawing.Color color, WindowsPoint leftTopPoint, WindowsPoint rightBottomPoint)
        {
            bool isHighlightSuccess = false;
            try
            {
                if (page != null)
                {
                    var tempRotate = page.Rotate;
                    page.Rotate = Aspose.Pdf.Rotation.None;

                    var rect = new Rectangle(leftTopPoint.X, leftTopPoint.Y, rightBottomPoint.X, rightBottomPoint.Y);
                    var textFragmentAbsorber = new TextFragmentAbsorber();
                    textFragmentAbsorber.TextSearchOptions = new TextSearchOptions(rect);
                    textFragmentAbsorber.Visit(page);
                    var textList = textFragmentAbsorber.TextFragments.Where(text => !string.IsNullOrEmpty(text.Text)).ToList();

                    if (textList.Count != 0)
                    {
                        if (color != System.Drawing.Color.Transparent)
                        {
                            var highlightAnnotation = HighLightTextFragment(page, textList, color);
                            page.Annotations.Add(highlightAnnotation);
                            _lastHighlightAnnotation = highlightAnnotation;
                        }
                        else
                        {
                            RemoveHighlight(page, leftTopPoint, rightBottomPoint);
                        }
                        isHighlightSuccess = true;
                    }
                    page.Rotate = tempRotate;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return isHighlightSuccess;
        }

        // Change a highlight text selected by selectedPoint
        public static bool ChangeHighlight(Page page, System.Drawing.Color color, WindowsPoint selectedPoint, bool isChangeLastHighlight)
        {
            bool isChanged = false;
            try
            {
                if (page != null)
                {
                    foreach (var annotation in page.Annotations)
                    {
                        if (annotation is HighlightAnnotation highlightAnnotation)
                        {
                            Color highlightColor = Color.FromRgb(color);
                            if (isChangeLastHighlight)
                            {
                                isChanged = ChangeHighlight(page, _lastHighlightAnnotation, color);
                                break;
                            }

                            if (IsSelectedPointInsideItemRect(highlightAnnotation.GetRectangle(true), ConvertFromSystemPoint(selectedPoint)))
                            {
                                isChanged = ChangeHighlight(page, highlightAnnotation, color);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return isChanged;
        }

        private static bool ChangeHighlight(Page page, HighlightAnnotation highlightAnnotation, System.Drawing.Color color)
        {
            var isChanged = false;
            if (highlightAnnotation == null)
            {
                return isChanged;
            }

            try
            {
                if (color == System.Drawing.Color.Transparent)
                {
                    if (page.Annotations.Contains(highlightAnnotation))
                    {
                        page.Annotations.Delete(highlightAnnotation);
                        if (highlightAnnotation.Popup != null)
                        {
                            page.Annotations.Delete(highlightAnnotation.Popup);
                        }
                        if (highlightAnnotation.InReplyTo != null)
                        {
                            page.Annotations.Delete(highlightAnnotation.InReplyTo);
                        }
                        isChanged = true;
                    }
                }
                else
                {
                    Color highlightColor = Color.FromRgb(color);
                    highlightAnnotation.Color = highlightColor;
                    highlightAnnotation.Modified = DateTime.Now;
                    isChanged = true;
                }
            }
            catch
            {
                isChanged = false;
            }
            return isChanged;
        }

        // Remove all highlight text in a selected rectangle
        public static bool RemoveHighlight(Page page, WindowsPoint leftTopPoint, WindowsPoint rightBottomPoint)
        {
            bool isRemove = false;
            try
            {
                if (page != null)
                {
                    var tempRotate = page.Rotate;
                    page.Rotate = Aspose.Pdf.Rotation.None;

                    var rect = new Rectangle(leftTopPoint.X, leftTopPoint.Y, rightBottomPoint.X, rightBottomPoint.Y);
                    foreach (var annotation in page.Annotations)
                    {
                        if (annotation is HighlightAnnotation highlightAnnotation)
                        {
                            if (IsRectIntersect(highlightAnnotation.GetRectangle(true), rect))
                            {
                                page.Annotations.Delete(highlightAnnotation);
                                if (highlightAnnotation.Popup != null)
                                {
                                    page.Annotations.Delete(highlightAnnotation.Popup);
                                }
                                if (highlightAnnotation.InReplyTo != null)
                                {
                                    page.Annotations.Delete(highlightAnnotation.InReplyTo);
                                }
                                isRemove = true;
                            }
                        }
                    }
                    page.Rotate = tempRotate;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return isRemove;
        }

        // Find the comment selected by selectedPoint
        public static bool FindSelectedComment(Page page, WindowsPoint selectedPoint)
        {
            bool isFound = false;
            try
            {
                if (page != null)
                {
                    return page.Annotations.ToList().Exists(annotation => (annotation is TextAnnotation textAnnotation)
                            && IsSelectedPointInsideItemRect(textAnnotation.GetRectangle(false), ConvertFromSystemPoint(selectedPoint)));
                }
            }
            catch (Exception)
            {
                return false;
            }

            return isFound;
        }

        // Find if there are comments intersect with the selected rectangle
        public static bool FindSelectedComment(Page page, WindowsPoint leftTopPoint, WindowsPoint rightBottomPoint)
        {
            try
            {
                if (page != null)
                {
                    var rect = new Rectangle(leftTopPoint.X, leftTopPoint.Y, rightBottomPoint.X, rightBottomPoint.Y);
                    var result = page.Annotations.ToList().Exists(annotation => (annotation is TextAnnotation textAnnotation)
                        && IsRectIntersect(textAnnotation.GetRectangle(false), rect));

                    return result;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        // Add comment at the right-bottom side of the selected point
        public static bool AddComment(Page page, string commentText, WindowsPoint leftTopPoint, WindowsPoint rightBottomPoint, List<TextAnnotation> annotations)
        {
            bool isAdd = false;
            try
            {
                if (page != null)
                {
                    var rect = new Rectangle(leftTopPoint.X, leftTopPoint.Y, rightBottomPoint.X, rightBottomPoint.Y);
                    var name = FileOperation.GenerateUniqueName("comment-{0:x6}");
                    var textAnnotation = new TextAnnotation(page, rect)
                    {
                        Name = name,
                        Title = Environment.UserName,
                        Subject = "Comment",
                        Icon = TextIcon.Comment,
                        Modified = DateTime.Now,
                        State = AnnotationState.None,
                        Color = Color.Yellow,
                        Contents = commentText,
                        Flags = AnnotationFlags.NoZoom | AnnotationFlags.Print,
                    };

                    page.Annotations.Add(textAnnotation);

                    var annotation = page.Annotations.FindByName(name) as TextAnnotation;
                    annotations.Add(annotation);

                    isAdd = true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return isAdd;
        }

        // Edit comment by index
        public static bool EditComment(Page page, int index, string commentText)
        {
            bool isEdited = false;
            try
            {
                if (page != null)
                {
                    var annotation = page.Annotations[index];
                    if (annotation is TextAnnotation textAnnotation)
                    {
                        if (textAnnotation.Contents != commentText)
                        {
                            textAnnotation.Contents = commentText;
                            textAnnotation.Modified = DateTime.Now;
                            isEdited = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return isEdited;
        }

        // Delete the comment selected by selectedPoint
        public static bool DeleteComment(Page page, WindowsPoint selectedPoint, List<TextAnnotation> annotations)
        {
            bool isDeleted = false;
            try
            {
                if (page != null)
                {
                    foreach (var annotation in page.Annotations)
                    {
                        if (annotation is TextAnnotation textAnnotation)
                        {
                            if (IsSelectedPointInsideItemRect(textAnnotation.GetRectangle(false), ConvertFromSystemPoint(selectedPoint)))
                            {
                                page.Annotations.Delete(textAnnotation);
                                if (textAnnotation.Popup != null)
                                {
                                    page.Annotations.Delete(textAnnotation.Popup);
                                }
                                if (textAnnotation.InReplyTo != null)
                                {
                                    page.Annotations.Delete(textAnnotation.InReplyTo);
                                }
                                annotations.Add(textAnnotation);
                                isDeleted = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return isDeleted;
        }

        // Delete all comments in a selected rectangle
        public static bool DeleteComment(Page page, WindowsPoint leftTopPoint, WindowsPoint rightBottomPoint, List<TextAnnotation> annotations)
        {
            bool isDeleted = false;
            try
            {
                if (page != null)
                {
                    var rect = new Rectangle(leftTopPoint.X, leftTopPoint.Y, rightBottomPoint.X, rightBottomPoint.Y);
                    foreach (var annotation in page.Annotations)
                    {
                        if (annotation is TextAnnotation textAnnotation)
                        {
                            if (IsRectIntersect(textAnnotation.GetRectangle(false), rect))
                            {
                                page.Annotations.Delete(textAnnotation);
                                if (textAnnotation.Popup != null)
                                {
                                    page.Annotations.Delete(textAnnotation.Popup);
                                }
                                if (textAnnotation.InReplyTo != null)
                                {
                                    page.Annotations.Delete(textAnnotation.InReplyTo);
                                }
                                annotations.Add(textAnnotation);
                                isDeleted = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return isDeleted;
        }

        // Get comment color brush by its author
        public static Brush GetAnnotationBrush(string userName)
        {
            string color;
            if (UserColorList.ContainsKey(userName))
            {
                color = UserColorList[userName];
            }
            else
            {
                color = ColorsList[UserColorList.Count % 10];
                UserColorList.Add(userName, color);
            }

            var converter = new BrushConverter();
            return (Brush)converter.ConvertFromString(color);
        }

        // Add signature (StampAnnotation) at the selected point
        public static bool AddSignature(Page page, BitmapSource sourceImage, WindowsPoint leftTopPoint, WindowsPoint rightBottomPoint)
        {
            bool isSuccess = false;

            try
            {
                var rect = new Rectangle(leftTopPoint.X, leftTopPoint.Y, rightBottomPoint.X, rightBottomPoint.Y);

                var stream = new MemoryStream();
                var encoder = new PngBitmapEncoder();
                var outputFrame = BitmapFrame.Create(sourceImage);
                encoder.Frames.Add(outputFrame);
                encoder.Save(stream);
                stream.Position = 0;

                if (page.Rotate != Aspose.Pdf.Rotation.None)
                {
                    // if the page is rotated, rotate the image back so it doesn't appear to be rotated.
                    Aspose.Imaging.RotateFlipType rotateFlipType = Aspose.Imaging.RotateFlipType.RotateNoneFlipNone;
                    switch (page.Rotate)
                    {
                        case Aspose.Pdf.Rotation.on90:
                            rotateFlipType = Aspose.Imaging.RotateFlipType.Rotate270FlipNone;
                            break;
                        case Aspose.Pdf.Rotation.on180:
                            rotateFlipType = Aspose.Imaging.RotateFlipType.Rotate180FlipNone;
                            break;
                        case Aspose.Pdf.Rotation.on270:
                            rotateFlipType = Aspose.Imaging.RotateFlipType.Rotate90FlipNone;
                            break;
                    }

                    using (Aspose.Imaging.Image image = Aspose.Imaging.Image.Load(stream))
                    {
                        image.RotateFlip(rotateFlipType);
                        image.Save(stream);
                    }
                }

                var stampAnnotation = new StampAnnotation(page, rect)
                {
                    Title = Environment.UserName,
                    Subject = "Signature",
                    Modified = DateTime.Now,
                    Image = stream,
                    Flags = AnnotationFlags.NoZoom | AnnotationFlags.Print,
                };

                page.Annotations.Add(stampAnnotation);
                isSuccess = true;
            }
            catch
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        // Delete the signature (StampAnnotation) selected by selected point
        public static bool DeleteSignature(Page page, WindowsPoint selectedPoint)
        {
            bool isDeleted = false;
            try
            {
                if (page != null)
                {
                    foreach (var annotation in page.Annotations)
                    {
                        if (annotation is StampAnnotation stampAnnotation)
                        {
                            if (IsSelectedPointInsideItemRect(stampAnnotation.GetRectangle(false), ConvertFromSystemPoint(selectedPoint)))
                            {
                                page.Annotations.Delete(stampAnnotation);
                                if (stampAnnotation.Popup != null)
                                {
                                    page.Annotations.Delete(stampAnnotation.Popup);
                                }
                                if (stampAnnotation.InReplyTo != null)
                                {
                                    page.Annotations.Delete(stampAnnotation.InReplyTo);
                                }
                                isDeleted = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return isDeleted;
        }

        // Delete the all signatures (StampAnnotation) in a selected rectangle
        public static bool DeleteSignature(Page page, WindowsPoint leftTopPoint, WindowsPoint rightBottomPoint)
        {
            bool isDeleted = false;
            try
            {
                if (page != null)
                {
                    var rect = new Rectangle(leftTopPoint.X, leftTopPoint.Y, rightBottomPoint.X, rightBottomPoint.Y);
                    foreach (var annotation in page.Annotations)
                    {
                        if (annotation is StampAnnotation stampAnnotation)
                        {
                            if (IsRectIntersect(stampAnnotation.GetRectangle(false), rect))
                            {
                                page.Annotations.Delete(stampAnnotation);
                                if (stampAnnotation.Popup != null)
                                {
                                    page.Annotations.Delete(stampAnnotation.Popup);
                                }
                                if (stampAnnotation.InReplyTo != null)
                                {
                                    page.Annotations.Delete(stampAnnotation.InReplyTo);
                                }
                                isDeleted = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return isDeleted;
        }

        // Find if there are signature (StampAnnotation) intersect with the selected rectangle
        public static bool FindSelectedSignature(Page page, WindowsPoint leftTopPoint, WindowsPoint rightBottomPoint)
        {
            try
            {
                if (page != null)
                {
                    var rect = new Rectangle(leftTopPoint.X, leftTopPoint.Y, rightBottomPoint.X, rightBottomPoint.Y);
                    var result = page.Annotations.ToList().Exists(annotation => (annotation is StampAnnotation stampAnnotation)
                        && IsRectIntersect(stampAnnotation.GetRectangle(false), rect));

                    return result;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        // Find if there are signature (StampAnnotation) selected by selectedPoint
        public static bool FindSelectedSignature(Page page, WindowsPoint selectedPoint)
        {
            try
            {
                if (page != null)
                {
                return page.Annotations.ToList().Exists(annotation => (annotation is StampAnnotation stampAnnotation)
                        && IsSelectedPointInsideItemRect(stampAnnotation.GetRectangle(false), ConvertFromSystemPoint(selectedPoint)));
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        // Save pdf
        public static bool SaveCurrentPdf(Document pdfDocument, string newFileName)
        {
            try
            {
                pdfDocument.Save(newFileName);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static HighlightAnnotation HighLightTextFragment(Page page, List<TextFragment> textFragments, System.Drawing.Color color)
        {
            if (textFragments.Count == 1)
            {
                return new HighlightAnnotation(page, textFragments[0].Rectangle)
                {
                    Color = Color.FromRgb(color),
                    Opacity = color.A * 1.0 / byte.MaxValue,
                    Modified = DateTime.Now,
                    QuadPoints = new Point[]
                    {
                        new Point(textFragments[0].Rectangle.LLX, textFragments[0].Rectangle.URY),
                        new Point(textFragments[0].Rectangle.URX, textFragments[0].Rectangle.URY),
                        new Point(textFragments[0].Rectangle.LLX, textFragments[0].Rectangle.LLY),
                        new Point(textFragments[0].Rectangle.URX, textFragments[0].Rectangle.LLY)
                    }
                };
            }

            var offset = 0;
            var quadPoints = new Point[textFragments.Count * 4];
            foreach (var fragment in textFragments)
            {
                var limitLength = 1;
                if (fragment.Rectangle.URX - fragment.Rectangle.LLX <= 2
                    || fragment.Rectangle.URY - fragment.Rectangle.LLY <= 2)
                {
                    limitLength = 0;
                }
                quadPoints[offset + 0] = new Point(fragment.Rectangle.LLX + limitLength, fragment.Rectangle.URY - limitLength);
                quadPoints[offset + 1] = new Point(fragment.Rectangle.URX - limitLength, fragment.Rectangle.URY - limitLength);
                quadPoints[offset + 2] = new Point(fragment.Rectangle.LLX + limitLength, fragment.Rectangle.LLY + limitLength);
                quadPoints[offset + 3] = new Point(fragment.Rectangle.URX - limitLength, fragment.Rectangle.LLY + limitLength);
                offset += 4;
            }

            var llx = quadPoints.Min(pt => pt.X);
            var lly = quadPoints.Min(pt => pt.Y);
            var urx = quadPoints.Max(pt => pt.X);
            var ury = quadPoints.Max(pt => pt.Y);
            return new HighlightAnnotation(page, new Rectangle(llx, lly, urx, ury))
            {
                Color = Color.FromRgb(color),
                Opacity = color.A * 1.0 / byte.MaxValue,
                Modified = DateTime.Now,
                QuadPoints = quadPoints
            };
        }

        private static bool IsSelectedPointInsideItemRect(Rectangle itemRect, Point point)
        {
            return itemRect.Contains(point);
        }

        private static bool IsItemRectInsideSelectedRect(Rectangle itemRect, Rectangle selectedRect)
        {
            return selectedRect.Contains(new Point(itemRect.LLX, itemRect.LLY)) && selectedRect.Contains(new Point(itemRect.URX, itemRect.URY));
        }

        private static bool IsRectIntersect(Rectangle itemRect, Rectangle selectedRect)
        {
            return itemRect.IsIntersect(selectedRect);
        }

        private static Point ConvertFromSystemPoint(WindowsPoint point)
        {
            return new Point(point.X, point.Y);
        }

        public static Aspose.Pdf.Rotation ConvertDegreesToRotation(DegreesSelected degrees)
        {
            switch (degrees)
            {
                case DegreesSelected.On90DegreesClockwise:
                    return Aspose.Pdf.Rotation.on90;
                case DegreesSelected.On180Degrees:
                    return Aspose.Pdf.Rotation.on180;
                case DegreesSelected.On270Clockwise:
                    return Aspose.Pdf.Rotation.on270;
            }
            return Aspose.Pdf.Rotation.None;
        }

        public static Aspose.Pdf.Rotation CombineRotation(Aspose.Pdf.Rotation curRotation, Aspose.Pdf.Rotation srcRotation)
        {
            const int RotationEnumCount = 4;
            int result = ((int)curRotation + (int)srcRotation) % RotationEnumCount;
            return (Aspose.Pdf.Rotation)result;
        }

        public static bool DoRotatePages(Page[] pages, DegreesSelected degrees)
        {
            try
            {
                foreach (var page in pages)
                {
                    page.Rotate = CombineRotation(page.Rotate, ConvertDegreesToRotation(degrees));
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static string[] FilterFilesSupportConvertToPdf(string[] fileList)
        {
            // Supported file types: PDF, DOC*, XLS*, PPT*, BMP, CCITT, EMF, EXIF, GIF, JFIF, ICO, JP*G, PNG, TIF*, WMF 
            var supportedFileList = new List<string>();

            foreach (var file in fileList)
            {
                var fstrExt = Path.GetExtension(file).ToLower();
                if (FileTypeSupportConvert.Contains(fstrExt))
                {
                    supportedFileList.Add(file);
                }
            }

            return supportedFileList.ToArray();
        }

        public static bool IsCloudItem(WzSvcProviderIDs id)
        {
            if (id == WzSvcProviderIDs.SPID_CLOUD_DROPBOX
                || id == WzSvcProviderIDs.SPID_CLOUD_BOX
                || id == WzSvcProviderIDs.SPID_CLOUD_SKYDRIVE
                || id == WzSvcProviderIDs.SPID_CLOUD_ONEDRIVE
                || id == WzSvcProviderIDs.SPID_CLOUD_GOOGLE
                || id == WzSvcProviderIDs.SPID_CLOUD_SUGARSYNC
                || id == WzSvcProviderIDs.SPID_CLOUD_CLOUDME
                || id == WzSvcProviderIDs.SPID_CLOUD_FTP
                || id == WzSvcProviderIDs.SPID_CLOUD_AMAZON_S3
                || id == WzSvcProviderIDs.SPID_CLOUD_YOUSENDIT
                || id == WzSvcProviderIDs.SPID_CLOUD_AZURE
                || id == WzSvcProviderIDs.SPID_CLOUD_FACEBOOK
                || id == WzSvcProviderIDs.SPID_CLOUD_TWITTER
                || id == WzSvcProviderIDs.SPID_CLOUD_LINKEDIN
                || id == WzSvcProviderIDs.SPID_CLOUD_EMAIL
                || id == WzSvcProviderIDs.SPID_CLOUD_SHAREPOINT
                || id == WzSvcProviderIDs.SPID_CLOUD_ZIPSHARE
                || id == WzSvcProviderIDs.SPID_CLOUD_MEDIAFIRE
                || id == WzSvcProviderIDs.SPID_CLOUD_HIGHTAIL
                || id == WzSvcProviderIDs.SPID_CLOUD_OFFICE365
                || id == WzSvcProviderIDs.SPID_CLOUD_SWIFTSTACK
                || id == WzSvcProviderIDs.SPID_CLOUD_GOOGLECLOUD
                || id == WzSvcProviderIDs.SPID_CLOUD_IBMCLOUD
                || id == WzSvcProviderIDs.SPID_CLOUD_RACKSPACE
                || id == WzSvcProviderIDs.SPID_CLOUD_OPENSTACK
                || id == WzSvcProviderIDs.SPID_CLOUD_ALIBABACLOUD
                || id == WzSvcProviderIDs.SPID_CLOUD_WASABI
                || id == WzSvcProviderIDs.SPID_CLOUD_S3COMPATIBLE
                || id == WzSvcProviderIDs.SPID_CLOUD_AZUREBLOB
                || id == WzSvcProviderIDs.SPID_CLOUD_WEBDAV
                || id == WzSvcProviderIDs.SPID_CLOUD_CENTURYLINK
                || id == WzSvcProviderIDs.SPID_CLOUD_OVH
                || id == WzSvcProviderIDs.SPID_CLOUD_IONOS
                || id == WzSvcProviderIDs.SPID_CLOUD_TEAMSSHAREPOINT
                || id == WzSvcProviderIDs.SPID_CLOUD_NASCLOUD)
            {
                return true;
            }

            return false;
        }

        public static bool IsLocalPortableDeviceItem(WzSvcProviderIDs id)
        {
            return id == WzSvcProviderIDs.SPID_LOCAL_PORTABLE_DEVICE;
        }

        public static WzCloudItem4 InitWzCloudItem()
        {
            var profile = new WzProfile2() { authId = null, Id = WzSvcProviderIDs.SPID_LOCAL_DRIVE, name = null };

            var systemCreatedTime = new SYSTEMTIME()
            {
                wYear = 0,
                wMonth = 0,
                wDay = 0,
                wDayOfWeek = 0,
                wHour = 0,
                wMinute = 0,
                wSecond = 0,
                wMilliseconds = 0
            };

            var systemModifiedTime = new SYSTEMTIME()
            {
                wYear = 0,
                wMonth = 0,
                wDay = 0,
                wDayOfWeek = 0,
                wHour = 0,
                wMinute = 0,
                wSecond = 0,
                wMilliseconds = 0
            };

            var item = new WzCloudItem4()
            {
                profile = profile,
                itemId = null,
                parentId = null,
                name = null,
                path = null,
                length = 0,
                isFolder = false,
                isDownloadable = false,
                created = systemCreatedTime,
                modified = systemModifiedTime
            };

            return item;
        }

        public static WzCloudItem4 InitCloudItemFromPath(string localFilePath)
        {
            var fileInfo = new FileInfo(localFilePath);
            bool isDirectory = (fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
            DateTime createTime = File.GetCreationTime(localFilePath);
            DateTime modifiedTime = File.GetLastWriteTime(localFilePath);
            var profile = new WzProfile2() { authId = "30", Id = WzSvcProviderIDs.SPID_LOCAL_DRIVE, name = null };

            var systemCreatedTime = new SYSTEMTIME()
            {
                wYear = (ushort)createTime.Year,
                wMonth = (ushort)createTime.Month,
                wDay = (ushort)createTime.Day,
                wDayOfWeek = (ushort)createTime.DayOfWeek,
                wHour = (ushort)createTime.Hour,
                wMinute = (ushort)createTime.Minute,
                wSecond = (ushort)createTime.Second,
                wMilliseconds = (ushort)createTime.Millisecond
            };

            var systemModifiedTime = new SYSTEMTIME()
            {
                wYear = (ushort)modifiedTime.Year,
                wMonth = (ushort)modifiedTime.Month,
                wDay = (ushort)modifiedTime.Day,
                wDayOfWeek = (ushort)modifiedTime.DayOfWeek,
                wHour = (ushort)modifiedTime.Hour,
                wMinute = (ushort)modifiedTime.Minute,
                wSecond = (ushort)modifiedTime.Second,
                wMilliseconds = (ushort)modifiedTime.Millisecond
            };

            var item = new WzCloudItem4()
            {
                profile = profile,
                itemId = localFilePath,
                parentId = fileInfo.Directory.FullName,
                name = fileInfo.Name,
                path = localFilePath,
                length = isDirectory ? 0 : fileInfo.Length,
                isFolder = isDirectory,
                isDownloadable = !isDirectory,
                created = systemCreatedTime,
                modified = systemModifiedTime
            };

            return item;
        }

        public static WzCloudItem4 GetOpenPickerDefaultFolder()
        {
            WzCloudItem4 defaultFolder = InitWzCloudItem();

            defaultFolder.itemId = PdfUtilSettings.Instance.RecordOpenPickerPath;
            defaultFolder.profile.authId = PdfUtilSettings.Instance.RecordOpenPickerAuthId;

            return defaultFolder;
        }

        public static void SetOpenPickerDefaultFolder(WzCloudItem4 item)
        {
            string folderPath = item.itemId;

            if (IsCloudItem(item.profile.Id))
            {
                folderPath = item.parentId;
            }
            else if (IsLocalPortableDeviceItem(item.profile.Id))
            {
                folderPath = item.path;
            }
            else if (!item.isFolder)
            {
                folderPath = Path.GetDirectoryName(folderPath);
            }

            PdfUtilSettings.Instance.RecordOpenPickerPath = folderPath;
            PdfUtilSettings.Instance.RecordOpenPickerAuthId = item.profile.authId;
        }

        public static WzCloudItem4 GetSavePickerDefaultFolder()
        {
            WzCloudItem4 defaultFolder = InitWzCloudItem();

            defaultFolder.itemId = PdfUtilSettings.Instance.RecordSavePickerPath;
            defaultFolder.profile.authId = PdfUtilSettings.Instance.RecordSavePickerAuthId;

            return defaultFolder;
        }

        public static void SetSavePickerDefaultFolder(WzCloudItem4 item)
        {
            string folderPath = item.itemId;

            if (IsCloudItem(item.profile.Id))
            {
                folderPath = item.parentId;
            }
            else if (IsLocalPortableDeviceItem(item.profile.Id))
            {
                folderPath = item.path;
            }
            else
            {
                if (item.itemId == string.Empty || item.itemId == null)
                {
                    // Upload entry doesn't have id 
                    folderPath = Path.Combine(item.parentId, item.name);
                }

                if (!item.isFolder)
                {
                    folderPath = Path.GetDirectoryName(folderPath);
                }
            }

            PdfUtilSettings.Instance.RecordSavePickerPath = folderPath;
            PdfUtilSettings.Instance.RecordSavePickerAuthId = item.profile.authId;
        }

        public static BitmapSource BitmapSourceFromImage(System.Windows.Controls.Image image, int dpi = 96)
        {
            var rtb = new RenderTargetBitmap((int)image.ActualWidth, (int)image.ActualHeight, dpi, dpi, PixelFormats.Pbgra32);
            rtb.Render(image);

            return rtb;
        }

        public static BitmapSource BitmapSourceFromBrush(Brush drawingBrush, int width, int heigth, int dpi = 96)
        {
            var rtb = new RenderTargetBitmap(width, heigth, dpi, dpi, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                context.DrawRectangle(drawingBrush, null, new System.Windows.Rect(0, 0, width, heigth));
            }
            rtb.Render(drawingVisual);
            return rtb;
        }

        public static BitmapSource LoadImageFromPath(string path)
        {
            BitmapSource bitmap = null;
            string ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    var jpegBitmap = new JpegBitmapDecoder(new Uri(path), BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad).Frames[0];
                    bitmap = CorrectWithExifOrientation(jpegBitmap);
                    break;

                case ".png":
                    bitmap = new PngBitmapDecoder(new Uri(path), BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad).Frames[0];
                    break;

                case ".bmp":
                    bitmap = new BmpBitmapDecoder(new Uri(path), BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad).Frames[0];
                    break;

                case ".tif":
                case ".tiff":
                    var TiffBitmap = new TiffBitmapDecoder(new Uri(path), BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad).Frames[0];
                    bitmap = CorrectWithExifOrientation(TiffBitmap);
                    break;

                case ".ico":
                case ".icon":
                    bitmap = new IconBitmapDecoder(new Uri(path), BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad).Frames[0];
                    break;

                default:
                    break;
            }

            return new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
        }

        public static BitmapSource CorrectWithExifOrientation(BitmapSource bitmap)
        {
            if (bitmap.Metadata is BitmapMetadata metadata && metadata.ContainsQuery("System.Photo.Orientation"))
            {
                // Respect EXIF orientation
                var orientation = metadata.GetQuery("System.Photo.Orientation");
                if (orientation != null)
                {
                    switch ((ushort)orientation)
                    {
                        case 2: // Mirror horizontal
                            {
                                bitmap = new TransformedBitmap(bitmap, new ScaleTransform(-1, 1));
                                break;
                            }

                        case 3: // Rotate 180 degrees
                            {
                                bitmap = new TransformedBitmap(bitmap, new RotateTransform(180));
                                break;
                            }

                        case 4: // Mirror vertical
                            {
                                bitmap = new TransformedBitmap(new TransformedBitmap(bitmap, new RotateTransform(180)), new ScaleTransform(-1, 1));
                                break;
                            }

                        case 5: // Mirror horizontal and rotate 270 degrees
                            {
                                bitmap = new TransformedBitmap(new TransformedBitmap(bitmap, new RotateTransform(-90)), new ScaleTransform(1, -1));
                                break;
                            }

                        case 6: // Rotate 90 degrees
                            {
                                bitmap = new TransformedBitmap(bitmap, new RotateTransform(90));
                                break;
                            }

                        case 7: // Mirror horizontal and rotate 90 degrees
                            {
                                bitmap = new TransformedBitmap(new TransformedBitmap(bitmap, new RotateTransform(90)), new ScaleTransform(1, -1));
                                break;
                            }

                        case 8: // Rotate 270 degrees
                            {
                                bitmap = new TransformedBitmap(bitmap, new RotateTransform(-90));
                                break;
                            }

                        case 1: // Normal
                        default:
                            break;
                    }
                }
            }
            return bitmap;
        }
    }
}
