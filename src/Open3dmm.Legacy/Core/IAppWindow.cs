namespace Open3dmm
{
    public interface IAppWindow
    {
        Gob Gob { get; }
        CursorType Cursor { get; set; }
        void SetCursorImage(ICursorImage? cursorImage, CursorType cursorType);
        InputState GetInputState();
        PT GetCursorPosition();
        PT PointToClient(PT point);
        PT PointToScreen(PT point);
    }

    public static class AppWindowExtensions
    {
        public static void SetCursorImage(this IAppWindow appWindow, Core.Resolvers.IResolver resolver, int number, CursorType cursorType)
        {
            if (resolver.TryResolve<Curs>(new(Tags.GGCR, number), out var cursor))
            {
                appWindow.SetCursorImage(cursor, cursorType);
            }
        }
    }
}
