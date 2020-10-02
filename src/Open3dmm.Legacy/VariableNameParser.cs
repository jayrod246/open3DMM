using Open3dmm.Core.IO;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Open3dmm
{
    public static class VariableNameParser
    {
        public static unsafe string Parse(ReadOnlySpan<byte> input)
        {
            var variableName = new StringBuilder(8);
            var chr = 0;
            var bitsRead = 0;
            var b = (byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in input[0]));
            foreach (var bit in new FlippedBitEnumerator(b, input.Length))
            {
                chr |= bit << (5 - bitsRead);
                bitsRead++;

                if (bitsRead == 6)
                {
                    variableName.Append(ParseVariableChar(chr));

                    chr = 0;
                    bitsRead = 0;
                }
            }
            return variableName.ToString().TrimEnd('_');
        }

        public static string Parse(ulong parameterId)
        {
            var bytes = IOHelper.GetBytes(ref parameterId);
            bytes[2..4].Fill(0);
            bytes[..2].Reverse();
            bytes[2..].Reverse();
            return Parse(bytes);
        }

        private static char ParseVariableChar(int packed)
            => (char)(packed switch
            {
                >= 1 and <= 10 => '0' - 1 + packed,
                >= 11 and <= 36 => 'A' - 11 + packed,
                >= 37 and <= 62 => 'a' - 37 + packed,
                _ => '_',
            });
    }
}
