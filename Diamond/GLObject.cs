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

        public string Name { get; protected set; } = "GLObject";

        internal abstract GLWrapper Wrapper { get; }
        public int Id => Wrapper.Id;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (GraphicsContext.CurrentContext == null)
                    Logger.Error("No graphics context, cannot delete {0}", this);
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