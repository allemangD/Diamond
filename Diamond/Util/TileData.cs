using Diamond.Buffers;
using OpenTK;

namespace Diamond.Util
{
    /// <summary>
    /// Vertex buffer data for instanced rendering
    /// </summary>
    [VertexData(Divisor = 1)]
    public struct TileData
    {
        /// <summary>
        /// The global position of the instance
        /// </summary>
        [VertexPointer("glbpos", 3)]
        [VertexPointer("global_pos", 3)]
        public Vector3 Position;

        /// <summary>
        /// Create a new TileData
        /// </summary>
        /// <param name="position">The global position of the instance</param>
        public TileData(Vector3 position)
        {
            Position = position;
        }
    }
}