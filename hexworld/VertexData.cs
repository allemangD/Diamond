using Diamond.Buffers;
using Newtonsoft.Json;
using OpenTK;

namespace hexworld
{
    public struct VertexData
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

        public VertexData(Vector3 position, Vector2 uv, Vector3 normal)
        {
            Position = position;
            UV = uv;
            Normal = normal;
        }
    }
}