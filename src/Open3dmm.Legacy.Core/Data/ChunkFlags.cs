using System;

namespace Open3dmm.Core.Data
{
    [Flags]
    public enum ChunkFlags : byte
    {
        Uncompressed = 0,
        Root = 2,
        Compressed = 4,
        Unknown = 16,
        UncompressedMain = Root | Uncompressed,
        CompressedMain = Root | Compressed,
    }
}
