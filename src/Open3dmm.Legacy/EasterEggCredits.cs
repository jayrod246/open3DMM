namespace Open3dmm
{
    internal partial class EasterEggCredits : Component
    {
        public bool flag;

        public App App { get; }
        public Cex Exchange { get; }

        public EasterEggCredits(App app, Cex exchange) : base(0)
        {
            App = app;
            Exchange = exchange;
            exchange.AddListener(this, 1, MessageFlags.Other);
        }

        private bool OnNewMovie(Message arg)
        {
            if (flag && App.InputState.HasFlag(InputState.Shift))
            {
                flag = false;
                Application.Current.ShowMessageBox("Thank you for using Open 3D Movie Maker!", 0, 0);
                return true;
            }
            return false;
        }

        private bool OnCancel(Message m)
        {
            flag = App.InputState.HasFlag(InputState.Control);
            return false;
        }
    }
}
