using System;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    #region ThisCall
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall0(IntPtr ecx);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall1(IntPtr ecx, IntPtr arg1);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall2(IntPtr ecx, IntPtr arg1, IntPtr arg2);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall3(IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall4(IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall5(IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall6(IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall7(IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall8(IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall9(IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall10(IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall11(IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate IntPtr ThisCall12(IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12);
    #endregion

    #region Cdecl
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl0();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl1(IntPtr arg1);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl2(IntPtr arg1, IntPtr arg2);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl3(IntPtr arg1, IntPtr arg2, IntPtr arg3);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl4(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl5(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl6(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl7(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl8(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl9(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl10(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl11(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Cdecl12(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12);
    #endregion

    #region StdCall
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall0();
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall1(IntPtr arg1);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall2(IntPtr arg1, IntPtr arg2);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall3(IntPtr arg1, IntPtr arg2, IntPtr arg3);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall4(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall5(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall6(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall7(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall8(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall9(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall10(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall11(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr StdCall12(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12);
    #endregion

    public static class UnmanagedFunctionCall
    {
        #region ThisCall
        public static IntPtr ThisCall(IntPtr address, IntPtr ecx)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall0>(address).Invoke(ecx);
        }

        public static IntPtr ThisCall(IntPtr address, IntPtr ecx, IntPtr arg1)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall1>(address).Invoke(ecx, arg1);
        }

        public static IntPtr ThisCall(IntPtr address, IntPtr ecx, IntPtr arg1, IntPtr arg2)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall2>(address).Invoke(ecx, arg1, arg2);
        }

        public static IntPtr ThisCall(IntPtr address, IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall3>(address).Invoke(ecx, arg1, arg2, arg3);
        }

        public static IntPtr ThisCall(IntPtr address, IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall4>(address).Invoke(ecx, arg1, arg2, arg3, arg4);
        }

        public static IntPtr ThisCall(IntPtr address, IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall5>(address).Invoke(ecx, arg1, arg2, arg3, arg4, arg5);
        }

        public static IntPtr ThisCall(IntPtr address, IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall6>(address).Invoke(ecx, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public static IntPtr ThisCall(IntPtr address, IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall7>(address).Invoke(ecx, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public static IntPtr ThisCall(IntPtr address, IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall8>(address).Invoke(ecx, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public static IntPtr ThisCall(IntPtr address, IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall9>(address).Invoke(ecx, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public static IntPtr ThisCall(IntPtr address, IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall10>(address).Invoke(ecx, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public static IntPtr ThisCall(IntPtr address, IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall11>(address).Invoke(ecx, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        public static IntPtr ThisCall(IntPtr address, IntPtr ecx, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12)
        {
            return Marshal.GetDelegateForFunctionPointer<ThisCall12>(address).Invoke(ecx, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }
        #endregion

        #region StdCall
        public static IntPtr StdCall(IntPtr address)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall0>(address).Invoke();
        }

        public static IntPtr StdCall(IntPtr address, IntPtr arg1)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall1>(address).Invoke(arg1);
        }

        public static IntPtr StdCall(IntPtr address, IntPtr arg1, IntPtr arg2)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall2>(address).Invoke(arg1, arg2);
        }

        public static IntPtr StdCall(IntPtr address, IntPtr arg1, IntPtr arg2, IntPtr arg3)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall3>(address).Invoke(arg1, arg2, arg3);
        }

        public static IntPtr StdCall(IntPtr address, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall4>(address).Invoke(arg1, arg2, arg3, arg4);
        }

        public static IntPtr StdCall(IntPtr address, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall5>(address).Invoke(arg1, arg2, arg3, arg4, arg5);
        }

        public static IntPtr StdCall(IntPtr address, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall6>(address).Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public static IntPtr StdCall(IntPtr address, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall7>(address).Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public static IntPtr StdCall(IntPtr address, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall8>(address).Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public static IntPtr StdCall(IntPtr address, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall9>(address).Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public static IntPtr StdCall(IntPtr address, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall10>(address).Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public static IntPtr StdCall(IntPtr address, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall11>(address).Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        public static IntPtr StdCall(IntPtr address, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12)
        {
            return Marshal.GetDelegateForFunctionPointer<StdCall12>(address).Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }
        #endregion
    }
}
