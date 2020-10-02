using static Open3dmm.MessageFlags;
using static Open3dmm.KnownMessageValues;

namespace Open3dmm
{
    partial class Gok
    {
        protected override MessageCallbackCollection GetCallbacks() => Events<Gok>.Callbacks;

        static Gok()
        {
            Events<Gok>.Init(
                new((int)Click, Self, (x, m) => (x as Gok).OnClick(m)),
                new((int)Alarm, Self, (x, m) => (x as Gok).VirtualFunc39(m)),
                new((int)MouseExit, Self, (x, m) => (x as Gok).OnMouseOver(m)),
                new((int)Char, Self | Broadcast | Other, (x, m) => (x as Gok).VirtualFunc41(m)),
                new((int)UpdateState_61A81, Self | Broadcast | Other, (x, m) => (x as Gok).VirtualFunc41(m)),
                new((int)Fallback, Self | Broadcast | Other, (x, m) => (x as Gok).VirtualFunc41(m))
            );
        }
    }
}
