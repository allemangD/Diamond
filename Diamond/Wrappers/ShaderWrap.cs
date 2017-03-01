using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Wrappers
{
    internal sealed class ShaderWrap : Wrapper
    {
        internal ShaderWrap(ShaderType shaderType)
        {
            Id = GL.CreateShader(shaderType);
            ShaderType = shaderType;
        }

        public readonly ShaderType ShaderType;

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

        public void Compile() => GL.CompileShader(Id);

        public override void GLDelete() => GL.DeleteShader(Id);
    }
}