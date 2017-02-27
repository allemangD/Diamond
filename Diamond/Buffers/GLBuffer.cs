using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Buffers
{
    public class GLBuffer<T> : GLObject where T : struct
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

        public void SubData<T>(SubArray<T> data) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            Bind();
            GL.BufferSubData(Target, (IntPtr) (data.Offset * size), (IntPtr) (data.Length * size), data.Array);
        }
    }
}