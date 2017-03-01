using System;
using NLog;

namespace Diamond
{
    public abstract class GLObject : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _disposed;

        public string Name { get; protected set; } = "GLObject";

        internal abstract GLWrapper Wrapper { get; }
        public int Id => Wrapper.Id;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
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