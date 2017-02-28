using Diamond.Buffers;
using OpenTK;

namespace Diamond
{
    [VertexData]
    public struct ObjVertex
    {
        [VertexPointer("position", 3)]
        public Vector3 Position;

        [VertexPointer("uv", 2)]
        public Vector2 UV;

        [VertexPointer("normal", 3)]
        public Vector3 Normal;

        public ObjVertex(Vector3 position, Vector2 uv, Vector3 normal)
        {
            Position = position;
            UV = uv;
            Normal = normal;
        }
    }
}