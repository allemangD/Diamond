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
        [VertexAttrib(0, 3)] public Vector3 Position;

        public Vert(float x, float y, float z = 0)
        {
            Position = new Vector3(x, y, z);
        }
    }

    [VertexData(1)]
    public struct Entity
    {
        [VertexAttrib(1, 3)] public Vector3 Position;

        public Entity(float x, float y, float z = 0)
        {
            Position = new Vector3(x, y, z);
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

        private Buffer<Vert> _sqrBuff;
        private Buffer<uint> _sqrElem;
        private VertexArray _sqrAo;

        private Buffer<Entity> _entBuff;

        private Program _pgm;

        /// <inheritdoc />
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            _sqrAo?.Dispose();
            _sqrBuff?.Dispose();
            _sqrElem?.Dispose();

            _pgm?.Dispose();
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (var vs = Shader.FromFile("res/direct.vs.glsl"))
            using (var white = Shader.FromFile("res/white.fs.glsl"))
            {
                _pgm = Program.FromShaders(vs, white);
            }

            _sqrBuff = Buffer.FromData(new[]
            {
                new Vert(-.5f, -.5f),
                new Vert(+.5f, -.5f),
                new Vert(+.5f, +.5f),
                new Vert(-.5f, +.5f),
            });

            _sqrElem = Buffer.FromData(new uint[]
            {
                0, 1, 2, 2, 3, 0
            });

            _entBuff = Buffer.FromData(new[]
            {
                new Entity(-1, -1),
                new Entity(-1, 1),
                new Entity(1, -1),
                new Entity(1, 1),
            });

            Program.Current = _pgm;
            _sqrAo = VertexArray.Create();
            _sqrAo.Attach(_sqrBuff);
            _sqrAo.Attach(_entBuff);
            _sqrAo.ElementArrayBuffer = _sqrElem;
        }

        /// <inheritdoc />
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Viewport(ClientRectangle);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            var proj = Matrix4.CreateOrthographic(6f, 6f * Height / Width, -10f, 10f);
            var view = Matrix4.LookAt(Vector3.Zero, -Vector3.One, Vector3.UnitZ);

            Program.Current = _pgm;
            VertexArray.Current = _sqrAo;
            GL.UniformMatrix4(0, false, ref proj);
            GL.UniformMatrix4(1, false, ref view);
            GL.DrawElementsInstanced(PrimitiveType.Triangles, _sqrElem.Size, DrawElementsType.UnsignedInt, IntPtr.Zero,
                _entBuff.Size);

            SwapBuffers();
        }
    }
}