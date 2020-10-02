using System.Runtime.InteropServices;

namespace Open3dmm.Core.Actors
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LogicalActorEvent
    {
        public ActorEventType Type;
        public int Frame;
        public PathIndex Path;

        public override string ToString()
        {
            return $"{Frame}: {(int)Type} {Type}, P:{Path.Base + Path.Tween}, Delay: {Path.Delay}";
        }
    }
}
