using Veldrid;

namespace Open3dmm
{
    public partial class Gob : Component
    {
        private static int _rtiCounter = int.MinValue;
        private static Gob _root;

        private Gob _parent;
        private Gob _firstChild;
        private Gob _next;

        private IOpen3dmmWindow _associatedWindow;
        private LTRB _localRectangle;
        private LTRB _clipRectangle;

        private ParameterList _parameters;

        public IComponentGraph ComponentGraph { get; private set; }
        public IAppWindow AppWindow { get; private set; }

        public Gob(int gid, IComponentGraph graph, IAppWindow window) : base(gid)
        {
            Init(new(gid, null, false, InvalidateOptions.Inherit));
            ComponentGraph = graph;
            AppWindow = window;
        }

        public Gob(GobOptions options) : base(options.Id)
        {
            Init(options);
        }

        public static Gob Root => _root;

        public Gob Parent => _parent;
        public Gob FirstChild => _firstChild;
        public Gob Next => _next;
        public Gob Previous
        {
            get
            {
                var curr = Parent?.FirstChild ?? _root;

                if (curr == this)
                    return null;

                while (true)
                {
                    if (curr.Next == this)
                        return curr;

                    if (curr.Next == null)
                        return null;

                    curr = curr.Next;
                }
            }
        }

        public int RuntimeId { get; } = _rtiCounter++;
        public int Flags_6c { get; set; }

        public IOpen3dmmWindow Window
        {
            get => _associatedWindow;
            set
            {
                if (_associatedWindow == value)
                    return;

                if (_associatedWindow != null)
                {
                    _associatedWindow.Resized -= CalculateRectangle;
                }

                _associatedWindow = value;

                if (_associatedWindow != null)
                {
                    _associatedWindow.Resized += CalculateRectangle;
                    CalculateRectangle();
                }
            }
        }

        public LTRB ActualRect { get; set; }
        public Anchor Anchor { get; set; }
        public Curs ArrowCursor { get; set; }

        private void Init(GobOptions options)
        {
            Flags_6c = (int)options.Unk | 0x200;
            if (options.Relative is Gob gob)
            {
                if (!options.MakeSibling)
                {
                    _parent = gob;
                    _next = gob.FirstChild;
                    gob._firstChild = this;
                }
                else
                {
                    _parent = gob.Parent;
                    _next = gob.Next;
                    gob._next = this;
                }

                ComponentGraph = gob.ComponentGraph;
                AppWindow = gob.AppWindow;
            }
            else
                _root = this;
            UpdateRectangle(options.Rect, options.Anchor);
            Flags_6c &= ~0x200;
        }

        public virtual bool OnMouseEvent(Message m)
        {
            return true;
        }

        public virtual bool OnMouseOver(Message m)
        {
            AppWindow.SetCursorImage(ArrowCursor, CursorType.Arrow);
            return true;
        }

        public virtual bool OnCharEvent(Message m)
        {
            return false;
        }

        public virtual bool VirtualFunc23(Message m)
        {
            return false;
        }

        public virtual bool VirtualFunc24(Message m)
        {
            return false;
        }

        public virtual bool VirtualFunc25(Message m)
        {
            return false;
        }

        public virtual bool ShowToolTip(ref Hbal hbal, int x, int y)
        {
            return false;
        }

        public ParameterList GetParameters(bool create = false)
        {
            if (create)
                _parameters ??= new();
            return _parameters;
        }

        public virtual void ChangeCursorArrow(int ownerState)
        {
            AppWindow.SetCursorImage(ArrowCursor, CursorType.Arrow);
        }

        public PT TransformPoint(PT pt, CoordinateSpace from, CoordinateSpace to)
        {
            TransformPoint(ref pt, from, to);
            return pt;
        }

        public void TransformPoint(ref PT pt, CoordinateSpace from, CoordinateSpace to)
        {
            pt -= GetPosition(from);
            pt += GetPosition(to);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (FirstChild != null)
                    FirstChild.Dispose();

                if (Parent == null)
                    _root = Next;
                else
                {
                    var prev = Previous;
                    if (prev == null)
                        Parent._firstChild = Next;
                    else
                        prev._next = Next;
                }
            }
            base.Dispose(disposing);
        }

        public void MoveToSibling(Gob sibling)
        {
            if (sibling == null || Parent == sibling.Parent)
            {
                var prev = Previous;
                if (prev != sibling)
                {
                    if (prev == null)
                        Parent._firstChild = Next;
                    else
                        prev._next = Next;

                    if (sibling == null)
                    {
                        _next = Parent.FirstChild;
                        Parent._firstChild = this;
                    }
                    else
                    {
                        _next = sibling.Next;
                        sibling._next = this;
                    }
                }
            }
        }

        public PT GetCursorPosition()
        {
            return TransformPoint(AppWindow.GetCursorPosition(), CoordinateSpace.Window, CoordinateSpace.None);
        }

        public static Gob Get(int gid) => _root?.Find(gid, true);
        public static Gob GetByGlobalId(int globalId) => _root?.FindByRuntimeId(globalId, true);

