using Diamond.Buffers;
using Newtonsoft.Json;
using OpenTK;

namespace hexworld
{
    [VertexData(Divisor = 1)]
    public struct Tile
    {
        [JsonProperty("pos")]
        [VertexPointer("glbpos", 3)]
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