using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hexworld.Util;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace hexworld
{
    public struct Vert
    {
        public static readonly int SizeInBytes = sizeof(float) * 5;

        public Vector3 Point;
        public Vector2 TexCoord;

        public Vert(Vector3 point, Vector2 texCoord)
        {
            Point = point;
            TexCoord = texCoord;
        }

        public Vert(float px, float py, float pz, float tx, float ty)
            : this(new Vector3(px, py, pz), new Vector2(tx, ty))
        {
        }
    }

    public class HexWindow : GameWindow
    {
        private readonly Vert[] _verts =
        {
            // +X
            new Vert(+.5f, +.5f, -.5f, 1.0f, 0.5f), new Vert(+.5f, +.5f, +.5f, 1.0f, 0.0f), new Vert(+.5f, -.5f, +.5f, 0.5f, 0.0f),
            new Vert(+.5f, -.5f, +.5f, 0.5f, 0.0f), new Vert(+.5f, -.5f, -.5f, 0.5f, 0.5f), new Vert(+.5f, +.5f, -.5f, 1.0f, 0.5f),
            // -X
            new Vert(-.5f, +.5f, +.5f, 0.5f, 0.0f), new Vert(-.5f, +.5f, -.5f, 0.5f, 0.5f), new Vert(-.5f, -.5f, -.5f, 1.0f, 0.5f),
            new Vert(-.5f, -.5f, -.5f, 1.0f, 0.5f), new Vert(-.5f, -.5f, +.5f, 1.0f, 0.0f), new Vert(-.5f, +.5f, +.5f, 0.5f, 0.0f),
            // +Y
            new Vert(+.5f, +.5f, -.5f, 0.5f, 0.5f), new Vert(-.5f, +.5f, -.5f, 1.0f, 0.5f), new Vert(-.5f, +.5f, +.5f, 1.0f, 0.0f),
            new Vert(-.5f, +.5f, +.5f, 1.0f, 0.0f), new Vert(+.5f, +.5f, +.5f, 0.5f, 0.0f), new Vert(+.5f, +.5f, -.5f, 0.5f, 0.5f),
            // -Y
            new Vert(+.5f, -.5f, +.5f, 1.0f, 0.0f), new Vert(-.5f, -.5f, +.5f, 0.5f, 0.0f), new Vert(-.5f, -.5f, -.5f, 0.5f, 0.5f),
            new Vert(-.5f, -.5f, -.5f, 0.5f, 0.5f), new Vert(+.5f, -.5f, -.5f, 1.0f, 0.5f), new Vert(+.5f, -.5f, +.5f, 1.0f, 0.0f),
            // +Z
            new Vert(+.5f, +.5f, +.5f, 0.5f, 0.0f), new Vert(-.5f, +.5f, +.5f, 0.0f, 0.0f), new Vert(-.5f, -.5f, +.5f, 0.0f, 0.5f),
            new Vert(-.5f, -.5f, +.5f, 0.0f, 0.5f), new Vert(+.5f, -.5f, +.5f, 0.5f, 0.5f), new Vert(+.5f, +.5f, +.5f, 0.5f, 0.0f),
            // -Z
            new Vert(+.5f, +.5f, -.5f, 0.5f, 0.5f), new Vert(-.5f, +.5f, -.5f, 0.0f, 0.5f), new Vert(-.5f, -.5f, -.5f, 0.0f, 1.0f),
            new Vert(-.5f, -.5f, -.5f, 0.0f, 1.0f), new Vert(+.5f, -.5f, -.5f, 0.5f, 1.0f), new Vert(+.5f, +.5f, -.5f, 0.5f, 0.5f),
        };

        private Matrix4 _view = Matrix4.Identity;
        private Matrix4 _proj = Matrix4.Identity;

        private Program _pgm;

        private Texture _tex1;
        private Texture _tex2;

        public HexWindow(int width, int height)
            : base(width, height)
        {
            Width = width;
            Height = height;
            X = (DisplayDevice.Default.Width - Width) / 2;
            Y = (DisplayDevice.Default.Height - Height) / 2;
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            const float s = 200f;
            _view = Matrix4.LookAt(10 * Vector3.One, Vector3.Zero, Vector3.UnitZ);
            _proj = Matrix4.CreateOrthographic(Width / s, Height / s, 0, 100);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var vbo = new VBO();
            vbo.Data(_verts);

            var vs = new Shader(ShaderType.VertexShader) {Source = File.ReadAllText("s.vs.glsl")};
            if (!vs.Compile())
                Console.Out.WriteLine($"vs: {vs.Log}");

            var fs = new Shader(ShaderType.FragmentShader) {Source = File.ReadAllText("s.fs.glsl")};
            if (!fs.Compile())
                Console.Out.WriteLine($"fs: {fs.Log}");

            _pgm = new Program();
            _pgm.Attach(vs);
            _pgm.Attach(fs);
            if (!_pgm.Link())
                Console.Out.WriteLine($"pgm: {_pgm.Log}");

            vbo.Bind();
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vert.SizeInBytes, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vert.SizeInBytes, Vector3.SizeInBytes);
            VBO.Unbind();

            _tex1 = Texture.FromBitmap(new Bitmap("tex.png"));

            _tex2 = Texture.FromBitmap(new Bitmap("tex2.png"));
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Viewport(ClientRectangle);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);

            _pgm.Use();

            _tex1.Bind(0);
            _tex2.Bind(1);

            GL.Uniform1(_pgm.GetUniform("tex"), 0);

            GL.UniformMatrix4(_pgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(_pgm.GetUniform("proj"), false, ref _proj);

            GL.DrawArrays(PrimitiveType.Triangles, 0, _verts.Length);

            GL.Uniform1(_pgm.GetUniform("tex"), 1);
            _view = Matrix4.CreateTranslation(0, 2, 0) * _view;
            GL.UniformMatrix4(_pgm.GetUniform("view"), false, ref _view);

            GL.DrawArrays(PrimitiveType.Triangles, 0, _verts.Length);

            SwapBuffers();
        }
    }
}