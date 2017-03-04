using System.Collections.Generic;
using System.IO;
using Diamond.Wrappers;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Shaders
{
    /// <summary>
    /// Manges a OpenGL Shader object
    /// </summary>
    public class Shader : GLObject
    {
        internal readonly ShaderWrap Wrapper;

        /// <summary>
        /// The source used to create this shader
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// The type of this shader
        /// </summary>
        public ShaderType Type { get; }

        internal Shader(ShaderWrap wrapper, string source, ShaderType type, string name)
        {
            Wrapper = wrapper;
            Source = source;
            Type = type;
            Name = name;
        }

        public override string ToString() => Name == null
            ? $"{Wrapper}"
            : $"{Wrapper} \'{Name}\'";

        public override void Dispose()
        {
            Logger.Debug("Disposing {0}", this);
            Wrapper.Dispose();
        }

        #region Factory Methods

        // Used to infer shader type based on file extension
        private static readonly Dictionary<string, ShaderType> Extensions = new Dictionary<string, ShaderType>
        {
            [".vs"] = ShaderType.VertexShader,
            [".vert"] = ShaderType.VertexShader,
            [".fs"] = ShaderType.FragmentShader,
            [".frag"] = ShaderType.FragmentShader,
            [".gs"] = ShaderType.GeometryShader,
            [".geom"] = ShaderType.GeometryShader,
        };

        /// <summary>
        /// Create and compile a shader from glsl source code
        /// </summary>
        /// <param name="source">The glsl source</param>
        /// <param name="type">The type of shader to create</param>
        /// <param name="name">The name of this GLObject</param>
        /// <returns>The compiled Shader, or null if initialization failed</returns>
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
                service.Dispose();
                return null;
            }

            Logger.Debug("Successfully compiled {0}", service);

            return service;
        }

        /// <summary>
        /// Create and compile a shader from a glsl source file
        /// </summary>
        /// <param name="path">The path to the glsl source file</param>
        /// <param name="type">The type of the shader to create</param>
        /// <param name="name">The name of this GLObject</param>
        /// <returns></returns>
        public static Shader FromFile(string path, ShaderType type, string name = null)
        {
            if (!File.Exists(path))
            {
                Logger.Warn("Could not find glsl file {0}", path);
                return null;
            }

            if (name == null)
                name = Path.GetFileNameWithoutExtension(path);

            return FromSource(File.ReadAllText(path), type, name);
        }

        /// <summary>
        /// Create and compile a shader from a glsl source file. Shader type is inferred from file extension.
        /// Extension must be .vs, .vert, .fs, .frag, .gs, or .geom. This can optionally be followed by .glsl or .txt,
        /// but the shader type extension must be present.
        /// </summary>
        /// <param name="path">The path to the glsl source file</param>
        /// <param name="name">The name of this GLObject</param>
        /// <returns>The compiled shader, or null if initialization failed or shader type cannot be inferred</returns>
        public static Shader FromFile(string path, string name = null)
        {
            if (!File.Exists(path))
            {
                Logger.Warn("Could not find glsl file {0}", path);
                return null;
            }

            var ext = Path.GetExtension(path);
            var fileName = Path.GetFileNameWithoutExtension(path);

            // get sub-extension if real extension is not valid
            if (ext != null)
                if (!Extensions.ContainsKey(ext))
                    ext = Path.GetExtension(fileName);

            // if no extension, no sub-extension, or invalid sub-extension
            if (ext == null || !Extensions.ContainsKey(ext))
            {
                Logger.Warn("Could not infer shader type from glsl file name {0}", path);
                return null;
            }

            var type = Extensions[ext];
            if (name == null)
                name = fileName;

            return FromFile(path, type, name);
        }

        #endregion
    }
}