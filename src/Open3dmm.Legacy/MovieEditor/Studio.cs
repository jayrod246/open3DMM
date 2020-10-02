using Open3dmm.Core;
using Open3dmm.Core.Actors;
using Open3dmm.Core.Data;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using Open3dmm.Core.Scenes;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;

namespace Open3dmm.MovieEditor
{
    // TODO: SSCB, SMCC, BRWR, FUN_0040d740, FUN_0040ec30, SetUpMovie
    public partial class Studio : Gob
    {
        public IResolver Resolver { get; set; }
        public TagManager TagManager { get; }
        public Smcc Smcc { get; set; }
        public int Field0xbc { get; set; }
        public GenericStrings MiscAppStrings { get; set; }
        public IList<ColorBgra> ColorPalette { get; set; }
        public Movie Movie { get; set; }
        public Browser ActorList { get; set; }
        public Browser PropList { get; set; }
        public List<int> Field_0x90 { get; set; }

        public bool FUN_0040f1c0(Message m)
        {
            var view = GetOrCreateMovieView();
            var tool = Convert.ToInt32(m.ParamA);
            switch (tool)
            {
                case 0:
                    view.ChangeState(new MvuDoNothing());
                    break;
                case 1:
                    view.ChangeState(new MvuReposition());
                    break;
            }
            return true;
        }

        public bool ShowBrowser(Message m)
        {
            switch (Convert.ToInt32(m.ParamA))
            {
                case 131134:
                    new Browser(Resolver, 131366, Tags.BKTH, false, SelectBackground, null, Movie.CurrentScene?.BackgroundReference);
                    break;

                case 131196:
                    // TODO: Transfrom CAM ID to CATH ID
                    var id = Movie.CurrentScene.CameraId;
                    if (Movie.CurrentScene != null
                        && TagManager.TryGetItem(Movie.CurrentScene.BackgroundReference, true, out var chunk))
                    {
                        GlobalReference? selected = null;
                        foreach (var child in chunk.Children)
                        {
                            if (child.Identifier.Tag == Tags.CATH)
                            {
                                var data = chunk.File.GetChunk(child.Identifier).Section.Span;
                                var identifier = MemoryMarshal.Read<ChunkIdentifier>(data.Slice(4));
                                if (identifier == new ChunkIdentifier(Tags.CAM, id))
                                {
                                    selected = new(Movie.CurrentScene.BackgroundReference.ProductKey, child.Identifier);
                                    break;
                                }
                            }
                        }
                        new Browser(Resolver, 131367, Tags.CATH, false, SelectCameraAngle, Movie.CurrentScene.BackgroundReference, selected);
                    }
                    break;

                case 131135:
                    new Browser(Resolver, 131368, Tags.TMTH, false, SelectActorOrProp);
                    break;

                case 131136:
                    new Browser(Resolver, 131369, Tags.PRTH, false, SelectActorOrProp);
                    break;

                case 131139:
                    new Browser(Resolver, 131372, Tags.SMTH, true, PlaySound);
                    break;

                case 131140:
                    new Browser(Resolver, 131371, Tags.SFTH, true, PlaySound);
                    break;

                case 131141:
                    new Browser(Resolver, 131373, Tags.SVTH, true, PlaySound);
                    break;
            }

            Application.Current.Exchange.Enqueue(new((int)KnownMessageValues.Browser_Active, null, 1));
            return true;
        }

        private void SelectCameraAngle(bool confirmed, int index, GlobalReference reference)
        {
            if (!confirmed)
                return;

            if (TagManager.TryGetThumIdentifier<ReferenceIdentifier>(reference, out var identifier))
            {
                Movie.CurrentScene.ChangeCamera(identifier.Index);
            }
        }

        private void SelectActorOrProp(bool confirmed, int index, GlobalReference reference)
        {
            if (!confirmed)
                return;

            if (TagManager.TryGetThumIdentifier<ChunkIdentifier>(reference, out var identifier))
            {
                TagManager.TryGetProduct(reference.ProductKey, out var product);
                if (product.Get3cnModule().TryResolve<Template>(identifier, out var template))
                    GetOrCreateMovieView().ChangeState(new MvuSpawn(template, ActorSpawned));
            }
        }

