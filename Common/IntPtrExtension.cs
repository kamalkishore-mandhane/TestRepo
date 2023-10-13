using System;

namespace Applets.Common
{
    public static class IntPtrExtension
    {
        public static unsafe int ToIntUnsafe(this IntPtr value)
        {
            return (int)value.ToPointer();
        }

        public static unsafe short ToShortUnsafe(this IntPtr value)
        {
            return (short)value.ToPointer();
        }

        public static unsafe IntPtr Offset(this IntPtr value, int offset)
        {
            return (IntPtr)((byte*)(value.ToPointer()) + offset);
        }

        public static unsafe short LOWORD(this IntPtr value)
        {
            return (short)value.ToPointer();
        }

        public static unsafe short HIWORD(this IntPtr value)
        {
            return (short)((int)value.ToPointer() >> 16);
        }
    }
}
