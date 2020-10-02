using Open3dmm.Core;
using Open3dmm.Core.Resolvers;
using System;
using Veldrid;

namespace Open3dmm
{
    public abstract class Gorp
    {
        public Gorp(IScopedResolver resolver, ChunkIdentifier identifier)
        {
            Resolver = resolver;
            Tag = identifier.Tag;
            Number = identifier.Number;
        }

        public IScopedResolver Resolver { get; }
        public Tag Tag { get; }
        public int Number { get; }

        public virtual void VirtualFunc3(int width, int height)
        {
        }

        public abstract LTRB GetRect();
        public virtual LTRB GetRectOrigin()
        {
            var rc = GetRect();
            rc.Offset(-rc.Left, -rc.Top);
            return rc;
        }
        public abstract void Draw(CommandList commandList, in RectangleF dest);

        public virtual bool VirtualFunc9()
        {
            return false;
        }

        public virtual bool HitTest(PT pt)
        {
            return false;
        }
    }
}
