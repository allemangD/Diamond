using System;
using System.Drawing;
using Diamond.Level;
using Diamond.Shaders;
using Diamond.Textures;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace hexworld
{
    public class HexRender : GameWindow
    {
        #region Fields

        #region GLObjects

        private Texture _grass;
        private Texture _stone;
        private Texture _gray;
        private Texture _door;

        private Level _level;

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _grass?.Dispose();
            _stone?.Dispose();
            _gray?.Dispose();
            _door?.Dispose();

            _level?.Dispose();
        }

        #endregion

        private Matrix4 _view;
        private Matrix4 _proj;

        private double _time;

        #endregion


        public HexRender(int width, int height)
            : base(width, height, new GraphicsMode(32, 24, 0, 0))
        {
            Width = width;
            Height = Height;
            X = (DisplayDevice.Default.Width - Width) / 2;
            Y = (DisplayDevice.Default.Height - Height) / 2;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _level = Level.LoadLevel(@"res\level.json");

            _grass = Texture.FromBitmap(new Bitmap(@"res\grass.png"));
            _stone = Texture.FromBitmap(new Bitmap(@"res\stone.png"));
            _gray = Texture.FromBitmap(new Bitmap(@"res\gray.png"));
            _door = Texture.FromBitmap(new Bitmap(@"res\door.png"));
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            _time += e.Time;

            _view = Matrix4.CreateRotationZ((float) _time / 3) *
                    Matrix4.LookAt(10 * Vector3.One, Vector3.Zero, Vector3.UnitZ);
            _proj = Matrix4.CreateOrthographic(Width / 100f, Height / 100f, -100, 100);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Viewport(ClientRectangle);

            GL.ClearColor(0.2392157F, 0.5607843F, 0.9960784F, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            _grass.Bind(0);
            _stone.Bind(1);
            _gray.Bind(2);
            _door.Bind(3);

            var pgm = _level.Programs["textured"];
            pgm.Use();
            GL.Uniform1(pgm.GetUniform("tex"), 0);
            GL.UniformMatrix4(pgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(pgm.GetUniform("proj"), false, ref _proj);
            
            _level.Draw();

            SwapBuffers();
        }
    }
}