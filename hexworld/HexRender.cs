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

        private Program _pgm;
        private Buffer<float> _buf;

        /// <inheritdoc />
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            _pgm?.Dispose();
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _pgm = Program.FromFiles("res/obj.fs.glsl", "res/obj.vs.glsl");

            Program.Current = _pgm;

            _buf = Buffer.FromData(new float[]
            {
                -.8f, -.8f,
                +.8f, -.8f,
                +.0f, +.8f
            });
        }

        /// <inheritdoc />
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Viewport(ClientRectangle);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            SwapBuffers();
        }
    }
}