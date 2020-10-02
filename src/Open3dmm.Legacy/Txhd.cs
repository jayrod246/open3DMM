using Open3dmm.Core;
using Open3dmm.Core.Data;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Open3dmm
{
    public struct HtopValues
    {
        public int CnoGraphics;
        public int Id;
        public int RelativeId;
        public int CnoScript;
        public int OffsetX;
        public int OffsetY;
        public int CtgSound;
        public int CnoSound;
    }
    public class Txhd : Txrd
    {
        public IResolver Resolver { get; set; }
        public HtopValues Values { get; set; }
        public int Field0x1c { get; set; }
        public int Field0x3c4 { get; set; }

        public Txhd(IResolver resolver, Docb relative, bool makeSibling) : base(relative, makeSibling)
        {
            Resolver = resolver;
            Values = new HtopValues()
            {
                CtgSound = 0,
                CnoGraphics = -1,
                CnoScript = -1,
                CnoSound = -1,
            };
        }

        public bool FUN_0044fe80(int startPos, out int nextPos, out AgpaItem item)
        {
            bool found = BinarySearch(0xc0, startPos & 0xffffff, out var runInfo);
            nextPos = runInfo.End;
            int i;
            if (found && runInfo.Start == startPos)
            {
                nextPos = runInfo.Start;
                i = runInfo.Value;
            }
            else
            {
                if (nextPos > Text.Length)
                {
                    item = null;
                    return false;
                }
                if (found)
                    i = Glmp.GetAt(runInfo.Index + 1).Value;
                else
                    i = Glmp.GetAt(runInfo.Index).Value;
            }

            item = Agpa[i - 1];
            return true;
        }

        public Txhd(IResolver resolver, ChunkyFile file, Tag tag, int number, IDictionary<int, string> strg, int v) : this(resolver, null, false)
        {
            if (!VirtualFunc62(file, tag, number, strg, v))
                throw new NotImplementedException();
        }

        public override bool VirtualFunc56(AgpaItem item)
        {
            if (base.VirtualFunc56(item))
                return true;
            int n = item.Body.Length;
            int code = item.Code;
            if (code == 0x40)
            {
                if (n >= 5)
                    return true;
            }
            else if (code == 0xc0)
            {
                if (n >= 4)
                {
                    using IReadOnlyStream block = BinaryStream.Create(item.Body.Span);
                    var tag = block.Read<Tag>();
                    int x;
                    if (tag == Tags.EDIT || tag == Tags.MBMP)
                        x = 1;
                    else if (tag == Tags.GOKD)
                        x = 2;
                    else
                        return false;

                    if (n - 4 >= x * 4)
                        return true;
                }
            }

            return false;
        }

        public virtual bool VirtualFunc62(ChunkyFile file, Tag tag, int number, IDictionary<int, string> strg, int v)
        {
            if (file.TryGetChunk((tag, number), out var cHtop))
            {
                if (cHtop.Flags.HasFlag(ChunkFlags.Unknown))
                {
                    file = ParseHtop(cHtop.Section.Memory.Span);
                    cHtop = file.GetChunksOfType(tag).SingleOrDefault();
                    if (cHtop == null)
                        return false;
                }

                using var bHtop = BinaryStream.Create(cHtop.Section.Memory.Span).Decompress();
                if ((bHtop.Length == 0x24 || bHtop.Length == 0x1c) && bHtop.MagicNumber())
                {
                    var values = new HtopValues()
                    {
                        CtgSound = 0,
                        CnoSound = -1
                    };
                    bHtop.ReadTo(IOHelper.GetBytes(ref values));
                    if (file.TryGetChunk(cHtop.Identifier, (0, Tags.RTXT), out var c)
                        && VirtualFunc55(file, c.Identifier, (v & 1) != 0))
                    {
                        if ((v & 2) != 0 && strg != null)
                        {
                            // TODO: Continue VirtualFunc62 here
                            //SetFlag4(true);
                            //VirtualFunc48();
                        }

                        Values = values;

                        return true;
                    }
                }
            }

            return false;
        }

        private void SetFlag4(bool v)
        {
            Field0x1c = FlagHelper.SetFlag(Field0x1c, 4, v);
            //if (v == ((Field0x1c & 4) == 0))
            //    Field0x1c ^= 4;
        }

        private ChunkyFile ParseHtop(ReadOnlySpan<byte> bytes)
        {
            var stack = new Stack<(ChunkIdentifier, int)>();
            var file = new ChunkyFile();
            int index = 0;
            ChunkIdentifier id = default;
            using var block = BinaryStream.Create(bytes).Decompress();

            while (true)
            {
                if (block.MagicNumber()
                    && block.TryRead(out Tag newTag)
                    && block.TryRead(out int unk1)
                    && block.TryRead(out int sectionLength)
                    && block.TryRead(out int unk2)
                    && block.TryRead(out int unk3))
                {
                    var newID = file.Add(newTag).Identifier;
                    file.Write(newID, block.ReadTo<byte>(new byte[sectionLength]));
                    if (index > 0)
                    {
                        file.AddParentChild(id, new(newID, unk1));
                        index--;
                    }
                    if (unk2 <= 0)
                    {
                        while (index < 1 && stack.Count > 0)
                        {
                            (id, index) = stack.Pop();
                        }
                    }
                    else
                    {
                        stack.Push((id, index));
                        id = newID;
                        index = unk2;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

                if (stack.Count == 0 && index == 0)
                {
                    break;
                }
            }
            return file;
        }

        public bool FUN_0045ad10(int key, out int start, out int end, out byte chr, out int unk, out string str)
        {
            if ((byte)(key >> 24) != 0)
                throw new InvalidOperationException("Bad assumption that 'key' contains only position information");

            BinarySearch(0x40, key & 0xffffff, out var runInfo);
            end = runInfo.End;
            chr = 0;
            unk = -1;
            str = string.Empty;

            if (runInfo.Value > 0)
            {
                var agpaItem = Agpa[runInfo.Value - 1];
                if (agpaItem.Body.Length - 5 >= 0)
                {
                    using IReadOnlyStream block = BinaryStream.Create(agpaItem.Body.Span);
                    chr = block.Read<byte>();
                    if (chr != 0)
                    {
                        unk = block.Read<int>();
                        if (block.Remainder > 0)
                        {
                            if (!block.Assert(EncodingTypes.Default))
                                throw new InvalidOperationException();
                            str = block.LengthPrefixedString();
                        }
                    }
                }
            }

            start = runInfo.Start;
            return chr != 0;
        }
    }
}