using System;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Wrappers
{
    /// <summary>
    /// Wrapper class for OpenGL Buffer objects
    /// </summary>
    internal sealed class BufferWrap : Wrapper
    {
        #region Constructor, GLDelete()

        internal BufferWrap(BufferTarget target, BufferUsageHint usage)
            : base(GL.GenBuffer())
        {
            Target = target;
            Usage = usage;
        }

        protected override void GLDelete() => GL.DeleteBuffer(Id);

        #endregion

        #region Properties

        #region Stored

        /// <summary>
        /// BufferTarget parameter used in gl* calls
        /// </summary>
        public BufferTarget Target { get; }

        /// <summary>
        /// BufferUsageHint parameter using in glBufferData calls
        /// </summary>
        public BufferUsageHint Usage { get; set; }

        #endregion

        /// <summary>
        /// Binds this buffer (glBindBuffer)
        /// </summary>
        public void Bind() => GL.BindBuffer(Target, Id);

        /// <summary>
        /// Upload data to this buffer (glBufferData)
        /// </summary>
        /// <typeparam name="T">Type of value to upload</typeparam>
        /// <param name="size">Size of T in bytes</param>
        /// <param name="data">Values to upload</param>
        public void Data<T>(int size, T[] data) where T : struct
        {
            Bind();
            GL.BufferData(Target, (IntPtr) (size * data.Length), data, Usage);
        }

        /// <summary>
        /// Upload a range data to this buffer (glBufferSubData)
        /// </summary>
        /// <typeparam name="T">Type of value to upload</typeparam>
        /// <param name="size">Size of T in bytes</param>
        /// <param name="offset">Offset of upload range in bytes</param>
        /// <param name="count">Number of bytes to upload</param>
        /// <param name="data">All values to upload (offset will be applied to both this and the target)</param>
        public void SubData<T>(int size, int offset, int count, T[] data) where T : struct
        {
            Bind();
            GL.BufferSubData(Target, (IntPtr) (offset * size), (IntPtr) (count * size), data);
        }

        #endregion

        public override string ToString() => $"Buffer Wrapper - {Target} ({Id})";
    }
}