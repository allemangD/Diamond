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

        private Buffer<Vert> _vbo;

        private VertexArray _triVao;
        private Buffer<uint> _triIbo;
        private Program _whitePgm;

        private VertexArray _recVao;
        private Buffer<uint> _recIbo;
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

            _vbo = Buffer.FromData(new[]
            {
                new Vert(-.8f, -.8f),
                new Vert(+.8f, -.8f),
                new Vert(+.0f, +.8f),
                new Vert(-.9f, -.5f),
                new Vert(+.9f, -.5f),
                new Vert(+.9f, +.5f),
                new Vert(+.9f, +.5f),
                new Vert(-.9f, +.5f),
                new Vert(-.9f, -.5f)
            });

            _triIbo = Buffer.FromData(new uint[]
            {
                0, 1, 2
            });

            Program.Current = _redPgm;
            _triVao = VertexArray.Create();
            _triVao.Attach(_vbo);
            _triVao.ElementArrayBuffer = _triIbo;

            _recIbo = Buffer.FromData(new uint[]
            {
                3, 4, 5, 6, 7, 8
            });

            Program.Current = _whitePgm;
            _recVao = VertexArray.Create();
            _recVao.Attach(_vbo);
            _recVao.ElementArrayBuffer = _recIbo;
        }

        /// <inheritdoc />
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            _triVao?.Dispose();
            _recVao?.Dispose();

            _triIbo?.Dispose();
            _recIbo?.Dispose();

            _vbo?.Dispose();

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
            GL.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, 0);

            Program.Current = _whitePgm;
            VertexArray.Current = _recVao;
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }
    }
}