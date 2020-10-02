using Open3dmm;
using Open3dmm.Core.Data;
using Open3dmm.Core.Resolvers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Open3dmm.Core.Actors
{
    public class ActionInfo : ResolvableObject
    {
        // TODO: ActionInfo implementation.
        public GenericGroup<(Fixed, int)> Cells => ResolveReference<GenericGroup<(Fixed, int)>>(new ReferenceIdentifier(0, Tags.GGCL));
        public IList<FixedMatrix4x3> Transforms => ResolveReference<GenericList<FixedMatrix4x3>>(new ReferenceIdentifier(0, Tags.GLXF));

        public bool TryGetBmdl(int cell, int bodyPart, out int bmdlId)
        {
            bmdlId = Unsafe.As<byte, short>(ref Cells.GetPayload(cell)[bodyPart * 4]);
            return bmdlId >= 0;
        }

        public bool TryGetTransform(int cell, int bodyPart, out Matrix4x4 transform)
        {
            int index = Unsafe.As<byte, short>(ref Cells.GetPayload(cell)[bodyPart * 4 + 2]);
            if (index >= 0 && index < Transforms.Count)
            {
                transform = Transforms[index];
                return true;
            }
            transform = default;
            return false;
        }

        protected override void ResolveCore()
        {
        }
    }
}
