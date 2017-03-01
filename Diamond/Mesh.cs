using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Linq;
using Diamond.Buffers;
using Diamond.Util;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Diamond
{
    public class Mesh<T> where T : struct
    {
        public SubArray<T> Vertices;
        public PrimitiveType Primitive;

        public string Name { get; set; }

        private static readonly VertexDataInfo tVdi;

        static Mesh()
        {
            tVdi = VertexDataInfo.GetInfo<T>();
        }

        public Mesh(T[] vertices, PrimitiveType primitive = PrimitiveType.Triangles)
            : this(new SubArray<T>(vertices), primitive)
        {
        }

        public Mesh(SubArray<T> vertices, PrimitiveType primitive = PrimitiveType.Triangles)
        {
            Vertices = vertices;
            Primitive = primitive;
        }

        public void Draw()
        {
            tVdi.EnableVertexPointers();

            GL.DrawArrays(Primitive, Vertices.Offset, Vertices.Length);

            tVdi.DisableVertexPointers();
        }

        public void DrawInstanced<TI>(SubArray<TI> instanceArray) where TI : struct
        {
            var tiVdi = VertexDataInfo.GetInfo<TI>();

            tVdi.EnableVertexPointers();
            tiVdi.EnableVertexPointers();

            GL.DrawArraysInstancedBaseInstance(Primitive, Vertices.Offset, Vertices.Length, instanceArray.Length,
                instanceArray.Offset);

            tVdi.DisableVertexPointers();
            tiVdi.DisableVertexPointers();
        }
    }

    public static class Mesh
    {
        public static Mesh<ObjVertex>[] FromObj(string file, bool join = true)
        {
            var lines = File.ReadAllLines(file);

            var meshes = new List<Mesh<ObjVertex>>();
            var name = file;
            var vs = new List<Vector3>();
            var vts = new List<Vector2>();
            var vns = new List<Vector3>();
            var faces = new List<ObjVertex>();

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
                            meshes.Add(new Mesh<ObjVertex>(faces.ToArray()) {Name = name});
                            faces.Clear();
                        }
                        name = items[1];
                        break;
                }
            }

            if (faces.Count > 0)
                meshes.Add(new Mesh<ObjVertex>(faces.ToArray()) {Name = name});

            if (join)
                Join(meshes);

            return meshes.ToArray();
        }

        public static T[] Join<T>(params Mesh<T>[] meshes) where T : struct => Join((IEnumerable<Mesh<T>>) meshes);

        public static T[] Join<T>(IEnumerable<Mesh<T>> meshes) where T : struct
        {
            return SubArray.Join(meshes.Select(x => x.Vertices));
        }
    }
}