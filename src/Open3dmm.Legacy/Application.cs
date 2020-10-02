using Open3dmm.Core;
using Open3dmm.Core.Resolvers;
using Open3dmm.Core.Veldrid;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Open3dmm
{
    [Flags]
    public enum AppState
    {
        WindowClosed = 1,
        Four = 4,
        Tooltip = 8,
        ThirtyTwo = 32,
        Focused = 16
    }
    public abstract partial class Application : Component
    {
        private int _runtimeId = int.MinValue;
        public int GenerateRuntimeId() => _runtimeId++;

        public Component Find(int id)
        {
            if (id == 0)
                return null;
            if (id == 1)
                return this;
            return (Component)Clok.Get(id) ?? Gob.Get(id);
        }

        public float UiScale { get; set; } = 1f;
        protected List<AppContext> ContextStack { get; }
        public AppState AppState { get; set; }
        public IOpen3dmmWindow Window { get; }
        public Cex Exchange { get; set; }
        public Cex Cex_0xa4 { get; set; }
        public Usac Usac { get; set; }
        public Gob MouseOverGob { get; set; }
        public SoundManager SoundManager { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public VeldridGraphicsContext GraphicsContext { get; private set; }
        public Docb RootDoc { get; internal set; }
        public int WorkCounter { get; set; }
        public Gob LastMouseOverGob { get; set; }
        public int Time { get; set; }
        public ICursorImage WaitCursor { get; private set; }
        public ICursorImage ArrowCursor { get; private set; }
        public Action<Texture> DelegateRemoveImGuiBinding { get; set; }
        public Func<Texture, IntPtr> DelegateGetOrCreateImGuiBinding { get; set; }
        public int MouseOverTime { get; set; }
        public int TimeUnscaled { get; set; }
        public InputState InputState { get; set; }
        protected readonly ISortedList<int, int> blobs;
        protected int field_0x7c;

        public void PromoteMessage(Message message)
        {
            if (ContextStack.Count > 0)
            {
                var cex = ContextStack[^1].Cex;
                cex.Enqueue(message);
            }
        }

        public void FlushUserEvents(InputTypes inputTypes)
        {
            flushRequested |= inputTypes;
        }

        public IntPtr GetOrCreateImGuiBinding(Texture texture)
        {
            return DelegateGetOrCreateImGuiBinding(texture);
        }

        public void RemoveImGuiBinding(Texture texture)
        {
            DelegateRemoveImGuiBinding(texture);
        }

        public static Application Current { get; private set; }
        public int DisplaySettings_Flags { get; set; }

        public Application(IOpen3dmmWindow window, VeldridGraphicsContext graphicsContext) : base(1)
        {
            if (Current != null)
                throw new InvalidOperationException("More than one app instance.");
            Current = this;

            Window = window;
            ContextStack = new List<AppContext>();
            SoundManager = new BassSoundManager(); // new DummySoundManager();
            GraphicsContext = graphicsContext;
            GraphicsDevice = graphicsContext.GraphicsDevice;
            blobs = SortedList.Create<int, int>();
        }

        public virtual int GetTextSize()
        {
            return 12;
        }

        public virtual string GetTextFont()
        {
            return "Times New Roman";
        }

        PT lastCursorPos;
        InputState lastInput;
        private InputTypes flushRequested;

        public virtual bool UpdateWindow(Message m)
        {
            if (AppState.HasFlag(AppState.WindowClosed))
                return false;

            if (InputState.HasFlag(InputState.LeftButton))
                return true;

            if (Gob.Root != null)
            {
                var mousePosition = GetMousePositionScaled();
                Gob.Root.HitTest(mousePosition, out var hitResult);

                if (hitResult.GobHit == MouseOverGob)
                {
                    NotifyMouseOver(mousePosition, false);
                }
                else
                {
                    MouseOverTime = 0;

                    if (MouseOverGob != null)
                    {
                        Current.Exchange.Enqueue(new((int)KnownMessageValues.MouseExit, MouseOverGob));
                        DeleteTooltip();
                    }

                    Time = Usac.GetTime();
                    MouseOverGob = hitResult.GobHit;
                    NotifyMouseOver(mousePosition, true);
                }

                if (MouseOverGob != null)
                {
                    var t = Usac.GetTime() - Time;

                    if (AppState.HasFlag(AppState.Tooltip) || t > TimeUnscaled)
                    {
                        ShowTooltip();
                    }
                }
            }

            return true;
        }

        private void NotifyMouseOver(PT pos, bool force)
        {
            if (!force && lastCursorPos == pos && lastInput == InputState)
                return;

            lastCursorPos = pos;
            lastInput = InputState;
            Current.Exchange.Enqueue(new((int)KnownMessageValues.MouseOver, MouseOverGob, pos.X, pos.Y, (int)lastInput));
        }

        public PT GetMousePositionScaled()
        {
            return (PT)((Vector2)Window.GetMousePosition() / UiScale);
        }

        public void ShowTooltip()
        {
            if (LastMouseOverGob != MouseOverGob)
            {
                var tooltip = Gob.Get(17) as Hbal;
                if (MouseOverGob.ShowToolTip(ref tooltip, (int)lastCursorPos.X, (int)lastCursorPos.Y))
                {
                    AppState |= AppState.Tooltip;
                    LastMouseOverGob = MouseOverGob;
                }
                else
                {
                    AppState &= ~AppState.Tooltip;
                    tooltip?.Dispose();
                    LastMouseOverGob = null;
                }
            }
        }

        public virtual bool VirtualFunc44(Message m, out object result)
        {
            result = 1;
            return true;
        }

        public virtual bool VirtualFunc46(Message m)
        {
            return true;
        }

        public virtual bool VirtualFunc42(Message m)
        {
            VirtualFunc22(0);
            return true;
        }

        public virtual void VirtualFunc22(int param_1)
        {
            AppState |= AppState.WindowClosed;
        }

        public virtual void FocusChanged(bool focused)
        {
            AppState = FlagHelper.SetFlag(AppState, AppState.Focused, focused);
        }

        public virtual void RaiseMessageHandlerDisposed(Component messageHandler)
        {
            Current.Exchange.RemoveReferences(messageHandler);

            foreach (var ctx in ContextStack)
                ctx.Cex.RemoveReferences(messageHandler);

            if (MouseOverGob == messageHandler)
            {
                MouseOverGob = null;
                DeleteTooltip();
            }
        }

        public void DeleteTooltip()
        {
            var tooltip = Gob.Get(17);
            if (tooltip != null)
            {
                if (MouseOverGob == tooltip)
                    MouseOverGob = null;
                LastMouseOverGob = null;
                tooltip.Dispose();
            }
        }

        public virtual void OnUpdate(float deltaSeconds)
        {
            Window.DoEvents();
            Window.Input.Flush(flushRequested);
            flushRequested = 0;
            InputState = Window.Input.State;
        }

        public virtual void VirtualFunc60()
        {
            DeleteTooltip();
            AppState &= ~AppState.Tooltip;
            Time = Usac.GetTime();
        }

        public virtual void ChangeCursor(ICursorImage newCursor, CursorType cursorType)
        {
            switch (cursorType)
            {
                case CursorType.Arrow:
                    ArrowCursor = newCursor;
                    break;
                case CursorType.Wait:
                    WaitCursor = newCursor;
                    break;
            }
            UpdateCursor();
        }

        public virtual void UpdateCursor()
        {
            const int IDC_ARROW = 32512;
            const int IDC_WAIT = 32514;
            int lpCursorName;

            if (WorkCounter < 1)
                lpCursorName = IDC_ARROW;
            else lpCursorName = IDC_WAIT;
            SetCursor(LoadCursorA(0, lpCursorName));
        }

        public virtual void StartLongOp()
        {
            if (WorkCounter++ == 0)
                UpdateCursor();
        }

        [DllImport("user32")] static extern IntPtr LoadCursorA(int hInstance, int lpCursorName);
        [DllImport("user32")] static extern IntPtr SetCursor(IntPtr hCursor);

        public int ShowMessageBox(string str, int buttons, int icon)
        {
            var type = buttons switch
            {
                1 => 1,
                2 => 4,
                3 => 3,
                _ => 0,
            };

            type |= icon switch
            {
                1 => 0x40,
                2 => 0x20,
                3 => 0x30,
                4 => 0x10,
                _ => 0,
            };

            return MessageBoxCore(str, string.Empty, type) switch
            {
                2 => 2,
                7 => 0,
                _ => 1,
            };
        }

        protected virtual int MessageBoxCore(string text, string caption, int type)
        {
            return 0;
        }

        public virtual void EndLongOp(bool forceReset)
        {
            if (WorkCounter != 0)
            {
                if (forceReset)
                    WorkCounter = 0;
                else
                    WorkCounter--;
                if (WorkCounter == 0)
                    UpdateCursor();
            }
        }

        public bool GetProp(int a, out int value)
        {
            switch (a)
            {
                case 1:
                    value = 0;
                    break;
                case 2:
                    value = Convert.ToInt32(AppState.HasFlag(AppState.Four));
                    break;
                case 3:
                    value = TimeScale(TimeUnscaled, 60, 1000);
                    break;
                default:
                    return blobs.TryGetValue(a, out value);
            }
            return true;
        }

        public bool SetProp(int a, int b)
        {
            switch (a)
            {
                case 1:
                    // IsZoomed() ... ShowWindow()
                    return true;
                case 2:
                    throw new NotImplementedException();
                case 3:
                    if (b < 0)
                        b = 0;
                    else if (b > 0x7ae1479)
                        b = 0x7ae1479;
                    TimeUnscaled = (b * 1000) / 60;
                    VirtualFunc60();
                    return true;
                default:
                    blobs.Set(a, b);
                    return true;
            }
        }

        public static int TimeScale(int t, int a, int b)
        {
            t = (t * a) / b;
            if (t < 0)
                return t - 1;
            if (t > 0)
                return t + 1;
            return 0;
        }

        public virtual void ChangeCursor(IResolver resolver, int number, CursorType cursorType)
        {
            if (resolver.TryResolve<Curs>(new ChunkIdentifier(Tags.GGCR, number), out var cursor))
            {
                ChangeCursor(cursor, cursorType);
            }
        }

        public abstract void PumpMessages();

        public virtual void OnMouseEvent(MouseEvent mouseEvent)
        {
        }

        public virtual void OnCharEvent(char chr)
        {
        }

        public void SetMousePosition(Vector2 pos)
        {
            Window.SetMousePosition((PT)(pos * UiScale));
        }

        public void SetCursorVisible(bool visible)
        {
            Window.CursorVisible = visible;
        }
    }

    //public class Gnv : IDisposable
    //{
    //    Gpt _target;
    //    LTRB _from;
    //    LTRB _to;

    //    public void Dispose()
    //    {
    //        _target?.Dispose();
    //    }

    //    public void Blit(Gnv other, LTRB from, LTRB to)
    //    {
    //        if (other.Transform(from, out from) &&
    //            this.Transform(to, out to))
    //        {
    //            _target.Blit(other._target, from, to);
    //        }
    //    }

    //    public bool Transform(LTRB rect, out LTRB result)
    //    {
    //        result = rect;
    //        result.Transform(_from, _to);
    //        return result.IsValid();
    //    }
    //}

    //public enum GptPixelFormat
    //{
    //    Rgb,
    //    Indexed,
    //}

    //public abstract class Gpt : IDisposable
    //{
    //    private Action _cleanUp;
    //    private int _width;
    //    private int _height;

    //    public int OffsetX { get; set; }
    //    public int OffsetY { get; set; }
    //    public int Width => _width;
    //    public int Height => _height;

    //    public Gpt(int width, int height, GptPixelFormat format)
    //    {
    //        _width = width;
    //        _height = height;
    //    }

    //    protected virtual void Dispose(bool disposing)
    //    {
    //        if (!_disposedValue)
    //        {
    //            if (disposing)
    //            {
    //                _cleanUp?.Invoke();
    //            }
    //            _cleanUp = null;
    //            _disposedValue = true;
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        Dispose(disposing: true);
    //        GC.SuppressFinalize(this);
    //    }

    //    public void Resize(int width, int height)
    //    {
    //        if (Width == width && Height == height)
    //            return;

    //        if (width < 0 || height < 0)
    //            throw new ArgumentOutOfRangeException();

    //        _width = width;
    //        _height = height;
    //    }

    //    public abstract void Blit(Gpt other, LTRB from, LTRB to);
    //}
}
