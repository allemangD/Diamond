using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Diamond.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Buffers
{
    public static class VBO
    {
        public static void Unbind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }

    public class VBO<T> : GLObject where T : struct
    {
        public VBO()
            : base((uint) GL.GenBuffer())
        {
        }

        protected override void Delete()
        {
            GL.DeleteBuffer(Id);
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
        }

        public void Data(T[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
        {
            Bind();
            var size = Marshal.SizeOf(typeof(T));
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (size * data.Length), data, usage);
            VBO.Unbind();
        }

        private static readonly int Stride;
        private static readonly VertexPointerAttribute[] Attributes;

        static VBO()
        {
            var attribList = new List<VertexPointerAttribute>();
            Stride = Marshal.SizeOf(typeof(T));

            foreach (var fieldInfo in typeof(T).GetFields())
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(VertexPointerAttribute), false);
                if (attrs.Length == 0) continue;

                var offset = (int) Marshal.OffsetOf(typeof(T), fieldInfo.Name);
                foreach (var attr in attrs)
                {
                    var vpa = (VertexPointerAttribute) attr;
                    vpa.Offset = offset;
                    attribList.Add(vpa);
                }
            }

            Attributes = attribList.ToArray();
        }

        public void AttribPointers(Program pgm)
        {
            Bind();
            foreach (var attr in Attributes)
            {
                if (!pgm.TryGetAttribute(attr.Name, out int loc)) continue;
                GL.VertexAttribPointer(loc, attr.Size, attr.Type, attr.Normalized, Stride, attr.Offset);
                GL.VertexAttribDivisor(loc, attr.Divisor);
            }
            VBO.Unbind();
        }
    }
}