using Diamond.Buffers;
using Newtonsoft.Json;
using OpenTK;

namespace hexworld
{
    [VertexData]
    public struct Vertex
    {
        [JsonProperty("pos")]
        [VertexPointer("locpos", 3)]
        public Vector3 Position;

        [JsonProperty("uv")]
        [VertexPointer("coord", 2)]
        public Vector2 UV;

        [JsonProperty("norm")]
        [VertexPointer("norm", 3)]
        public Vector3 Normal;

        public Vertex(Vector3 position, Vector2 uv, Vector3 normal)
        {
            Position = position;
            UV = uv;
            Normal = normal;
        }

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}, {nameof(UV)}: {UV}, {nameof(Normal)}: {Normal}";
        }
    }
}