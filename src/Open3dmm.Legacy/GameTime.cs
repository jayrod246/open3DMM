using System.Diagnostics;

namespace Open3dmm
{
    public static class GameTime
    {
        static readonly Stopwatch stopwatch = Stopwatch.StartNew();
        static float previous = 0f;
        static float current = 0f;

        public static void Tick()
        {
            previous = current;
            current = (float)stopwatch.Elapsed.TotalSeconds;
        }

        public static float DeltaSeconds => current - previous;
    }
}
