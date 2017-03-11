using System;
using System.Diagnostics;
using Diamond.Shaders;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Diamond;
using Buffer = Diamond.Buffer;

namespace hexworld
{
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

        private int _triVao;
        private Buffer<float> _triVbo;
        private Program _whitePgm;

        private int _recVao;
        private Buffer<float> _recVbo;
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

                _triVbo = Buffer.FromData(new float[]
                {
                    -.8f, -.8f,
                    +.8f, -.8f,
                    +.0f, +.8f
                });
                _triVao = GL.GenVertexArray();
                GL.BindVertexArray(_triVao);
                Program.Current = _redPgm;
                Buffer.ArrayBuffer = _triVbo;
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);

                _recVbo = Buffer.FromData(new float[]
                {
                    -.9f, -.5f,
                    +.9f, -.5f,
                    +.9f, +.5f,
                    +.9f, +.5f,
                    -.9f, +.5f,
                    -.9f, -.5f,
                });
                _recVao = GL.GenVertexArray();
                GL.BindVertexArray(_recVao);
                Program.Current = _whitePgm;
                Buffer.ArrayBuffer = _recVbo;
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
            }
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
            GL.BindVertexArray(_triVao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            Program.Current = _whitePgm;
            GL.BindVertexArray(_recVao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            SwapBuffers();
        }
    }
}