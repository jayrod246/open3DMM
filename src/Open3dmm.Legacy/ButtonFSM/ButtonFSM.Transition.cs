using System.Runtime.CompilerServices;

namespace Open3dmm
{
    partial class ButtonFSM
    {
        public struct Transition
        {
            public Transition(uint destinationState, bool raiseClick)
            {
                DestinationState = destinationState;
                RaisesClickEvent = raiseClick;
            }

            public uint DestinationState;
            public bool RaisesClickEvent;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Transition(uint destinationState) => new(destinationState, false);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Transition(in (uint destinationState, bool raiseClick) x) => new(x.destinationState, x.raiseClick);
        }
    }
}
