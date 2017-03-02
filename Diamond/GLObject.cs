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
        /// Delegate Dispose to underlying wrapper class
        /// </summary>
        public abstract void Dispose();
    }
}