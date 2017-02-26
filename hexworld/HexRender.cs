using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diamond;
using Diamond.Buffers;
using Diamond.Shaders;
using Diamond.Textures;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace hexworld
{
    public class HexRender : GameWindow
    {
        private readonly VertexData[] _cubeVerts =
            JsonConvert.DeserializeObject<VertexData[]>(File.ReadAllText("cube.json"));

        private readonly TileData[] _tilesData =
            JsonConvert.DeserializeObject<TileData[]>(File.ReadAllText("tiles.json"));

        private Program _pgm;

        private Texture _grass;
        private Texture _stone;

        private Matrix4 _view;
        private Matrix4 _proj;

        private VBO<TileData> _tileVbo;
        private VBO<VertexData> _cubeVbo;

        private double _time;

        public HexRender(int width, int height)
            : base(width, height, new GraphicsMode(32, 24, 0, 0))
        {
            Width = width;
            Height = Height;
            X = (DisplayDevice.Default.Width - Width) / 2;
            Y = (DisplayDevice.Default.Height - Height) / 2;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            _time += e.Time;

            _view = Matrix4.LookAt(10 * Vector3.One, Vector3.Zero, Vector3.UnitZ);
            _proj = Matrix4.CreateOrthographic(Width / 100f, Height / 100f, -100, 100);

            // wavy blocks around perimeter
            for (var i = 0; i < 16; i++)
            {
                var ti = _tilesData[i];
                _tilesData[i].Position.Z = (float) (Math.Sin((_time + ti.Position.X - ti.Position.Y / 1.5) / 1.5) * .25);
            }

            _tileVbo.Bind();
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr) (0), (IntPtr) (16 * 3 * sizeof(float)), _tilesData);
            VBO.Unbind();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (var vs = Shader.FromFile("s.vs.glsl", ShaderType.VertexShader))
            using (var fs = Shader.FromFile("s.fs.glsl", ShaderType.FragmentShader))
            {
                if (!vs.Compiled | !fs.Compiled)
                {
                    Debug.WriteLine("Failed to compile shaders:");
                    Debug.WriteLineIf(!vs.Compiled, $"Vertex Log:\n{vs.Log}");
                    Debug.WriteLineIf(!fs.Compiled, $"Fragment Log:\n{fs.Log}");
                    Exit();
                    return;
                }

                _pgm = Program.FromShaders(vs, fs);

                if (!_pgm.Link())
                {
                    Debug.WriteLine($"Failed to link program:\n{_pgm.Log}");
                    Exit();
                    return;
                }
            }

            _cubeVbo = new VBO<VertexData>();
            _cubeVbo.Data(_cubeVerts, BufferUsageHint.StaticDraw);
            _cubeVbo.AttribPointers(_pgm);

            _tileVbo = new VBO<TileData>();
            _tileVbo.Data(_tilesData, BufferUsageHint.DynamicDraw);
            _tileVbo.AttribPointers(_pgm);

            _grass = Texture.FromBitmap(new Bitmap("grass.png"));
            _stone = Texture.FromBitmap(new Bitmap("stone.png"));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _pgm.Dispose();

            _tileVbo.Dispose();
            _cubeVbo.Dispose();

            _grass.Dispose();
            _stone.Dispose();
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