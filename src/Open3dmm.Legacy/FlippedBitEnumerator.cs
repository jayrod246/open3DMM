using System.Runtime.CompilerServices;

namespace Open3dmm
{
    public unsafe struct FlippedBitEnumerator
    {
        private byte* _pointer;
        private int _bit;
        private int _remainingBytes;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FlippedBitEnumerator(byte* pointer, int count)
        {
            _bit = -1;
            _pointer = pointer;
            _remainingBytes = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            var remaining = _remainingBytes - 1;
            if (remaining < 0)
                return false;

            var bit = _bit + 1;
            if (bit < 8)
            {
                _bit = bit;
                return true;
            }

            _bit = 0;
            _remainingBytes = remaining;
            _pointer++;
            return true;
        }

        public int Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return (*_pointer >> (7 - _bit)) & 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FlippedBitEnumerator GetEnumerator()
            => this;
    }
}
