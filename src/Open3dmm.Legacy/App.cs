using Open3dmm.Core;
using Open3dmm.Core.Data;
using Open3dmm.Core.GUI;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using Open3dmm.Core.Veldrid;
using Open3dmm.MovieEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Open3dmm
{
    class AppWindow : IAppWindow
    {
        private readonly App _app;

        public Gob Gob { get; }
        public CursorType Cursor { get; set; }

        public AppWindow(App app, IComponentGraph componentGraph)
        {
            _app = app;
            Gob = new(2, componentGraph, this);
        }

        public PT GetCursorPosition()
        {
            return _app.GetMousePositionScaled();
        }

        public PT PointToClient(PT point)
        {
            return _app.Window.ClientToScreen(point);
        }

        public PT PointToScreen(PT point)
        {
            return _app.Window.ClientToScreen(point);
        }

        public void SetCursorImage(ICursorImage cursorImage, CursorType cursorType)
        {
            _app.ChangeCursor(cursorImage, cursorType);
        }

        public InputState GetInputState()
        {
            return _app.InputState;
        }
    }

    class ComponentGraph : IComponentGraph
    {
        private readonly App _app;
        private Stack<Cex> _cexStack = new();

        public ComponentGraph(App app)
        {
            _app = app;
        }

        public void BeginExchange()
        {
            _cexStack.Push(_app.Exchange);
            _app.Exchange = new();
        }

        public void EndExchange()
        {
            _app.Exchange = _cexStack.Pop();
        }

        public int GenIds(int count = 1)
        {
            return Component.NewIds(count);
        }

        public Component GetComponent(int componentId)
        {
            return _app.Find(componentId);
        }

        public bool TryGetExchange(out Cex cex)
        {
            cex = _app.Exchange;
            return cex != null;
        }
    }

    public partial class App : Application
    {
        private Movie movie;

        public override int GetTextSize()
        {
            if (textSize == 0)
                if (!miscAppStrings.TryGetString(14, out var str)
                    || !int.TryParse(str, out textSize))
                    textSize = base.GetTextSize();
            return textSize;
        }

        public override string GetTextFont()
        {
            if (textFont == null)
                if (!miscAppStrings.TryGetString(6, out textFont))
                    textFont = base.GetTextFont();
            return textFont;
        }

        public Movie TakeMovie()
        {
            return Interlocked.Exchange(ref movie, null);
        }

        public bool GoToStudio(Message m)
        {
            SoundManager.StopAll();
            if (!LoadStudioFiles(null, 1))
            {
                Current.Exchange.Enqueue(new((int)KnownMessageValues.Studio_ResetMaybe, null));
            }
            else
            {
                Kwa.Instance.Find(m.ParamA)?.Dispose();
                int b = (int)m.ParamB;
                if (b != -1)
                {
                    if (Kwa.Instance.Find(131074) is Gok studio)
                    {
                        studio.RunScript(b | 65536, ReadOnlySpan<int>.Empty, out _, out _);
                    }
                }
            }
            return true;
        }

        private bool LoadStudioFiles(string path, int unk)
        {
            Studio = new Studio(20005, MainModule, TagManager.Default, path, unk);
            return true;
        }

        private ChunkyFile mainFile;
        private GenericStrings<int> studioFilenames;
        private GenericStrings<int> buildingFilenames;
        private GenericStrings<int> sharedFilenames;
        private GenericStrings<int> miscAppStrings;
        public DataFileGroup MainModule { get; set; }
        private string mainDir;
        private Dictionary<string, ChunkyFile> allFiles;
        private Queue<string> missingFileQueue = new Queue<string>();
        private IList<ChunkyFile> studioFiles;
        private IList<ChunkyFile> buildingFiles;
        private int textSize;
        private string textFont;

        public App(IOpen3dmmWindow window, VeldridGraphicsContext graphicsContext) : base(window, graphicsContext)
        {
        }

        private void HandleErrors()
        {
            if (!Errors.LastError(out int errorCode))
                return;

            Errors.Flush();

            var htop = errorCode switch
            {
                0 => 536893440,
                1 => 536893441,
                2 => 536893442,
                100 => 536893443,
                204 => 536893453,
                400 => 536893456,
                402 => 536893458,
                500 => 536893460,
                602 => 536893513,
                603 => 536893525,
                10000 => 536893463,
                10300 => 536893471,
                10202 => 536893470,
                100000 => 536893477,
                100002 => 536893479,
                100003 => 536893480,
                100004 => 536893481,
                100005 => 536893482,
                100006 => 536893483,
                100007 => 536893484,
                100008 => 536893485,
                100009 => 536893486,
                100010 => 536893487,
                100011 => 536893488,
                100012 => 536893489,
                100013 => 536893490,
                100014 => 536893491,
                100015 => 536893492,
                100016 => 536893493,
                100017 => 536893494,
                100018 => 536893495,
                100019 => 536893496,
                100021 => 536893498,
                100022 => 536893499,
                100023 => 536893500,
                100024 => 536893501,
                100025 => 536893502,
                100026 => 536893503,
                100027 => 536893504,
                100028 => 536893505,
                100030 => 536893507,
                100031 => 536893508,
                100032 => 536893509,
                100033 => 536893510,
                100034 => 536893511,
                100035 => 536893512,
                100036 => 536893514,
                100038 => 536893516,
                100039 => 536893517,
                100040 => 536893518,
                100041 => 536893519,
                100042 => 536893520,
                100043 => 536893521,
                100044 => 536893522,
                100046 => 536893528,

                205 or 300
                or 401 or 403
                or 600 or 601
                or 10200 or 10201
                or 10100 or 10101
                or 10102 or 10103
                or 10301 or 10302 or 10303
                or 11000 or 11001
                or 100020 or 100029 or 100037 => genericError(),
                _ => -1,
            };
            if (htop is -1)
                return;
            MiscAppStrings.TryGetString(27, out _);
            Kwa.Instance.ShowError(MainModule, htop, 0);
            Kwa.Instance.Strg.Remove(1321);
            int genericError()
            {
                Kwa.Instance.Strg[1321] = errorCode.ToString();
                return 536893526;
            }
        }

        public GenericStrings<int> MiscAppStrings => miscAppStrings;

        public IList<ColorBgra> Palette
        {
            get => palette;
            set
            {
                this.palette = value;
                var buf = new int[256];
                for (int i = 0; i < 256; i++)
                {
                    var c = palette[i];
                    c.A = 0xff;
                    buf[i] = c.ToBgra();
                }
                GraphicsDevice.UpdateTexture(GraphicsContext.Palette, buf, 0, 0, 0, GraphicsContext.Palette.Width, GraphicsContext.Palette.Height, GraphicsContext.Palette.Depth, 0, 0);
                GraphicsContext.NotifyPaletteChanged();
            }
        }
        public InvalidateOptions FallbackInvalidateOptions { get; set; } = InvalidateOptions.Region;
        public int Field_0xd50 { get; set; }
        public int Field_0xd54 { get; set; }
        private int _mouseClickCounter;
        public Studio Studio { get; set; }

        private Queue<Action> backgroundQueue = new Queue<Action>();
        private IList<ColorBgra> palette;
        public const int ProductKey = 2;

        public void Start()
        {
            OnWindowResized();
            Window.Resized += OnWindowResized;
            Window.FocusGained += OnFocusGained;
            Window.FocusLost += OnFocusLost;
            //Window.MouseDown += OnMouseEvent;
            //Window.MouseUp += OnMouseEvent;
            Usac = new Usac();
            Exchange = new();
            Exchange.AddListener(this, 0x7fffffff, MessageFlags.Broadcast);
            new EasterEggCredits(this, Current.Exchange);
            allFiles = new Dictionary<string, ChunkyFile>(128);
            mainDir = TagManager.Default.RootDirectory;
            string mainChk = Path.Combine(mainDir, "3dmovie.chk");

            var products = new (string aliases, int key)[]
            {
                ("3D Movie Maker/3DMovie", 2),
                ("N3DMM EXPANSION/N3DMMExp", 3),
                ("Doraemon Character Kit/DORAEMON", 4),
                ("Expansions/EXPANSIONS", 5)
            };

            foreach (var (aliases, key) in products)
                TagManager.Default.RegisterProduct(aliases, key);

            mainFile = new ChunkyFile(mainChk);
            if (LoadUserData() && LoadStrings() && SetHomeDirectory())
            {
                MainModule = new DataFileGroup(buildingFilenames.Count + studioFilenames.Count + sharedFilenames.Count);
                buildingFiles = new List<ChunkyFile>(buildingFilenames.Count);
                studioFiles = new List<ChunkyFile>(studioFilenames.Count);

                if (CollectChk(sharedFilenames, MainModule, null)
                    && CollectChk(buildingFilenames, MainModule, buildingFiles)
                    && CollectChk(studioFilenames, MainModule, studioFiles))
                {
                    PatchBackstageShadowDuringAnimation();
                    var root = new AppWindow(this, new ComponentGraph(this)).Gob;

                    root.Window = Window;

                    var kwa = new Kwa(new GobOptions(0, root, false, InvalidateOptions.Region, rect: new(-320, -240, 320, 240), anchor: new(0.5f, 0.5f, 0.5f, 0.5f)));
                    var sceg = kwa.CreateScriptEngine(MainModule, kwa);
                    if (MainModule.TryResolve<Script>(new ChunkIdentifier(Tags.GLOP, 0x50001), out var script))
                    {
                        sceg.Run(script, out var r, out var r2);
                        if (!GetProp(0x23510, out var cond) || !SetProp(0x23300, cond == 0 ? 0x10200 : 0x10280))
                            throw new InvalidOperationException();

                        //SetProp(0x23300, 0x11800);
                        if (MainModule.TryResolve(new ChunkIdentifier(Tags.GLOP, 0), out script))
                        {
                            SetColorTable(131073);
                            sceg.Run(script, out r, out r2);
                        }
                    }
                }
                else
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Some files are missing!");
                    while (missingFileQueue.Count > 0)
                        sb.AppendLine(missingFileQueue.Dequeue());
                    ShowMessageBox(sb.ToString(), 0, 0);
                    throw new InvalidOperationException();
                }
            }
            else
                throw new InvalidOperationException();
            //GoToStudio(new Message(0, this, null, 71680, 1));
        }

        public void SetColorTable(int paletteCno)
        {
            if (MainModule.TryResolve<GenericList<ColorBgra>>(new ChunkIdentifier(Tags.GLCR, paletteCno), out var pal))
            {
                Palette = pal;
            }
        }

        private void PatchBackstageShadowDuringAnimation()
        {
            if (MainModule.TryResolve<Gokd>(new ChunkIdentifier(Tags.GOKD, 69640), out var gokd))
                gokd.SetLayout(new GokdLayout(0, 0, 0, 145));
        }

        private void OnWindowResized()
        {
            //if (Find(20005) is Studio studio)
            //{
            //    UiScale = 1f;
            //    studio.WindowResized(Window.Width, Window.Height);
            //}
            //else
            //{
            //    if ((float)Window.Width / Window.Height < 1.333f)
            //        UiScale = Window.Width / 640f;
            //    else
            //        UiScale = Window.Height / 480f;
            //    UiScale = Math.Max(1f, UiScale);
            //}
            Gob.Root?.UpdateRectangle(new(0, 0, Window.Width, Window.Height), null);
        }

        public void OnGobTreeUpdated()
        {
            OnWindowResized();
        }

        public override void OnCharEvent(char c)
        {
            // TODO: Does not consider modifier keys (shift/ctrl/etc.)
            Current.Exchange.Enqueue(new((int)KnownMessageValues.Char, null, (int)c));
        }

        public override void OnMouseEvent(MouseEvent ev)
        {
            const int DBLCLICKTIME = 500;

            VirtualFunc60();
            if (ev.Down && ev.MouseButton == MouseButton.Left)
            {
                if (Gob.Root is null || !Gob.Root.HitTest(GetMousePositionScaled(), out var result))
                {
                    MouseOverGob = null;
                    return;
                }
                var messageTime = Environment.TickCount;
                int diff = messageTime - MouseOverTime;

                if (MouseOverGob != result.GobHit || diff < 0 || DBLCLICKTIME <= diff)
                {
                    _mouseClickCounter = 1;
                }
                else
                {
                    _mouseClickCounter++;
                }

                if (MouseOverGob != null && MouseOverGob != result.GobHit)
                {
                    Current.Exchange.Enqueue(new((int)KnownMessageValues.MouseExit, MouseOverGob));
                }
                MouseOverGob = result.GobHit;
                MouseOverTime = messageTime;
                MouseOverGob.RaiseMouseClicked(result.PointHit, _mouseClickCounter, InputState);
            }
        }

        private void OnFocusLost()
        {
            FocusChanged(false);
        }

        private void OnFocusGained()
        {
            FocusChanged(true);
        }

        bool CollectChk(GenericStrings<int> listOfNames, DataFileGroup module, ICollection<ChunkyFile> files = null)
        {
            bool success = true;
            foreach (var grouping in listOfNames)
            {
                var path = Path.Combine(mainDir, grouping.Value) + ".CHK";
                var file = GetFile(path);
                if (file != null)
                {
                    module.AddFile(new DataFile(file, TagManager.Default.Factory, grouping.Key));
                    files?.Add(file);
                }
                else
                {
                    missingFileQueue.Enqueue(path);
                    success = false;
                }
            }
            return success;
        }

        ChunkyFile GetFile(string path)
        {
            path = path.ToLowerInvariant();
            if (!allFiles.TryGetValue(path, out var file))
            {
                if (!File.Exists(path))
                    return null;
                allFiles[path] = file = new ChunkyFile(path);
            }
            return file;
        }

        public override void OnUpdate(float deltaSeconds)
        {
            base.OnUpdate(deltaSeconds);
            if (!AppState.HasFlag(AppState.Focused))
                return;
            GameTime.Tick();

            EndLongOp(true);
            HandleErrors();
            if (!Current.Exchange.Dispatch() && Current.Exchange.Capture == null)
            {
                Current.Exchange.Enqueue(new((int)KnownMessageValues.UpdateState_61A81, null, AppState.HasFlag(AppState.Focused) ? 1 : 0));
                Current.Exchange.Enqueue(new((int)KnownMessageValues.Update, null));
            }
        }

        public void Run(Action<float> onUpdateDelegate = null)
        {
            this.onUpdateDelegate = onUpdateDelegate;
            Start();
            PumpMessages();
        }

        Action<float> onUpdateDelegate = null;

        public override void PumpMessages()
        {
            const float fps = 1f / 60f;
            var sw = new Stopwatch();
            sw.Start();
            var lastUpdate = sw.Elapsed;

            while (true)
            {
                if ((AppState.HasFlag(AppState.WindowClosed) || AppState.HasFlag(AppState.ThirtyTwo)) && field_0x7c >= 1)
                    return;

                var update = sw.Elapsed;
                var updateSeconds = (float)(update - lastUpdate).TotalSeconds;
                if (updateSeconds >= fps)
                {
                    OnUpdate(updateSeconds);
                    if (!Window.Exists)
                        goto DeadWindow;
                    onUpdateDelegate?.Invoke(updateSeconds);
                }

                Thread.Sleep(1);
            }
        DeadWindow:
            return;
        }

        bool LoadStrings()
        {
            return GetStrings(4, out studioFilenames)
            && GetStrings(5, out buildingFilenames)
            && GetStrings(7, out sharedFilenames)
            && GetStrings(6, out miscAppStrings);
        }

        bool GetStrings(int number, out GenericStrings<int> strings)
        {
            if (mainFile.TryGetChunk(new(Tags.GST, number), out var chunk))
            {
                using var block = BinaryStream.Create(chunk.Section.Memory).Decompress();
                strings = new GenericStrings<int>();
                strings.FromStream(block);
                return true;
            }
            strings = null;
            return false;
        }

        public bool LoadUserData()
        {
            for (int i = 0; i < 8; i++)
                SetProp(0x23500 + i, 1);
            SetProp(0x23503, 0);
            return true;
        }

        public bool SetHomeDirectory()
        {
            return SetProp(0x23510, 0);
        }

        public override void FocusChanged(bool focused)
        {
            base.FocusChanged(focused);

            if (Current.Exchange != null)
            {
                if (!focused)
                {
                    if (!Current.Exchange.Exists((int)KnownMessageValues.App_ShowModalWindow))
                    {
                        Current.Exchange.Enqueue(new((int)KnownMessageValues.App_ShowModalWindow, null));
                        Field_0xd50 = 1;
                        Field_0xd54 = 0;
                    }
                }
                else
                {
                    if (Cex_0xa4 is null)
                    {
                        Current.Exchange.Clear((int)KnownMessageValues.App_ShowModalWindow);
                    }
                    else if (!Cex_0xa4.Exists((int)KnownMessageValues.GotFocus))
                    {
                        Cex_0xa4.Enqueue(new((int)KnownMessageValues.GotFocus, null));
                    }
                }
            }
        }
    }
}
