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

        internal ShaderWrapper(ShaderType shaderType)
        {
            Id = GL.CreateShader(shaderType);
            ShaderType = shaderType;
        }

        public override void GLDelete()
        {
            GL.DeleteShader(Id);
        }

        public void Compile()
        {
            GL.CompileShader(Id);
        }
    }

    public class Shader : GLObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ShaderWrapper _shader;

        public override int Id => _shader.Id;
        protected override GLWrapper Wrapper => _shader;

        public string Name { get; }
        public string Source { get; }

        private Shader(ShaderWrapper shader, string source, string name)
        {
            _shader = shader;
            Name = name;
            Source = source;
        }

        #region Factory Methods

        public static Shader FromSource(string source, ShaderType type, string name = "Shader")
        {
            var wrapper = new ShaderWrapper(type);
            Logger.Debug("Created {0} \'{1}\' {2}", type, name, wrapper.Id);

            wrapper.Source = source;
            wrapper.Compile();

            if (!wrapper.Compiled)
            {
                Logger.Warn("Failed to compile {0} \'{1}\' {2}", type, name, wrapper.Id);
                Logger.Debug("InfoLog for {0} \'{1}\' {2}", type, name, wrapper.Id);
                wrapper.Dispose();
                return null;
            }

            Logger.Debug("Successfully compiled {0} \'{1}\' {2}", type, name, wrapper.Id);
            return new Shader(wrapper, source, name);
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

            var extensions = new Dictionary<string, ShaderType>
            {
                [".vs"] = ShaderType.VertexShader,
                [".vert"] = ShaderType.VertexShader,
                [".fs"] = ShaderType.VertexShader,
                [".frag"] = ShaderType.VertexShader,
            };

            if (ext != null)
                if (!extensions.ContainsKey(ext))
                    ext = Path.GetExtension(name);

            if (ext == null || !extensions.ContainsKey(ext))
            {
                Logger.Warn("Could not infer shader type from glsl file name {0}", path);
                return null;
            }

            var type = extensions[ext];
            return FromFile(path, type);
        }

        #endregion
    }
}