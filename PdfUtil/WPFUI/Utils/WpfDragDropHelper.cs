using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using ComIDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace PdfUtil.WPFUI.Utils
{
    static class WpfDragDropHelper
    {
        #region DLL/COM imports

        [DllImport("user32.dll")]
        public static extern uint RegisterClipboardFormat(string lpszFormatName);

        [ComImport]
        [Guid("4657278A-411B-11d2-839A-00C04FD918D0")]
        public class DragDropHelper
        {
        }

        [ComImport]
        [Guid("4657278B-411B-11D2-839A-00C04FD918D0")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDropTargetHelper
        {
            uint DragEnter([In] IntPtr hwndTarget, [In, MarshalAs(UnmanagedType.Interface)] ComIDataObject dataObject, [In] ref NativeMethods.POINT pt, [In] int effect);

            uint DragLeave();

            uint DragOver([In] ref NativeMethods.POINT pt, [In] int effect);

            uint Drop([In, MarshalAs(UnmanagedType.Interface)] ComIDataObject dataObject, [In] ref NativeMethods.POINT pt, [In] int effect);

            uint Show([In] bool show);
        }

        #endregion

        #region public types

        // Values used with the DROPDESCRIPTION structure to specify the drop image.
        public enum DropImageType
        {
            Invalid = -1,
            None = 0,
            Copy = 1,
            Move = 2,
            Link = 4,
            Label = 6,
            Warning = 7,
            NoImage = 8
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Size = 1044)]
        public struct DropDescription
        {
            // A DROPIMAGETYPE indicating the stock image to use.
            public DropImageType type;

            // Text such as "Move to %1".
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szMessage;

            // Text such as "Documents", inserted as specified by szMessage.
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szInsert;
        }

        #endregion

        private static readonly DragDropHelper dragDropHelper = new DragDropHelper();

        private const string DropDescriptionFormat = "DropDescription";

        public static void DragEnter(DragEventArgs e, Visual visual, IInputElement relativeTo)
        {
            NativeMethods.POINT pt;
            Point point = e.GetPosition(relativeTo);
            pt.X = (int)point.X;
            pt.Y = (int)point.Y;
            HwndSource source = PresentationSource.FromVisual(visual) as HwndSource;
            ((IDropTargetHelper)dragDropHelper).DragEnter(source.Handle, (ComIDataObject)e.Data, ref pt, (int)e.Effects);
        }

        public static void DragOver(DragEventArgs e, IInputElement relativeTo)
        {
            NativeMethods.POINT pt;
            Point point = e.GetPosition(relativeTo);
            pt.X = (int)point.X;
            pt.Y = (int)point.Y;
            ((IDropTargetHelper)dragDropHelper).DragOver(ref pt, (int)e.Effects);
        }

        public static void Drop(DragEventArgs e, IInputElement relativeTo)
        {
            NativeMethods.POINT pt;
            Point point = e.GetPosition(relativeTo);
            pt.X = (int)point.X;
            pt.Y = (int)point.Y;
            ((IDropTargetHelper)dragDropHelper).Drop((ComIDataObject)e.Data, ref pt, (int)e.Effects);
        }

        public static void DragLeave()
        {
            ((IDropTargetHelper)dragDropHelper).DragLeave();
        }

        // Sets the drop description for the drag image manager.
        public static void SetDropDescription(System.Windows.IDataObject dataObject, DropImageType type, string message, string insert)
        {
            if (string.IsNullOrEmpty(message) || message.Length > 259 || (!string.IsNullOrEmpty(insert) && insert.Length > 259))
            {
                // return if no message or message/insert too long
                return;
            }

            FillFormatETC(DropDescriptionFormat, TYMED.TYMED_HGLOBAL, out FORMATETC formatETC);
            IntPtr num = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(DropDescription)));
            var dropDescription = new DropDescription
            {
                type = type,
                szMessage = message,
                szInsert = insert
            };

            try
            {
                Marshal.StructureToPtr((object)dropDescription, num, false);
                STGMEDIUM medium;
                medium.pUnkForRelease = (object)null;
                medium.tymed = TYMED.TYMED_HGLOBAL;
                medium.unionmember = num;
                ((ComIDataObject)dataObject).SetData(ref formatETC, ref medium, true);
            }
            catch
            {
                Marshal.FreeHGlobal(num);
            }
        }

        private static void FillFormatETC(string format, TYMED tymed, out FORMATETC formatETC)
        {
            formatETC.cfFormat = (short)RegisterClipboardFormat(format);
            formatETC.dwAspect = DVASPECT.DVASPECT_CONTENT;
            formatETC.lindex = -1;
            formatETC.ptd = IntPtr.Zero;
            formatETC.tymed = tymed;
        }
    }
}
