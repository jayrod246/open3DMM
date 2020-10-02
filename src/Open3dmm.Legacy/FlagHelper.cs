using System;

namespace Open3dmm
{
    public static class FlagHelper
    {
        enum Dummy : int { }

        public static bool HasFlag(int input, int flag)
        {
            return ((Dummy)input).HasFlag((Dummy)flag);
        }

        public static int SetFlag(int input, int flag, bool value)
        {
            if (value == ((input & flag) == 0))
                input ^= flag;
            return input;
        }

        public static T SetFlag<T>(T input, T flag, bool value) where T : Enum, IConvertible
        {
            return (T)Enum.ToObject(typeof(T), SetFlag(input.ToInt32(null), flag.ToInt32(null), value));
        }
    }
}
