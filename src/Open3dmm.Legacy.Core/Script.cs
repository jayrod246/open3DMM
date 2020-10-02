using Open3dmm;
using Open3dmm.Core.Data;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Open3dmm.Core
{
    public class Script : ResolvableObject
    {
        public IList<Instruction> Instructions { get; set; }
        public IList<string> Strings { get; set; }

        protected override void ResolveCore()
        {
            using var block = Metadata.GetBlock();
            Instructions = new List<Instruction>();
            IOHelper.GL(block, Instructions);

            if (TryResolveReference<GenericStrings>(new ReferenceIdentifier(0, Tags.GSTX), out var gstx))
            {
                throw new NotImplementedException();
                //using var gstx = item.Open().Decompress();
                //var scriptStrings = new List<StringGrouping>();
                //IOHelper.GST(gstx, scriptStrings);
                //Strings = scriptStrings.Select(s => s.Value).ToList();
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Instruction
    {
        [FieldOffset(0)]
        private int _literal;

        [FieldOffset(0)]
        private ushort _code;

        [FieldOffset(2)]
        private byte _args;

        [FieldOffset(3)]
        private byte _parameterCode;

        public int Literal => _literal;

        public int Args => _args;

        public int Code => _parameterCode is 0 ? _code : _parameterCode;
    }
}
