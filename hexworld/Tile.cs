using Diamond.Buffers;
using Newtonsoft.Json;
using OpenTK;

namespace hexworld
{
    [VertexData]
    public struct Tile
    {
        [JsonProperty("pos")]
        [VertexPointer("glbpos", 3, Divisor = 1)]
        public Vector3 Position;

        public Tile(Vector3 position)
        {
            Position = position;
        }

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}";
        }
    }
}