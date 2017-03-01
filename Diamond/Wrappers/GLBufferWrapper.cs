using System;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Wrappers
{
    internal class GLBufferWrapper : GLWrapper
    {
        public BufferTarget Target { get; }
        public BufferUsageHint Usage { get; set; }

        internal GLBufferWrapper(BufferTarget target, BufferUsageHint usage)
        {
            Id = GL.GenBuffer();
            Target = target;
            Usage = usage;
        }

        public void Bind() => GL.BindBuffer(Target, Id);

        public void Data<T>(int size, T[] data) where T : struct
        {
            Bind();
            GL.BufferData(Target, (IntPtr) (size * data.Length), data, Usage);
        }

        public void SubData<T>(int size, int offset, int count, T[] data) where T : struct
        {
            Bind();
            GL.BufferSubData(Target, (IntPtr) (offset * size), (IntPtr) (count * size), data);
        }

        public override void GLDelete() => GL.DeleteBuffer(Id);
    }
}