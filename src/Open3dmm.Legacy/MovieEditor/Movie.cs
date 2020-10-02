using Open3dmm.Core.Actors;
using Open3dmm.Core.Data;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using Open3dmm.Core.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Open3dmm.MovieEditor
{
    public class Movie : Docb
    {
        public string Filename { get; set; }
        public ChunkyFile Chunks { get; set; }
        public IScopedResolver Scope { get; set; }

        public Smcc Smcc { get; set; }
        public ChunkIdentifier Identifier { get; set; }
        public Scene CurrentScene { get; set; }
        public int CurrentSceneIndex { get; set; }
        public IList<Scene> Scenes { get; } = new List<Scene>();
        public int Field0x154 { get; set; }

        public Movie(Smcc smcc, string path, int number) : base(null, false)
        {
            Smcc = smcc;
            if (string.IsNullOrWhiteSpace(path))
            {
                Chunks = new ChunkyFile();
                if (number == -1)
                    number = 0;
                Scope = new DataFile(Chunks, TagManager.Default.Factory, 0);
                Identifier = InitializeNewFile(Scope, number);
            }
            else
            {
                Chunks = new ChunkyFile(path);
                Scope = new DataFile(Chunks, TagManager.Default.Factory, 0);
                if (number == -1)
                {
                    var movieChunk = Scope.File.GetChunksOfType(Tags.MVIE).FirstOrDefault();
                    Identifier = movieChunk.Identifier;
                }
                else
                    Identifier = new ChunkIdentifier(Tags.MVIE, number);

                if (Scope.TryResolve<GenericStrings<int>>(Identifier, Tags.GST , 1, out var products))
                    foreach (var p in products)
                        TagManager.Default.RegisterProduct(p.Value, p.Key);
            }
        }

        private static ChunkIdentifier InitializeNewFile(IScopedResolver scope, int number)
        {
            var newMovie = scope.File.Add(Tags.MVIE);
            newMovie.Label = "A New Movie";
            return newMovie.Identifier;
        }

        public Movie(bool lowQualitySetting, Smcc smcc, string moviePath, int v2) : base(null, false)
        {
            Smcc = smcc;
            Chunks = new ChunkyFile();
            Scope = new DataFile(Chunks, TagManager.Default.Factory, 0);
            Identifier = InitializeNewFile(Scope, 0);
        }

        private IList<GlobalReference> GetReferencesFromActor(ChunkyFile file, int number, out bool error)
        {
            var identifier = (Tags.ACTR, number);
            error = false;
            var list = new List<GlobalReference>();
            if (file.TryGetChunk(identifier, out var cActor))
            {
                using var bActor = BinaryStream.Create(cActor.Section.Memory.Span).Decompress();
                if (TryReadActor(bActor, out var actor))
                {
                    if (actor.Reference.ProductKey == 0)
                    {
                        if (file.TryGetChunk(identifier, (0, Tags.TMPL), out var cTmpl)
                            && TryGetFont(file, cTmpl.Identifier, out var rTdf, out error))
                        {
                            list.Add(rTdf);
                        }
                    }
                    if (!error)
                    {
                        list.Insert(0, actor.Reference);
                        if (file.TryGetChunk(identifier, (0, Tags.GGAE), out var cEvents))
                        {
                            var bEvents = BinaryStream.Create(cEvents.Section.Span).Decompress();
                            var events = new Group<LogicalActorEvent>();
                            events.LoadBlock(bEvents);
                            for (int i = 0; i < events.Count; i++)
                            {
                                if (TryGetReferenceFromEvent(events, i, out var r, out _))
                                {
                                    list.Add(r);
                                }
                            }
                        }
                        return list;
                    }
                }
            }
            error = true;
            return null;
        }

        public virtual Mvu CreateView(GobOptions options)
        {
            return new Mvu(this, options, Smcc.Width, Smcc.Height);
        }

        private bool TryGetReferenceFromEvent(Group<LogicalActorEvent> events, int i, out GlobalReference r, out LogicalActorEvent ev)
        {
            ev = events.GetItem(i, out var data);
            using IReadOnlyStream block = BinaryStream.Create(data);
            switch (ev.Type)
            {
                case ActorEventType.Costume:
                    block.Skip(8);
                    if (block.Assert(0))
                    {
                        r = block.Read<GlobalReference>();
                        return true;
                    }
                    break;
                case ActorEventType.Sound:
                    block.Skip(28);
                    r = block.Read<GlobalReference>();
                    return true;
            }
            r = default;
            return false;
        }

        private bool TryGetFont(ChunkyFile file, ChunkIdentifier tmplID, out GlobalReference rTdf, out bool error)
        {
            error = false;
            rTdf = default;
            if (file.TryGetChunk(tmplID, (0, Tags.TDT), out var cTdt))
            {
                var bTdt = BinaryStream.Create(cTdt.Section.Memory.Span).Decompress();
                error = !bTdt.MagicNumber()
                    || !bTdt.TrySkip(4)
                    || !bTdt.TryRead(out rTdf);
                return !error;
            }
            return false;
        }

        private bool TryReadActor(IReadOnlyStream block, out LogicalActor actor)
        {
            bool isTrimmed = block.Length == 0x28;
            if (isTrimmed || block.Length == 0x2c)
            {
                actor = block.Read<LogicalActor>();
                if (isTrimmed)
                {
                    block.Position = block.Length - 16;
                    actor.Reference = block.Read<GlobalReference>();
                    actor.End = int.MaxValue;
                }
                return isTrimmed;
            }
            actor = default;
            return false;
        }

        internal void FUN_004a22b0()
        {
            throw new NotImplementedException();
        }

        public void FUN_004a27d0(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                if (string.IsNullOrWhiteSpace(Filename))
                {
                    Filename = Smcc.GetAppString(5);
                    Smcc.VirtualFunc30(Filename);
                }
            }
            else
            {

            }
        }
    }
}