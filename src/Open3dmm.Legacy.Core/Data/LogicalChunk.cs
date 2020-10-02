using Open3dmm;

namespace Open3dmm.Core.Data
{
    public struct LogicalChunk
    {
        private const int SectionLengthShift = 8;
        private const int FlagsMask = 0xFF;

        private ChunkIdentifier identifier;
        private int sectionOffset;
        private int flags_sectionLength;
        private ushort numReferences;
        private ushort timesReferenced;

        public LogicalChunk(ChunkIdentifier identifier, int sectionOffset, int sectionLength, ChunkFlags flags, ushort numReferences, ushort timesReferenced)
        {
            this.identifier = identifier;
            this.sectionOffset = sectionOffset;
            this.numReferences = numReferences;
            this.timesReferenced = timesReferenced;
            this.flags_sectionLength = 0;
            SectionLength = sectionLength;
            Flags = flags;
        }

        public ChunkFlags Flags {
            get => (ChunkFlags)(flags_sectionLength & FlagsMask);
            set => flags_sectionLength = (SectionLength << SectionLengthShift) | (byte)value;
        }

        public int SectionLength {
            get => flags_sectionLength >> SectionLengthShift;
            set => flags_sectionLength = (value << SectionLengthShift) | (byte)Flags;
        }
        public ChunkIdentifier Identifier { get => this.identifier; set => this.identifier = value; }
        public int SectionOffset { get => this.sectionOffset; set => this.sectionOffset = value; }
        public ushort NumReferences { get => this.numReferences; set => this.numReferences = value; }
        public ushort TimesReferenced { get => this.timesReferenced; set => this.timesReferenced = value; }

        public static readonly int Size = 20;
    }
}
