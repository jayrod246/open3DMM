using static Open3dmm.MessageFlags;
using static Open3dmm.KnownMessageValues;

namespace Open3dmm
{
    partial class Application
    {
        protected override MessageCallbackCollection GetCallbacks() => Events<Application>.Callbacks;

        static Application()
        {
            Events<Application>.Init(
                new((int)App_Close, Self | Broadcast, (x, m) => (x as Application).VirtualFunc42(m)),
                // action: CMH::DispatchVirtualFunc43    
                // func:  CMH::DispatchVirtualFunc44
                new((int)App_DebugView, Self | Broadcast, null),
                // action:CMH::DispatchVirtualFunc46
                // func:  CMH::DispatchVirtualFunc44
                new((int)App_ActivateMdiChild, Self | Broadcast, (x, m) => (x as Application).VirtualFunc46(m)),
                // CMH::DispatchVirtualFunc45
                new((int)Update, Self | Broadcast, (x, m) => (x as Application).UpdateWindow(m)),
                // CMH::DispatchVirtualFunc65
                new((int)GotFocus, Self | Broadcast, null)
            );
        }
    }
}
