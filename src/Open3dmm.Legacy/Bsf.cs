using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open3dmm
{
    public class BsfChunk
    {
        public object Unk { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public List<byte> Data { get; set; }

        public BsfChunk()
        {
            Data = new List<byte>();
        }
    }

    public class Bsf
    {
        public List<BsfChunk> Group { get; set; }

        public int SomePosition { get; set; }

        public Bsf()
        {
            Group = new List<BsfChunk>();
        }

        public bool FUN_00456130(Span<byte> data, int length, int pos, int param_4)
        {
            if (param_4 == 0 && length == 0)
                return true;

            AddMaybe(pos);
            int i = GetIndexFromPos(pos, out var start, out _);
            start = pos - start;

            if (start > 0 && Group[i].Unk != null)
            {
                i = AddMaybe(pos);
                start = 0;
            }

            if (start >= 1)
            {
                if (param_4 > 0)
                {
                    if (start + param_4 < Group[i].Length)
                    {
                        Group[i].Data.RemoveRange(start, param_4);
                        Group[i].Length -= param_4;
                        SomePosition -= param_4;
                        param_4 = 0;
                    }
                    else
                    {
                        int n = Group[i].Length - start;
                        Group[i].Data.RemoveRange(start, n);
                        param_4 -= n;
                        Group[i].Length = start;
                        SomePosition -= n;
                        i++;
                        start = 0;
                    }
                }
            }

            while (param_4 > 0)
            {
                if (param_4 < Group[i].Length)
                {
                    if (Group[i].Unk == null)
                        Group[i].Data.RemoveRange(0, param_4);
                    else
                        Group[i].Start += param_4;
                    Group[i].Length -= param_4;
                    SomePosition -= param_4;
                    param_4 = 0;
                }
                else
                {
                    param_4 -= Group[i].Length;
                    SomePosition -= Group[i].Length;
                    Group.RemoveAt(i);
                    if (i < Group.Count)
                        break;
                }
            }

            if (length < 1) goto LAB_004563f3;

            if (i >= Group.Count || Group[i].Unk != null)
            {
                if (i > 0 && Group[i].Unk == null)
                {
                    i--;
                    start = Group[i].Length;
                    Group[i].Data.InsertRange(start, data.ToArray());
                    Group[i].Length += length;
                }
                else
                {
                    Group.Insert(i, new BsfChunk()
                    {
                        Length = length,
                    });
                    Group[i].Data.AddRange(data.ToArray());
                }
            }
            else
            {
                Group[i].Data.InsertRange(start, data.ToArray());
                Group[i].Length += length;
            }

            SomePosition += length;

        LAB_004563f3:
            FUN_00455d30(pos, pos + length);
            return true;
        }

        private void FUN_00455d30(int pos, int v)
        {
            // TODO: Implement FUN_00455d30

            //if (--pos < 0)
            //    pos = 0;
            //int index = GetIndexFromPos(pos, out var start, out _);
            //while(start < v)
        }

        public int AddMaybe(int pos)
        {
            int index = GetIndexFromPos(pos, out var start, out _);
            if (start == pos)
                return index;
            var item = Group[index];
            int length = pos - start;
            item.Length -= length;
            if (item.Unk != null)
                item.Start += length;
            Group.Insert(index + 1, item);
            if (item.Unk == null)
            {
                // MoveRange
                item.Data.InsertRange(0, Group[index].Data.Skip(length).Take(item.Length));
                Group[index].Data.RemoveRange(length, item.Length);
            }
            item.Length = length;
            if (item.Unk != null)
                item.Start -= length;
            Group[index] = item;
            return index;
        }

        public int GetIndexFromPos(int pos, out int start, out int length)
        {
            int x = pos;
            for (int i = 0; i < Group.Count; i++)
            {
                var item = Group[i];
                if (x < item.Length)
                {
                    start = pos - x;
                    length = item.Length;
                    return i;
                }
                x -= item.Length;
            }
            start = SomePosition;
            length = 0;
            return Group.Count;
        }
    }
}
