using Open3dmm.Core.Resolvers;

namespace Open3dmm
{
    public class Hbal : Gok
    {
        public Hbal(GobOptions options) : base(options, Application.Current.SoundManager)
        {
        }

        public Txhg Txhg { get; set; }

        public virtual bool VirtualFunc58(Woks owner, Txhd txhd, in HtopValues values, IResolver resolver)
        {
            return LoadGokd(owner, values.CnoGraphics, resolver) && VirtualFunc59(txhd, in values, resolver);
        }

        public virtual bool VirtualFunc59(Txhd txhd, in HtopValues values, IResolver resolver)
        {
            var oldTxhg = this.Txhg;
            Txhg = new Txhg(Owner, txhd, new GobOptions(0, this, false, InvalidateOptions.Region));
            if (Txhg != null)
            {
                Txhg.GetParameters(true)
                    .Set("_ctgSound", values.CtgSound)
                    .Set("_cnoSound", values.CnoSound);

                GorpSize = Txhg.GetTextBounds();

                Gob relative;
                if (values.RelativeId == 0
                    || (relative = Find(values.RelativeId)) == null)
                    relative = Parent;
                PT pos;

                if (relative is Gok gok)
                    pos = gok.GetPositionWithLayout(CoordinateSpace.Local);
                else
                {
                    var rect = relative.GetRectangle(CoordinateSpace.Local);
                    pos = new((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
                }

                relative.TransformPoint(ref pos, CoordinateSpace.Local, CoordinateSpace.Screen);
                Parent.TransformPoint(ref pos, CoordinateSpace.Screen, CoordinateSpace.None);
                var childPos = GetPositionWithLayout(CoordinateSpace.Local);
                pos = new(values.OffsetX - childPos.X + pos.X, values.OffsetY - childPos.Y + pos.Y);
                SetGorp(Gorp, pos.X, pos.Y);
                // UpdateRect(new Rectangle(pos.X, pos.Y, App.Instance.Window.Width - pos.X, App.Instance.Window.Height - pos.Y), null);
                oldTxhg?.Dispose();
                return true;
            }
            Txhg?.Dispose();
            Txhg = oldTxhg;
            return false;
        }

        public override void SetGorp(Gorp gorp, int offsetX, int offsetY)
        {
            base.SetGorp(gorp, offsetX, offsetY);
            var rc = GetRectangle(CoordinateSpace.Local);
            var parRect = Parent.GetRectangle(CoordinateSpace.None);
            rc.TryGetIntersection(parRect, out var intersection);
            if (intersection.IsValidIntersection(rc))
            {
                rc.KeepInsideOf(parRect);
                UpdateRectangle(rc, null);
            }
            var newRect = new LTRB(0, 0, GorpSize.X, GorpSize.Y);
            newRect.CenterWith(GetDrawRect());
            Txhg.UpdateRectangle(newRect, null);
        }

        public LTRB GetDrawRect()
        {
            return Gorp?.GetRectOrigin() ?? GetRectangle(CoordinateSpace.None);
        }
    }
}
