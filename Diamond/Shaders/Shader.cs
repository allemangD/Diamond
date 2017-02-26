using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Shaders
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

        public bool Compiled
        {
            get
            {
                GL.GetShader(Id, ShaderParameter.CompileStatus, out int success);
                return success != 0;
            }
        }

        public Shader(ShaderType type)
            : base((uint) GL.CreateShader(type))
        {
            Type = type;
        }

        protected override void Delete()
        {
            GL.DeleteShader(Id);
        }

        public bool Compile()
        {
            GL.CompileShader(Id);
            GL.GetShader(Id, ShaderParameter.CompileStatus, out int success);
            return success != 0;
        }

        public static Shader FromFile(string path, ShaderType type)
        {
            return FromFile(path, type, out bool success);
        }

        public static Shader FromFile(string path, ShaderType type, out bool success)
        {
            var s = new Shader(type);
            s.Source = File.ReadAllText(path);
            success = s.Compile();
            return s;
        }
    }
}