using Newtonsoft.Json;
using OpenTK;

namespace Diamond.Level
{
    internal class TileInfo
    {
        [JsonProperty("mesh")]
        public string Mesh { get; set; }

        [JsonProperty("pos")]
        public Vector3 Position { get; set; }

        public TileData TileData => new TileData(Position);

        public override string ToString()
        {
            return $"Mesh: {Mesh}, Position: {Position}";
        }
    }
}