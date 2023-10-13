using Aspose.Pdf.Annotations;
using PdfUtil.WPFUI.ViewModel;
using System;
using System.Windows;
using System.Windows.Media;

namespace PdfUtil.WPFUI.Model
{
    
    public class CommentItem : ObservableObject
    {
        private Aspose.Pdf.Page _pdfPage;
        private TextAnnotation _textAnnotation;

        private Rect _addPosition;
        private bool _isSelected;

        public CommentItem(Aspose.Pdf.Page pageItem, TextAnnotation textAnnotation)
        {
            _pdfPage = pageItem;
            _textAnnotation = textAnnotation;
        }

        public CommentItem(Aspose.Pdf.Page pageItem, Rect position)
        {
            _pdfPage = pageItem;
            _addPosition = position;
        }

        public int PageNumber => _pdfPage.Number;

        public string PageNumberTitle => string.Format(Properties.Resources.COMMENT_PAGE_TITLE, _pdfPage.Number);

        public Brush ColorBrush => PdfHelper.GetAnnotationBrush(_textAnnotation?.Title ?? Environment.UserName);

        public string User => _textAnnotation?.Title ?? Environment.UserName;

        public DateTime ModifyDate => _textAnnotation?.Modified ?? DateTime.Now;

        public string ModifyDateString => _textAnnotation?.Modified.ToString("MMM d, yyyy HH:mmtt", System.Globalization.CultureInfo.CurrentUICulture)
                                            ?? Properties.Resources.COMMENT_ADDING_TEXT;

        public string Content => string.IsNullOrEmpty(_textAnnotation?.Contents ?? null) ? Properties.Resources.COMMENT_EMPTY_CONTENT : _textAnnotation.Contents;

        public Rect AddPosition => _addPosition;

        public bool IsInAddingMode => _textAnnotation == null;

        public TextAnnotation Annotation
        {
            get => _textAnnotation;
            set
            {
                if (_textAnnotation == null && value != null)
                {
                    _textAnnotation = value;
                    Notify(nameof(IsInAddingMode));
                    Notify(nameof(Content));
                    Notify(nameof(ModifyDateString));
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    Notify(nameof(IsSelected));
                }
            }
        }

        public void SetPage(Aspose.Pdf.Page page)
        {
            _pdfPage = page;
        }
    }
}
