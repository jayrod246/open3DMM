using Open3dmm.Core.Scenes;
using System.Numerics;

namespace Open3dmm.Core.Actors
{
    public class BodyPart : SceneNode
    {
        public Body Body { get; }
        public int BodySet { get; set; }
        public Bmdl Model { get; set; }
        public Bmdl NextModel { get; set; }
        public Matrix4x4 Transform { get; set; }
        public Mtrl Material { get; set; }
        public Bmdl ModelOverride { get; set; }

        public BodyPart(Body body)
        {
            Body = body;
        }
    }
}
