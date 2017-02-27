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
        #region Fields

        #region GLObjects

        private Program _pgm;

        private Texture _grass;
        private Texture _stone;
        private Texture _gray;

        private GLBuffer<Tile> _tileGLBuffer;
        private GLBuffer<Vertex> _vertexGLBuffer;

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

        #endregion

        private Matrix4 _view;
        private Matrix4 _proj;

        private SubArray<Tile> _grassTiles;
        private SubArray<Tile> _stoneTiles;
        private SubArray<Tile> _grayTiles;

        private Mesh<Vertex> _cubeMesh;
        private Mesh<Vertex> _panelMesh;
        private Mesh<Vertex> _sidesMesh;

        private Tile[] _allTiles;
        private Vertex[] _allVertices;

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

            var vsPath = @"res\s.vs.glsl";
            var fsPath = @"res\s.fs.glsl";

            using (var vs = Shader.FromFile(vsPath, ShaderType.VertexShader))
            using (var fs = Shader.FromFile(fsPath, ShaderType.FragmentShader))
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

            _cubeMesh = Mesh.FromJson<Vertex>(File.ReadAllText(@"res\data_vert_cubes.json"));
            _panelMesh = Mesh.FromJson<Vertex>(File.ReadAllText(@"res\data_vert_panels.json"));
            _sidesMesh = Mesh.FromJson<Vertex>(File.ReadAllText(@"res\data_vert_sides.json"));

            _grassTiles = new SubArray<Tile>(
                JsonConvert.DeserializeObject<Tile[]>(File.ReadAllText(@"res\data_tile_grass.json")));
            _stoneTiles = new SubArray<Tile>(
                JsonConvert.DeserializeObject<Tile[]>(File.ReadAllText(@"res\data_tile_stone.json")));
            _grayTiles = new SubArray<Tile>(
                JsonConvert.DeserializeObject<Tile[]>(File.ReadAllText(@"res\data_tile_gray.json")));

            _allTiles = SubArray.Join(_stoneTiles, _grassTiles, _grayTiles);
            _allVertices = Mesh.Join(_panelMesh, _cubeMesh, _sidesMesh);

            _tileGLBuffer = new GLBuffer<Tile>(BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
            _tileGLBuffer.Data(_allTiles);

            _vertexGLBuffer = new GLBuffer<Vertex>(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
            _vertexGLBuffer.Data(_allVertices);

            _pgm.SetAttribPointers(_tileGLBuffer);
            _pgm.SetAttribPointers(_vertexGLBuffer);

            _grass = Texture.FromBitmap(new Bitmap(@"res\grass.png"));
            _stone = Texture.FromBitmap(new Bitmap(@"res\stone.png"));
            _gray = Texture.FromBitmap(new Bitmap(@"res\gray.png"));
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            _time += e.Time;

            _view = Matrix4.CreateRotationZ((float) _time / 3) *
                    Matrix4.LookAt(10 * Vector3.One, Vector3.Zero, Vector3.UnitZ);
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

            _grass.Bind(0);
            _stone.Bind(1);
            _gray.Bind(2);

            GL.Uniform1(_pgm.GetUniform("tex"), 0);
            GL.UniformMatrix4(_pgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(_pgm.GetUniform("proj"), false, ref _proj);

            _cubeMesh.DrawInstanced(_grassTiles);

            GL.Uniform1(_pgm.GetUniform("tex"), 1);
            GL.UniformMatrix4(_pgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(_pgm.GetUniform("proj"), false, ref _proj);

            _panelMesh.DrawInstanced(_stoneTiles);

            GL.Uniform1(_pgm.GetUniform("tex"), 2);
            GL.UniformMatrix4(_pgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(_pgm.GetUniform("proj"), false, ref _proj);

            _sidesMesh.DrawInstanced(_grayTiles);

            SwapBuffers();
        }
    }
}