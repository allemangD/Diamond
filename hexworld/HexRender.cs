using System;
using System.Diagnostics;
using Diamond.Shaders;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Diamond;
using Diamond.Attributes;
using Buffer = Diamond.Buffer;

namespace hexworld
{
    [VertexData()]
    public struct Vert
    {
        [VertexAttrib(0, 2)]
        public Vector2 Position;

        public Vert(float x, float y)
        {
            Position = new Vector2(x, y);
        }
    }

    public class HexRender : GameWindow
    {
        public HexRender(int width, int height)
            : base(width, height, new GraphicsMode(32, 24, 0, 0))
        {
            Width = width;
            Height = height;
            X = (DisplayDevice.Default.Width - Width) / 2;
            Y = (DisplayDevice.Default.Height - Height) / 2;
        }

        private VertexArray _triVao;
        private Buffer<Vert> _triVbo;
        private Program _whitePgm;

        private VertexArray _recVao;
        private Buffer<Vert> _recVbo;
        private Program _redPgm;

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (var vs = Shader.FromFile("res/direct.vs.glsl"))
            using (var red = Shader.FromFile("res/red.fs.glsl"))
            using (var white = Shader.FromFile("res/white.fs.glsl"))
            {
                _whitePgm = Program.FromShaders(vs, red);
                _redPgm = Program.FromShaders(vs, white);
            }

            _triVbo = Buffer.FromData(new[]
            {
                new Vert(-.8f, -.8f),
                new Vert(+.8f, -.8f),
                new Vert(+.0f, +.8f)
            });
            Program.Current = _redPgm;
            _triVao = VertexArray.Create();
            _triVao.Attach(_triVbo);

            _recVbo = Buffer.FromData(new[]
            {
                new Vert(-.9f, -.5f),
                new Vert(+.9f, -.5f),
                new Vert(+.9f, +.5f),
                new Vert(+.9f, +.5f),
                new Vert(-.9f, +.5f),
                new Vert(-.9f, -.5f)
            });
            Program.Current = _whitePgm;
            _recVao = VertexArray.Create();
            _recVao.Attach(_recVbo);
        }

        /// <inheritdoc />
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            _triVbo?.Dispose();
            _recVbo?.Dispose();

            _whitePgm?.Dispose();
            _redPgm?.Dispose();
        }

        /// <inheritdoc />
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Viewport(ClientRectangle);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            Program.Current = _redPgm;
            VertexArray.Current = _triVao;
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            Program.Current = _whitePgm;
            VertexArray.Current = _recVao;
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            SwapBuffers();
        }
    }
}