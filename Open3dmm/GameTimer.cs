using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Open3dmm
{
    class GameTimer
    {
        private TimeSpan _accumulatedElapsedTime;
        private readonly GameTime _gameTime = new GameTime();
        private Stopwatch _gameTimer;
        private long _previousTicks = 0;
        private int _updateFrameLag;
        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);
        private bool _suppressDraw;

        public bool IsActive { get; set; }
        public bool IsFixedTimeStep { get; set; } = true;
        public TimeSpan TargetElapsedTime { get; set; } = TimeSpan.FromTicks(166667);

        public GameTimer()
        {
            _gameTimer = new Stopwatch();
            _gameTimer.Start();
        }

        public void Tick()
        {
        RetryTick:
            // Advance the accumulated elapsed time.
            var currentTicks = _gameTimer.Elapsed.Ticks;
            _accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - _previousTicks);
            _previousTicks = currentTicks;

            if (IsFixedTimeStep && _accumulatedElapsedTime < TargetElapsedTime)
            {
#if WINDOWS && !DESKTOPGL
                // Sleep for as long as possible without overshooting the update time
                var sleepTime = (TargetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds;
                Utilities.TimerHelper.SleepForNoMoreThan(sleepTime);
#endif
                // Keep looping until it's time to perform the next update
                goto RetryTick;
            }

            // Do not allow any update to take longer than our maximum.
            if (_accumulatedElapsedTime > _maxElapsedTime)
                _accumulatedElapsedTime = _maxElapsedTime;

            if (IsFixedTimeStep)
            {
                _gameTime.ElapsedGameTime = TargetElapsedTime;
                var stepCount = 0;

                // Perform as many full fixed length time steps as we can.
                while (_accumulatedElapsedTime >= TargetElapsedTime)
                {
                    _gameTime.TotalGameTime += TargetElapsedTime;
                    _accumulatedElapsedTime -= TargetElapsedTime;
                    ++stepCount;

                    DoUpdate(_gameTime);
                }

                //Every update after the first accumulates lag
                _updateFrameLag += Math.Max(0, stepCount - 1);

                //If we think we are running slowly, wait until the lag clears before resetting it
                if (_gameTime.IsRunningSlowly)
                {
                    if (_updateFrameLag == 0)
                        _gameTime.IsRunningSlowly = false;
                }
                else if (_updateFrameLag >= 5)
                {
                    //If we lag more than 5 frames, start thinking we are running slowly
                    _gameTime.IsRunningSlowly = true;
                }

                //Every time we just do one update and one draw, then we are not running slowly, so decrease the lag
                if (stepCount == 1 && _updateFrameLag > 0)
                    _updateFrameLag--;

                // Draw needs to know the total elapsed time
                // that occured for the fixed length updates.
                _gameTime.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
            }
            else
            {
                // Perform a single variable length update.
                _gameTime.ElapsedGameTime = _accumulatedElapsedTime;
                _gameTime.TotalGameTime += _accumulatedElapsedTime;
                _accumulatedElapsedTime = TimeSpan.Zero;

                DoUpdate(_gameTime);
            }

            // Draw unless the update suppressed it.
            if (_suppressDraw)
                _suppressDraw = false;
            else
            {
                DoDraw(_gameTime);
            }
        }

        private void DoDraw(GameTime gameTime)
        {
            Draw?.Invoke(gameTime);
            NativeAbstraction.GraphicsDevice.Present();
        }

        private void DoUpdate(GameTime gameTime)
        {
            Updated?.Invoke(gameTime);
        }

        public event Action<GameTime> Draw;
        public event Action<GameTime> Updated;
    }
}
