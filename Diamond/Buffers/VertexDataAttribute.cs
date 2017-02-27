using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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