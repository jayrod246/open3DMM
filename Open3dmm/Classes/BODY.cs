namespace Open3dmm.Classes
{
    public class BODY : BASE
    {
        public ref RECTANGLE Rect => ref GetField<RECTANGLE>(0x24);
    }
}
