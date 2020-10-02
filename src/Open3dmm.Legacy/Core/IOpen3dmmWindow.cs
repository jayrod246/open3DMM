using System;
using System.Collections.Generic;

namespace Open3dmm
{
    public enum Key
    {
        Up,
        Down,
        Left,
        Right,
        BackSpace,
        Enter,
        Space,
    }

    public enum MouseButton
    {
        Left,
        Right,
        Middle,
    }

    public struct KeyEvent
    {
        public bool Down { get; set; }
        public bool Repeat { get; set; }
        public Key Key { get; set; }
    }

    public struct MouseEvent
    {
        public MouseButton MouseButton { get; set; }
        public bool Down { get; set; }
    }

    public interface IOpen3dmmMouse
    {
        IReadOnlyList<MouseEvent> Events { get; }
        float WheelDelta { get; }
        PT MousePosition { get; set; }

        bool IsMouseDown(MouseButton button);
        bool SetCapture(bool enable);
    }

    public interface IOpen3dmmKeyboard
    {
        IReadOnlyList<KeyEvent> Events { get; }
        IReadOnlyList<char> CharPresses { get; }
    }

    public interface IOpen3dmmInput
    {
        InputState State { get; }
        IOpen3dmmMouse Mouse { get; }
        IOpen3dmmKeyboard Keyboard { get; }
        void Flush(InputTypes inputTypes);
    }

    public interface IOpen3dmmWindow
    {
        IOpen3dmmInput Input { get; }
        int Width { get; }
        int Height { get; }
        event Action Resized;
        event Action FocusGained;
        event Action FocusLost;
        PT ClientToScreen(PT point);
        bool CursorVisible { get; set; }
        void SetMousePosition(PT point);
        PT GetMousePosition();
        bool Exists { get; }
        void DoEvents();
    }
}
