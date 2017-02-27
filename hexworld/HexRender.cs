using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        #region Fields

        #region GLObjects

        private Program _jsonPgm;
        private Program _objPgm;

        private Texture _grass;
        private Texture _stone;
        private Texture _gray;

        private GLBuffer<Tile> _tileBuffer;
        private GLBuffer<Vertex> _vertexBuffer;
        private GLBuffer<ObjVertex> _objBuffer;

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _jsonPgm?.Dispose();
            _objPgm?.Dispose();

            _tileBuffer?.Dispose();
            _vertexBuffer?.Dispose();
            _objBuffer?.Dispose();

            _grass?.Dispose();
            _stone?.Dispose();
            _gray?.Dispose();
        }

        #endregion

        private Matrix4 _view;
        private Matrix4 _proj;

        private SubArray<Tile> _grassTiles;
        private SubArray<Tile> _stoneTiles;
        private SubArray<Tile> _grayTiles;
        private SubArray<Tile> _tableTiles;

        private Mesh<Vertex> _cubeMesh;
        private Mesh<Vertex> _panelMesh;
        private Mesh<Vertex> _sidesMesh;
        private Mesh<ObjVertex> _objMesh;

        private Tile[] _allTiles;
        private Vertex[] _allVertices;
        private ObjVertex[] _allObjVertices;

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

            using (var vs = Shader.FromFile(@"res\s.vs.glsl", ShaderType.VertexShader))
            using (var fs = Shader.FromFile(@"res\s.fs.glsl", ShaderType.FragmentShader))
            {
                if (!vs.Compiled | !fs.Compiled)
                {
                    Debug.WriteLine("Failed to compile shaders:");
                    Debug.WriteLineIf(!vs.Compiled, $"Vertex Log ({vs.Id}):\n{vs.Log}");
                    Debug.WriteLineIf(!fs.Compiled, $"Fragment Log ({fs.Id}):\n{fs.Log}");
                    Exit();
                    return;
                }

                _jsonPgm = Program.FromShaders(vs, fs);

                if (!_jsonPgm.Link())
                {
                    Debug.WriteLine($"Failed to link program ({_jsonPgm.Id}):\n{_jsonPgm.Log}");
                    Exit();
                    return;
                }
            }

            using (var vs = Shader.FromFile(@"res\obj.vs.glsl", ShaderType.VertexShader))
            using (var fs = Shader.FromFile(@"res\obj.fs.glsl", ShaderType.FragmentShader))
            {
                if (!vs.Compiled | !fs.Compiled)
                {
                    Debug.WriteLine("Failed to compile shaders:");
                    Debug.WriteLineIf(!vs.Compiled, $"Vertex Log ({vs.Id}):\n{vs.Log}");
                    Debug.WriteLineIf(!fs.Compiled, $"Fragment Log ({fs.Id}):\n{fs.Log}");
                    Exit();
                    return;
                }

                _objPgm = Program.FromShaders(vs, fs);

                if (!_objPgm.Link())
                {
                    Debug.WriteLine($"Failed to link program ({_objPgm.Id}):\n{_jsonPgm.Log}");
                    Exit();
                    return;
                }
            }

            _cubeMesh = Mesh.FromJson<Vertex>(@"res\data_vert_cubes.json");
            _panelMesh = Mesh.FromJson<Vertex>(@"res\data_vert_panels.json");
            _sidesMesh = Mesh.FromJson<Vertex>(@"res\data_vert_sides.json");
            _objMesh = Mesh.FromObj(@"res\table.obj");

            _grassTiles = new SubArray<Tile>(
                JsonConvert.DeserializeObject<Tile[]>(File.ReadAllText(@"res\data_tile_grass.json")));
            _stoneTiles = new SubArray<Tile>(
                JsonConvert.DeserializeObject<Tile[]>(File.ReadAllText(@"res\data_tile_stone.json")));
            _grayTiles = new SubArray<Tile>(
                JsonConvert.DeserializeObject<Tile[]>(File.ReadAllText(@"res\data_tile_gray.json")));
            _tableTiles = new SubArray<Tile>(
                JsonConvert.DeserializeObject<Tile[]>(File.ReadAllText(@"res\data_tile_table.json")));


            _allTiles = SubArray.Join(_stoneTiles, _grassTiles, _grayTiles, _tableTiles);
            _tileBuffer = new GLBuffer<Tile>(BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
            _tileBuffer.Data(_allTiles);

            _allVertices = Mesh.Join(_panelMesh, _cubeMesh, _sidesMesh);
            _vertexBuffer = new GLBuffer<Vertex>(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
            _vertexBuffer.Data(_allVertices);

            _allObjVertices = Mesh.Join(_objMesh);
            _objBuffer = new GLBuffer<ObjVertex>(BufferTarget.ArrayBuffer);
            _objBuffer.Data(_allObjVertices);

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

            _tileBuffer.SubData(_grassTiles);

            _tileBuffer.Bind();
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
//            GL.Enable(EnableCap.CullFace);
//            GL.CullFace(CullFaceMode.Back);

            _jsonPgm.Use();

            _jsonPgm.SetAttribPointers(_tileBuffer);
            _jsonPgm.SetAttribPointers(_vertexBuffer);

            _grass.Bind(0);
            _stone.Bind(1);
            _gray.Bind(2);

            GL.Uniform1(_jsonPgm.GetUniform("tex"), 0);
            GL.UniformMatrix4(_jsonPgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(_jsonPgm.GetUniform("proj"), false, ref _proj);

            _cubeMesh.DrawInstanced(_grassTiles);

            GL.Uniform1(_jsonPgm.GetUniform("tex"), 1);

            _panelMesh.DrawInstanced(_stoneTiles);

            GL.Uniform1(_jsonPgm.GetUniform("tex"), 2);

            _sidesMesh.DrawInstanced(_grayTiles);

            _objPgm.Use();

            _objPgm.SetAttribPointers(_tileBuffer);
            _objPgm.SetAttribPointers(_objBuffer);

            GL.Uniform1(_jsonPgm.GetUniform("tex"), 2);
            GL.UniformMatrix4(_jsonPgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(_jsonPgm.GetUniform("proj"), false, ref _proj);

            _objMesh.DrawInstanced(_tableTiles);

            SwapBuffers();
        }
    }
}