using Diamond.Buffers;
using OpenTK;

namespace Diamond.Util
{
    /// <summary>
    /// Vertex buffer data for Wavefront meshes
    /// </summary>
    [VertexData]
    public struct ObjVertex
    {
        /// <summary>
        /// Vertex position (v)
        /// </summary>
        [VertexPointer("position", 3)]
        [VertexPointer("v", 3)]
        public Vector3 Position;

        /// <summary>
        /// UV coordinate (vt)
        /// </summary>
        [VertexPointer("uv", 2)]
        [VertexPointer("vt", 2)]
        public Vector2 UV;

        /// <summary>
        /// Vertex normal (vn)
        /// </summary>
        [VertexPointer("normal", 3)]
        [VertexPointer("vn", 3)]
        public Vector3 Normal;

        /// <summary>
        /// Create a new ObjVertex
        /// </summary>
        /// <param name="position">The vertex position (v)</param>
        /// <param name="uv">The uv coordinate (vt)</param>
        /// <param name="normal">The vertex normal (n)</param>
        public ObjVertex(Vector3 position, Vector2 uv, Vector3 normal)
        {
            Position = position;
            UV = uv;
            Normal = normal;
        }
    }
}