using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Diamond.Buffers;
using Diamond.Shaders;
using Newtonsoft.Json;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Level
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
}