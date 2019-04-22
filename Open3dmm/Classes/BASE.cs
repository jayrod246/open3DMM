using System;
using System.Runtime.InteropServices;

namespace Open3dmm.Classes
{
    public struct VTABLE
    {
        public IntPtr Address;
    }

    public class BASE : NativeObject
    {
        public VTABLE Vtable => GetField<VTABLE>(0);
        public int NumReferences => GetField<int>(4);

        public ClassID GetClassID()
        {
            EnsureNotDisposed();
            return new ClassID((int)UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable, 4), NativeHandle.Address));
        }

        #region VirtualCall
        public IntPtr VirtualCall(int offset)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address);
        }
        public IntPtr VirtualCall(int offset, IntPtr arg1)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address, arg1);
        }
        public IntPtr VirtualCall(int offset, IntPtr arg1, IntPtr arg2)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address, arg1, arg2);
        }
        public IntPtr VirtualCall(int offset, IntPtr arg1, IntPtr arg2, IntPtr arg3)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address, arg1, arg2, arg3);
        }
        public IntPtr VirtualCall(int offset, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address, arg1, arg2, arg3, arg4);
        }
        public IntPtr VirtualCall(int offset, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address, arg1, arg2, arg3, arg4, arg5);
        }
        public IntPtr VirtualCall(int offset, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address, arg1, arg2, arg3, arg4, arg5, arg6);
        }
        public IntPtr VirtualCall(int offset, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }
        public IntPtr VirtualCall(int offset, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }
        public IntPtr VirtualCall(int offset, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }
        public IntPtr VirtualCall(int offset, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public IntPtr VirtualCall(int offset, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }
        public IntPtr VirtualCall(int offset, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12)
        {
            return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Vtable.Address) + offset, NativeHandle.Address, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

        #endregion
    }
}
