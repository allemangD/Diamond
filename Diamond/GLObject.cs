using System;

namespace Diamond
{
    public abstract class GLObject : IDisposable
    {
        public uint Id { get; protected set; }

        protected GLObject(uint id)
        {
            Id = id;
        }

        protected abstract void Delete();

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Delete();
            GC.SuppressFinalize(this);
        }

        ~GLObject()
        {
            Dispose();
        }

        public static explicit operator uint(GLObject o) => o.Id;
        public static explicit operator int(GLObject o) => (int) o.Id;
    }
}