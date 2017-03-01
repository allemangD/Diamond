using System;
using NLog;
using OpenTK.Graphics;

namespace Diamond.Wrappers
{
    internal abstract class Wrapper : IDisposable
    {
        protected static readonly Logger Logger = LogManager.GetLogger("Wrapper");

        public int Id { get; private set; }

        public static explicit operator int(Wrapper o) => o.Id;

        protected Wrapper(int id)
        {
            Id = id;
        }

        #region IDisposable

        protected abstract void GLDelete();

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            // no managed resources to dispose

            if (GraphicsContext.CurrentContext == null)
                Logger.Error("No graphics context, cannot delete {0}", this);
            else
                GLDelete();

            Id = 0;

            _disposed = true;
        }

        #region Implemented

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Wrapper()
        {
            Dispose(false);
        }

        #endregion

        #endregion

        public override string ToString() => $"{GetType().Name} {Id}";
    }
}