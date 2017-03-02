using System;
using NLog;
using OpenTK.Graphics;

namespace Diamond.Wrappers
{
    internal abstract class Wrapper : IDisposable
    {
        /// <summary>
        /// Logger for all Wrapper types.
        /// </summary>
        protected static readonly Logger Logger = LogManager.GetLogger("Wrapper");

        /// <summary>
        /// The OpenGL name of this object
        /// </summary>
        public int Id { get; private set; }
        
        // Force wrapper types to generate an Id at creation time
        protected Wrapper(int id)
        {
            Id = id;
        }

        public override string ToString() => $"{GetType().Name} {Id}";

        #region IDisposable

        /// <summary>
        /// Delete this OpenGL object (glDelete*)
        /// </summary>
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
    }
}