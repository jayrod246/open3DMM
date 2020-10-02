namespace Open3dmm.Core.Scenes
{
    public abstract class SceneNodeVisitor
    {
        public abstract bool VisitNode(SceneNode node);

        public abstract void LeaveNode(SceneNode node);
    }
}
