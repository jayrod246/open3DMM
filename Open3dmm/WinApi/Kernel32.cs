using System;
using System.Runtime.InteropServices;

namespace Open3dmm.WinApi
{
    internal static class Kernel32
    {
        [DllImport(LibraryNames.KERNEL32)]
        public static extern IntPtr LoadLibrary(string dllPath);

        [DllImport(LibraryNames.KERNEL32)]
        public static extern bool FreeLibrary(IntPtr hmodule);

        [DllImport(LibraryNames.KERNEL32, CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport(LibraryNames.KERNEL32, CharSet = CharSet.Auto)]
        public static extern IntPtr GetProcAddress(IntPtr hmodule, [MarshalAs(UnmanagedType.LPStr)]string procName);

        [DllImport(LibraryNames.KERNEL32, SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [MarshalAs(UnmanagedType.AsAny)] object lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);
    }
}
