using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Diamond.Buffers;
using Diamond.Shaders;
using Diamond.Textures;
using Diamond.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Level
{
    public class Level : IDisposable
    {
        public Dictionary<string, Program> Programs { get; private set; }

        private TileData[] _allTiles;
        private ObjVertex[] _allVertices;

        private Mesh<ObjVertex>[] _meshes;
        private TileGroup[] _tileGroups;

        private Texture[] _textures;

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
                .Select(path => Mesh.FromObj(Path.Combine(dir, (string) path), false))
                .SelectMany(objects => objects)
                .ToArray();

            var meshDict = meshes
                .ToDictionary(mesh => mesh.Name, mesh => mesh);

            var allVertices = Mesh.Join(meshes);

            var programs = levelData["shaders"]
                .Select(shader => new
                {
                    name = (string) shader["name"],
                    program = Program.FromFiles(
                        shader["files"]
                            .Select(path => Path.Combine(dir, (string) path))
                            .ToArray())
                })
                .ToDictionary(s => s.name, s => s.program);

            var texturePaths = levelData["textures"]
                .Select(path => (string) path)
                .ToArray();
            var textures = texturePaths.Select(path => Texture.FromBitmap(new Bitmap(Path.Combine(dir, path)))).ToArray();
            var textureMap = texturePaths.Select((path, i) => new {path = path, i = i})
                .ToDictionary(v => v.path, v => v.i);

            var tilegroups = levelData["tiles"]
                .Select(tile => new
                {
                    info = new
                    {
                        mesh = meshDict[(string) tile["mesh"]],
                        shader = programs[(string) tile["shader"]],
                        texture = textureMap[(string) tile["tex"]]
                    },
                    pos = tile["pos"].ToObject<Vector3>()
                })
                .GroupBy(tile => tile.info)
                .Select(group => new TileGroup(group.Key.mesh, group.Key.shader, group.Key.texture,
                    new SubArray<TileData>(
                        group.Select(data => new TileData(data.pos))
                            .ToArray())))
                .ToArray();

            var tileArrays = tilegroups
                .Select(group => group.Tiles);

            var allTiles = SubArray.Join(tileArrays);

            var level = new Level
            {
                _allTiles = allTiles,
                _allVertices = allVertices,
                _meshes = meshes,
                _tileGroups = tilegroups,
                Programs = programs,
                _textures = textures
            };

            level.InitializeBuffers();

            return level;
        }

        public void Draw()
        {
            for (var i = 0; i < _textures.Length; i++)
            {
                var texture = _textures[i];
                texture.Bind(i);
            }

            foreach (var tileGroup in _tileGroups)
            {
                var pgm = tileGroup.Program;
                pgm.Use();
                GL.Uniform1(pgm.GetUniform("tex"), tileGroup.Texture);
                pgm.SetAttribPointers(_vertexBuffer);
                pgm.SetAttribPointers(_tileBuffer);
                tileGroup.Mesh.DrawInstanced(tileGroup.Tiles);
            }
        }

        public void Dispose()
        {
            _tileBuffer?.Dispose();
            _vertexBuffer?.Dispose();

            foreach (var texture in _textures)
                texture?.Dispose();

            foreach (var program in Programs.Values)
                program?.Dispose();

            GC.SuppressFinalize(this);
        }

        ~Level()
        {
            Dispose();
        }
    }
}