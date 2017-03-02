using System;
using Diamond.Buffers;
using Diamond.Shaders;
using Diamond.Textures;
using Diamond.Util;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Render
{
    public class RenderGroup
    {
        public SubArray<TileData> Tiles;
        public Mesh<ObjVertex> Mesh;

        public Buffer<TileData> TileBuffer;
        public Buffer<ObjVertex> MeshBuffer;

        public Program Program;
        public Texture Texture;

        public Camera Camera;

        public void Draw()
        {
            Program.Use();

            TileBuffer.PointTo(Program);
            MeshBuffer.PointTo(Program);

            Texture.Bind(0);

            var texLoc = Program.UniformLocation("tex");
            var viewLoc = Program.UniformLocation("view");
            var projLoc = Program.UniformLocation("proj");

            if (texLoc.HasValue)
                GL.Uniform1(texLoc.Value, 0);
            if (viewLoc.HasValue)
                GL.UniformMatrix4(viewLoc.Value, false, ref Camera.View);
            if (projLoc.HasValue)
                GL.UniformMatrix4(projLoc.Value, false, ref Camera.Projection);

            Mesh.DrawInstanced(Tiles);
        }
    }
}