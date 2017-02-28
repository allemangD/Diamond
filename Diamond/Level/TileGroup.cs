using Diamond.Buffers;

namespace Diamond.Level
{
    internal class TileGroup
    {
        public SubArray<TileData> Tiles;
        public Mesh<ObjVertex> Mesh;

        public TileGroup(SubArray<TileData> tiles, Mesh<ObjVertex> mesh)
        {
            Tiles = tiles;
            Mesh = mesh;
        }
    }
}