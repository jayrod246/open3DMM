using Open3dmm;
using Open3dmm.Core.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open3dmm.Core
{
    internal static class ThrowHelper
    {
        #region Likely Obsolete
        // TODO: Clean up these throw helpers.
        public static Exception IOBadHeaderMagicNumber(int badMagicNumber)
        {
            return new InvalidOperationException($"Bad header: magic number {badMagicNumber:X8} must be 0x03030001 or 0x05050001.");
        }

        public static Exception IOBadElementSize(int actualSize, int expectedSize)
        {
            return new InvalidOperationException($"Bad generic group element size: expected {expectedSize}, got {actualSize}.");
        }

        public static Exception GroupIndexOutOfBounds()
        {
            return new IndexOutOfRangeException();
        }

        public static Exception GroupDataLength()
        {
            return new InvalidOperationException("Data length must be greater than or equal to element size.");
        }
        #endregion

        public static Exception EndOfStreamReached() => new InvalidOperationException("End of data stream was reached.");

        public static Exception BadSection(ChunkIdentifier identifier) => new InvalidOperationException($"{identifier} : {identifier.Tag} section could not be read because it was corrupt or invalid.");

        public static Exception ReferenceAlreadyExists(ChunkIdentifier identifier, ReferenceIdentifier referenceIdentifier) => new ArgumentException($"A reference with the same reference identifier {referenceIdentifier} already exists inside chunk {identifier}.");

        public static Exception IndexOutOfRange() => new IndexOutOfRangeException();

        public static Exception ReferenceHasNoContainer() => new InvalidOperationException("A reference was not a part of a container.");

        public static Exception BadMagicNumber(int badValue) => new InvalidOperationException($"A bad magic number was encountered: {badValue:X8}");

        public static Exception MissingReference(ChunkIdentifier identifier, ReferenceIdentifier reference) => new InvalidOperationException($"A reference could not be resolved: {identifier.Tag} {identifier.Number} -> {reference.Tag}:{reference.Index}");

        public static Exception MissingReferenceIdentifier(ChunkIdentifier identifier, ChunkIdentifier isReferencedBy) => new InvalidOperationException($"A reference identifier was not found for {identifier} under {isReferencedBy}.");

        public static Exception MissingChunk(ChunkIdentifier identifier) => new InvalidOperationException($"A chunk was not found: {identifier.Tag} {identifier.Number}");

        public static Exception MissingFile(string fileName) => new FileNotFoundException($"A file could not be found: {fileName}", fileName);

        public static Exception BadGroupKeySize(int keySize, int desiredKeySize) => new InvalidOperationException($"A bad key size for group, expected {desiredKeySize} but got {keySize}.");

        public static Exception ObjectDisposed(string nameOfObject) => new ObjectDisposedException(nameOfObject);

        public static Exception ChunkAlreadyExists(ChunkIdentifier identifier) => new ArgumentException($"A chunk with the same identifier {identifier} already exists inside container.");

        public static Exception OutsideScope(IScopedResolver scope, IResolvableObject obj) => new InvalidOperationException("An object was outside the current scope.");

        public static Exception NotResolvable(object obj) => throw new InvalidOperationException("An object was non-resolvable.");

        public static Exception BadEncodingTypeCode(ushort encodingTypeCode) => new InvalidOperationException($"An encoding type code 0x{encodingTypeCode:X4} was not recognized. Expected either ASCII (0x0303) or Unicode (0x0505).");
    }
}
