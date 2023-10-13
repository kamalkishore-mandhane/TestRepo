using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PdfUtil.WPFUI.Utils
{
    public static class WebAuthBroker
    {

        public enum WebAuthExtraDataType
        {
            FileAssociation = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WebAuthExtraData
        {
            public int dataStringLength;
            public WebAuthExtraDataType dataType;
            public IntPtr data;
        }

        public static Task<string> ShowAuth(IntPtr owner, IntPtr icon, string title, int width, int height, string url, string endUrl, bool titleMode, bool silent)
        {
            return Task<string>.Factory.StartNew(() =>
            {
                var sb = new StringBuilder(4096);
                switch (NativeMethods.WebAuth(owner, icon, title, width, height, url, endUrl, titleMode, sb, 4096, silent))
                {
                    case 0:
                        return sb.ToString();
                    case 1:
                        throw new TaskCanceledException();
                    default:
                        throw new InvalidOperationException();
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public static void BlockUIAndShowAuth(IntPtr owner, IntPtr icon, string title, int width, int height, string url, string endUrl, bool titleMode, bool silent, WebAuthExtraData extraData = default)
        {
            try
            {
                Task<string>.Factory.StartNew(() =>
                {
                    var sb = new StringBuilder(4096);
                    switch (NativeMethods.WebAuth(owner, icon, title, width, height, url, endUrl, titleMode, sb, 4096, silent, extraData))
                    {
                        case 0:
                            return sb.ToString();
                        case 1:
                            throw new TaskCanceledException();
                        default:
                            throw new InvalidOperationException();
                    }
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).WaitWithMsgPump();
            }
            catch (Exception e)
            {
                if (e.InnerException is TaskCanceledException)
                {
                    return;
                }
                throw e;
            }
        }

        private static class NativeMethods
        {
            [DllImport("WebAuthBroker32.dll", CharSet = CharSet.Unicode, EntryPoint = "WebAuth", ExactSpelling = true)]
            private static extern int WebAuth32(IntPtr owner, IntPtr icon, string title, int width, int height, string url, string endUrl, bool titleMode, StringBuilder data, int len, bool silent, WebAuthExtraData extraData);

            [DllImport("WebAuthBroker64.dll", CharSet = CharSet.Unicode, EntryPoint = "WebAuth", ExactSpelling = true)]
            private static extern int WebAuth64(IntPtr owner, IntPtr icon, string title, int width, int height, string url, string endUrl, bool titleMode, StringBuilder data, long len, bool silent, WebAuthExtraData extraData);

            public static int WebAuth(IntPtr owner, IntPtr icon, string title, int width, int height, string url, string endUrl, bool titleMode, StringBuilder data, int len, bool silent, WebAuthExtraData extraData = default)
            {
                if (IntPtr.Size == 4)
                {
                    return WebAuth32(owner, icon, title, width, height, url, endUrl, titleMode, data, len, silent, extraData);
                }
                else
                {
                    return WebAuth64(owner, icon, title, width, height, url, endUrl, titleMode, data, len, silent, extraData);
                }
            }
        }
    }
}
