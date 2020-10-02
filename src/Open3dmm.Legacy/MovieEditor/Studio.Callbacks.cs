using static Open3dmm.MessageFlags;
using static Open3dmm.KnownMessageValues;

namespace Open3dmm.MovieEditor
{
    partial class Studio
    {
        protected override MessageCallbackCollection GetCallbacks() => Events<Studio>.Callbacks;

        static Studio()
        {
            Events<Studio>.Init(
                new((int)Studio_Tool, Self | Broadcast, (x, m) => (x as Studio).FUN_0040f1c0(m)),
                new((int)Browser_Open, Self | Broadcast, (x, m) => (x as Studio).ShowBrowser(m))
            );
        }
    }
}
