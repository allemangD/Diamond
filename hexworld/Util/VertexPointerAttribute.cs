using System;
using OpenTK.Graphics.OpenGL4;

namespace hexworld.Util
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class VertexPointerAttribute : Attribute
    {
        public string Name { get; }
        public int Size { get; }
        public VertexAttribPointerType Type { get; set; } = VertexAttribPointerType.Float;
        public bool Normalized { get; set; } = false;
        public int Divisor { get; set; } = 0;
        public int Offset { get; set; } = 0;

        public VertexPointerAttribute(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }
}