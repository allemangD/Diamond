using System;
using System.Runtime.InteropServices;
using Diamond.Util;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Buffers
{
    public class GLBuffer<T> : GLWrapper where T : struct
    {
        public readonly BufferTarget Target;
        public readonly BufferUsageHint Usage;

        public GLBuffer(BufferTarget target, BufferUsageHint usage = BufferUsageHint.StaticDraw)
            : base((uint) GL.GenBuffer())
        {
            Target = target;
            Usage = usage;
        }

        public void Bind()
        {
            GL.BindBuffer(Target, Id);
        }

        protected override void Delete() => GL.DeleteBuffer(Id);

        public void Data(T[] data)
        {
            var size = Marshal.SizeOf<T>();
            Bind();
            GL.BufferData(Target, (IntPtr) (size * data.Length), data, Usage);
        }

        public void SubData(SubArray<T> data)
        {
            var size = Marshal.SizeOf<T>();
            Bind();
            GL.BufferSubData(Target, (IntPtr) (data.Offset * size), (IntPtr) (data.Length * size), data.Array);
        }
    }
}