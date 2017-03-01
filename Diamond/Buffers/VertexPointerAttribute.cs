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
        /// The offset of this attribute within each element
        /// Corresponds to the <code>offset</code> parameter to <code>glVertexAttribPointer</code>
        /// </summary>
        // todo this, and other values, should be moved into a different type for use with VertexDataInfo.
        // this class should just mark fields with their use - other types should manage binding those fields
        public int Offset { get; internal set; } = 0;

        /// <summary>
        /// Mark a field with information about how to point a shader attribute to it
        /// </summary>
        /// <param name="name">The name of the attribute to point to this</param>
        /// <param name="size">The number of elements to read from this field</param>
        public VertexPointerAttribute(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }
}