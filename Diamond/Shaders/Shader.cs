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
        public readonly ShaderType ShaderType;

        /// <summary>
        /// The source file name, if it was loaded from a file.
        /// </summary>
        public string SourceFile { get; private set; }

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
        public string InfoLog => GL.GetShaderInfoLog((int) Id).Trim();

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
        /// <param name="shaderType">The type of the shader to create</param>
        public Shader(ShaderType shaderType)
            : base((uint) GL.CreateShader(shaderType))
        {
            ShaderType = shaderType;
        }

        protected override void Delete() => GL.DeleteShader(Id);

        /// <summary>
        /// Compile the shader.
        /// </summary>
        /// <returns>Compilation success</returns>
        public bool Compile()
        {
            GL.CompileShader(Id);

            var compiled = Compiled;
            if (!compiled)
            {
                Log.Warn("Failed to compile {0} {1} {2}", ShaderType, Id, SourceFile);
                Log.Debug("{0} {1} InfoLog\n{2}", ShaderType, Id, InfoLog);
            }

            return compiled;
        }

        /// <summary>
        /// Creates and compiles a shader from a source file. Infers shader type from file extension
        /// Extension must be of the form .[type] or .[type].glsl
        /// Valid types are vs, vert, fs, and frag.
        /// </summary>
        /// <param name="path">Source file location</param>
        /// <returns>The compiled shader</returns>
        public static Shader FromFile(string path)
        {
            var ex = Path.GetExtension(path);

            if (ex == ".glsl")
            {
                var name = Path.GetFileNameWithoutExtension(path);
                if (Path.HasExtension(name))
                    ex = Path.GetExtension(name);
            }

            switch (ex)
            {
                case ".vs":
                case ".vert":
                    return FromFile(path, ShaderType.VertexShader);
                case ".fs":
                case ".frag":
                    return FromFile(path, ShaderType.FragmentShader);
                default:
                    throw new ShaderException("Can't infer shader type from extension");
            }
        }

        /// <summary>
        /// Creates and compiles a shader from a source file.
        /// </summary>
        /// <param name="path">Source file location</param>
        /// <param name="type">Type of the shader</param>
        /// <returns>The compiled shader</returns>
        public static Shader FromFile(string path, ShaderType type)
        {
            var s = new Shader(type)
            {
                Source = File.ReadAllText(path),
                SourceFile = path
            };

            s.Compile();
            return s;
        }
    }
}