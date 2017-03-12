using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diamond.Attributes
{
    /// <summary>
    /// Use this struct's contents as vertex attribute information
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class VertexDataAttribute : Attribute
    {
        /// <summary>
        /// The VertexAttribDivisor for all attribs associated with this struct
        /// </summary>
        public int Divisor { get; }

        /// <summary>
        /// Mark a struct as a source for a VAO's attrib pointer information
        /// </summary>
        /// <param name="divisor"></param>
        public VertexDataAttribute(int divisor = 0)
        {
            Divisor = divisor;
        }
    }
}