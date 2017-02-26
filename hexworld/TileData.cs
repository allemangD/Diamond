using Diamond.Buffers;
using Newtonsoft.Json;
using OpenTK;

namespace hexworld
{
    public struct TileData
    {
        [JsonProperty("pos")]
        [VertexPointer("glbpos", 3, Divisor = 1)]
        public Vector3 Position;

        public TileData(Vector3 position)
        {
            Position = position;
        }
    }
}