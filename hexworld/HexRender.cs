using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Diamond;
using Diamond.Buffers;
using Diamond.Level;
using Diamond.Shaders;
using Diamond.Textures;
using Newtonsoft.Json.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace hexworld
{
    public class HexRender : GameWindow
    {
        #region Fields

        #region GLObjects

        private Program _texPgm;

        private Texture _doorTex;
        private Texture _grassTex;

        private GLBuffer<ObjVertex> _meshBuffer;
        private GLBuffer<TileData> _tileBuffer;

        protected override void OnClosed(EventArgs e)
        {
            _texPgm?.Dispose();

            _doorTex?.Dispose();
            _grassTex?.Dispose();

            _meshBuffer?.Dispose();
            _tileBuffer?.Dispose();
        }

        #endregion

        private SubArray<TileData> _floorTiles;
        private SubArray<TileData> _doorTiles;

        private Mesh<ObjVertex> _cubeMesh;

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

            _texPgm = Program.FromFiles("res/obj.vs.glsl", "res/obj.fs.glsl");

            _doorTex = Texture.FromFile("res/door.png");
            _grassTex = Texture.FromFile("res/grass.png");

            _meshBuffer = new GLBuffer<ObjVertex>(BufferTarget.ArrayBuffer);
            _tileBuffer = new GLBuffer<TileData>(BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);

            var dir = "res";

            var json = JObject.Parse(File.ReadAllText("res/level.json"));

            var allTiles = json["tiles"]
                .GroupBy(ti => ti["tex"])
                .Select(g => g
                    .Select(ti => ti["pos"])
                    .Select(pos => pos.ToObject<Vector3>())
                    .Select(pos => new TileData(pos))
                    .ToArray())
                .Select(arr => new SubArray<TileData>(arr))
                .ToArray();

            _doorTiles = allTiles[0];
            _floorTiles = allTiles[1];

            _tileBuffer.Data(SubArray.Join(_doorTiles, _floorTiles));

            var cubeMesh = json["models"]
                .Select(path => (string) path)
                .Select(path => Path.Combine(dir, path))
                .Select(path => Mesh.FromObj(path, false))
                .SelectMany(meshes => meshes)
                .First(mesh => mesh.Name == "Cube");

            _cubeMesh = cubeMesh;

            _meshBuffer.Data(cubeMesh.Vertices.ToArray());

            // meshes, tiles, etc

            // can still use JObject queries to keep it short,
            // but don't use Level. hat structure is broken
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

            _texPgm.Use();

            _texPgm.SetAttribPointers(_meshBuffer);
            _texPgm.SetAttribPointers(_tileBuffer);

            _grassTex.Bind(0);

            GL.Uniform1(_texPgm.GetUniform("tex"), 0);
            GL.UniformMatrix4(_texPgm.GetUniform("view"), false, ref _view);
            GL.UniformMatrix4(_texPgm.GetUniform("proj"), false, ref _proj);

            _cubeMesh.DrawInstanced(_floorTiles);

            SwapBuffers();
        }
    }
}