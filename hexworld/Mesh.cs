using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Diamond.Buffers;
using Diamond.Shaders;
using Newtonsoft.Json;
using OpenTK.Graphics.OpenGL4;

namespace hexworld
{
    public class Mesh<T> where T : struct
    {
        public SubArray<T> Vertices;
        public PrimitiveType Primitive;

        private static VertexDataInfo tVdi;
        private static List<VertexPointerAttribute> attribs;

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
        public static Mesh<T> FromJson<T>(string json) where T : struct
        {
            var vertices = JsonConvert.DeserializeObject<T[]>(json);
            return new Mesh<T>(vertices);
        }


        public static T[] Join<T>(params Mesh<T>[] meshes) where T : struct => Join((IEnumerable<Mesh<T>>) meshes);

        public static T[] Join<T>(IEnumerable<Mesh<T>> meshes) where T : struct
        {
            return SubArray.Join(meshes.Select(x => x.Vertices));
        }
    }
}