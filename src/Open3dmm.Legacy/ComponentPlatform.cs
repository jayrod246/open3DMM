using Open3dmm;
using System;
using System.Collections.Generic;

[assembly: ComponentPlatform(typeof(ComponentPlatform))]

namespace Open3dmm
{
    public class ComponentPlatform : IComponentPlatform
    {
        private int _uniqueId = 0;

        public IErrorService Errors { get; } = new ErrorService();
        public Cex ComponentExchange { get; } = new Cex();

        public ComponentPlatform()
        {
        }

        public Component FindComponent(int cid) => Application.Current.Find(cid);

        public int GenerateComponentIds(int count)
        {
            int n = count;
            while (n > 0)
            {
                n--;
                if (_uniqueId == 0)
                {
                    _uniqueId = int.MinValue;
                    n = count - 1;
                }
                if (FindComponent(_uniqueId) != null)
                    n = count;
                _uniqueId++;
            }
            return _uniqueId - count;
        }

        private class ErrorService : IErrorService
        {
            private readonly List<int> _errors = new(20);

            public void Flush()
            {
                _errors.Clear();
            }

            public bool IsThrown(int error)
            {
                return _errors.Contains(error);
            }

            public void Throw(int error)
            {
                if (_errors.Count >= 20)
                    _errors.RemoveAt(0);
                _errors.Add(error);
            }

            public bool LastError(out int error)
            {
                return (error = _errors.Count == 0 ? 0 : _errors[^1]) != 0;
            }

            public IEnumerable<int> Enumerate() => _errors;
        }
    }
}
