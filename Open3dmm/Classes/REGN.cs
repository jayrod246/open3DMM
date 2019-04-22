using System.Runtime.InteropServices;

namespace Open3dmm.Classes
{
    public class REGN : BASE
    {
        public ref RECTANGLE Rectangle => ref GetField<RECTANGLE>(0x08);

        public bool RegionIsWhack(out RECTANGLE value)
        {
            value = Rectangle;
            if (value.Y2 <= value.Y1 || value.X2 <= value.X1)
                return true;
            return false;
        }

        [HookFunction(0x00426330, CallingConvention = CallingConvention.ThisCall)]
        public unsafe bool TryGetRectangle(RECTANGLE* dest)
        {
            var success = RegionIsWhack(out var result);
            if (dest != null)
                *dest = result;
            return success;
        }
    }
}
