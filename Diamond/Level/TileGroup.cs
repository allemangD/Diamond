using Diamond.Buffers;
using Diamond.Shaders;

namespace Diamond.Level
{
    internal class TileGroup
    {
        public SubArray<TileData> Tiles;
        public Mesh<ObjVertex> Mesh;
        public Program Program;
        public int Texture;

        public TileGroup(Mesh<ObjVertex> mesh, Program program, int texture, SubArray<TileData> tiles)
        {
            Mesh = mesh;
            Program = program;
            Tiles = tiles;
            Texture = texture;
        }
    }
}