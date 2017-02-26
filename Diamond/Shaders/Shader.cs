using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Shaders
{
    /// <summary>
    /// Wraps methods for GL Shader objects.
    /// </summary>
    public class Shader : GLObject
    {
        /// <summary>
        /// The type of this shader.
        /// </summary>
        public readonly ShaderType Type;
       
        /// <summary>
        /// Gets and sets the shader source with <code>glShaderSource</code> and <code>glGetShaderSource</code>.
        /// </summary>
        public string Source
        {
            get
            {
                var sb = new StringBuilder(1024);
                GL.GetShaderSource(Id, sb.Capacity, out int length, sb);
                return sb.ToString();
            }
            set { GL.ShaderSource((int) Id, value); }
        }

        /// <summary>
        /// Retrieves this shader's compilation log with <code>glGetShaderInfoLog</code>.
        /// </summary>
        public string Log => GL.GetShaderInfoLog((int) Id).Trim();

        /// <summary>
        /// Checks the compilation status of this shader with <code>glGetShader</code>.
        /// </summary>
        public bool Compiled
        {
            get
            {
                GL.GetShader(Id, ShaderParameter.CompileStatus, out int success);
                return success != 0;
            }
        }

        /// <summary>
        /// Creates a wrapper for a gl Shader object.
        /// </summary>
        /// <param name="type">The type of the shader to create</param>
        public Shader(ShaderType type)
            : base((uint) GL.CreateShader(type))
        {
            Type = type;
        }

        /// <summary>
        /// Frees this gl object. Called by <code>GLObject.Dispose()</code>.
        /// </summary>
        protected override void Delete()
        {
            GL.DeleteShader(Id);
        }

        /// <summary>
        /// Compile the shader.
        /// </summary>
        /// <returns>Compilation success</returns>
        public bool Compile()
        {
            GL.CompileShader(Id);
            return Compiled;
        }

        /// <summary>
        /// Creates and compiles a shader from a source file.
        /// </summary>
        /// <param name="path">Source file location</param>
        /// <param name="type">Type of the shader</param>
        /// <returns>The compiled shader</returns>
        public static Shader FromFile(string path, ShaderType type)
        {
            return FromFile(path, type, out bool success);
        }

        /// <summary>
        /// Creates and compiles a shader from a source file.
        /// </summary>
        /// <param name="path">Source file location</param>
        /// <param name="type">Type of the shader</param>
        /// <param name="success">Compilation success</param>
        /// <returns>The compiled shader</returns>
        public static Shader FromFile(string path, ShaderType type, out bool success)
        {
            var s = new Shader(type);
            s.Source = File.ReadAllText(path);
            success = s.Compile();
            return s;
        }
    }
}