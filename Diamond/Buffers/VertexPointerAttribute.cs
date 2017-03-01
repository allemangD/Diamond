using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Diamond.Shaders;
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
        public int Offset { get; set; } = 0;

        public VertexPointerAttribute(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }

    public class VertexDataInfo
    {
        public readonly IReadOnlyCollection<VertexPointerAttribute> Pointers;
        public readonly int Stride;
        public readonly int Divisor;

        private VertexDataInfo(IList<VertexPointerAttribute> pointers, int stride, int divisor)
        {
            Pointers = new ReadOnlyCollection<VertexPointerAttribute>(pointers);
            Stride = stride;
            Divisor = divisor;
        }

        public void EnableVertexPointers()
        {
            if (Program.Current == null)
                throw new InvalidOperationException("Cant render a mesh with no active shader.");

            foreach (var attr in Pointers)
            {
                var loc = Program.Current.AttributeLocation(attr.Name);
                if (!loc.HasValue)
                    continue;
                GL.EnableVertexAttribArray((int) loc);
                GL.VertexAttribDivisor((int) loc, Divisor);
            }
        }

        public void DisableVertexPointers()
        {
            if (Program.Current == null)
                throw new InvalidOperationException("Cant render a mesh with no active shader.");

            foreach (var attr in Pointers)
            {
                var loc = Program.Current.AttributeLocation(attr.Name);
                if (!loc.HasValue)
                    continue;
                GL.DisableVertexAttribArray((int) loc);
            }
        }

        private static readonly Dictionary<Type, VertexDataInfo> attribCache =
            new Dictionary<Type, VertexDataInfo>();

        public static VertexDataInfo GetInfo<T>() where T : struct
        {
            if (attribCache.ContainsKey(typeof(T))) return attribCache[typeof(T)];

            var vertexDataAttributes = typeof(T).GetCustomAttributes(typeof(VertexDataAttribute), false);

            if (vertexDataAttributes.Length != 1)
                return null;

            var vertdataattrib = (VertexDataAttribute) vertexDataAttributes[0];
            var divisor = vertdataattrib.Divisor;


            var attribList = new List<VertexPointerAttribute>();
            var stride = Marshal.SizeOf<T>();

            foreach (var fieldInfo in typeof(T).GetFields())
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(VertexPointerAttribute), false);
                if (attrs.Length == 0) continue;

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