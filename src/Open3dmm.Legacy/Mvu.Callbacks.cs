using static Open3dmm.KnownMessageValues;
using static Open3dmm.MessageFlags;

namespace Open3dmm
{
    partial class Mvu
    {
        protected override MessageCallbackCollection GetCallbacks() => Events<Mvu>.Callbacks;

        static Mvu()
        {
            Events<Mvu>.Init(
                new((int)Mvu_CopyPath, Self | Broadcast, (x, m) => (x as Mvu).VirtualFunc41(m)),
                new((int)Mvu_Cut, Self | Broadcast, (x, m) => (x as Mvu).VirtualFunc41(m)),
                new((int)Mvu_CutConfirm, Self | Broadcast, (x, m) => (x as Mvu).VirtualFunc41(m)),
                new((int)Mvu_Copy, Self | Broadcast, (x, m) => (x as Mvu).VirtualFunc41(m)),
                new((int)Mvu_CopyConfirm, Self | Broadcast, (x, m) => (x as Mvu).VirtualFunc41(m)),
                new((int)Mvu_Paste, Self | Broadcast, (x, m) => (x as Mvu).VirtualFunc41(m)),
                new(102, Self | Broadcast, null),
                new((int)App_Save, Self | Broadcast, (x, m) => (x as Mvu).VirtualFunc40(m)),
                new(104, Self | Broadcast, (x, m) => (x as Mvu).VirtualFunc40(m)),
                new((int)App_SaveAs, Self | Broadcast, (x, m) => (x as Mvu).VirtualFunc40(m)),
                new((int)Update, Self | Broadcast, (x, m) => (x as Mvu).UpdateMovie(m)),
                new((int)MouseExit, Self | Broadcast, (x, m) => (x as Mvu).OnMouseExit(m)),
                new((int)Fallback, 0, null)
            );
        }
    }
}
