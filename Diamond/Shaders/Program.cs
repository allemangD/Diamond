using System.Collections.Generic;
using System.Text;
using Diamond.Buffers;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Shaders
{
    public class Program : GLWrapper
    {
        public static Program Current { get; private set; }

        private readonly Dictionary<string, int> _uniforms = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _attributes = new Dictionary<string, int>();

        public string InfoLog => GL.GetProgramInfoLog(Id).Trim();

        public Program()
        {
            Id = GL.CreateProgram();
        }

        protected override void Delete()
        {
            GL.DeleteProgram(Id);
        }

        public void Attach(Shader shader)
        {
            GL.AttachShader(Id, shader.Id);
        }

        public bool Linked
        {
            get
            {
                GL.GetProgram(Id, GetProgramParameterName.LinkStatus, out int success);
                return success != 0;
            }
        }

        public bool Link()
        {
            _uniforms.Clear();
            _attributes.Clear();

            GL.LinkProgram(Id);

            if (!Linked)
            {
                Logger.Warn("Failed to link Program {0}", Id);
                Logger.Debug("Program {0} InfoLog\n{1}", Id, InfoLog);
                return false;
            }

            Logger.Info("Successfully linked Program {0}", Id);

            GL.GetProgram(Id, GetProgramParameterName.ActiveUniforms, out int uniformcount);
            for (var i = 0; i < uniformcount; i++)
            {
                var sb = new StringBuilder(256);
                GL.GetActiveUniformName(Id, i, sb.Capacity, out int length, sb);
                _uniforms[sb.ToString()] = i;
            }

            GL.GetProgram(Id, GetProgramParameterName.ActiveAttributes, out int attributecount);
            for (var i = 0; i < attributecount; i++)
            {
                var sb = new StringBuilder(256);
                GL.GetActiveAttrib(Id, i, sb.Capacity, out int length, out int size,
                    out ActiveAttribType type, sb);
                _attributes[sb.ToString()] = i;
            }

            return true;
        }

        public bool TryGetUniform(string name, out int id)
        {
            return _uniforms.TryGetValue(name, out id);
        }

        public int GetUniform(string name)
        {
            if (TryGetUniform(name, out int id)) return id;

            Logger.Warn("Attempted to access uniform {0} on Program {1}", name, Id);
            throw new ShaderException($"Shader Program {Id} does not contain uniform '{name}'");
        }

        public bool TryGetAttribute(string name, out int id)
        {
            return _attributes.TryGetValue(name, out id);
        }

        public int GetAttribute(string name)
        {
            if (TryGetAttribute(name, out int id)) return id;

            Logger.Warn("Attempted to access attribute {0} on Program {1}", name, Id);
            throw new ShaderException($"Shader Program {Id} does not contain id '{name}'");
        }

        public void SetAttribPointers<T>(GLBuffer<T> buff) where T : struct
        {
            var vdi = VertexDataInfo.GetInfo<T>();

            buff.Bind();
            foreach (var attr in vdi.Pointers)
            {
                if (!TryGetAttribute(attr.Name, out int loc)) continue;

                GL.VertexAttribPointer(loc, attr.Size, attr.Type, attr.Normalized, vdi.Stride, attr.Offset);
            }
        }

        public void Use()
        {
            GL.UseProgram(Id);
            Current = this;
        }

        public static void UseDefault()
        {
            GL.UseProgram(0);
            Current = null;
        }

        public static Program FromShaders(params Shader[] shaders)
        {
            var p = new Program();

            foreach (var shader in shaders)
            {
                p.Attach(shader);
            }

            p.Link();

            return p;
        }

        public static Program FromFiles(params string[] files)
        {
            var shaders = new Shader[files.Length];

            for (var i = 0; i < files.Length; i++)
                shaders[i] = Shader.FromFile(files[i]);

            var p = FromShaders(shaders);

            for (var i = 0; i < shaders.Length; i++)
                shaders[i]?.Dispose();

            return p;
        }
    }
}