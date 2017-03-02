using System.Collections.Generic;
using System.Linq;
using Diamond.Wrappers;
using NLog;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Shaders
{
    /// <summary>
    /// Manages an OpenGL Program object
    /// </summary>
    public class Program : GLObject
    {
        internal readonly ProgramWrap Wrapper;

        /// <summary>
        /// The currently active program. Manually invoking glUseProgram will break this.
        /// </summary>
        public static Program Current { get; private set; }

        // keep a cache of uniform and attributes to prevent repeated queries
        private readonly Dictionary<string, int> _uniforms = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _attributes = new Dictionary<string, int>();

        internal Program(ProgramWrap wrapper, string name)
        {
            Wrapper = wrapper;
            Name = name;
        }

        // todo change these to not use int? - possibly use TryGet, or return negative value if not present

        /// <summary>
        /// Get the location of a uniform
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <returns>The location, or no value if uniform not present</returns>
        public int? UniformLocation(string name)
        {
            if (_uniforms.ContainsKey(name)) return _uniforms[name];
            return null;
        }

        /// <summary>
        /// Get the location of an attribute
        /// </summary>
        /// <param name="name">The name of the attribute</param>
        /// <returns>The location, or no value if attribute not present</returns>
        public int? AttributeLocation(string name)
        {
            if (_attributes.ContainsKey(name)) return _attributes[name];
            return null;
        }

        /// <summary>
        /// Use this Program to render. Also updates Program.Current
        /// </summary>
        public void Use()
        {
            Wrapper.Use();
            Current = this;
        }

        /// <summary>
        /// Use the default shader to render
        /// </summary>
        //? Could create static Program instance which wraps the default shader
        // ie Shader.Default.Use()
        // would also allow sending arrays to the default attribs like gl_Vertex etc.
        public static void UseDefault()
        {
            GL.UseProgram(0);
            Current = null;
        }

        /// <summary>
        /// Helper method to try to link this program
        /// </summary>
        /// <returns></returns>
        private bool Link()
        {
            _uniforms.Clear();
            _attributes.Clear();

            Wrapper.Link();

            if (!Wrapper.Linked)
                return false;

            for (var i = 0; i < Wrapper.ActiveUniforms; i++)
                _uniforms[Wrapper.UniformName(i)] = i;

            for (var i = 0; i < Wrapper.ActiveAttributes; i++)
                _attributes[Wrapper.AttributeName(i)] = i;

            return true;
        }

        /// <summary>
        /// Helper method to attach a shader to this program
        /// </summary>
        /// <param name="shader">The shader to attach</param>
        private void Attach(Shader shader)
        {
            Wrapper.Attach(shader.Wrapper);
        }

        public override string ToString() => $"Program {Wrapper} \'{Name}\'";

        public override void Dispose()
        {
            Logger.Debug("Disposing {0}", this);
            Wrapper.Dispose();
        }

        #region Factory Methods

        /// <summary>
        /// Create a program from compiled shaders
        /// </summary>
        /// <param name="name">The name of this GLObject</param>
        /// <param name="shaders">The shaders to use in this program</param>
        /// <returns>The linked program, or null if initialization failed</returns>
        public static Program FromShaders(string name, params Shader[] shaders) => FromShaders(name,
            (IEnumerable<Shader>) shaders);

        /// <summary>
        /// Create a program from compiled shaders
        /// </summary>
        /// <param name="name">The name of this GLObject</param>
        /// <param name="shaders">The shaders to use in this program</param>
        /// <returns>The linked program, or null if initialization failed</returns>
        public static Program FromShaders(string name, IEnumerable<Shader>shaders)
        {
            if (shaders == null)
            {
                Logger.Error("Cannot create program {0} with no shaders.", name);
                return null;
            }

            var wrapper = new ProgramWrap();
            var service = new Program(wrapper, name);

            Logger.Debug("Created {0}", service);

            foreach (var shader in shaders)
            {
                if (shader == null)
                {
                    Logger.Error("One or more shaders failed to compile - cannot create program {0}", name);
                    service.Dispose();
                    return null;
                }

                service.Attach(shader);
            }

            var linked = service.Link();

            if (!linked)
            {
                Logger.Warn("Failed to link {0}", service);
                Logger.Debug("InfoLog for {0}", service);
                service.Dispose();
                return null;
            }

            Logger.Debug("Successfully linked {0}", service);

            return service;
        }

        /// <summary>
        /// Create a program from compiled shaders
        /// </summary>
        /// <param name="shaders">The shaders to use in this program</param>
        /// <returns>The linked program, or null if initialization failed</returns>
        public static Program FromShaders(params Shader[] shaders) => FromShaders((IEnumerable<Shader>) shaders);

        /// <summary>
        /// Create a program from compiled shaders
        /// </summary>
        /// <param name="shaders">The shaders to use in this program</param>
        /// <returns>The linked program, or null if initialization failed</returns>
        public static Program FromShaders(IEnumerable<Shader> shaders)
        {
            var shaderList = shaders.ToList(); // prevent multiple enumeration
            var shaderNames = shaderList.Select(s => s.Name);
            string name = $"[{string.Join(", ", shaderNames)}]";
            return FromShaders(name, shaderList);
        }

        /// <summary>
        /// Create shaders from glsl source files, and create a program using them.
        /// Shader types must be inferrable from file extensions.
        /// </summary>
        /// <param name="paths">The glsl source files</param>
        /// <returns>The linked program, or null if initialization faileds</returns>
        public static Program FromFiles(params string[] paths)
        {
            if (paths == null)
            {
                Logger.Warn("Cannot create a program from no shaders.");
                return null;
            }

            var shaders = paths.Select(path => Shader.FromFile(path)).ToList();

            var program = FromShaders(shaders);

            foreach (var shader in shaders)
                shader?.Dispose();

            return program;
        }

        #endregion
    }
}