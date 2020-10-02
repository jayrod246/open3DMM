using System;

namespace Open3dmm
{
    public class Usac
    {
        private const int NO_SCALE = 0x10000;

        public Usac()
        {
            Elapsed = 0;
            Scale = NO_SCALE;
            LastUpdate = Environment.TickCount;
        }

        private int LastUpdate;
        private int Elapsed;
        private int Scale;

        public static int CalculateFrameDelay(int a, int b, int c)
        {
            return (int)((long)a * b / c);
        }

        public int GetTime()
        {
            return Elapsed + CalculateDelta(out _);
        }

        public int CalculateDelta(out int ticks)
        {
            ticks = Environment.TickCount;
            int delta = ticks - LastUpdate;
            if (Scale != NO_SCALE)
                ScaleCalculation(ref delta, Scale);
            return delta;
        }

        public void ScaleTime(int scale)
        {
            if (Scale != scale)
            {
                int delta = CalculateDelta(out int ticks);
                LastUpdate = ticks;
                Elapsed += delta;
                Scale = scale;
            }
        }

        private void ScaleCalculation(ref int value, int scale)
        {
            value = (int)((long)value * scale / NO_SCALE);
        }
    }
}
