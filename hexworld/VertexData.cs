using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hexworld.Util;
using Newtonsoft.Json;
using OpenTK;

namespace hexworld
{
    public partial class HexRender
    {
        public struct Tile
        {
            [JsonProperty("pos")]
            [VertexPointer("glbpos", 3, Divisor = 1)]
            public Vector3 Position;

            public Tile(Vector3 position)
            {
                Position = position;
            }
        }

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
        }

        private readonly Vertex[] cubeVerts = JsonConvert.DeserializeObject<Vertex[]>(File.ReadAllText("cube.json"));
        private Tile[] tiles = JsonConvert.DeserializeObject<Tile[]>(File.ReadAllText("tiles.json"));
    }
}