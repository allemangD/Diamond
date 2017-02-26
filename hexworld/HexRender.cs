using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using hexworld.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace hexworld
{
    public partial class HexRender : GameWindow
    {
        private Program _pgm;

        // todo: generate texture atlas
        // or at least embed sub-uvs and materials into json
        private Texture _grass;

        private Texture _stone;

        private Matrix4 _view;
        private Matrix4 _proj;

        private VBO<Tile> _tileVbo;
        private VBO<Vertex> _cubeVbo;

        public HexRender(int width, int height)
            : base(width, height, new GraphicsMode(32, 24, 0, 0))
        {
            Width = width;
            Height = Height;
            X = (DisplayDevice.Default.Width - Width) / 2;
            Y = (DisplayDevice.Default.Height - Height) / 2;
        }

        private double _t;

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            _t += e.Time;

            _view = Matrix4.LookAt(10 * Vector3.One, Vector3.Zero, Vector3.UnitZ);
            _proj = Matrix4.CreateOrthographic(Width / 100f, Height / 100f, -100, 100);

            for (var i = 0; i < 16; i++)
            {
                var ti = tiles[i];
                tiles[i].Position.Z = (float) (Math.Sin((_t + ti.Position.X - ti.Position.Y / 1.5) / 1.5) * .25);
            }

            _tileVbo.Bind();
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr) (0), (IntPtr) (16 * 3 * sizeof(float)), tiles);
            VBO.Unbind();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _cubeVbo = new VBO<Vertex>();
            _cubeVbo.Data(cubeVerts, BufferUsageHint.StaticDraw);

            _tileVbo = new VBO<Tile>();
            _tileVbo.Data(tiles, BufferUsageHint.DynamicDraw);

            using (var vs = Shader.FromFile("s.vs.glsl", ShaderType.VertexShader))
            using (var fs = Shader.FromFile("s.fs.glsl", ShaderType.FragmentShader))
            {
                _pgm = Program.FromShaders(vs, fs);

                if (!vs.Compiled | !fs.Compiled)
                {
                    Console.Out.WriteLine("Failed to compile shaders:");
                    if (!vs.Compiled)
                        Console.Out.WriteLine("Vertex Shader:\n" + vs.Log.Trim());
                    if (!fs.Compiled)
                        Console.Out.WriteLine("Fragment Shader:\n" + fs.Log.Trim());
                    Console.Out.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                    Exit();
                    return;
                }
            }

            if (!_pgm.LinkStatus)
            {
                Console.Out.WriteLine("Failed to link program:");
                Console.Out.WriteLine(_pgm.Log.Trim());
                Console.Out.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Exit();
                return;
            }

            _cubeVbo.AttribPointers(_pgm);
            _tileVbo.AttribPointers(_pgm);

            _grass = Texture.FromBitmap(new Bitmap("grass.png"));
            _stone = Texture.FromBitmap(new Bitmap("stone.png"));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _pgm.Dispose();

            _grass.Dispose();
            _stone.Dispose();

            _tileVbo.Dispose();
            _cubeVbo.Dispose();
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
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            _pgm.Use();
            _pgm.EnableAllAttribArrays();

            _grass.Bind(0);
            _stone.Bind(1);

            GL.Uniform1(_pgm.GetUniform("tex"), 0);
            GL.UniformMatrix4(_pgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(_pgm.GetUniform("proj"), false, ref _proj);

            GL.DrawArraysInstancedBaseInstance(PrimitiveType.Triangles, 0, 36, 16, 0);

            GL.Uniform1(_pgm.GetUniform("tex"), 1);
            GL.UniformMatrix4(_pgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(_pgm.GetUniform("proj"), false, ref _proj);

            GL.DrawArraysInstancedBaseInstance(PrimitiveType.Triangles, 0, 36, 5, 16);

            _pgm.DisableAllAttribArrays();

            SwapBuffers();
        }
    }
}