using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Open3dmm
{
    public class Hbtn : Gok
    {
        public Hbtn(GobOptions options) : base(options, Application.Current.SoundManager)
        {
        }

        public override bool HitTest(PT pt, bool precise)
        {
            if (base.HitTest(pt, precise))
                return true;
            if (ChildId != 0 && Parent is Txhg txhg)
            {
                TransformPoint(ref pt, CoordinateSpace.None, CoordinateSpace.Local);
                return txhg.VirtualFunc67_HitTestText(pt.X, pt.Y, out var childId, out var unk) && ChildId == childId && Unk == unk;
            }
            return false;
        }

        public override bool OnClick(Message m)
        {
            if (Parent is Txhg txhg)
            {
                txhg.VirtualFunc68(ChildId, Unk, m.ParamC, Id);
            }
            return true;
        }

        public int Unk { get; set; }
        public int ChildId { get; set; }
    }
}