        private void ActorSpawned(bool success)
        {
            if (!success)
            {
                Application.Current.Exchange.Enqueue(new((int)KnownMessageValues.Mvu_SpawnActorCancel, null));
                return;
            }

            Application.Current.Exchange.Enqueue(new((int)KnownMessageValues.Mvu_SpawnActorConfirm, null));
            var root = Kwa.Instance;
            if (root.Find(131074) is Gok gok_131074)
            {
                gok_131074.RunScript(65574, default, out _, out _);
            }
        }

        private void PlaySound(bool confirmed, int index, GlobalReference reference)
        {
            if (confirmed)
                return;

            if (TagManager.TryGetThumIdentifier<ChunkIdentifier>(reference, out var identifier))
            {
                TagManager.TryGetProduct(reference.ProductKey, out var product);
                Application.Current.SoundManager.StopAll();
                Application.Current.SoundManager.PlaySound(product.Get3cnModule(), identifier);
            }
        }
        private void SelectBackground(bool confirmed, int index, GlobalReference reference)
        {
            if (!confirmed)
                return;

            var file = Movie.Scope.File;
            var cScen = file.Add(Tags.SCEN);
            file.AddParentChild(Movie.Identifier, new(cScen.Identifier, 0));
            Movie.Scope.TryResolve<Scene>(cScen.Identifier, out var newScene);
            newScene.BackgroundReference = reference;
            Movie.Scenes.Add(newScene);
            Movie.CurrentScene = newScene;
            Movie.CurrentSceneIndex = newScene.Metadata.Resolver.File.GetChunk(Movie.Identifier).GetChildID(newScene.Metadata.GetItem());

            // TODO: Extract this somewhere else, too dirty!
            var newPalette = new ColorBgra[256];
            (Application.Current as App).Palette.CopyTo(newPalette, 0);
            Movie.CurrentScene.GetBackground().Palette.CopyTo(newPalette, 10);
            (Application.Current as App).Palette = newPalette;
            FUN_004103f0();
            Smcc.FUN_004101d0(16);
            Application.Current.Exchange.Enqueue(new((int)KnownMessageValues.Mvu_Background, null));
        }

        public Studio(int id, IResolver resolver, TagManager tagManager, string moviePath, int unk) : base(new GobOptions(id, Kwa.Instance, false, InvalidateOptions.Inherit))
        {
            Resolver = resolver;
            TagManager = tagManager;
            Smcc = new Smcc(544, 306, 0x40000, 0, this);
            var movie = (Application.Current as App).TakeMovie();
            int someFlag = moviePath != null || movie != null ? 0 : 1;
            if (RunScript(someFlag))
            {
                if (movie == null)
                {
                    if (!FUN_0040ec30(moviePath, -1))
                    {
                        if (unk != 0 || !FUN_0040ec30(null, -1))
                            throw new InvalidOperationException();
                    }
                }
                else
                {
                    movie.Smcc = Smcc;
                    if (!SetUpMovie(movie))
                        throw new InvalidOperationException();
                    movie.FUN_004a22b0();
                }
            }
        }

        public void FUN_004103f0()
        {
            var root = Kwa.Instance;
            if (root.Find(131074) is Gok gok_131074)
            {
                if (Movie.CurrentScene == null)
                {
                    gok_131074.RunScript(65570, default, out _, out _);
                    return;
                }
                gok_131074.RunScript(65573, default, out _, out _);
            }
            if (root.Find(131078) is Gok gok_131078)
            {
                gok_131078.ChangeState(2);
            }
            if (root.Find(131079) is Gok gok_131079)
            {
                gok_131079.ChangeState(2);
            }
            if (root.Find(131080) is Gok gok_131080)
            {
                gok_131080.ChangeState(2);
            }
            if (root.Find(131083) is Gok gok_131083)
            {
                gok_131083.ChangeState(3);
            }
            if (root.Find(131085) is Gok gok_131085)
            {
                gok_131085.ChangeState(3);
            }

            if (root.Find(131086) is Gok gok_131086)
            {
                gok_131086.ChangeState(3);
            }
        }

        private bool SetUpMovie(Movie movie)
        {
            Application.Current.StartLongOp();
            Smcc.SceneSlider.Movie = null;
            Movie = movie;
            _ = GetOrCreateMovieView();
            (Kwa.Instance.Find(131074) as Gok).RunScript(0x10022, ReadOnlySpan<int>.Empty, out _, out _);
            return true;
        }

