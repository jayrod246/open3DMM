using Open3dmm.Core.GUI;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Open3dmm.Core.GUI
{
    [Flags]
    public enum GokdMiscFlags:int
    {
        NoHitGraphic = 1,
        NoHitPrecise = 2,
        NoHit = 4,
        NoSlip = 8,
        SoundEnabled = 32,
    }

    public class Gokd : ResolvableObject
    {
        private readonly ISortedList<int, GokdLayout> layouts;
        public IList<GokdStateBlob> StateBlobs { get; }
        public GokdMiscFlags MiscFlags { get; set; }

        public Gokd()
        {
            layouts = SortedList.Create<int, GokdLayout>();
            StateBlobs = new List<GokdStateBlob>();
        }

        public void SetLayout(GokdLayout layout)
        {
            layouts.Set(layout.ParentId, layout);
        }

        public bool RemoveLayout(int parentId)
        {
            return layouts.Remove(parentId);
        }

        protected override void ResolveCore()
        {
            using var block = Metadata.GetBlock();
            if (!block.MagicNumber())
                throw ThrowHelper.BadMagicNumber(block.Read<int>());
            MiscFlags = block.Read<GokdMiscFlags>();

            while (true)
            {
                if (block.Remainder < 16)
                    throw ThrowHelper.BadSection(Metadata.Key);
                var layout = block.Read<GokdLayout>();
                SetLayout(layout);
                if (layout.ParentId == 0)
                    break;
            }

            if (block.Remainder % 28 != 0)
                throw ThrowHelper.BadSection(Metadata.Key);

            foreach (var blob in block.ReadTo(stackalloc GokdStateBlob[checked((int)(block.Remainder / 28))]))
                StateBlobs.Add(blob);

            // TODO: Find a way to add MBMPs back to background update or?
            //foreach (var r in GetResolve().Scope.GetRange(GetResolve().Identifier, "MBMP"))
            //{
            //    if (GetResolve().Scope.TryResolve<Mbmp>(r, out var mbmp))
            //        App.Instance.AddBackgroundUpdate(() => mbmp.GetTexture(App.Instance.GraphicsDevice));
            //}
        }

        [Obsolete("Use GetLayout() instead")]
        public ReadOnlySpan<int> GetData(int parentId)
        {
            var layout = GetLayout(parentId);
            return MemoryMarshal.Cast<byte, int>(IOHelper.GetBytes(ref layout)).ToArray();
        }

        public GokdLayout GetLayout(int parentId)
        {
            if (!layouts.TryGetValue(parentId, out var layout))
                layouts.TryGetValue(0, out layout);
            return layout;
        }

        public bool TryGetStateBlob(int modifier, int state, out GokdStateBlob stateBlob)
        {
            state = 1 << ((byte)state & 31);

            foreach (var sb in StateBlobs)
            {
                if (sb.Match(modifier, state))
                {
                    stateBlob = sb;
                    return true;
                }
            }
            stateBlob = default;
            return false;
        }
    }
}
