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
    public class VertexBuffer<T> : IDisposable where T : struct
    {
        private Logger Logger = LogManager.GetLogger("VertexBuffer");

        public string Name { get; set; }
        public Buffer<T> Buffer;
        public SubArray<T> Vertices;
        public PrimitiveType Primitive;

        private static readonly VertexDataInfo tVdi;

        static VertexBuffer()
        {
            tVdi = VertexDataInfo.GetInfo<T>();
        }

        internal VertexBuffer(Buffer<T> buffer, SubArray<T> vertices, PrimitiveType primitive, string name)
        {
            Buffer = buffer;
            Vertices = vertices;
            Primitive = primitive;
            Name = name;
        }

        public void Draw()
        {
            Buffer.PointTo(Program.Current);

            tVdi.EnableVertexPointers();

            GL.DrawArrays(Primitive, Vertices.Offset, Vertices.Length);

            tVdi.DisableVertexPointers();
        }

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
            var lines = File.ReadAllLines(file);

            var buffer = Buffer<ObjVertex>.Empty(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw,
                $"{file} buffer");

            var vs = new List<Vector3>();
            var vts = new List<Vector2>();
            var vns = new List<Vector3>();
            var faces = new List<ObjVertex>();
            var vertices = new List<ObjVertex>();

            var name = file;

            var vertBuffers = new List<VertexBuffer<ObjVertex>>();

            foreach (var line in lines)
            {
                if (line.StartsWith("#")) continue;

                var items = line.Split(' ');

                switch (items[0])
                {
                    case "v":
                        var v = new Vector3(
                            float.Parse(items[1]),
                            float.Parse(items[2]),
                            float.Parse(items[3]));
                        vs.Add(v);
                        break;
                    case "vt":
                        var vt = new Vector2(
                            float.Parse(items[1]),
                            1 - float.Parse(items[2]));
                        vts.Add(vt);
                        break;
                    case "vn":
                        var vn = new Vector3(
                            float.Parse(items[1]),
                            float.Parse(items[2]),
                            float.Parse(items[3]));
                        vns.Add(vn);
                        break;
                    case "f":
                        for (var i = 1; i < 4; i++)
                        {
                            var inds = items[i].Split('/');
                            var vi = inds[0] == "" ? Vector3.Zero : vs[int.Parse(inds[0]) - 1];
                            var vti = inds[1] == "" ? Vector2.Zero : vts[int.Parse(inds[1]) - 1];
                            var vni = inds[2] == "" ? Vector3.Zero : vns[int.Parse(inds[2]) - 1];
                            var f = new ObjVertex(vi, vti, vni);
                            faces.Add(f);
                        }
                        break;
                    case "o":
                        if (faces.Count > 0)
                        {
                            var count = vertices.Count;
                            vertices.AddRange(faces);
                            vertBuffers.Add(new VertexBuffer<ObjVertex>(
                                buffer,
                                new SubArray<ObjVertex>(vertices.ToArray(), count, faces.Count),
                                PrimitiveType.Triangles,
                                name));
                        }
                        name = items[1];
                        faces.Clear();
                        break;
                    default:
                        break;
                }
            }

            if (faces.Count > 0)
            {
                var count = vertices.Count;
                vertices.AddRange(faces);
                vertBuffers.Add(new VertexBuffer<ObjVertex>(
                    buffer, new SubArray<ObjVertex>(vertices.ToArray(), count, faces.Count),
                    PrimitiveType.Triangles,
                    name));
            }

            var data = vertices.ToArray();

            foreach (var vertexBuffer in vertBuffers)
                vertexBuffer.Vertices.Array = data;

            buffer.Data(vertices.ToArray());

            return vertBuffers.ToArray();
        }
    }
}