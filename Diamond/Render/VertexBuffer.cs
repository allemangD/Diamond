using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Diamond.Buffers;
using Diamond.Shaders;
using Diamond.Util;
using NLog;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Buffer = Diamond.Buffers.Buffer;

namespace Diamond.Render
{
    /// <summary>
    /// Manage a vertex buffer object
    /// </summary>
    /// <typeparam name="T">Buffer data type</typeparam>
    public class VertexBuffer<T> : IDisposable where T : struct
    {
        private Logger Logger = LogManager.GetLogger("VertexBuffer");

        /// <summary>
        /// The name of this buffer object for identification
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The underlying buffer for this object
        /// </summary>
        public Buffer<T> Buffer;

        /// <summary>
        /// A subset of the Buffer's array for this buffer
        /// </summary>
        public SubArray<T> Vertices;

        /// <summary>
        /// Primitive type to render this object
        /// </summary>
        public PrimitiveType Primitive;

        /// <summary>
        /// Vertex data info for this type of data
        /// </summary>
        private static readonly VertexDataInfo tVdi = VertexDataInfo.GetInfo<T>();

        internal VertexBuffer(Buffer<T> buffer, SubArray<T> vertices, PrimitiveType primitive, string name)
        {
            Buffer = buffer;
            Vertices = vertices;
            Primitive = primitive;
            Name = name;
        }

        /// <summary>
        /// Render this buffer
        /// </summary>
        public void Draw()
        {
            Buffer.PointTo(Program.Current);

            tVdi.EnableVertexPointers();

            GL.DrawArrays(Primitive, Vertices.Offset, Vertices.Length);

            tVdi.DisableVertexPointers();
        }

        /// <summary>
        /// Render this buffer using a second buffer as instance data
        /// </summary>
        /// <typeparam name="TI"></typeparam>
        /// <param name="instance"></param>
        public void DrawInstanced<TI>(VertexBuffer<TI> instance) where TI : struct
        {
            var tiVdi = VertexBuffer<TI>.tVdi;

            if (tiVdi.Divisor == 0)
            {
                Logger.Error("Cannot render mesh with instanced of type {0} - Divisor is 0", typeof(TI).Name);
                return;
            }

            Buffer.PointTo(Program.Current);
            instance.Buffer.PointTo(Program.Current);

            tVdi.EnableVertexPointers();
            tiVdi.EnableVertexPointers();

            GL.DrawArraysInstancedBaseInstance(Primitive, Vertices.Offset, Vertices.Length, instance.Vertices.Length,
                instance.Vertices.Offset);

            tVdi.DisableVertexPointers();
            tiVdi.DisableVertexPointers();
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }

    /// <summary>
    /// Static operations for vertex buffers
    /// </summary>
    public static class VertexBuffer
    {
        public static VertexBuffer<T>[] FromArrays<T>(T[][] arrays, PrimitiveType primitive = PrimitiveType.Triangles,
            string name = null) where T : struct
        {
            var buffer = Buffer.Empty<T>(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw, name);

            var vertices = arrays.SelectMany(x => x).ToArray();
            buffer.Data(vertices);

            var vertBuffers = new List<VertexBuffer<T>>();

            var offset = 0;
            foreach (var array in arrays)
            {
                vertBuffers.Add(new VertexBuffer<T>(
                    buffer,
                    new SubArray<T>(vertices, offset, array.Length),
                    primitive, name));
                offset += array.Length;
            }

            return vertBuffers.ToArray();
        }

        public static VertexBuffer<T>[] FromArrays<T>(IEnumerable<IEnumerable<T>> arrays,
            PrimitiveType primitive = PrimitiveType.Triangles, string name = null) where T : struct =>
            FromArrays(arrays.Select(x => x.ToArray()).ToArray(), primitive, name);

        public static VertexBuffer<ObjVertex>[] FromWavefront(string file)
        {
            var lines = File.ReadAllLines(file).Where(l => !l.StartsWith("#")).Select(l => l.Split(' ')).ToArray();

            var buffer = Buffer<ObjVertex>.Empty(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw,
                $"{file} buffer");

            var name = file;

            var vertBuffers = new List<VertexBuffer<ObjVertex>>();

            // get all positional data
            var vs = lines
                .Where(items => items[0] == "v") // only "v" lines
                .Select(items => new Vector3(    // get a vector3 from the values
                    float.Parse(items[1]),
                    float.Parse(items[2]),
                    float.Parse(items[3])))
                .ToArray();

            // get all uv data
            var vts = lines
                .Where(items => items[0] == "vt") // only "vt" lines
                .Select(items => new Vector2(     // get a vector2 from the values
                    float.Parse(items[1]),
                    1 - float.Parse(items[2])))   // flip along y
                .ToArray();

            // get all normal data
            var vns = lines
                .Where(items => items[0] == "vn") // only "vn" lines
                .Select(items => new Vector3(     // get a vector3 from the values
                    float.Parse(items[1]),
                    float.Parse(items[2]),
                    float.Parse(items[3])))
                .ToArray();

            // get all vertex data
            var vertices = lines
                .Where(items => items[0] == "f")      // only "f" liens
                .Select(items => items.Skip(1))       // skip the "f" item
                .Select(items => items                // get each vertex from the triangle
                    .Select(inds => inds.Split('/'))) // split items into indices
                .SelectMany(inds => inds)             // collapse nested index array into array of indexes
                .Select(inds => new ObjVertex(        // get vertexdata from the value data at each index
                    inds[0] == "" ? Vector3.Zero : vs[int.Parse(inds[0]) - 1],
                    inds[1] == "" ? Vector2.Zero : vts[int.Parse(inds[1]) - 1],
                    inds[2] == "" ? Vector3.Zero : vns[int.Parse(inds[2]) - 1]))
                .ToArray();

            buffer.Data(vertices); // upload vertex data to the buffer

            var offset = 0; // offset of each vertexbuffer
            var count = 0; // length of each vertexbuffer

            foreach (var items in lines.Where(items => items[0] == "o" || items[0] == "f"))
            {
                if (items[0] != "f")
                {
                    if (count > 0)
                    {
                        vertBuffers.Add(new VertexBuffer<ObjVertex>(
                            buffer,
                            new SubArray<ObjVertex>(vertices, offset, count),
                            PrimitiveType.Triangles,
                            name));
                    }

                    // reset for next vbo and move offset to the end of this one
                    name = items[1];
                    offset += count;
                    count = 0;
                }
                else
                {
                    count += items.Length - 1; // size of current vbo increases by that many vertices
                }
            }

            // at the last vbo, or only vbo.
            if (count > 0)
            {
                vertBuffers.Add(new VertexBuffer<ObjVertex>(
                    buffer,
                    new SubArray<ObjVertex>(vertices, offset, count),
                    PrimitiveType.Triangles,
                    name));
            }

            return vertBuffers.ToArray();
        }
    }
}