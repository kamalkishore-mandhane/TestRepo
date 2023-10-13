using System;
using System.Runtime.InteropServices;

namespace SafeShare.Converter.ConvertUtil
{
    public static class IMGVWRService
    {
        #region 32 API

        [DllImport("WZIMGV32.dll", EntryPoint = "resize_imageex3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool resize_image32(string pwszImagePath, string pwszSaveToPath, int uiWidth, int uiHeight, ref bool fResized, ref long errorCode, ref byte errInfo);

        #endregion 32 API

        #region 64 API

        [DllImport("WZIMGV64.dll", EntryPoint = "resize_imageex3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool resize_image64(string pwszImagePath, string pwszSaveToPath, int uiWidth, int uiHeight, ref bool fResized, ref long errorCode, ref byte errInfo);

        #endregion 64 API

        internal static bool Is32Bit()
        {
            return IntPtr.Size == 4;
        }

        public static bool ResizeImage(string pwszImagePath, string pwszSaveToPath, int uiWidth, int uiHeight, ref bool fResized, ref long errorCode, ref byte errInfo)
        {
            if (Is32Bit())
            {
                return resize_image32(pwszImagePath, pwszSaveToPath, uiWidth, uiHeight, ref fResized, ref errorCode, ref errInfo);
            }
            else
            {
                return resize_image64(pwszImagePath, pwszSaveToPath, uiWidth, uiHeight, ref fResized, ref errorCode, ref errInfo);
            }
        }
    }
}