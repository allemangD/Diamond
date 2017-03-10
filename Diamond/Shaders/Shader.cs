using System.Collections.Generic;
using System.IO;
using NLog;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Shaders
{
    /// <summary>
    /// Wrap an OpenGL shader object
    /// </summary>
    public sealed class Shader : GLObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region ctor, Delete()

        /// <summary>
        /// Create a shader object wrapper
        /// </summary>
        /// <param name="type">The type of this shader</param>
        private Shader(ShaderType type)
            : base(GL.CreateShader(type))
        {
            ShaderType = type;
            Logger.Debug("Created {0}", this);
        }

        /// <inheritdoc />
        protected override void Delete()
        {
            Logger.Debug("Disposing {0}", this);
            GL.DeleteShader(Id);
        }

        #endregion

        #region Properties

        #region Queries

        #endregion

        #region Stored

        /// <summary>
        /// Store the source code to prevent repeated queries to glGetShaderSource
        /// </summary>
        private string _source;

        /// <summary>
        /// GLSL source code for this shader
        /// </summary>
        public string Source
        {
            get => _source;
            set
            {
                _source = value;
                GL.ShaderSource(Id, _source);

                Logger.Debug("Set shader source for {0}", this);
            }
        }

        /// <summary>
        /// The compilation status of this shader
        /// </summary>
        public bool Compiled { get; private set; }

        /// <summary>
        /// The type of this shader
        /// </summary>
        public ShaderType ShaderType { get; private set; }

        /// <summary>
        /// The InfoLog for this program
        /// </summary>
        public string InfoLog { get; private set; }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Get a property of this shader
        /// </summary>
        /// <param name="param">The shader property to get</param>
        /// <returns>The int value of the shader property</returns>
        private int Get(ShaderParameter param)
        {
            GL.GetShader(Id, param, out int res);
            return res;
        }

        /// <summary>
        /// Try to compile this shader
        /// </summary>
        public void Compile()
        {
            Logger.Debug("Compiling {0}", this);
            GL.CompileShader(Id);
            // compilation status can only change after glCompileShader
            Compiled = Get(ShaderParameter.CompileStatus) != 0;

            if (Compiled)
                Logger.Trace("Successfully compiled {0}", this);
            else
            {
                InfoLog = GL.GetShaderInfoLog(Id).Trim();

                Logger.Error("Failed to compile {0}", this);
                Logger.Trace("InfoLog for {0}: \n{1}", this, InfoLog);
            }
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"'Shader {Id}: {ShaderType}'";

        #endregion

        #region Factory Methods

        /// <summary>
        /// Map file extensions to appropriate shader type
        /// </summary>
        private static readonly Dictionary<string, ShaderType> Extensions = new Dictionary<string, ShaderType>
        {
            [".vs"] = ShaderType.VertexShader,
            [".vert"] = ShaderType.VertexShader,
            [".gs"] = ShaderType.GeometryShader,
            [".geom"] = ShaderType.GeometryShader,
            [".fs"] = ShaderType.FragmentShader,
            [".frag"] = ShaderType.FragmentShader,
        };

        /// <summary>
        /// Create and compile a shader from source
        /// </summary>
        /// <param name="source">The GLSL source for the shader</param>
        /// <param name="type">The type of the shader</param>
        /// <returns>The compiled shader, or null if initialization failed</returns>
        public static Shader FromSource(string source, ShaderType type)
        {
            var shader = new Shader(type) {Source = source};
            shader.Compile();

            if (!shader.Compiled)
            {
                shader.Dispose();
                return null;
            }

            return shader;
        }

        /// <summary>
        /// Create and compile a shader from a source file
        /// </summary>
        /// <param name="path">The path to the source file</param>
        /// <param name="type">The type of the shader</param>
        /// <returns>The compiled shader, or null if initialization failed</returns>
        public static Shader FromFile(string path, ShaderType type)
        {
            if (!File.Exists(path))
            {
                Logger.Warn("Could not find glsl file {0}", path);
                return null;
            }

            var source = File.ReadAllText(path);
            return FromSource(source, type);
        }

        /// <summary>
        /// Create and compile a shader from a source file, and infer the type of the shader from the file extension.
        /// <para></para>
        /// File must have extension or sub-extension <code>.vs</code>, <code>.fs</code>, <code>.gs</code>, <code>.vert</code>,
        /// <code>.frag</code>, or <code>.geom</code>. For example: <code>shader.fs</code>, <code>shader.vert.glsl</code>, 
        /// <code>shader.gs.txt</code>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Shader FromFile(string path)
        {
            if (!File.Exists(path))
            {
                Logger.Warn("Could not find glsl file {0}", path);
                return null;
            }

            var ext = Path.GetExtension(path);
            var file = Path.GetFileNameWithoutExtension(path);

            // get sub-extension if real extension is not valid
            if (ext != null)
                if (!Extensions.ContainsKey(ext))
                    ext = Path.GetExtension(file);

            // if no extension, no sub-extension, or invalid sub-extension
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