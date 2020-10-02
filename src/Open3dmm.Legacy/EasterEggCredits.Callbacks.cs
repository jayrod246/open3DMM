using static Open3dmm.MessageFlags;
using static Open3dmm.KnownMessageValues;

namespace Open3dmm
{
    partial class EasterEggCredits
    {
        protected override MessageCallbackCollection GetCallbacks() => Events<EasterEggCredits>.Callbacks;

        static EasterEggCredits()
        {
            Events<EasterEggCredits>.Init(
                new((int)Browser_CancelMaybe, Other, (x, m) => (x as EasterEggCredits).OnCancel(m)),
                new((int)App_New, Other, (x, m) => (x as EasterEggCredits).OnNewMovie(m))
            );
        }
    }
}
