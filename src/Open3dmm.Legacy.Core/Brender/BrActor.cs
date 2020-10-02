using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open3dmm.Core.Brender
{
    public unsafe struct BrActor
    {
        public BrActor* Next;
        public BrActor** Previous;
        public BrActor* FirstChild;
        public BrActor* Parent;
        public ushort Depth;
        public BrActorTypes Type;
        public byte* Identifier;
        public BrModel* Model;
        public BrMaterial* Material;
        public BrRenderStyles RenderStyle;
        public BrTransform Transform;
        public void* TypeData;
    }
}
