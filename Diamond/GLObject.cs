using System;
using Diamond.Wrappers;
using NLog;

namespace Diamond
{
    /// <summary>
    /// Provide managed access to OpenGL objects
    /// </summary>
    public abstract class GLObject : IDisposable
    {
        /// <summary>
        /// Logger for all GLObjects
        /// </summary>
        protected static readonly Logger Logger = LogManager.GetLogger("GLObject");

        /// <summary>
        /// Name of this GLObject used for identification
        /// </summary>
        public string Name { get; protected set; } = "GLObject";

        /// <summary>
        /// Underlying managed wrapper to this OpenGL object
        /// </summary>
        internal abstract Wrapper Wrapper { get; }

        /// <summary>
        /// The OpenGL name of this object
        /// </summary>
        public int Id => Wrapper.Id;

        #region IDisposable

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

        #region Implemented

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~GLObject()
        {
            Dispose(false);
        }

        #endregion

        #endregion
    }
}