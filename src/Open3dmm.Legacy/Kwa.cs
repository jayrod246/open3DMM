using System;

namespace Open3dmm
{

    public class Kwa : Woks
    {
        public static Kwa Instance { get; private set; }

        public Kwa(GobOptions options) : base(options)
        {
            Instance = this;
        }
    }
}
