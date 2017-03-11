using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Shaders
{
    /// <summary>
    /// Wrap and OpenGL program object
    /// </summary>
    public sealed class Program : GLObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Static

        private static Program _current;

        /// <summary>
        /// The currently active program, or null if the default program is active.
        /// </summary>
        public static Program Current
        {
            get => _current;
            set => Use(value);
        }

        /// <summary>
        /// Use this program. If program is null, use the default program.
        /// </summary>
        public static void Use(Program program)
        {
            if (program != null && !program.Linked)
            {
                Logger.Error("Cannot use {0} because it is not linked", program);
                program = null;
            }

            GL.UseProgram(program?.Id ?? 0);
            Logger.Trace("Using {0}", (object) program ?? "default program");

            _current = program;
        }

        #endregion

        #region ctor, Delete()

        /// <inheritdoc />
        private Program()
            : base(GL.CreateProgram())
        {
            Logger.Debug("Created {0}", this);
        }

        /// <inheritdoc />
        protected override void Delete()
        {
            Logger.Debug("Disposing {0}", this);
            GL.DeleteProgram(Id);
        }

        #endregion

        #region Properties

        #region Queries

        /// <summary>
        /// Gets the number of active uniforms
        /// </summary>
        public int ActiveUniforms => Get(GetProgramParameterName.ActiveUniforms);

        /// <summary>
        /// Gets the number of active attributes
        /// </summary>
        public int ActiveAttributes => Get(GetProgramParameterName.ActiveAttributes);

        #endregion

        #region Stored

        /// <summary>
        /// The InfoLog for this program since it was last linked
        /// </summary>
        public string InfoLog { get; private set; }

        /// <summary>
        /// The link status of this program
        /// </summary>
        public bool Linked { get; private set; }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Get a property of this program. invokes glGetProgram and returns an int result.
        /// </summary>
        /// <param name="param">The program property to get</param>
        /// <returns>The int value of the program property</returns>
        private int Get(GetProgramParameterName param)
        {
            GL.GetProgram(Id, param, out int res);
            return res;
        }

        /// <summary>
        /// Try to link this program. If linking fails, the InfoLog is updated, and attribute and uniform caches are reset.
        /// If linking is successful, attribute and uniform caches are generated
        /// </summary>
        public void Link()
        {
            _attributes.Clear();
            _uniforms.Clear();

            Logger.Debug("Linking {0}", this);
            GL.LinkProgram(Id);
            // link status can only change after link attempt
            Linked = Get(GetProgramParameterName.LinkStatus) != 0;

            if (Linked)
            {
                Logger.Trace("Successfully linked {0}", this);

                for (var i = 0; i < ActiveAttributes; i++)
                {
                    var sb = new StringBuilder(256);
                    GL.GetActiveAttrib(Id, i, sb.Capacity, out int length, out int size, out ActiveAttribType type, sb);
                    var name = sb.ToString();
                    _attributes[name] = i;
                }

                for (var i = 0; i < ActiveUniforms; i++)
                {
                    var sb = new StringBuilder(256);
                    GL.GetActiveUniform(Id, i, sb.Capacity, out int length, out int size, out ActiveUniformType type,
                        sb);
                    var name = sb.ToString();
                    _uniforms[name] = i;
                }
            }
            else
            {
                InfoLog = GL.GetProgramInfoLog(Id).Trim();

                Logger.Error("Failed to link {0}", this);
                Logger.Trace("InfoLog for {0}:\n{1}", this, InfoLog);
            }
        }

        /// <summary>
        /// Use this program. Equivalent to Program.Use(this);
        /// </summary>
        public void Use() => Use(this);

        #region Attribute Locations

        private readonly Dictionary<string, int> _attributes = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _uniforms = new Dictionary<string, int>();

        /// <summary>
        /// Check if this program has an active attribute
        /// </summary>
        /// <param name="name">The attribute name</param>
        /// <returns>Whether the program has an active attribute</returns>
        public bool HasAttribute(string name) => _attributes.ContainsKey(name);

        /// <summary>
        /// Check if this program has an active uniform
        /// </summary>
        /// <param name="name">The uniform name</param>
        /// <returns>Whether the program has an active uniform</returns>
        public bool HasUniform(string name) => _uniforms.ContainsKey(name);

        /// <summary>
        /// Get the location of an attribute by name. Throws InvalidOperationException if the attribute
        /// is not found. This can be checked with HasAttribute
        /// </summary>
        /// <param name="name">The attribute name</param>
        /// <returns>The location of the attribute</returns>
        public int AttributeLocation(string name)
        {
            if (!HasAttribute(name))
                throw new InvalidOperationException($"{this} does not have active attribute '{name}'");
            return _attributes[name];
        }

        /// <summary>
        /// Get the location of an uniform by name. Throws InvalidOperationException if the uniform
        /// is not found. This can be checked with HasUniform
        /// </summary>
        /// <param name="name">The uniform name</param>
        /// <returns>The location of the uniform</returns>
        public int UniformLocation(string name)
        {
            if (!HasUniform(name)) throw new InvalidOperationException($"{this} does not have active uniform '{name}'");
            return _uniforms[name];
        }

        #endregion

        /// <summary>
        /// Attach a shader to this program. If shader is null, does nothing. If shader is not compiled
        /// it is still attached but program link will likely fail.
        /// </summary>
        /// <param name="shader">The shader to attach</param>
        public void Attach(Shader shader)
        {
            if (shader == null)
            {
                Logger.Error("Cannot attach null shader to {0}", this);
                return;
            }

            if (!shader.Compiled)
                Logger.Warn("{0} is not compiled. Program link will likely fail.", shader);

            Logger.Debug("Attaching {0} to {1}", shader, this);
            GL.AttachShader(Id, shader.Id);
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"'Program {Id}'";

        #endregion

        #region Factory Methods

        /// <summary>
        /// Create and link a program from precompiled shaders. If any shader is null or uncompiled it is still
        /// attached, although program link will likely fail. 
        /// </summary>
        /// <param name="shaders">The shaders used in this program</param>
        /// <returns>A linked program, or null if initialization failed</returns>
        public static Program FromShaders(params Shader[] shaders) => FromShaders((IEnumerable<Shader>) shaders);

        /// <summary>
        /// Create and link a program from precompiled shaders. If any shader is null or uncompiled it is still
        /// attached, although program link will likely fail. 
        /// </summary>
        /// <param name="shaders">The shaders used in this program</param>
        /// <returns>A linked program, or null if initialization failed</returns>
        public static Program FromShaders(IEnumerable<Shader> shaders)
        {
            if (shaders == null)
            {
                Logger.Error("Cannot create program from no shaders.");
                return null;
            }

            var program = new Program();

            foreach (var shader in shaders)
                program.Attach(shader);

            program.Link();

            if (program.Linked) return program;

            program.Dispose();
            return null;
        }

        /// <summary>
        /// Create and link a program from glsl source files. Shader types must be inferrable from file extensions.
        /// <para></para>
        /// See <see cref="Shader.FromFile(string,ShaderType)"/> for file extension details
        /// </summary>
        /// <param name="files"></param>
        /// <returns>The linked program, or null if initialization failed</returns>
        public static Program FromFiles(params string[] files) => FromFiles((IEnumerable<string>) files);

        /// <summary>
        /// Create and link a program from glsl source files. Shader types must be inferrable from file extensions.
        /// <para></para>
        /// See <see cref="Shader.FromFile(string,ShaderType)"/> for file extension details
        /// </summary>
        /// <param name="files"></param>
        /// <returns>The linked program, or null if initialization failed</returns>
        public static Program FromFiles(IEnumerable<string> files)
        {
            if (files == null)
            {
                Logger.Warn("Cannot create a program from no shaders");
                return null;
            }

            var shaders = files.Select(Shader.FromFile).ToList();

            var program = FromShaders(shaders);

            foreach (var shader in shaders)
            {
                shader?.Dispose();
            }

            return program;
        }

        #endregion
    }
}