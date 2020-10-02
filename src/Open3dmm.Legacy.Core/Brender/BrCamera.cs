using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open3dmm.Core.Brender
{
    public unsafe struct BrCamera
    {
        public byte* Identifier;
        public BrCameraTypes Type;
        public BrAngle FieldOfView;
        public Fixed Near;
        public Fixed Far;
        public Fixed Aspect;
        public Fixed Width;
        public Fixed Height;
        public Fixed Distance;
    }
}
