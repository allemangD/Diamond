using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Diamond.Buffers;
using Diamond.Shaders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Level
{
    public class Level
    {
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
            var levelData = JObject.Parse(File.ReadAllText(file));

            var dir = Path.GetDirectoryName(file);

            // this is horrendous, but not as bad as trying to directly deserialize it.
            
            var meshes = levelData["models"]
                .Select(path => Mesh.FromObj(Path.Combine(dir, (string) path)))
                .SelectMany(objects => objects)
                .ToArray();

            var meshDict = meshes
                .ToDictionary(mesh => mesh.Name, mesh => mesh);

            var allVertices = Mesh.Join(meshes);

            var tilegroups = levelData["tiles"]
                .Select(tile => new
                {
                    info = new
                    {
                        mesh = meshDict[(string) tile["mesh"]]
                    },
                    pos = tile["pos"].ToObject<Vector3>()
                })
                .GroupBy(tile => tile.info)
                .Select(group => new TileGroup(new SubArray<TileData>(
                        group.Select(data => new TileData(data.pos))
                            .ToArray()),
                    group.Key.mesh))
                .ToArray();

            var tileArrays = tilegroups
                .Select(group => group.Tiles);

            var allTiles = SubArray.Join(tileArrays);

            var level = new Level
            {
                _allTiles = allTiles,
                _allVertices = allVertices,
                _meshes = meshes,
                _tileGroups = tilegroups
            };

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