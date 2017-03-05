using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Diamond.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Buffers
{
    /// <summary>
    /// Get vertex pointer information about a struct to infer how to point shader attributes to it
    /// </summary>
    public class VertexDataInfo
    {
        /// <summary>
        /// All shader attributes supported by this type
        /// </summary>
        public IReadOnlyCollection<VertexPointerAttribute> Pointers { get; }

        /// <summary>
        /// The size of this type in bytes
        /// </summary>
        public int Stride { get; }

        /// <summary>
        /// The pointer divisor for this type. A value of 0 indicates per-vertex, a value of 1+ 
        /// indicates every n instances
        /// </summary>
        public readonly int Divisor;

        /// <summary>
        /// Create an info class with pre-computed values
        /// </summary>
        /// <param name="pointers">The type's supported attributes</param>
        /// <param name="stride">The type's stride</param>
        /// <param name="divisor">The type's pointer divisor</param>
        private VertexDataInfo(IList<VertexPointerAttribute> pointers, int stride, int divisor)
        {
            Pointers = new ReadOnlyCollection<VertexPointerAttribute>(pointers);
            Stride = stride;
            Divisor = divisor;
        }

        /// <summary>
        /// Enable the attributes associated with this type on Program.Current
        /// </summary>
        public void EnableVertexPointers()
        {
            if (Program.Current == null)
                throw new InvalidOperationException("Cant render a mesh with no active shader.");

            foreach (var attr in Pointers)
            {
                if (!Program.Current.HasAttribute(attr.Name))
                    continue;
                var loc = Program.Current.AttributeLocation(attr.Name);
                GL.EnableVertexAttribArray(loc);
                GL.VertexAttribDivisor(loc, Divisor);
            }
        }

        /// <summary>
        /// Disable the attributes associated with this type on Program.Current
        /// </summary>
        public void DisableVertexPointers()
        {
            if (Program.Current == null)
                throw new InvalidOperationException("Cant render a mesh with no active shader.");

            foreach (var attr in Pointers)
            {
                if (!Program.Current.HasAttribute(attr.Name))
                    continue;
                var loc = Program.Current.AttributeLocation(attr.Name);
                GL.DisableVertexAttribArray(loc);
            }
        }

        /// <summary>
        /// A cache of already computed information to prevent redundant reflection calls
        /// </summary>
        private static readonly Dictionary<Type, VertexDataInfo> attribCache =
            new Dictionary<Type, VertexDataInfo>();

        /// <summary>
        /// Get the VertexDataInfo for a particular type
        /// </summary>
        /// <typeparam name="T">The type to analyse</typeparam>
        /// <returns>The VertexDataInfo for the type, or null if the type is not supported</returns>
        public static VertexDataInfo GetInfo<T>() where T : struct
        {
            if (attribCache.ContainsKey(typeof(T))) return attribCache[typeof(T)];

            var vertexDataAttributes = typeof(T).GetCustomAttributes(typeof(VertexDataAttribute), false);

            // the type must have [VertexData]
            if (vertexDataAttributes.Length != 1)
                return null;

            var vertdataattrib = (VertexDataAttribute) vertexDataAttributes[0];
            var divisor = vertdataattrib.Divisor;


            var attribList = new List<VertexPointerAttribute>();
            var stride = Marshal.SizeOf<T>();

            foreach (var fieldInfo in typeof(T).GetFields())
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(VertexPointerAttribute), false);

                // all fields must have [VertexPointer]
                if (attrs.Length == 0)
                    return null;

                var offset = (int) Marshal.OffsetOf<T>(fieldInfo.Name);
                foreach (var attr in attrs)
                {
                    var vpa = (VertexPointerAttribute) attr;
                    vpa.Offset = offset;
                    attribList.Add(vpa);
                }
            }

            return new VertexDataInfo(attribList, stride, divisor);
        }
    }
}