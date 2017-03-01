using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using OpenTK.Graphics;

namespace Diamond
{
    public abstract class GLObject : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool _disposed;

        public abstract int Id { get; }

        protected abstract GLWrapper Wrapper { get; }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (GraphicsContext.CurrentContext == null)
                    Logger.Warn("No graphics context, cannot dispose GLObject: {0}", Wrapper);
                else
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