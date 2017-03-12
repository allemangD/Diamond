using System;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Attributes
{
    /// <summary>
    /// Use this field as a source for vertex attribute data
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class VertexAttribAttribute : Attribute
    {
        /// <summary>
        /// The attribute ID to send this value 
        /// </summary>
        public int Attribute { get; }

        /// <summary>
        /// The number of elements for this attribute
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Whether this attribute should be normalized
        /// </summary>
        public bool Normalized { get; set; } = false;

        /// <summary>
        /// The eleemnt type for this attribute
        /// </summary>
        public VertexAttribPointerType Type { get; set; } = VertexAttribPointerType.Float;

        /// <summary>
        /// Mark a field as a source for a vertex attribute
        /// </summary>
        /// <param name="attribute">The attribute ID for this value</param>
        /// <param name="size">The number of elements for this attribute</param>
        public VertexAttribAttribute(int attribute, int size)
        {
            Attribute = attribute;
            Size = size;
        }
    }
}