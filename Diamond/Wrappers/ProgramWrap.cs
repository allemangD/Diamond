using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Wrappers
{
    /// <summary>
    /// Wrapper class for OpenGL Program objects
    /// </summary>
    internal sealed class ProgramWrap : Wrapper
    {
        #region Constructor, GLDelete()

        internal ProgramWrap()
            : base(GL.CreateProgram())
        {
        }

        protected override void GLDelete() => GL.DeleteProgram(Id);

        #endregion

        #region Properties

        /// <summary>
        /// Get the number of active uniforms for this program
        /// </summary>
        public int ActiveUniforms => Get(GetProgramParameterName.ActiveUniforms);

        /// <summary>
        /// Get the number of active attributes for this program
        /// </summary>
        public int ActiveAttributes => Get(GetProgramParameterName.ActiveAttributes);

        /// <summary>
        /// Check whether this program has been Linked
        /// </summary>
        public bool Linked => Get(GetProgramParameterName.LinkStatus) != 0;

        /// <summary>
        /// Get the InfoLog related to this program. Unless Link() failed, should be null.
        /// </summary>
        public string InfoLog => GL.GetProgramInfoLog(Id).Trim(); // trim to remove trailing newlines

        #endregion

        #region Methods

        /// <summary>
        /// Get a parameter from this program (glGetProgram)
        /// </summary>
        /// <param name="parameter">The parameter to get</param>
        /// <returns>The int value of the parameter</returns>
        public int Get(GetProgramParameterName parameter)
        {
            GL.GetProgram(Id, parameter, out int res);
            return res;
        }

        /// <summary>
        /// Attach a compiled shader to this program (glAttachShader)
        /// </summary>
        /// <param name="shader"></param>
        public void Attach(ShaderWrap shader) => GL.AttachShader(Id, shader.Id);

        /// <summary>
        /// Link this program (glLinkProgram)
        /// </summary>
        public void Link() => GL.LinkProgram(Id);

        /// <summary>
        /// Use this program (glUseProgram)
        /// </summary>
        public void Use() => GL.UseProgram(Id);

        /// <summary>
        /// Get the name of the uniform at a location
        /// </summary>
        /// <param name="location">The uniform id</param>
        /// <returns>The uniform name</returns>
        public string UniformName(int location)
        {
            var sb = new StringBuilder(64);
            GL.GetActiveUniformName(Id, location, sb.Capacity, out int length, sb);
            return sb.ToString();
        }

        /// <summary>
        /// Get the name of the attribute at a location
        /// </summary>
        /// <param name="location">The attribute id</param>
        /// <returns>The attribute name</returns>
        public string AttributeName(int location)
        {
            var sb = new StringBuilder(64);
            GL.GetActiveAttrib(Id, location, sb.Capacity, out int length, out int size, out ActiveAttribType type, sb);
            return sb.ToString();
        }

        #endregion

        public override string ToString() => $"Program Wrapper - ({Id})";
    }
}