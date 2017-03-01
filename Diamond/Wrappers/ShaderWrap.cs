using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Wrappers
{
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

        public ShaderType ShaderType { get; }

        #endregion

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

        public void Compile() => GL.CompileShader(Id);

        #endregion

        public override string ToString() => $"Shader Wrapper - {ShaderType} ({Id})";
    }
}