using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using Open3dmm.VeldridSdl;
using Open3dmm.Core.Graphics;
using Open3dmm.Core.Veldrid;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Open3dmm
{
    public class LegacyEntrypoint
    {
        private static GraphicsDevice graphicsDevice;
        private static VeldridGraphicsContext graphicsContext;
        private static ImGuiRenderer guiRenderer;
        private static CommandList commandList;
        private static App app;
        public static ImFontPtr ComicSans;
        public static ImFontPtr ComicSansBold;

        public static void Main(string[] args)
        {
            var window = VeldridStartup.CreateWindow(new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 640,
                WindowHeight = 480,
                WindowTitle = "Open 3D Movie Maker"
            });

            graphicsDevice = VeldridStartup.CreateGraphicsDevice(window);
            graphicsContext = new VeldridGraphicsContext(graphicsDevice);
            guiRenderer = new ImGuiRenderer(graphicsDevice, graphicsDevice.SwapchainFramebuffer.OutputDescription, window.Width, window.Height, ColorSpaceHandling.Linear);

            window.Resized += () =>
            {
                graphicsDevice.ResizeMainWindow((uint)window.Width, (uint)window.Height);
                guiRenderer.WindowResized(window.Width, window.Height);
            };

            CreateResources();

            var fonts = new Dictionary<string, string>();
            foreach (var filename in Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Fonts)))
            {
                if (Path.GetExtension(filename).ToLowerInvariant() != ".ttf")
                    continue;
                if (!TryGetFontName(filename, out var name))
                    continue;
                fonts.Add(name.ToLowerInvariant(), filename);
            }

            ComicSans = ImGui.GetIO().Fonts.AddFontFromFileTTF(fonts["comic sans ms"], 16);
            ComicSansBold = ImGui.GetIO().Fonts.AddFontFromFileTTF(fonts["comic sans ms bold"], 16);
            guiRenderer.RecreateFontDeviceTexture();

            // TestEditor(window);
            StartMainLoop(window);
        }

        private static void StartMainLoop(Veldrid.Sdl2.Sdl2Window window)
        {
            var sw = new Stopwatch();
            sw.Start();
            var lastUpdate = sw.Elapsed;
            app = new App(new VeldridSdlWindow(window), graphicsContext)
            {
                DelegateGetOrCreateImGuiBinding = (tex) => guiRenderer.GetOrCreateImGuiBinding(graphicsDevice.ResourceFactory, tex),
                DelegateRemoveImGuiBinding = (texView) => guiRenderer.GetOrCreateImGuiBinding(graphicsDevice.ResourceFactory, texView),
            };

            app.Run(updateSeconds =>
            {
                guiRenderer.Update(updateSeconds, (app.Window as VeldridSdlWindow).InputSnapshot);
                BeginDraw();
                Draw(updateSeconds);
                EndDraw();
            });
        }

        static void Draw(float deltaSeconds)
        {
            if (Gob.Root != null)
                DrawGui(Gob.Root, new RectangleF(float.MinValue / 2, float.MinValue / 2, float.MaxValue, float.MaxValue));
        }

        static void BeginDraw()
        {
            commandList.Begin();
            //ImGui.GetIO().FontGlobalScale = App.Instance.UiScale;
            //ImGui.PushFont(ComicSans);
            //ImGui.Begin("mainWindow", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBackground);
        }

        static void EndDraw()
        {
            //ImGui.PopFont();
            ImGui.End();
            ShowDebugView();
            if (!ImGui.GetIO().WantCaptureMouse)
            {
                foreach (var ev in app.Window.Input.Mouse.Events)
                    Application.Current.OnMouseEvent(ev);
            }

            if (!ImGui.GetIO().WantCaptureKeyboard)
            {
                foreach (var c in app.Window.Input.Keyboard.CharPresses)
                    Application.Current.OnCharEvent(c);
            }

            commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);

            commandList.ClearColorTarget(0, RgbaFloat.Black);
            guiRenderer.Render(graphicsDevice, commandList);

            commandList.End();
            graphicsDevice.SubmitCommands(commandList);
            graphicsDevice.SwapBuffers();
        }

        static ImmutableHashSet<Gob> selectedDebugGob = ImmutableHashSet<Gob>.Empty;
        static bool selectedSomethingThisFrame;

        private static void ShowDebugView()
        {
            ImGui.Begin("DEBUBBUBUBBB");
            //ImGui.SetWindowFontScale(1f / App.Instance.UiScale);
            var root = Gob.Root;
            if (root != null)
            {
                selectedSomethingThisFrame = false;
                debugItem(root);
            }
            ImGui.End();

            foreach (var gob in selectedDebugGob)
            {
                var open = true;
                ImGui.Begin($"Info: {gob.Id}###DEBUGGOB{gob.RuntimeId}", ref open);
                if (!open)
                {
                    selectedDebugGob = selectedDebugGob.Remove(gob);
                    continue;
                }
                var parameters = gob.GetParameters(false);
                ImGui.Text($"State = {(gob as Gok)?.StateFlags ?? 0}");
                ImGui.Text($"ModifierState = {(gob as Gok)?.Owner.ModifierState ?? 0}");
                ImGui.Spacing();

                ImGui.Text("Parameters");
                if ((parameters?.Count ?? 0) == 0)
                {
                    ImGui.Text("<None>");
                }
                else
                {
                    foreach (var p in parameters)
                    {
                        int idx = (int)(p.Key & 0xffff0000) >> 16;
                        ImGui.Text($"{VariableNameParser.Parse(p.Key)}[{idx}] = {p.Value}");
                    }
                }
                ImGui.End();
            }

            var cex = (Cex)Application.Current.Exchange;

            ImGui.Begin($"Listeners###DEBUG-Listeners");
            foreach (var l in cex.GetListeners())
            {
                ImGui.Text($"{l.Priority} :: {l.Component.GetType().Name} - {l.Component.Id} {l.Kinds.ToString()}");
            }
            ImGui.End();

            static void debugItem(Gob gob)
            {
                bool isHovered;
                bool clicked;
                var name = $"{gob.GetType().Name} : {gob.Id}";
                if (gob.FirstChild != null)
                {
                    var isOpen = ImGui.TreeNode(name);
                    isHovered = ImGui.IsItemHovered();
                    clicked = ImGui.IsItemClicked(ImGuiMouseButton.Right);
                    if (gob is Gok gok && gok.Gorp is Gorb gorb)
                    {
                        ImGui.SameLine();
                        drawGorb(gorb);
                        isHovered |= ImGui.IsItemHovered();
                    }
                    if (isOpen)
                    {
                        debugItem(gob.FirstChild);
                        ImGui.TreePop();
                    }
                }
                else
                {
                    ImGui.Text(name);
                    isHovered = ImGui.IsItemHovered();
                    clicked = ImGui.IsItemClicked(ImGuiMouseButton.Right);
                    if (gob is Gok gok && gok.Gorp is Gorb gorb)
                    {
                        ImGui.SameLine();
                        drawGorb(gorb);
                        isHovered |= ImGui.IsItemHovered();
                    }
                }

                if (!selectedSomethingThisFrame && clicked && !selectedDebugGob.Contains(gob))
                {
                    selectedDebugGob = selectedDebugGob.Add(gob);
                    gob.Disposed += _ => selectedDebugGob = selectedDebugGob.Remove(gob);
                    selectedSomethingThisFrame = true;
                }

                if (isHovered)
                {
                    var rc = (RectangleF)gob.GetRectangle(CoordinateSpace.Window);
                    var s = Application.Current.UiScale;
                    rc.X *= s;
                    rc.Y *= s;
                    rc.Width *= s;
                    rc.Height *= s;
                    ImGui.GetForegroundDrawList().AddRect(new Vector2(rc.Left, rc.Top), new Vector2(rc.Right, rc.Bottom), 0xff0000ff, 0f, ImDrawFlags.RoundCornersNone, 2f * s);
                }

                if (gob.Next != null)
                    debugItem(gob.Next);
            }
            static void drawGorb(Gorb gorb)
            {
                if (gorb.Resolver.TryResolve<Mbmp>(new ChunkIdentifier(gorb.Tag, gorb.Number), out var mbmp))
                {
                    var tex = graphicsContext.GetTexture(mbmp);
                    var btex = Application.Current.GetOrCreateImGuiBinding(tex);
                    ImGui.Image(btex, new Vector2(16));
                }
            }
        }

        static void DrawDebug(Gob gob, ref int offset)
        {
            if (gob.Next != null)
                DrawDebug(gob.Next, ref offset);

            var pt = app.GetMousePositionScaled();
            var rc = gob.GetRectangle(CoordinateSpace.Window);
            if (gob.HitTest(new(pt.X - rc.Left, pt.Y - rc.Top), true))
            {
                ImGui.GetBackgroundDrawList().AddText(new Vector2(pt.X, pt.Y + offset), ImGui.ColorConvertFloat4ToU32(new Vector4(0, 1f, 0, 1)), gob.Id.ToString());
                offset += 16;
            }

            if (gob.FirstChild != null)
                DrawDebug(gob.FirstChild, ref offset);
        }

        static void DrawGui(Gob gob, in RectangleF clip)
        {
            if (gob.Next != null)
                DrawGui(gob.Next, in clip);

            var newClip = (RectangleF)gob.GetClipRectangle(CoordinateSpace.Window);
            newClip.X *= app.UiScale;
            newClip.Y *= app.UiScale;
            newClip.Width *= app.UiScale;
            newClip.Height *= app.UiScale;
            if (newClip.Intersect(in clip))
            {
                var dest = (RectangleF)gob.GetRectangle(CoordinateSpace.Window);
                dest.X *= app.UiScale;
                dest.Y *= app.UiScale;
                dest.Width *= app.UiScale;
                dest.Height *= app.UiScale;
                gob.Draw(commandList, in dest);
                if (gob.FirstChild != null)
                {
                    //ImGui.PushClipRect(new Vector2(newClip.Left, newClip.Top), new Vector2(newClip.Right, newClip.Bottom), false);
                    DrawGui(gob.FirstChild, in newClip);
                    //ImGui.PopClipRect();
                }
            }
        }

        static void CreateResources()
        {
            var factory = graphicsDevice.ResourceFactory;

            commandList = factory.CreateCommandList();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate IntPtr SDL_CreateRGBSurfaceFrom(IntPtr pixels,
                                      int width,
                                      int height,
                                      int depth,
                                      int pitch,
                                      uint Rmask,
                                      uint Gmask,
                                      uint Bmask,
                                      uint Amask);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void SDL_SetWindowIcon(IntPtr window, IntPtr surface);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void SDL_FreeSurface(IntPtr surface);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool GetFontResourceInfoW(
            string lpszFilename,
            [In, Out] ref int cbBuffer,
            [Out] StringBuilder lpBuffer,
            int dwQueryType
        );

        static unsafe bool TryGetFontName(string filename, out string name)
        {
            int bufferSize = 0;
            var sb = new StringBuilder();
            if (GetFontResourceInfoW(filename, ref bufferSize, null, 1))
            {
                sb.Capacity = bufferSize / sizeof(char);
                if (GetFontResourceInfoW(filename, ref bufferSize, sb, 1))
                {
                    name = sb.ToString();
                    return true;
                }
            }

            name = string.Empty;
            return false;
        }
    }
}
