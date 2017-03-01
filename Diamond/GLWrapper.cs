using System;
using System.Diagnostics;
using OpenTK.Graphics;
using NLog;

namespace Diamond
{
    internal abstract class GLWrapper : IDisposable
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public int Id { get; protected set; }

        public override string ToString() => $"{GetType().Name} {Id}";

        public static explicit operator int(GLWrapper o) => o.Id;

        #region IDisposable

        public abstract void GLDelete();

        private bool _disposed;

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~GLWrapper()
        {
            Dispose(false);
        }

        #endregion
    }
}