        private Mvu GetOrCreateMovieView()
        {
            var root = Kwa.Instance;
            return (root.Find(10) as Mvu) ?? Movie.CreateView(new GobOptions(10, root.Find(131165), false, InvalidateOptions.Inherit));
        }

        private bool FUN_0040ec30(string moviePath, int v)
        {
            if (moviePath != null)
            {
                if (!OpenMovieFromFilename(null, -1, out var success))
                    return false;
                if (!success)
                    return true;
            }
            return OpenMovieFromFilename(moviePath, v, out _);
        }

        private bool OpenMovieFromFilename(string moviePath, int v, out bool success)
        {
            bool hasMovie;
            success = true;
            if (Movie != null)
            {
                // TODO: Handle movie already open.
                // success = false;
                // hasMovie = true;
                throw new NotImplementedException("Handle movie already open.");
            }

            if (hasMovie = SetUpMovie(new Movie((Application.Current.DisplaySettings_Flags & 2) != 0, Smcc, moviePath, v)))
            {
                //if (Container.Woks?.FindDescendant(131074) is Gok gok) //&& Movie.Scene != null)
                //{
                //    gok.RunScript(65578, default, out _, out _);
                //    gok.RunScript(65579, default, out _, out _);
                //}
            }

            return hasMovie;
        }

        private bool RunScript(int someFlag)
        {
            try
            {
                Application.Current.StartLongOp();
                if (Resolver.TryResolve<GenericStrings>(new ChunkIdentifier(Tags.GST, 2), out var miscAppStrings))
                {
                    MiscAppStrings = miscAppStrings;
                    var sceg = Kwa.Instance.CreateScriptEngine(Resolver, Kwa.Instance);
                    if (sceg != null)
                    {
                        Resolver.TryResolve<GenericList<ColorBgra>>(new ChunkIdentifier(Tags.GLCR, 131073), out var palette);
                        ColorPalette = palette;

                        if (Resolver.TryResolve<Script>(new ChunkIdentifier(Tags.GLOP, 131072), out var script))
                        {
                            if (!sceg.Run(script, stackalloc int[] { Convert.ToInt32(someFlag == 0) }, out _, out _))
                                Application.Current.ShowMessageBox("Running script failed", 0, 3);
                            else
                            {
                                Smcc.SceneSlider = new SceneSlider(Movie);
                                if (Application.Current.Exchange.AddListener(this, 65536, MessageFlags.Broadcast))
                                    return true;
                            }
                        }
                    }
                }
            }
            finally
            {
                Application.Current.EndLongOp(false);
                ActorList = null;
                PropList = null;
                Field_0x90 = null;
            }
            return false;
        }

        public void ChangeArrowCursor(int id)
        {
            int number = id switch
            {
                1 => 0x65,
                2 => 3,
                3 => 0x6d,
                4 => 0x68,
                5 => 0x69,
                6 => 0x6a,
                7 => 0x6b,
                8 => 0x66,
                9 => 0x87,
                10 => 0x67,
                11 => 0x6e,
                12 => 0x79,
                13 => 0x78,
                14 => 0x6f,
                15 => 0x7a,
                16 => 0x6f,
                17 => 0x70,
                18 => 0x71,
                19 => 0x72,
                20 => 0x73,
                21 => 0x74,
                22 => 0x75,
                23 => 0x77,
                24 => 0x7c,
                25 => 0x8f,
                26 => 0x7d,
                27 => 0x7e,
                28 => 0x7f,
                29 => 0x80,
                30 => 0x6b,
                31 => 1,
                32 => 0x75,
                33 => 0x81,
                34 => 0x82,
                35 => 0x8a,
                36 => 0x8b,
                37 => 0x89,
                38 => 0x6c,
                39 => 0x85,
                40 => 0x84,
                41 => 0x83,
                42 => 0x8c,
                43 => 0x8e,
                44 => 0x8d,
                45 => 0x86,
                49 => 0x88,
                _ => -1,
            };
            if (number == -1) return;
            if (Resolver.TryResolve<Curs>(new ChunkIdentifier(Tags.GGCR, number), out var cursor))
                Application.Current.ChangeCursor(cursor, CursorType.Arrow);
        }

        Dictionary<int, Point> standardPositions = new Dictionary<int, Point>();
        bool hasStandardPositions;
        Rectangle viewRect;

