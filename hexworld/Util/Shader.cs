using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace hexworld.Util
{
    public class Shader : GLObject
    {
        public readonly ShaderType Type;

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

        public string Log => GL.GetShaderInfoLog((int) Id);

        public Shader(ShaderType type)
            : base((uint) GL.CreateShader(type))
        {
            Type = type;
        }

        public bool Compile()
        {
            GL.CompileShader(Id);
            GL.GetShader(Id, ShaderParameter.CompileStatus, out int success);
            return success != 0;
        }
    }
}