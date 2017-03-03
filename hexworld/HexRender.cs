using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Diamond.Buffers;
using Diamond.Render;
using Diamond.Shaders;
using Diamond.Textures;
using Diamond.Util;
using Newtonsoft.Json.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Buffer = Diamond.Buffers.Buffer;

namespace hexworld
{
    public class HexRender : GameWindow
    {
        #region Fields

        #region GLObjects

        private Program _texPgm;

        private Texture _doorTex;
        private Texture _grassTex;

        private Buffer<ObjVertex> _meshBuffer;
        private Buffer<TileData> _tileBuffer;

        private RenderGroup<TileData, ObjVertex> _renderGroup;

        protected override void OnClosed(EventArgs e)
        {
            _texPgm?.Dispose();

            _doorTex?.Dispose();
            _grassTex?.Dispose();

            _meshBuffer?.Dispose();
            _tileBuffer?.Dispose();
        }

        #endregion

        private VertexBuffer<TileData> _floorTiles;
        private VertexBuffer<TileData> _doorTiles;

        private VertexBuffer<ObjVertex> _cubeMesh;

        private Camera _camera;

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

            var tileVbo = VertexBuffer.FromArrays(allTiles, 0, "tiles");

            _doorTiles = tileVbo[0];
            _floorTiles = tileVbo[1];

            var cubeMesh = json["models"]
                .Select(path => (string) path)
                .Select(path => Path.Combine(dir, path))
                .Select(VertexBuffer.FromWavefront)
                .SelectMany(meshes => meshes)
                .First(mesh => mesh.Name == "Cube");

            _cubeMesh = cubeMesh;

            _camera = new Camera();

            _renderGroup = new RenderGroup<TileData, ObjVertex>()
            {
                Camera = _camera,
                Instance = _floorTiles,
                Program = _texPgm,
                Vertices = _cubeMesh,
                Texture = _grassTex
            };
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            _time += e.Time;

            _camera.View = Matrix4.CreateRotationZ((float) _time / 3) *
                           Matrix4.LookAt(10 * Vector3.One, Vector3.Zero, Vector3.UnitZ);
            _camera.Projection = Matrix4.CreateOrthographic(Width / 100f, Height / 100f, -100, 100);
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

            _renderGroup.Draw();

            SwapBuffers();
        }
    }
}