        internal void WindowResized(int width, int height)
        {
            //if (App.Instance.RootGob.Find(131074) is Gok root && App.Instance.RootGob.Find(10) is Mvu view)
            //{
            //    Gob child;
            //    if (!hasStandardPositions)
            //    {
            //        viewRect = view.GetRect(CoordinateSpace.Window);
            //        hasStandardPositions = true;
            //    }
            //    child = root.FirstChild;
            //    while (child != null)
            //    {
            //        if (child.Id != 131165)
            //        {
            //            if (!standardPositions.TryGetValue(child.Id, out var pt))
            //                standardPositions[child.Id] = pt = child.GetPosition(CoordinateSpace.Local);
            //            var (x, y) = (pt.X, pt.Y);
            //            if (y < viewRect.Top)
            //            {
            //                pt = new Point(width / 2 - (640 / 2) + x, y);
            //                child.UpdateRect(new Rectangle(pt.X, pt.Y, child.ActualRect.Width, child.ActualRect.Height), null);
            //            }
            //            else if (y >= viewRect.Bottom)
            //            {
            //                pt = new Point(width / 2 - (640 / 2) + x, height - 480 + y);
            //                child.UpdateRect(new Rectangle(pt.X, pt.Y, child.ActualRect.Width, child.ActualRect.Height), null);
            //            }
            //        }

            //        child = child.Next;
            //    }
            //    root.UpdateRect(new Rectangle(0, 0, width, height), null);
            //    var newViewRect = new Rectangle(0, 0, width - viewRect.Left - 640 + viewRect.Right, height - viewRect.Top - 480 + viewRect.Bottom);
            //    view.UpdateRect(newViewRect, null);
            //    view.Parent.UpdateRect(newViewRect.Offset(view.Parent.Rect.X, view.Parent.Rect.Y), null);

            //    if (App.Instance.RootGob.Find(131166) is Gok top)
            //    {
            //        top.CustomSetPosition(width / 2 - (top.ActualRect.Width / 2), 0);
            //    }

            //    if (App.Instance.RootGob.Find(131167) is Gok left)
            //    {
            //        left.CustomSetPosition(0, height / 2 - (left.ActualRect.Height / 2));
            //    }

            //    if (App.Instance.RootGob.Find(131168) is Gok right)
            //    {
            //        right.CustomSetPosition(width - right.ActualRect.Width, height / 2 - (right.ActualRect.Height / 2));
            //    }

            //    if (App.Instance.RootGob.Find(131169) is Gok bot)
            //    {
            //        bot.CustomSetPosition(width / 2 - (bot.ActualRect.Width / 2), height - bot.ActualRect.Height);
            //    }
            //}
        }

        public void PlayClickSound(int id, int flags)
        {
            int number = id switch
            {
                1 => (flags & 1) == 0
                ? (flags & 2) == 0
                ? 143383 : 143385
                : 143384,

                var x when InRange(x, (4..6)) => 143378,

                8 => (flags & 2) == 0
                ? 143374
                : 143373,
                9 => 143380,

                10 => (flags & 2) == 0
                ? 143375
                : 143376,
                11 => 143379,

                12 => 143420,
                13 => 143421,
                14 => 143418,
                15 => 143419,

                22 => 143415,
                24 => 143414,
                25 => 143413,
                26 => 143421,
                27 => 143420,
                28 => 143418,
                31 => 143372,
                32 => (flags & 2) == 0
                ? 143416
                : 143417,
                33 => 143396,
                34 => 143398,
                35 => 143400,
                36 => 143399,
                37 => 143377,
                38 => 143444,
                39 => 143390,
                40 => 143391,
                41 => 143392,
                42 => 143394,
                43 => 143397,
                44 => 143395,

                46 => 143404,
                47 => 143403,
                48 => 143402,
                50 => 143411,
                51 => 143412,
                52 => 143408,
                53 => 143407,

                _ => -1,
            };

            if (number == -1) return;

            int repeat = InRange(id, (4..6)) ? -1 : 1;
            Field0xbc = (InRange(id, (4..6)) || (InRange(id, (8..10)) && id != 9)) ? 1 : 0;
            Application.Current.SoundManager.PlaySound(Resolver, new ChunkIdentifier(Tags.WAVE, number), 10000, 1, repeat);
            return;
        }

        private static bool InRange(int x, Range range)
        {
            return x < range.End.Value && x >= range.Start.Value;
        }
    }
}
