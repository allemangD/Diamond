using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Shaders
{
    internal sealed class ShaderWrapper : GLWrapper
    {
        internal ShaderWrapper(ShaderType shaderType)
        {
            Id = GL.CreateShader(shaderType);
            ShaderType = shaderType;
        }

        public readonly ShaderType ShaderType;

        public string Source
        {
            get
            {
                var sb = new StringBuilder(1024);
                GL.GetShaderSource(Id, sb.Capacity, out int length, sb);
                return sb.ToString();
            }
            set { GL.ShaderSource(Id, value); }
        }

        public bool Compiled
        {
            get
            {
                GL.GetShader(Id, ShaderParameter.CompileStatus, out int res);
                return res != 0;
            }
        }

        public string InfoLog => GL.GetShaderInfoLog(Id).Trim();

        public void Compile() => GL.CompileShader(Id);

        public override void GLDelete() => GL.DeleteShader(Id);
    }

    public class Shader : GLObject
    {
        internal static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ShaderWrapper _shader;
        internal override GLWrapper Wrapper => _shader;

        public string Source { get; }
        public ShaderType Type { get; }

        internal Shader(ShaderWrapper shader, string source, ShaderType type, string name)
        {
            _shader = shader;
            Source = source;
            Type = type;
            Name = name;
        }

        public override string ToString() => $"{Type} \'{Name}\' ({Id})";

        #region Factory Methods

        private static readonly Dictionary<string, ShaderType> _extensions = new Dictionary<string, ShaderType>
        {
            [".vs"] = ShaderType.VertexShader,
            [".vert"] = ShaderType.VertexShader,
            [".fs"] = ShaderType.VertexShader,
            [".frag"] = ShaderType.VertexShader,
        };

        public static Shader FromSource(string source, ShaderType type, string name = "Shader")
        {
            var wrapper = new ShaderWrapper(type);
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
                if (!_extensions.ContainsKey(ext))
                    ext = Path.GetExtension(name);

            if (ext == null || !_extensions.ContainsKey(ext))
            {
                Logger.Warn("Could not infer shader type from glsl file name {0}", path);
                return null;
            }

            var type = _extensions[ext];
            return FromFile(path, type);
        }

        #endregion
    }
}