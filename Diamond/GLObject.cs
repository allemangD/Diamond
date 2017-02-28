using System;
using System.Diagnostics;
using OpenTK.Graphics;
using NLog;

namespace Diamond
{
    /// <summary>
    /// Parent class for all gl Object wrappers. 
    /// </summary>
    public abstract class GLObject : IDisposable
    {
        /// <summary>
        /// Logger for this class
        /// </summary>
        protected Logger Log { get; }

        /// <summary>
        /// The name of this object
        /// </summary>
        public uint Id { get; protected set; }

        /// <summary>
        /// Force all <code>GLObject</code>s to define their name.
        /// </summary>
        /// <param name="id">The name of this object</param>
        protected GLObject(uint id)
        {
            Id = id;

            Log = LogManager.GetLogger(GetType().FullName);
            Log.Trace("Created {0}", this);
        }

        /// <summary>
        /// Called to free the name of this object. Usually corresponds to <code>glDelete*</code>.
        /// </summary>
        protected abstract void Delete();

        /// <summary>
        /// Free the name of this object
        /// </summary>
        public void Dispose()
        {
            if (GraphicsContext.CurrentContext == null)
            {
                Log.Warn("No current context, assuming {0} is disposed.", this);
                return;
            }

            Delete();
            Log.Trace("Disposed {0}", this);
            GC.SuppressFinalize(this);
        }

        ~GLObject()
        {
            Dispose();
        }

        public override string ToString() => $"{GetType().Name} {Id}";

        public static explicit operator uint(GLObject o) => o.Id;
        public static explicit operator int(GLObject o) => (int) o.Id;
    }
}