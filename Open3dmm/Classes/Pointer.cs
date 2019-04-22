using System;

namespace Open3dmm.Classes
{
    public ref struct Pointer<T> where T : unmanaged
    {
        private readonly IntPtr address;
        public ref T Value {
            get {
                unsafe
                {
                    return ref *(T*)address;
                }
            }
        }

        public Pointer(IntPtr address)
        {
            this.address = address;
        }
    }
}
