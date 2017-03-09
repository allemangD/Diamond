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
    public class Program : GLObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Static

        private static Program _current;

        /// <summary>
        /// The currently active program
        /// </summary>
        public static Program Current
        {
            get => _current;
            set
            {
                if (value != null && !value.Linked)
                {
                    Logger.Error("Cannot use {0} because it is not linked", value);
                    value = null;
                }

                Logger.Debug("Using {0}", (object) value ?? "default program");
                GL.UseProgram((_current = value)?.Id ?? 0);
            }
        }

        #endregion

        #region Constructor, Delete()

        /// <summary>
        /// Create a program object wrapper
        /// </summary>
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
        /// The number of active uniforms
        /// </summary>
        public int ActiveUniforms => Get(GetProgramParameterName.ActiveUniforms);

        /// <summary>
        /// The number of active attributes
        /// </summary>
        public int ActiveAttributes => Get(GetProgramParameterName.ActiveAttributes);

        #endregion

        #region Stored

        /// <summary>
        /// The InfoLog for this program
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
        /// Get a property of this program
        /// </summary>
        /// <param name="param">The program property to get</param>
        /// <returns>The int value of the program property</returns>
        private int Get(GetProgramParameterName param)
        {
            GL.GetProgram(Id, param, out int res);
            return res;
        }

        /// <summary>
        /// Try to link this program
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
        /// Use this program
        /// <para></para>
        /// Equivalent to <code>Program.Current = value</code>
        /// </summary>
        public void Use() => Current = this;

        #region Attribute Locations

        private readonly Dictionary<string, int> _attributes = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _uniforms = new Dictionary<string, int>();

        /// <summary>
        /// Check if this program has an active attribute
        /// </summary>
        /// <param name="name">The attribute name</param>
        /// <returns>Whether the progrma has an active attribute</returns>
        public bool HasAttribute(string name) => _attributes.ContainsKey(name);

        /// <summary>
        /// Check if this program has an active uniform
        /// </summary>
        /// <param name="name">The uniform name</param>
        /// <returns>Whether the progrma has an active uniform</returns>
        public bool HasUniform(string name) => _uniforms.ContainsKey(name);

        /// <summary>
        /// Get the location of an attribute by name
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
        /// Get the location of an uniform by name
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
        /// Attach a shader to this program
        /// </summary>
        /// <param name="shader">The shader to attach</param>
        public void Attach(Shader shader)
        {
            Logger.Debug("Attaching {0} to {1}", shader, this);
            GL.AttachShader(Id, shader.Id);
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"'Program {Id}'";

        #endregion

        #region Factory Methods

        /// <summary>
        /// Create and link a program from precompiled shaders
        /// </summary>
        /// <param name="shaders">The shaders used in this program</param>
        /// <returns>A linked program, or null if initialization failed</returns>
        public static Program FromShaders(params Shader[] shaders) => FromShaders((IEnumerable<Shader>) shaders);

        /// <summary>
        /// Create and link a program from precompiled shaders
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
            {
                if (shader == null)
                {
                    Logger.Error("One or more shaders is null - cannot create program");
                    program.Dispose();
                    return null;
                }
                program.Attach(shader);
            }

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