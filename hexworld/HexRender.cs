using System;
using System.Drawing;
using System.IO;
using hexworld.Util;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace hexworld
{
    public class HexRender : GameWindow
    {
        private Program pgm;
        private Texture tex1;
        private Texture tex2;

        private Matrix4 view;
        private Matrix4 proj;

        private readonly float[] verts =
        {
             // +X
            +.5f, +.5f, -.5f, 1.0f, 0.5f,
            +.5f, +.5f, +.5f, 1.0f, 0.0f,
            +.5f, -.5f, +.5f, 0.5f, 0.0f,
            +.5f, -.5f, +.5f, 0.5f, 0.0f,
            +.5f, -.5f, -.5f, 0.5f, 0.5f,
            +.5f, +.5f, -.5f, 1.0f, 0.5f,
            // -X
            -.5f, +.5f, +.5f, 0.5f, 0.0f,
            -.5f, +.5f, -.5f, 0.5f, 0.5f,
            -.5f, -.5f, -.5f, 1.0f, 0.5f,
            -.5f, -.5f, -.5f, 1.0f, 0.5f,
            -.5f, -.5f, +.5f, 1.0f, 0.0f,
            -.5f, +.5f, +.5f, 0.5f, 0.0f,
            // +Y
            +.5f, +.5f, -.5f, 0.5f, 0.5f,
            -.5f, +.5f, -.5f, 1.0f, 0.5f,
            -.5f, +.5f, +.5f, 1.0f, 0.0f,
            -.5f, +.5f, +.5f, 1.0f, 0.0f,
            +.5f, +.5f, +.5f, 0.5f, 0.0f,
            +.5f, +.5f, -.5f, 0.5f, 0.5f,
            // -Y
            +.5f, -.5f, +.5f, 1.0f, 0.0f,
            -.5f, -.5f, +.5f, 0.5f, 0.0f,
            -.5f, -.5f, -.5f, 0.5f, 0.5f,
            -.5f, -.5f, -.5f, 0.5f, 0.5f,
            +.5f, -.5f, -.5f, 1.0f, 0.5f,
            +.5f, -.5f, +.5f, 1.0f, 0.0f,
            // +Z
            +.5f, +.5f, +.5f, 0.5f, 0.0f,
            -.5f, +.5f, +.5f, 0.0f, 0.0f,
            -.5f, -.5f, +.5f, 0.0f, 0.5f,
            -.5f, -.5f, +.5f, 0.0f, 0.5f,
            +.5f, -.5f, +.5f, 0.5f, 0.5f,
            +.5f, +.5f, +.5f, 0.5f, 0.0f,
            // -Z
            +.5f, +.5f, -.5f, 0.5f, 0.5f,
            -.5f, +.5f, -.5f, 0.0f, 0.5f,
            -.5f, -.5f, -.5f, 0.0f, 1.0f,
            -.5f, -.5f, -.5f, 0.0f, 1.0f,
            +.5f, -.5f, -.5f, 0.5f, 1.0f,
            +.5f, +.5f, -.5f, 0.5f, 0.5f,
        };

        public HexRender(int width, int height) : base(width, height)
        {
            Width = width;
            Height = Height;
            X = (DisplayDevice.Default.Width - Width) / 2;
            Y = (DisplayDevice.Default.Height - Height) / 2;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            view = Matrix4.LookAt(10 * Vector3.One, Vector3.Zero, Vector3.UnitZ);
            proj = Matrix4.CreateOrthographic(Width / 100f, Height / 100f, 0, 20);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var vbo = new VBO();
            vbo.Data(verts);

            var vs = new Shader(ShaderType.VertexShader)
            {
                Source = File.ReadAllText("s.vs.glsl")
            };
            vs.Compile();
            Console.Out.WriteLine(vs.Log);

            var fs = new Shader(ShaderType.FragmentShader)
            {
                Source = File.ReadAllText("s.fs.glsl")
            };
            fs.Compile();
            Console.Out.WriteLine(fs.Log);

            pgm = new Program();
            pgm.Attach(vs);
            pgm.Attach(fs);
            pgm.Link();
            Console.Out.WriteLine(pgm.Log);

            vbo.Bind();
            GL.EnableVertexAttribArray(pgm.GetAttribute("pos"));
            GL.VertexAttribPointer(pgm.GetAttribute("pos"), 3, VertexAttribPointerType.Float, false, 5 * sizeof(float),
                0);
            GL.EnableVertexAttribArray(pgm.GetAttribute("coord"));
            GL.VertexAttribPointer(pgm.GetAttribute("coord"), 2, VertexAttribPointerType.Float, false, 5 * sizeof(float),
                3 * sizeof(float));
            VBO.Unbind();

            tex1 = Texture.FromBitmap(new Bitmap("tex.png"));
            tex2 = Texture.FromBitmap(new Bitmap("tex2.png"));
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Viewport(ClientRectangle);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            pgm.Use();
            tex1.Bind(0);
            tex2.Bind(1);

            GL.Uniform1(pgm.GetUniform("tex"), 0);
            view = Matrix4.CreateTranslation(1,-1,0) * view;
            GL.UniformMatrix4(pgm.GetUniform("view"), false, ref view);
            GL.UniformMatrix4(pgm.GetUniform("proj"), false, ref proj);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            GL.Uniform1(pgm.GetUniform("tex"), 1);
            view = Matrix4.CreateTranslation(-2,2,0) * view;
            GL.UniformMatrix4(pgm.GetUniform("view"), false, ref view);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            SwapBuffers();
        }
    }
}