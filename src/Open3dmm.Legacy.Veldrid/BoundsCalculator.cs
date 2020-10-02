using Open3dmm.Core.Actors;
using Open3dmm.Core.Scenes;
using System.Collections.Generic;
using System.Numerics;

namespace Open3dmm.Core.Veldrid
{
    internal class BoundsCalculator : SceneNodeVisitor
    {
        private Stack<Matrix4x4> matrixStack = new Stack<Matrix4x4>();
        private Matrix4x4 currentMatrix;
        public Bounds Result;

        public BoundsCalculator(Matrix4x4 world)
        {
            currentMatrix = world;
        }

        public override bool VisitNode(SceneNode node)
        {
            if (node is BodyPart bp)
            {
                matrixStack.Push(currentMatrix);
                currentMatrix = bp.Transform * currentMatrix;
                var model = (bp.ModelOverride ?? bp.Model);
                if (model != null)
                {
                    var bounds = Bounds.Transform(model.GetBounds(), currentMatrix);
                    if (Result.Min == Result.Max)
                        Result = bounds;
                    else if (bounds.Min != bounds.Max)
                        Result = Bounds.Add(Result, bounds);
                }
            }
            return true;
        }

        public override void LeaveNode(SceneNode node)
        {
            if (node is BodyPart)
            {
                currentMatrix = matrixStack.Pop();
            }
        }
    }
}
