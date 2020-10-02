using static Open3dmm.MessageFlags;
using static Open3dmm.KnownMessageValues;

namespace Open3dmm
{
    partial class Ddg
    {
        protected override MessageCallbackCollection GetCallbacks() => Events<Ddg>.Callbacks;

        static Ddg()
        {
            Events<Ddg>.Init(
                new(102, Self | Broadcast, (x, m) => (x as Ddg).VirtualFunc39(m)),
                new(117, Self | Broadcast, (x, m) => (x as Ddg).VirtualFunc39(m)),
                new((int)App_Save, Self | Broadcast, (x, m) => (x as Ddg).VirtualFunc40(m)),
                new(104, Self | Broadcast, (x, m) => (x as Ddg).VirtualFunc40(m)),
                new((int)App_SaveAs, Self | Broadcast, (x, m) => (x as Ddg).VirtualFunc40(m)),
                new(118, Self | Broadcast, (x, m) => (x as Ddg).VirtualFunc41(m)),
                new(119, Self | Broadcast, (x, m) => (x as Ddg).VirtualFunc41(m)),
                new(120, Self | Broadcast, (x, m) => (x as Ddg).VirtualFunc41(m)),
                new(140, Self | Broadcast, (x, m) => (x as Ddg).VirtualFunc41(m)),
                new(121, Self | Broadcast, (x, m) => (x as Ddg).VirtualFunc41(m)),
                new((int)App_Undo, Self | Broadcast, (x, m) => (x as Ddg).VirtualFunc43(m)),
                new((int)App_Redo, Self | Broadcast, (x, m) => (x as Ddg).VirtualFunc43(m))
            );
        }
    }
}
