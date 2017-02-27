using System;

namespace Diamond.Buffers
{
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class VertexDataAttribute : Attribute
    {
        public int Divisor { get; set; } = 0;

        public VertexDataAttribute()
        {
        }
    }
}