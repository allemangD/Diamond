using Diamond.Shaders;
using Diamond.Textures;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Render
{
    /// <summary>
    /// Manage a group of buffers, ranges, and uniforms to render
    /// </summary>
    /// <typeparam name="TInstance">The type of data to use as Instance information</typeparam>
    /// <typeparam name="TVertex">The type of data to use as Vertex information</typeparam>
    public class RenderGroup<TInstance, TVertex> where TInstance : struct where TVertex : struct
    {
        /// <summary>
        /// The range of vertex values to render
        /// </summary>
        public VertexBuffer<TVertex> Vertices;

        /// <summary>
        /// The range of instance values to render
        /// </summary>
        public VertexBuffer<TInstance> Instance;

        /// <summary>
        /// The program to use to render this Rendergroup
        /// </summary>
        public Program Program;

        /// <summary>
        /// The Texture to use for this Rendergroup
        /// </summary>
        public Texture Texture;

        /// <summary>
        /// View and Projection information for this Rendergroup
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// Draw this rendergroup using the predefined settings.
        /// </summary>
        public void Draw()
        {
            Program.Use();

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

            if (Instance != null)
                Vertices.DrawInstanced(Instance);
            else
                Vertices.Draw();
        }
    }
}