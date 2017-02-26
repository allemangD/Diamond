using System;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Buffers
{
    /// <summary>
    /// Marks a field as an attribute to be sent to a shader. Must be used on public fields of a struct.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class VertexPointerAttribute : Attribute
    {
        /// <summary>
        /// The attribute name that the values of this field should point to
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The number of elements in this attribute
        /// Corresponds to the <code>size</code> parameter to <code>glVertexAttribPointer</code>.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// The element type of the attribute
        /// Corresponds to the <code>type</code> parameter to <code>glVertexAttribPointer</code>
        /// </summary>
        public VertexAttribPointerType Type { get; set; } = VertexAttribPointerType.Float;

        /// <summary>
        /// Whether to normalize the values of this attribute
        /// Corresponds to the <code>normalized</code> parameter to <code>glVertexAttribPointer</code>
        /// </summary>
        public bool Normalized { get; set; } = false;

        /// <summary>
        /// The divisor to Instanced draw calls
        /// Corresponds to the <code>divisor</code> parameter to <code>glVertexAttribDivisor</code>
        /// </summary>
        public int Divisor { get; set; } = 0;

        /// <summary>
        /// The offset of this attribute within each element
        /// Corresponds to the <code>offset</code> parameter to <code>glVertexAttribPointer</code>
        /// </summary>
        public int Offset { get; set; } = 0;

        public VertexPointerAttribute(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }
}