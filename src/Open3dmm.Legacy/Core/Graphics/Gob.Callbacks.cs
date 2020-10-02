using static Open3dmm.MessageFlags;
using static Open3dmm.KnownMessageValues;

namespace Open3dmm
{
    partial class Gob
    {
        protected override MessageCallbackCollection GetCallbacks() => Events<Gob>.Callbacks;

        static Gob()
        {
            Events<Gob>.Init(
                new((int)Char, Self | Broadcast, (x, m) => (x as Gob).OnCharEvent(m)),
                new((int)UpdateState_61A81, Self | Broadcast, (x, m) => (x as Gob).VirtualFunc24(m)),
                new((int)MouseDown, Self, (x, m) => (x as Gob).VirtualFunc25(m)),
                new((int)Gob_Unknown100004, Self, (x, m) => (x as Gob).VirtualFunc23(m)),
                new((int)App_MDIClose, Self, null),
                new((int)MousePressed, Self, (x, m) => (x as Gob).OnMouseEvent(m)),
                new((int)MouseDragMaybe, Self, (x, m) => (x as Gob).OnMouseEvent(m)),
                new((int)MouseOver, Self, (x, m) => (x as Gob).OnMouseOver(m))
            );
        }
    }
}
