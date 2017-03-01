using System.Collections.Generic;
using System.IO;
using Diamond.Wrappers;
using NLog;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Shaders
{
    public class Shader : GLObject
    {
        private readonly ShaderWrap _shader;
        internal override Wrapper Wrapper => _shader;

        public string Source { get; }
        public ShaderType Type { get; }

        internal Shader(ShaderWrap shader, string source, ShaderType type, string name)
        {
            _shader = shader;
            Source = source;
            Type = type;
            Name = name;
        }

        public override string ToString() => $"{Type} \'{Name}\' ({Id})";

        #region Factory Methods

        private static readonly Dictionary<string, ShaderType> Extensions = new Dictionary<string, ShaderType>
        {
            [".vs"] = ShaderType.VertexShader,
            [".vert"] = ShaderType.VertexShader,
            [".fs"] = ShaderType.FragmentShader,
            [".frag"] = ShaderType.FragmentShader,
        };
        
        public static Shader FromSource(string source, ShaderType type, string name = "Shader")
        {
            var wrapper = new ShaderWrap(type);
            var service = new Shader(wrapper, source, type, name);

            Logger.Debug("Created {0}", service);

            wrapper.Source = source;
            wrapper.Compile();

            if (!wrapper.Compiled)
            {
                Logger.Warn("Failed to compile {0}", service);
                Logger.Debug("InfoLog for {0}", service);
                wrapper.Dispose();
                return null;
            }

            Logger.Debug("Successfully compiled {0}", service);

            return service;
        }

        public static Shader FromFile(string path, ShaderType type)
        {
            if (!File.Exists(path))
            {
                Logger.Warn("Could not find glsl file {0}", path);
                return null;
            }

            var name = Path.GetFileNameWithoutExtension(path);
            return FromSource(File.ReadAllText(path), type, name);
        }

        public static Shader FromFile(string path)
        {
            if (!File.Exists(path))
            {
                Logger.Warn("Could not find glsl file {0}", path);
                return null;
            }

            var ext = Path.GetExtension(path);
            var name = Path.GetFileNameWithoutExtension(path);

            if (ext != null)
                if (!Extensions.ContainsKey(ext))
                    ext = Path.GetExtension(name);

            if (ext == null || !Extensions.ContainsKey(ext))
            {
                Logger.Warn("Could not infer shader type from glsl file name {0}", path);
                return null;
            }

            var type = Extensions[ext];

            return FromFile(path, type);
        }

        #endregion
    }
}