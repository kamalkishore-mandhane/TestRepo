using System;
using System.Runtime.InteropServices;

namespace SafeShare.WPFUI.Utils
{
    internal static class StackGuard
    {
        // The ReservedSize is large than actual needs. And CLR2 x64 need more size than CLR4.
        private const int ReservedSize = 0x30000;

        [ThreadStatic]
        private static unsafe void* _guard;

        public static unsafe bool CheckForSufficientStack()
        {
            void* guard = _guard;
            void* ptr = &guard;
            if (guard == null)
            {
                NativeMethods.MEMORY_BASIC_INFORMATION memInfo = new NativeMethods.MEMORY_BASIC_INFORMATION();
                NativeMethods.VirtualQuery((IntPtr)ptr, ref memInfo, (UIntPtr)Marshal.SizeOf(typeof(NativeMethods.MEMORY_BASIC_INFORMATION)));
                _guard = guard = (byte*)memInfo.AllocationBase.ToPointer() + ReservedSize;
            }
            return ptr > guard;
        }

        public static class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct MEMORY_BASIC_INFORMATION
            {
                public IntPtr BaseAddress;
                public IntPtr AllocationBase;
                public uint AllocationProtect;
                public UIntPtr RegionSize;
                public uint State;
                public uint Protect;
                public uint Type;
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern UIntPtr VirtualQuery(IntPtr address, ref MEMORY_BASIC_INFORMATION buffer, UIntPtr sizeOfBuffer);
        }
    }
}