        public Gob FindByRuntimeId(int globalId, bool inclusive = true)
        {
            if (inclusive && RuntimeId == globalId)
                return this;
            return FirstChild?.FindByRuntimeId(globalId) ?? Next?.FindByRuntimeId(globalId);
        }

        public Gob Find(int gid, bool inclusive = true)
        {
            if (inclusive && Id == gid)
                return this;
            return FirstChild?.Find(gid) ?? Next?.Find(gid);
        }

        public LTRB GetRectangle(CoordinateSpace coordinateSpace)
        {
            var rect = _localRectangle;
            var pt = GetPosition(coordinateSpace);
            rect.Offset(pt.X - rect.Left, pt.Y - rect.Top);
            return rect;
        }

        public LTRB GetClipRectangle(CoordinateSpace coordinateSpace)
        {
            var clipRect = _clipRectangle;
            var pt = GetPosition(coordinateSpace);
            clipRect.Offset(pt.X - _localRectangle.Left, pt.Y - _localRectangle.Top);
            return clipRect;
        }

        public PT GetPosition(CoordinateSpace coordinateSpace)
        {
            Gob gob;
            PT pt = default;

            switch (coordinateSpace)
            {
                case CoordinateSpace.Local:
                    pt.X = _localRectangle.Left;
                    pt.Y = _localRectangle.Top;
                    break;

                case CoordinateSpace.GPT:
                case CoordinateSpace.Window:
                case CoordinateSpace.Screen:
                    gob = this;
                    while (gob != null && gob.Window == null)
                    {
                        pt.X += gob._localRectangle.Left;
                        pt.Y += gob._localRectangle.Top;
                        gob = gob.Parent;
                    }
                    if (coordinateSpace == CoordinateSpace.Screen && gob?.Window != null)
                    {
                        pt = gob.Window.ClientToScreen(pt);
                    }
                    break;
                default:
                    return default;
            }

            return pt;
        }

        public IOpen3dmmWindow GetWindow()
        {
            var gob = this;
            while (gob != null && gob._associatedWindow == null)
            {
                gob = gob.Parent;
            }
            return gob?._associatedWindow;
        }

        public virtual void RaiseMouseClicked(PT pt, int clicks, InputState input)
        {
            if (ComponentGraph.TryGetExchange(out var cex))
            {
                cex.Enqueue(new((int)KnownMessageValues.MouseDown, this));
                cex.Enqueue(new((int)KnownMessageValues.UpdateState_61A81, null, 1, Id));
                cex.Enqueue(new((int)KnownMessageValues.MousePressed, this, pt.X, pt.Y, (int)input, clicks));
            }
        }

        public virtual void UpdateRectangle(LTRB? newRectangle, Anchor? anchor)
        {
            ActualRect = newRectangle ?? default;
            Anchor = anchor ?? default;
            CalculateRectangle();
        }

        private void CalculateRectangle()
        {
            LTRB rect, clip;

            clip = new(-1073741823, -1073741823, 1073741823, 1073741823);
            if (Window is not null)
            {
                rect = new(0, 0, Window.Width, Window.Height);
            }
            else
            {
                rect = ActualRect;

                if (Parent is not null)
                {
                    rect.AnchorTo(Parent._localRectangle, Anchor);
                    clip = Parent.GetClipRectangle(CoordinateSpace.None);
                }
            }

            clip.Intersect(rect);
            _localRectangle = rect;
            _clipRectangle = clip;
            VirtualFunc5();

            var child = FirstChild;
            while (child != null)
            {
                child.CalculateRectangle();
                child = child.Next;
            }
        }

        public virtual void VirtualFunc5()
        {
        }

        public void Offset(int offsetX, int offsetY)
        {
            if (offsetX == 0 && offsetY == 0) return;
            var newRect = _localRectangle;
            newRect.Offset(offsetX, offsetY);
            UpdateRectangle(newRect, null);
        }

        public virtual void Draw(CommandList commandList, in RectangleF dest)
        {
        }

        public virtual bool HitTest(PT pt, out HitTestResult result)
        {
            pt.X -= _localRectangle.Left;
            pt.Y -= _localRectangle.Top;

            if (HitTest(pt, false))
            {
                var child = FirstChild;
                while (child != null)
                {
                    if (child.HitTest(pt, out result))
                        return true;
                    child = child.Next;
                }
            }

            if (HitTest(pt, true))
            {
                result = new HitTestResult(this, pt);
                return true;
            }

            result = default;
            return false;
        }

        public virtual bool HitTest(PT pt, bool precise)
        {
            if (Id == 0x11)
                return false;
            return GetRectangle(CoordinateSpace.None).Contains(pt);
        }

        public virtual void MoveAbs(int x, int y)
        {
            var rc = GetRectangle(CoordinateSpace.Local);
            rc.Offset(x - rc.Left, y - rc.Top);
            UpdateRectangle(rc, null);
        }

        public T GetAnscestorOfType<T>()
        {
            var current = Parent;
            while (current != null)
            {
                if (current is T t)
                    return t;
                current = current.Parent;
            }
            return default;
        }

        public void MoveToFront()
        {
            MoveToSibling(null);
        }

        public bool IsAnscestorOf(Gob gob)
        {
            return this == gob || gob is not null && IsAnscestorOf(gob.Parent);
        }
    }
}
