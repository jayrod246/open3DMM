using System;

namespace Open3dmm
{
    public class Component : IDisposable
    {
        private static int _counter;

        public static int NewIds(int count)
        {
            if (count == 0)
            {
                return 0;
            }

            for (int x = 0; x < count; x++)
            {
                if (Application.Current.Find(_counter + x) != null)
                {
                    _counter += x + 1;
                    x = 0;
                }

                if (_counter + x >= 0)
                {
                    _counter = int.MinValue;
                    x = 0;
                }
            }

            int cid = _counter;
            _counter += count;
            return cid;
        }

        private bool _disposedValue;

        public event Action<Component> Disposed;

        public int Id { get; }

        public Component(int cid)
        {
            Id = cid != 0 ? cid : NewIds(1);
        }

        protected virtual MessageCallbackCollection GetCallbacks() => Events<Component>.Callbacks;

        public bool Handle(Message message)
            => GetCallbacks().Invoke(this, message);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Disposed?.Invoke(this);
                    Application.Current?.RaiseMessageHandlerDisposed(this);
                }
                Disposed = null;
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
