using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Wrappers
{
    /// <summary>
    /// Wrapper class for OpenGL Shader objects
    /// </summary>
    internal sealed class ShaderWrap : Wrapper
    {
        #region Constructor, GLDelete()

        internal ShaderWrap(ShaderType shaderType)
            : base(GL.CreateShader(shaderType))
        {
            ShaderType = shaderType;
        }

        protected override void GLDelete() => GL.DeleteShader(Id);

        #endregion

        #region Properties

        #region Stored

        /// <summary>
        /// The type of this shader - stored at creation time to prevent repeated queries
        /// </summary>
        public ShaderType ShaderType { get; }

        #endregion

        /// <summary>
        /// Get or set the source of this shader (glShaderSource)
        /// </summary>
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

        /// <summary>
        /// Check the compilation status of this shader
        /// </summary>
        public bool Compiled
        {
            get
            {
                GL.GetShader(Id, ShaderParameter.CompileStatus, out int res);
                return res != 0;
            }
        }

        public string InfoLog => GL.GetShaderInfoLog(Id);

        #endregion

        #region Methods

        /// <summary>
        /// Get a parameter of this shader (glGetShader)
        /// </summary>
        /// <param name="parameter">The parameter to get</param>
        /// <returns>The parameter value</returns>
        public int Get(ShaderParameter parameter)
        {
            GL.GetShader(Id, parameter, out int res);
            return res;
        }

        /// <summary>
        /// Compile this shader (glCompileShader)
        /// </summary>
        public void Compile() => GL.CompileShader(Id);

        #endregion

        public override string ToString() => $"Shader Wrapper - {ShaderType} ({Id})";
    }
}