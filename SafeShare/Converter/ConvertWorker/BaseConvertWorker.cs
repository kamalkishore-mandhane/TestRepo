using SafeShare.WPFUI.View;
using System;
using System.Collections.Generic;

namespace SafeShare.Converter.ConvertWorker
{
    public class BaseConvertWorker
    {
        public BaseConvertWorker()
        {
        }

        public List<string> SkipConvertButZipFileList
        {
            get;
            set;
        } = new List<string>();

        public void HandleConvertError(IntPtr owner, string conversionName, string fileName, int errorCode, string errorMsg, out CONVERT_ERROR_DIALOG choice, out bool notShowAgain)
        {
            ConversionErrorDlgView.ShowDialog(owner, conversionName, fileName, errorCode, errorMsg, out choice, out notShowAgain);
        }
    }
}