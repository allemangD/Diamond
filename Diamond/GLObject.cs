using System;
using Diamond.Wrappers;
using NLog;

namespace Diamond
{
    public abstract class GLObject : IDisposable
    {
        protected static readonly Logger Logger = LogManager.GetLogger("GLObject");

        private bool _disposed;

        public string Name { get; protected set; } = "GLObject";

        internal abstract Wrapper Wrapper { get; }
        public int Id => Wrapper.Id;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Logger.Debug("Disposing {0}", this);
                Wrapper.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~GLObject()
        {
            Dispose(false);
        }
    }
}