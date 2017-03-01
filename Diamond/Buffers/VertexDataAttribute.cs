using System;

namespace Diamond.Buffers
{
    /// <summary>
    /// Marks a struct as vertex data that can be sent to a shader attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class VertexDataAttribute : Attribute
    {
        /// <summary>
        /// The pointer divisor for this type. A value of 0 indicates per-vertex, a value of 1+ 
        /// indicates every n instances
        /// </summary>
        public int Divisor { get; set; } = 0;

        /// <summary>
        /// Mark a struct with information about how to iterate over vertex data of this type.
        /// All fields of this struct must have [VertexPointer]
        /// </summary>
        public VertexDataAttribute()
        {
        }
    }
}