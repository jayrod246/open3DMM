using System;

namespace Open3dmm.WinApi
{
    public class PInvoke
    {
        public static IntPtr Call(string libraryName, string procedureName)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address);
        }

        public static IntPtr Call(string libraryName, string procedureName, IntPtr arg1)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address, arg1);
        }

        public static IntPtr Call(string libraryName, string procedureName, IntPtr arg1, IntPtr arg2)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address, arg1, arg2);
        }

        public static IntPtr Call(string libraryName, string procedureName, IntPtr arg1, IntPtr arg2, IntPtr arg3)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address, arg1, arg2, arg3);
        }

        public static IntPtr Call(string libraryName, string procedureName, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address, arg1, arg2, arg3, arg4);
        }

        public static IntPtr Call(string libraryName, string procedureName, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address, arg1, arg2, arg3, arg4, arg5);
        }

        public static IntPtr Call(string libraryName, string procedureName, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public static IntPtr Call(string libraryName, string procedureName, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public static IntPtr Call(string libraryName, string procedureName, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public static IntPtr Call(string libraryName, string procedureName, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public static IntPtr Call(string libraryName, string procedureName, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public static IntPtr Call(string libraryName, string procedureName, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        public static IntPtr Call(string libraryName, string procedureName, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12)
        {
            var address = Kernel32.GetProcAddress(Kernel32.GetModuleHandle(libraryName), procedureName);
            return UnmanagedFunctionCall.StdCall(address, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }
    }
}
