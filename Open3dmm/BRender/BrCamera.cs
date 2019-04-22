using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open3dmm.BRender
{
    public unsafe struct BrCamera
    {
        public byte* Identifier;
        public BrCameraTypes Type;
        public BrAngle FieldOfView;
        public BrScalar Near;
        public BrScalar Far;
        public BrScalar Aspect;
        public BrScalar Width;
        public BrScalar Height;
        public BrScalar Distance;
    }
}
