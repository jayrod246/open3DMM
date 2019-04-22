using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    public class ManagedArray
    {
        private struct PinnedArray
        {
            public GCHandle Handle;
            public Array Array;
        }

        private static Dictionary<IntPtr, PinnedArray> arrays = new Dictionary<IntPtr, PinnedArray>();
        public static T[] FromPointer<T>(IntPtr address, int length)
        {
            if (!arrays.TryGetValue(address, out var pin) || pin.Array.Length != length || pin.Array.GetType().GetElementType() != typeof(T))
            {
                unsafe
                {
                    PinnedArray newPin;
                    newPin.Array = FromPointerOneTime<T>(new IntPtr(*(void**)address), length, out newPin.Handle);
                    *(void**)address = (void*)newPin.Handle.AddrOfPinnedObject();
                    arrays[address] = pin = newPin;
                }
            }
            return (T[])pin.Array;
        }

        public static T[] FromPointerOneTime<T>(IntPtr address, int length)
        {
            return FromPointerOneTime<T>(address, length, out _);
        }

        public static T[] FromPointerOneTime<T>(IntPtr address, int length, out GCHandle handle)
        {
            var arr = new T[length];
            handle = GCHandle.Alloc(arr, GCHandleType.Pinned); ;
            unsafe
            {
                void* b = (void*)handle.AddrOfPinnedObject();
                var size = Unsafe.SizeOf<T>() * length;
                Buffer.MemoryCopy((void*)address, b, size, size);
            }
            return arr;
        }
    }
}
