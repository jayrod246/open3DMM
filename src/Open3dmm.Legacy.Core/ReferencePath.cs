using Open3dmm;

namespace Open3dmm.Core
{
    public struct ReferencePath
    {
        public ReferencePath(Tag tag, int number, Tag referenceTag, int referenceIndex) : this(new ChunkIdentifier(tag, number), new ReferenceIdentifier(referenceIndex, referenceTag))
        {
        }

        public ReferencePath(ChunkIdentifier identifier, ReferenceIdentifier reference)
        {
            Identifier = identifier;
            Reference = reference;
        }

        public ChunkIdentifier Identifier { get; set; }
        public ReferenceIdentifier Reference { get; set; }
    }
}