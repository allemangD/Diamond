using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace hexworld.Util
{
    public class VBO : GLObject
    {
        public VBO()
            : base((uint) GL.GenBuffer())
        {
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
        }

        public void Data<T>(T[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw) where T : struct
        {
            Bind();
            var size = Marshal.SizeOf<T>();
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(size * data.Length), data, usage);
            Unbind();
        }

        public static void Unbind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}