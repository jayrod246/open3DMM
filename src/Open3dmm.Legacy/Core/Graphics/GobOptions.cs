using System;

namespace Open3dmm
{
    public struct GobOptions
    {
        public GobOptions(int id, Gob relative, bool makeSibling, InvalidateOptions unk, LTRB? rect = null, Anchor? anchor = null)
        {
            Id = id;
            Relative = relative;
            MakeSibling = makeSibling;
            Unk = unk;
            Rect = rect ?? default;
            Anchor = anchor ?? default;
        }

        public int Id { get; set; }
        public Gob Relative { get; set; }
        public bool MakeSibling { get; set; }
        public InvalidateOptions Unk { get; set; }
        public LTRB Rect { get; set; }
        public Anchor Anchor { get; set; }

        public static GobOptions NewChild(Woks woks, int parentId, int childId)
        {
            var parent = woks?.Find(parentId) ?? throw new InvalidOperationException("Parent Gob not found.");
            return new GobOptions(childId, parent, false, InvalidateOptions.Inherit, anchor: new(0, 0, 1, 1));
        }
    }
}
