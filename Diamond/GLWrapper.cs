using System;
using System.Diagnostics;
using OpenTK.Graphics;
using NLog;

namespace Diamond
{
    internal abstract class GLWrapper : IDisposable
    {
        protected static Logger Logger = LogManager.GetCurrentClassLogger();

        public int Id { get; protected set; }

        public override string ToString() => $"{GetType().Name} {Id}";

        #region IDisposable

        public abstract void GLDelete();

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            GLDelete();

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        public static explicit operator int(GLWrapper o) => o.Id;
    }
}