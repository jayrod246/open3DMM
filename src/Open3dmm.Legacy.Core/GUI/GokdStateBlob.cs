using System;

namespace Open3dmm.Core.GUI
{
    public struct GokdStateBlob
    {
        public int ModifierMask;
        public int ModifierValue;
        public int StateMask;

        public int Cursor;
        public int Script;
        public int ClickMessage;
        public int Help;

        public bool Match(int modifier, int state)
        {
            if ((StateMask & state) == 0)
                return false;

            return (ModifierMask & modifier) == ModifierValue;
        }
    }
}
