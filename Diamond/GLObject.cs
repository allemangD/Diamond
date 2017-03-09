using System;
using NLog;
using OpenTK.Graphics;

namespace Diamond
{
    public abstract class GLObject : IDisposable
    {
        /// <summary>
        /// The logger for GLObject-related info
        /// </summary>
        protected internal static Logger Logger = LogManager.GetLogger(nameof(GLObject));

        /// <summary>
        /// The OpenGL object name
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Force object name assignment
        /// </summary>
        /// <param name="id">The OpenGL object name</param>
        protected GLObject(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Free this object name on the GPU
        /// </summary>
        protected abstract void Delete();

        #region IDisposable

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (GraphicsContext.CurrentContext != null)
                Delete();
            else
                Logger.Error("Cannot delete {0} because there is no graphics context.", this);

            if (disposing)
            {
                // no managed resources to dispose
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        ~GLObject()
        {
            Dispose(false);
        }

        #endregion
    }
}