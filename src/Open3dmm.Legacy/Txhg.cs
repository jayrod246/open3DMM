using Open3dmm.Core;
using Open3dmm.Core.IO;
using System;

namespace Open3dmm
{
    public class Txhg : Txrg
    {
        byte field_0xd0;
        int field_0xd4;
        int field_0xdc;

        public Txhg(Woks owner, Txhd txhd, GobOptions options) : base(txhd, options)
        {
            Owner = owner;
            if (!VirtualFunc28())
                throw new InvalidOperationException();
        }

        public override bool OnMouseEvent(Message m)
        {
            var state = Owner.GetStateWithKeys((InputState)m.ParamC);

            if (m.Id == (int)KnownMessageValues.MousePressed)
            {
                if (VirtualFunc67_HitTestText(m.ParamA, m.ParamB, out field_0xd0, out field_0xd4))
                {
                    Exchange.Capture = this;
                    VirtualFunc69(state);
                    field_0xdc = state;
                }
            }
            else
            {
                if ((state & (int)InputState.LeftButton) == 0)
                {
                    Exchange.ReleaseCapture();

                    if (VirtualFunc67_HitTestText(m.ParamA, m.ParamB, out var tmp1, out var tmp2) && field_0xd0 == tmp1 && field_0xd4 == tmp2)
                    {
                        VirtualFunc68(tmp1, tmp2, field_0xdc, 0);
                    }
                }
            }

            return true;
        }

        public override bool HitTest(PT pt, bool precise)
        {
            return base.HitTest(pt, precise) && (!precise || VirtualFunc67_HitTestText(pt.X, pt.Y, out _, out _));
        }

        public override bool OnMouseOver(Message m)
        {
            int state = Owner.GetStateWithKeys((InputState)m.ParamC);

            if (VirtualFunc67_HitTestText(m.ParamA, m.ParamB, out _, out _))
            {
                state |= 512;
            }

            VirtualFunc69(state);
            return true;
        }

        public override bool VirtualFunc28()
        {
            var txhd = (Txhd)Txtb;
            int length = 0;
            int pos = 0;
            byte chr;
            while (true)
            {
                txhd.FUN_0045ad10(pos, out _, out pos, out chr, out _, out _);
                if (chr > length)
                    length = chr;
                if (pos > txhd.Text.Length)
                    break;
            }

            GidBase = 0;
            if (length != 0)
            {
                GidBase = NewIds(length) - 1;
                Parent.GetParameters(true)
                    .Set("_gidBase", GidBase);
            }
            int n = 0;
            while (true)
            {
                if (!txhd.FUN_0044fe80(n, out n, out var item))
                    return true;
                if (item != null)
                {
                    if (item.Body.Length >= 4)
                    {
                        using IReadOnlyStream block = BinaryStream.Create(item.Body.Span);
                        var tag = block.Read<Tag>();
                        if (tag == Tags.EDIT)
                        {

                        }
                        else if (tag == Tags.GOKD && block.Length >= 12)
                        {
                            var number = block.Read<int>();
                            var unk = block.Read<int>();
                            var measure = GetMeasureDataByChar(n, out _);
                            var x = measure.Length_2_2 + GetMarginX(measure.CharIndex, n);
                            var y = measure.LineHeight_2_2 + measure.LineStart;
                            var format = GetFormatData(n, out _, out _);

                            if (unk == -1)
                                txhd.FUN_0045ad10(n, out _, out _, out chr, out unk, out _);
                            else
                                txhd.FUN_0045ad10(n, out _, out _, out chr, out _, out _);

                            int cmhId = GidBase + chr;
                            if (chr == 0 || Owner.FindComponent(cmhId) != null)
                                cmhId = NewIds(1);
                            if (Owner.CreateHbtnChild(this, cmhId, number, txhd.Resolver, chr, unk, x, format.BaseLineOffset + y) == null)
                                return false;
                        }
                    }
                }
                n++;
            }
        }

        public Woks Owner { get; set; }
        public int GidBase { get; set; }

        public virtual bool VirtualFunc66(int v1, int v2, int v3, int v4, int number, out int value)
        {
            var txhd = (Txhd)Txtb;
            var myValues = txhd.Values;
            value = 0;
            if (myValues.CnoScript != -1 && txhd.Resolver.TryResolve<Script>(new ChunkIdentifier(Tags.GLOP, myValues.CnoScript), out var script))
            {
                var sceg = Owner.CreateScriptEngine(txhd.Resolver, this);
                var globalId = RuntimeId;

                if (!sceg.Run(script, stackalloc int[] { v1 & 0xff, v2, v3, v4 & 0xff, unchecked((int)number) }, out value, out _))
                    value = 0;

                return Owner.FindByRuntimeId(globalId) == this;
            }

            return true;
        }

        public virtual bool VirtualFunc67_HitTestText(int x, int y, out byte childId, out int unk)
        {
            var txhd = (Txhd)Txtb;
            if (HitTestTextMaybe(x, y, out int i, false))
            {
                bool hit = txhd.FUN_0045ad10(i, out _, out _, out childId, out unk, out _);
                return hit;
            }
            childId = 0;
            unk = 0;
            return false;
        }

        public virtual void VirtualFunc68(int v1, int number, int v3, int v4)
        {
            var txhd = (Txhd)Txtb;
            if (VirtualFunc66(v1, v3, v4, 0, number, out int value) && number != -1 && value == 0)
            {
                Owner.CreateHbalChild(Parent.Parent, txhd.Resolver, number, null);
            }
        }

        public virtual void VirtualFunc69(int state)
        {
            var gob = Parent;
            while (true)
            {
                if (gob == null)
                {
                    Application.Current.ChangeCursor(null, CursorType.Arrow);
                    return;
                }

                if (gob is Gok gok)
                {
                    gok.ChangeCursorArrow(state | 256);
                    return;
                }
            }
        }
    }
}