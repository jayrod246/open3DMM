using Open3dmm.Core;
using System.Numerics;

namespace Open3dmm.Core.Actors
{
    public struct PathStep
    {
        private FixedVector3 position;
        private Fixed weight;

        public PathStep(Vector3 position, float weight)
        {
            this.position = (FixedVector3)position;
            this.weight = (Fixed)weight;
        }

        public Vector3 Position { get => this.position; set => this.position = (FixedVector3)value; }
        public float Weight { get => this.weight; set => this.weight = (Fixed)value; }
    }
}
