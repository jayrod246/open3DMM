using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Sdl2;

namespace Open3dmm.VeldridSdl
{
    class VeldridSdlKeyboard : IOpen3dmmKeyboard
    {
        IReadOnlyList<KeyEvent> IOpen3dmmKeyboard.Events => Events;
        IReadOnlyList<char> IOpen3dmmKeyboard.CharPresses => CharPresses;

        public List<KeyEvent> Events { get; } = new();
        public List<char> CharPresses { get; } = new();
    }

    class VeldridSdlMouse : IOpen3dmmMouse
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int del_SDL_CaptureMouse(bool enabled);

        private static readonly del_SDL_CaptureMouse SDL_CaptureMouse = Sdl2Native.LoadFunction<del_SDL_CaptureMouse>("SDL_CaptureMouse");

        public HashSet<MouseButton> DownButtons { get; } = new();
        IReadOnlyList<MouseEvent> IOpen3dmmMouse.Events => Events;
        public List<MouseEvent> Events { get; } = new();
        public float WheelDelta { get; set; }
        public PT MousePosition { get; set; }
        public PT LastMousePosition { get; set; }
        public bool HasPositionChanged => MousePosition != LastMousePosition;

        public bool IsMouseDown(MouseButton button)
        {
            return DownButtons.Contains(button);
        }

        public bool SetCapture(bool enable)
        {
            return SDL_CaptureMouse(enable) == 0;
        }
    }

    class VeldridSdlWindow : IOpen3dmmWindow, IOpen3dmmInput
    {
        public InputSnapshot InputSnapshot { get; private set; }
        private readonly Sdl2Window _window;
        private InputState _state;
        private VeldridSdlMouse _mouse = new();
        private VeldridSdlKeyboard _keyboard = new();

        public VeldridSdlWindow(Sdl2Window window)
        {
            _window = window;
        }

        public int Width => _window.Width;
        public int Height => _window.Height;

        public IOpen3dmmInput Input => this;
        public bool CursorVisible { get => _window.CursorVisible; set => _window.CursorVisible = value; }
        public bool Exists => _window.Exists;
        public InputState State => _state;
        public IOpen3dmmMouse Mouse => _mouse;
        public IOpen3dmmKeyboard Keyboard => _keyboard;

        public event Action FocusGained {
            add => _window.FocusGained += value;
            remove => _window.FocusGained -= value;
        }

        public event Action FocusLost {
            add => _window.FocusLost += value;
            remove => _window.FocusLost -= value;
        }

        public event Action Resized {
            add => _window.Resized += value;
            remove => _window.Resized -= value;
        }

        public PT ClientToScreen(PT point)
        {
            var vpt = _window.ClientToScreen(new(point.X, point.Y));
            return new(vpt.X, vpt.Y);
        }

        public void DoEvents()
        {
            if (_mouse.HasPositionChanged)
            {
                var pt = _mouse.MousePosition;
                var vpt = _window.ScreenToClient(new(pt.X, pt.Y));
                _window.SetMousePosition(new(vpt.X, vpt.Y));
            }

            InputSnapshot = _window.PumpEvents();

            {
                var pos = InputSnapshot.MousePosition;
                var vpt = _window.ClientToScreen(new((int)pos.X, (int)pos.Y));
                _mouse.LastMousePosition = _mouse.MousePosition = new(vpt.X, vpt.Y);
                _mouse.WheelDelta = InputSnapshot.WheelDelta;
                _mouse.Events.Clear();

                foreach (var ev in InputSnapshot.MouseEvents)
                {
                    var newEvent = new MouseEvent()
                    {
                        MouseButton = ev.MouseButton switch
                        {
                            Veldrid.MouseButton.Left => MouseButton.Left,
                            Veldrid.MouseButton.Middle => MouseButton.Middle,
                            Veldrid.MouseButton.Right => MouseButton.Right,
                            _ => (MouseButton)(-1)
                        },
                        Down = ev.Down
                    };

                    if (Enum.IsDefined(newEvent.MouseButton))
                    {
                        if (newEvent.Down)
                            _mouse.DownButtons.Add(newEvent.MouseButton);
                        else
                            _mouse.DownButtons.Remove(newEvent.MouseButton);

                        if (_mouse.DownButtons.Contains(MouseButton.Left))
                            _state |= InputState.LeftButton;
                        else
                            _state &= ~InputState.LeftButton;

                        _mouse.Events.Add(newEvent);
                    }
                }
            }

            {

                _keyboard.CharPresses.Clear();
                _keyboard.CharPresses.AddRange(InputSnapshot.KeyCharPresses);

                _keyboard.Events.Clear();

                foreach (var ev in InputSnapshot.KeyEvents)
                {
                    if (ev.Key is Veldrid.Key.AltLeft or Veldrid.Key.AltRight)
                    {
                        if (ev.Down)
                            _state |= InputState.Alt;
                        else
                            _state &= ~InputState.Alt;
                    }
                    else if (ev.Key is Veldrid.Key.ShiftLeft or Veldrid.Key.ShiftRight)
                    {
                        if (ev.Down)
                            _state |= InputState.Shift;
                        else
                            _state &= ~InputState.Shift;
                    }
                    else if (ev.Key is Veldrid.Key.ControlLeft or Veldrid.Key.ControlRight)
                    {
                        if (ev.Down)
                            _state |= InputState.Control;
                        else
                            _state &= ~InputState.Control;
                    }

                    var newEvent = new KeyEvent()
                    {
                        Key = ev.Key switch
                        {
                            Veldrid.Key.Down => Key.Down,
                            Veldrid.Key.Up => Key.Up,
                            Veldrid.Key.Left => Key.Left,
                            Veldrid.Key.Right => Key.Right,
                            Veldrid.Key.BackSpace => Key.BackSpace,
                            Veldrid.Key.Space => Key.Space,
                            _ => (Key)(-1)
                        },
                        Repeat = ev.Repeat,
                        Down = ev.Down
                    };

                    if (Enum.IsDefined(newEvent.Key))
                        _keyboard.Events.Add(newEvent);
                }
            }
        }

        public void SetMousePosition(PT pt)
        {
            var vpt = _window.ClientToScreen(new(pt.X, pt.Y));
            Mouse.MousePosition = new(vpt.X, vpt.Y);
        }

        public PT GetMousePosition()
        {
            var pt = _mouse.MousePosition;
            var vpt = _window.ScreenToClient(new(pt.X, pt.Y));
            return new(vpt.X, vpt.Y);
        }

        public void Flush(InputTypes inputTypes)
        {
            if (inputTypes.HasFlag(InputTypes.Mouse))
            {
                _mouse.Events.Clear();
            }

            if (inputTypes.HasFlag(InputTypes.Keyboard))
            {
                _keyboard.Events.Clear();
                _keyboard.CharPresses.Clear();
            }
        }
    }
}
