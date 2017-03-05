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

            if (Program.HasUniform("tex"))
                GL.Uniform1(Program.UniformLocation("tex"), 0);
            if (Program.HasUniform("view"))
                GL.UniformMatrix4(Program.UniformLocation("view"), false, ref Camera.View);
            if (Program.HasUniform("proj"))
                GL.UniformMatrix4(Program.UniformLocation("proj"), false, ref Camera.Projection);

            if (Instance != null)
                Vertices.DrawInstanced(Instance);
            else
                Vertices.Draw();
        }
    }
}