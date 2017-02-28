using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
    public class Level
    {
        [JsonProperty("models")]
        private string[] MeshNames { get; set; }

        [JsonProperty("tiles")]
        private TileInfo[] TileInfos { get; set; }

        private TileData[] _allTiles;
        private ObjVertex[] _allVertices;

        private Mesh<ObjVertex>[] _meshes;
        private TileGroup[] _tileGroups;

        private GLBuffer<TileData> _tileBuffer;
        private GLBuffer<ObjVertex> _vertexBuffer;

        private void InitializeBuffers()
        {
            _tileBuffer = new GLBuffer<TileData>(BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
            _tileBuffer.Data(_allTiles);

            _vertexBuffer = new GLBuffer<ObjVertex>(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
            _vertexBuffer.Data(_allVertices);
        }

        public static Level LoadLevel(string file)
        {
            var level = JsonConvert.DeserializeObject<Level>(File.ReadAllText(file));

            var dir = Path.GetDirectoryName(file);


            // region assemble mesh map
            var meshes = new Dictionary<string, Mesh<ObjVertex>>();

            foreach (var meshPath in level.MeshNames)
            {
                var objects = Mesh.FromObj(Path.Combine(dir, meshPath));
                Debug.WriteLine(string.Join("\n", objects.Select(o => o.Name)));
                foreach (var mesh in objects)
                {
                    meshes[mesh.Name] = mesh;
                }
            }

            // region store all used meshes
            level._meshes = meshes.Values.ToArray();
            // join meshes
            level._allVertices = Mesh.Join(level._meshes);
            Debug.WriteLine(level._allVertices.Length);
            Debug.WriteLine(level._meshes[1].Vertices.Length);
            Debug.WriteLine(level._meshes[1].Vertices.Offset);

            var groupDict = new Dictionary<string, List<TileData>>();

            foreach (var tileInfo in level.TileInfos)
            {
                var meshName = tileInfo.Mesh;
                if (!groupDict.ContainsKey(meshName))
                    groupDict[meshName] = new List<TileData>();
                groupDict[meshName].Add(tileInfo.TileData);
            }

            var groupList = new List<TileGroup>();
            var tileSubArrayList = new List<SubArray<TileData>>();

            foreach (var kvp in groupDict)
            {
                var sa = new SubArray<TileData>(kvp.Value.ToArray());
                groupList.Add(new TileGroup(sa, meshes[kvp.Key]));
                tileSubArrayList.Add(sa);
            }

            level._tileGroups = groupList.ToArray();

            level._allTiles = SubArray.Join(tileSubArrayList);

            level.InitializeBuffers();

            return level;
        }

        public void Draw()
        {
            if (Program.Current == null)
                throw new Exception("cant render without a shader.");

            Program.Current.SetAttribPointers(_vertexBuffer);
            Program.Current.SetAttribPointers(_tileBuffer);

            foreach (var tileGroup in _tileGroups)
            {
                tileGroup.Mesh.DrawInstanced(tileGroup.Tiles);
            }
        }
    }

    public class TileInfo
    {
        [JsonProperty("mesh")]
        public string Mesh { get; set; }

        [JsonProperty("pos")]
        public Vector3 Position { get; set; }

        public TileData TileData => new TileData(Position);

        public override string ToString()
        {
            return $"Mesh: {Mesh}, Position: {Position}";
        }
    }

    public class TileGroup
    {
        public SubArray<TileData> Tiles;
        public Mesh<ObjVertex> Mesh;

        public TileGroup(SubArray<TileData> tiles, Mesh<ObjVertex> mesh)
        {
            Tiles = tiles;
            Mesh = mesh;
        }
    }

    public class HexRender : GameWindow
    {
        #region Fields

        #region GLObjects

        private Program _objPgm;

        private Texture _grass;
        private Texture _stone;
        private Texture _gray;

        private Level _level;

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _objPgm?.Dispose();

            _grass?.Dispose();
            _stone?.Dispose();
            _gray?.Dispose();

            // _level?.Dispose();
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

            _objPgm = Program.FromFiles(@"res\obj.vs.glsl", @"res\obj.fs.glsl");

            _level = Level.LoadLevel(@"res\level.json");

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

            if (_objPgm.Linked)
            {
                _objPgm.Use();

                _grass.Bind(0);
                _stone.Bind(1);
                _gray.Bind(2);

                GL.Uniform1(_objPgm.GetUniform("tex"), 2);
                GL.UniformMatrix4(_objPgm.GetUniform("view"), false, ref _view);
                GL.UniformMatrix4(_objPgm.GetUniform("proj"), false, ref _proj);

                _level.Draw();
            }

            SwapBuffers();
        }
    }
}