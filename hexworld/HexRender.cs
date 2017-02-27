using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        private Program _pgm;

        private Texture _grass;
        private Texture _stone;
        private Texture _gray;

        private Matrix4 _view;
        private Matrix4 _proj;

        private SubArray<Tile> _grassTiles;
        private SubArray<Tile> _stoneTiles;
        private SubArray<Tile> _grayTiles;

        private SubArray<Vertex> _cubeVertices;
        private SubArray<Vertex> _panelVertices;
        private SubArray<Vertex> _sidesVertices;

        private Tile[] _allTiles;
        private Vertex[] _allVertices;

        private GLBuffer _tileGLBuffer;
        private GLBuffer _vertexGLBuffer;

        private double _time;

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

            _cubeVertices = new SubArray<Vertex>(
                JsonConvert.DeserializeObject<Vertex[]>(File.ReadAllText("data_vert_cubes.json")));
            _panelVertices = new SubArray<Vertex>(
                JsonConvert.DeserializeObject<Vertex[]>(File.ReadAllText("data_vert_panels.json")));
            _sidesVertices = new SubArray<Vertex>(
                JsonConvert.DeserializeObject<Vertex[]>(File.ReadAllText("data_vert_sides.json")));

            _grassTiles = new SubArray<Tile>(
                JsonConvert.DeserializeObject<Tile[]>(File.ReadAllText("data_tile_grass.json")));
            _stoneTiles = new SubArray<Tile>(
                JsonConvert.DeserializeObject<Tile[]>(File.ReadAllText("data_tile_stone.json")));
            _grayTiles = new SubArray<Tile>(
                JsonConvert.DeserializeObject<Tile[]>(File.ReadAllText("data_tile_gray.json")));

            _allTiles = SubArray<Tile>.Join(_stoneTiles, _grassTiles, _grayTiles);
            _allVertices = SubArray<Vertex>.Join(_panelVertices, _cubeVertices, _sidesVertices);

            _tileGLBuffer = new GLBuffer(BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
            _tileGLBuffer.Data(_allTiles);

            _vertexGLBuffer = new GLBuffer(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
            _vertexGLBuffer.Data(_allVertices);

            _pgm.SetAttribPointers(_tileGLBuffer, typeof(Tile));
            _pgm.SetAttribPointers(_vertexGLBuffer, typeof(Vertex));

            _grass = Texture.FromBitmap(new Bitmap("grass.png"));
            _stone = Texture.FromBitmap(new Bitmap("stone.png"));
            _gray = Texture.FromBitmap(new Bitmap("gray.png"));
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            _time += e.Time;

            _view = Matrix4.CreateRotationZ((float) _time/3)*Matrix4.LookAt(10 * Vector3.One, Vector3.Zero, Vector3.UnitZ);
            _proj = Matrix4.CreateOrthographic(Width / 100f, Height / 100f, -100, 100);

            for (var i = 0; i < _grassTiles.Length; i++)
            {
                var ti = _grassTiles[i];
                _grassTiles[i].Position.Z =
                    (float) (Math.Sin((_time + ti.Position.X - ti.Position.Y / 1.5) / 1.5) * .25);
            }

            _tileGLBuffer.SubData(_grassTiles);

            _tileGLBuffer.Bind();
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr) (5 * 3 * sizeof(float)),
                (IntPtr) (16 * 3 * sizeof(float)), _grassTiles.ToArray());
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
            _gray.Bind(2);

            GL.Uniform1(_pgm.GetUniform("tex"), 0);
            GL.UniformMatrix4(_pgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(_pgm.GetUniform("proj"), false, ref _proj);

            GL.DrawArraysInstancedBaseInstance(PrimitiveType.Triangles,
                _cubeVertices.Offset, _cubeVertices.Length,
                _grassTiles.Length, _grassTiles.Offset);

            GL.Uniform1(_pgm.GetUniform("tex"), 1);
            GL.UniformMatrix4(_pgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(_pgm.GetUniform("proj"), false, ref _proj);

            GL.DrawArraysInstancedBaseInstance(PrimitiveType.Triangles,
                _panelVertices.Offset, _panelVertices.Length,
                _stoneTiles.Length, _stoneTiles.Offset);

            GL.Uniform1(_pgm.GetUniform("tex"), 2);
            GL.UniformMatrix4(_pgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(_pgm.GetUniform("proj"), false, ref _proj);

            GL.DrawArraysInstancedBaseInstance(PrimitiveType.Triangles,
                _sidesVertices.Offset, _sidesVertices.Length,
                _grayTiles.Length, _grayTiles.Offset);

            _pgm.DisableAllAttribArrays();

            SwapBuffers();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _pgm.Dispose();

            _tileGLBuffer.Dispose();
            _vertexGLBuffer.Dispose();

            _grass.Dispose();
            _stone.Dispose();
            _gray.Dispose();
        }
    }
}