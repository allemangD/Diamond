using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Wrappers
{
    internal sealed class ProgramWrap : Wrapper
    {
        #region Constructor, Delete()

        internal ProgramWrap()
        {
            Id = GL.CreateProgram();
        }

        public override void Delete() => GL.DeleteProgram(Id);

        #endregion

        #region Properties

        public int Get(GetProgramParameterName parameter)
        {
            GL.GetProgram(Id, parameter, out int res);
            return res;
        }

        public int ActiveUniforms => Get(GetProgramParameterName.ActiveUniforms);
        public int ActiveAttributes => Get(GetProgramParameterName.ActiveAttributes);
        public bool Linked => Get(GetProgramParameterName.LinkStatus) != 0;

        public string InfoLog => GL.GetProgramInfoLog(Id).Trim();

        #endregion

        #region Methods

        public void Attach(ShaderWrap shader) => GL.AttachShader(Id, shader.Id);

        public void Link() => GL.LinkProgram(Id);

        public void Use() => GL.UseProgram(Id);

        public string UniformName(int location)
        {
            var sb = new StringBuilder(64);
            GL.GetActiveUniformName(Id, location, sb.Capacity, out int length, sb);
            return sb.ToString();
        }

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