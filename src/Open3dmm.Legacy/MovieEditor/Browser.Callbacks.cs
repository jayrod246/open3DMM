using static Open3dmm.KnownMessageValues;
using static Open3dmm.MessageFlags;

namespace Open3dmm.MovieEditor
{
    partial class Browser
    {
        protected override MessageCallbackCollection GetCallbacks() => Events<Browser>.Callbacks;

        static Browser()
        {
            Events<Browser>.Init(
                new((int)Browser_PageRight, Self | Broadcast, (x, m) => (x as Browser).HandlePageRight(m)),
                new((int)Browser_PageLeft, Self | Broadcast, (x, m) => (x as Browser).HandlePageLeft(m)),
                new((int)Browser_CancelMaybe, Self | Broadcast, (x, m) => (x as Browser).HandleCloseButton(m)),
                new((int)Browser_ConfirmMaybe, Self | Broadcast, (x, m) => (x as Browser).HandleConfirm(m)),
                new((int)Browser_Selection, Self | Broadcast, (x, m) => (x as Browser).HandleSelectItem(m)),
                new((int)App_ShowOpenFileDialog, Self | Broadcast, (x, m) => (x as Browser).HandleImport(m))
            );
        }
    }
}
