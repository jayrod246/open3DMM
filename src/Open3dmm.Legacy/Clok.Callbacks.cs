using static Open3dmm.MessageFlags;
using static Open3dmm.KnownMessageValues;

namespace Open3dmm
{
    partial class Clok
    {
        protected override MessageCallbackCollection GetCallbacks() => Events<Clok>.Callbacks;

        static Clok()
        {
            Events<Clok>.Init(
                new MessageCallbackDescriptor((int)Fallback, Self | Broadcast | Other, (x, m) => (x as Clok).Handle(m))
            );
        }
    }
}
