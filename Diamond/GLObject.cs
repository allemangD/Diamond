using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Diamond
{
    /// <summary>
    /// Parent class for all gl Object wrappers. 
    /// </summary>
    public abstract class GLObject : IDisposable
    {
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
            Debug.WriteLine($"Created GLObject {ToString()} ({Id})");
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
                Debug.WriteLine($"No active context. Assuming {this} ({Id}) is disposed.", "[Warning]");
                return;
            }
            Delete();
            Debug.WriteLine($"Disposed GLObject {ToString()} ({Id})");
            GC.SuppressFinalize(this);
        }

        ~GLObject()
        {
            Dispose();
        }

        public static explicit operator uint(GLObject o) => o.Id;
        public static explicit operator int(GLObject o) => (int) o.Id;
    }
}