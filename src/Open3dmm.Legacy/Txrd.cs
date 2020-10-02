using Open3dmm.Core;
using Open3dmm.Core.Data;
using Open3dmm.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    public class Txrd : Txtb
    {
        public Txrd(Docb relative, bool makeSibling) : base(relative, makeSibling)
        {
            Field0x267 = 0;
            Field0x266 = 0;
            Field0x378 = 0;
            Field0x37c = 0;
        }

        public int Field0x267 { get; set; }
        public int Field0x266 { get; set; }
        public int Field0x378 { get; set; }
        public int Field0x37c { get; set; }
        public ISortedList<int, int> Glmp { get; set; }
        public List<AgpaItem> Agpa { get; set; }
        public string FontName { get; set; }
        public int FontIndex { get; set; }
        public int FontSize { get; set; }

        public bool BinarySearch(int type, int pos, out TextRunInfo runInfo)
        {
            bool success;
            int i = Glmp.BinarySearch(type << 24 | pos);
            runInfo.Type = 0;
            runInfo.Start = 0;
            runInfo.Value = 0;

            if (i < 0)
            {
                i = ~i;
                runInfo.Index = i;
                success = false;
                if (i > 0)
                {
                    if (type == (byte)(Glmp.GetAt(i - 1).Key >> 24))
                    {
                        runInfo.Index--;
                        success = true;
                    }
                }
            }
            else
            {
                runInfo.Index = i++;
                success = true;
            }

            KeyValuePair<int, int> tmp;
            if (success)
            {
                tmp = Glmp.GetAt(runInfo.Index);
                runInfo.Type = (byte)(tmp.Key >> 24);
                runInfo.Start = tmp.Key & 0xffffff;
                runInfo.Value = tmp.Value;
            }

            runInfo.End = Text.Length + 1;
            if (i < Glmp.Count)
            {
                tmp = Glmp.GetAt(i);
                if (i > 0 && type == (byte)(tmp.Key >> 24))
                {
                    runInfo.End = tmp.Key & 0xffffff;
                }
            }
            return success;
        }

        public virtual bool VirtualFunc55(ChunkyFile file, ChunkIdentifier identifier, bool flag)
        {
            if (VirtualFunc31(null, null, 0x0303))
            {
                if (file.TryGetChunk(identifier, out var cRtxt)
                 && file.TryGetChunk(identifier, (0, Tags.TEXT), out var cText)
                 && file.TryGetChunk(identifier, (0, Tags.GLMP), out var cGlmp))
                {
                    using var bRtxt = BinaryStream.Create(cRtxt.Section.Memory.Span).Decompress();
                    using var bText = BinaryStream.Create(cText.Section.Memory.Span).Decompress();
                    using var bGlmp = BinaryStream.Create(cGlmp.Section.Memory.Span).Decompress();

                    Span<int> values = stackalloc int[3];
                    if (bRtxt.MagicNumber() && bRtxt.TryRead(values))
                    {
                        if (values[1] > 4
                            && values[1] < 255
                            && values[0] > 0
                            && values[0] < 0x8000000)
                        {
                            // Font stuff
                            Field0x3c = values[0];
                            FontSize = values[1];
                            Field0x38 = values[2];
                            if (!bRtxt.Assert(EncodingTypes.Default))
                                throw new InvalidOperationException("Encoding not supported");
                            FontName = bRtxt.LengthPrefixedString();
                            FontIndex = Ntl.GetFont(FontName);

                            // Text stuff
                            int encoding = bText.Read<short>();
                            Text = bText.String(checked((int)bText.Remainder));

                            // Glmp stuff
                            Glmp = SortedList.Create<int, int>();
                            bGlmp.GL<ulong, KeyValuePair<int, int>>(Glmp, x => new KeyValuePair<int, int>((int)(x & 0xffffffff), (int)(x >> 32)));

                            if (file.TryGetChunk(identifier, (0, Tags.AGPA), out var cAgpa))
                            {
                                using var bAgpa = BinaryStream.Create(cAgpa.Section.Memory.Span).Decompress();
                                // TODO: This all looks hacky
                                Agpa = new List<AgpaItem>();
                                var gg = GenericGroup.FromStream(bAgpa);
                                for (int i = 0; i < gg.Count; i++)
                                {
                                    if (gg.IsIndexOutOfBounds(i))
                                        Agpa.Add(null);
                                    else
                                    {
                                        var item = gg.GetChunk(i);
                                        Agpa.Add(new AgpaItem
                                        {
                                            Field0x0 = MemoryMarshal.Read<ushort>(item),
                                            Field0x2 = MemoryMarshal.Read<ushort>(item.Slice(2)),
                                            Body = gg.GetPayload(i).ToArray()
                                        });
                                    }
                                }
                                int n = Agpa.Count;
                                while (--n >= 0)
                                {
                                    var item = Agpa[n];
                                    if (item != null && !VirtualFunc56(item))
                                        return false;
                                }
                            }

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public virtual bool VirtualFunc56(AgpaItem item)
        {
            int n = item.Body.Length;
            if (item.Code == 2)
            {
                if (n - 6 >= 0)
                {
                    using IStream block = BinaryStream.Create(item.Body.Span);
                    block.Skip(4);
                    var encoding = block.Read<short>();
                    if (block.Assert(EncodingTypes.Default))
                    {
                        var fontName = block.LengthPrefixedString();
                        var fontIndex = Ntl.GetFont(fontName);
                        block.Position = 0;
                        block.Write(fontIndex);
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void VirtualFunc57() => throw new NotImplementedException();
        public virtual void VirtualFunc58() => throw new NotImplementedException();
        public virtual void VirtualFunc59() => throw new NotImplementedException();
        public virtual void VirtualFunc60() => throw new NotImplementedException();
        public virtual void VirtualFunc61() => throw new NotImplementedException();
    }

    public class AgpaItem
    {
        public ushort Field0x0;
        public ushort Field0x2;
        public int Code => Field0x2 >> 8;

        public Memory<byte> Body;
    }

    public struct TextRunInfo
    {
        public byte Type;
        public int Start;
        public int End;
        public int Index;
        public int Value;
    }
}