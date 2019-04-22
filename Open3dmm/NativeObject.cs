using Open3dmm.Classes;
using System;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    public class NativeObject
    {
        private NativeHandle nativeHandle;

        public NativeHandle NativeHandle => nativeHandle;

        internal void SetHandle(NativeHandle nativeHandle)
        {
            if (this.nativeHandle != null)
                throw new InvalidOperationException("Native object is already associated with a native handle");
            this.nativeHandle = nativeHandle;
            Initialize();
        }

        protected ref T GetField<T>(int offset, bool boundsChecking = true) where T : unmanaged
        {
            return ref new Pointer<T>(CalculateAddressOfField(offset, boundsChecking)).Value;
        }

        protected T GetReference<T>(int offset, bool boundsChecking = true) where T : NativeObject
        {
            return FromPointer<T>(GetField<IntPtr>(offset, boundsChecking));
        }

        protected void SetReference<T>(T obj, int offset, bool boundsChecking = true) where T : NativeObject
        {
            obj.EnsureNotDisposed();
            GetField<IntPtr>(offset, boundsChecking) = obj.NativeHandle.Address;
        }

        public static T FromPointer<T>(IntPtr ptr) where T : NativeObject
        {
            if (ptr == IntPtr.Zero)
                return default;
            if (!NativeHandle.TryDereference(ptr, out var handle))
                throw new InvalidCastException();
            return handle.QueryInterface<T>();
        }

        protected virtual void Initialize()
        {
        }

        protected void EnsureNotDisposed()
        {
            if (nativeHandle.IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public static bool TryGetClassID(NativeHandle nativeHandle, out ClassID classID)
        {
            try
            {
                if (!nativeHandle.IsDisposed)
                {
                    classID = new ClassID((int)UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Marshal.ReadIntPtr(nativeHandle.Address), 4), nativeHandle.Address));
                    return true;
                }
            }
            catch
            {
            }

            classID = default;
            return false;
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public static bool TypeCheck(NativeHandle nativeHandle, ClassID classID)
        {
            try
            {
                if (!nativeHandle.IsDisposed)
                {
                    return UnmanagedFunctionCall.ThisCall(Marshal.ReadIntPtr(Marshal.ReadIntPtr(nativeHandle.Address)), nativeHandle.Address, new IntPtr(classID.Value)) != IntPtr.Zero;
                }
            }
            catch
            {
            }

            return false;
        }

        private IntPtr CalculateAddressOfField(int offset, bool boundsChecking)
        {
            EnsureNotDisposed();
            if (boundsChecking && offset >= nativeHandle.Size)
                throw new InvalidOperationException("Attempted to read outside the boundaries of a native object");
            return nativeHandle.Address + offset;
        }

        public ref byte GetPinnableReference()
        {
            EnsureNotDisposed();
            unsafe
            {
                return ref *(byte*)nativeHandle.Address;
            }
        }
    }
}
