namespace Open3dmm
{
    public interface IComponentGraph
    {
        int GenIds(int count = 1);
        Component GetComponent(int componentId);
        bool TryGetExchange(out Cex cex);
        void BeginExchange();
        void EndExchange();
    }
}
