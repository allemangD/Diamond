using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Diamond.Buffers;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Shaders
{
    public class Program : GLObject
    {
        private Dictionary<string, int> uniforms = new Dictionary<string, int>();
        private Dictionary<string, int> attributes = new Dictionary<string, int>();

        public string Log => GL.GetProgramInfoLog((int) Id).Trim();

        public Program()
            : base((uint) GL.CreateProgram())
        {
        }

        protected override void Delete() => GL.DeleteProgram(Id);

        public void Attach(Shader shader) => GL.AttachShader(Id, shader.Id);

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
            uniforms.Clear();
            attributes.Clear();
            GL.LinkProgram(Id);
            GL.GetProgram(Id, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0) return false;

            GL.GetProgram(Id, GetProgramParameterName.ActiveUniforms, out int uniformcount);
            for (var i = 0; i < uniformcount; i++)
            {
                var sb = new StringBuilder(256);
                GL.GetActiveUniformName((int) Id, i, sb.Capacity, out int length, sb);
                uniforms[sb.ToString()] = i;
            }

            GL.GetProgram(Id, GetProgramParameterName.ActiveAttributes, out int attributecount);
            for (var i = 0; i < attributecount; i++)
            {
                var sb = new StringBuilder(256);
                GL.GetActiveAttrib((int) Id, i, sb.Capacity, out int length, out int size,
                    out ActiveAttribType type, sb);
                attributes[sb.ToString()] = i;
            }

            return true;
        }

        public bool TryGetUniform(string name, out int id)
        {
            return uniforms.TryGetValue(name, out id);
        }

        public int GetUniform(string name)
        {
            if (!TryGetUniform(name, out int id))
                throw new ShaderException($"Shader Program {Id} does not contain uniform '{name}'");
            return id;
        }

        public bool TryGetAttribute(string name, out int id)
        {
            return attributes.TryGetValue(name, out id);
        }

        public int GetAttribute(string name)
        {
            if (!TryGetAttribute(name, out int id))
                throw new ShaderException($"Shader Program {Id} does not contain id '{name}'");
            return id;
        }

        public void SetAttribPointers(GLBuffer buff, Type vertexType)
        {
            if (vertexType.GetCustomAttributes(typeof(VertexDataAttribute), false).Length == 0)
            {
                throw new ShaderException($"Cannot attach buffer {buff} to program {this}" +
                                          $" with vertex {vertexType} because it has no" +
                                          $" VertexData attribute.");
            }

            var attribList = new List<VertexPointerAttribute>();
            var Stride = Marshal.SizeOf(vertexType);

            foreach (var fieldInfo in vertexType.GetFields())
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(VertexPointerAttribute), false);
                if (attrs.Length == 0) continue;

                var offset = (int) Marshal.OffsetOf(vertexType, fieldInfo.Name);
                foreach (var attr in attrs)
                {
                    var vpa = (VertexPointerAttribute) attr;
                    vpa.Offset = offset;
                    attribList.Add(vpa);
                }
            }

            buff.Bind();
            foreach (var attr in attribList)
            {
                if (!TryGetAttribute(attr.Name, out int loc)) continue;
                GL.VertexAttribPointer(loc, attr.Size, attr.Type, attr.Normalized, Stride, attr.Offset);
                GL.VertexAttribDivisor(loc, attr.Divisor);
            }
        }

        public void EnableAllAttribArrays()
        {
            foreach (var loc in attributes.Values)
                GL.EnableVertexAttribArray(loc);
        }

        public void DisableAllAttribArrays()
        {
            foreach (var loc in attributes.Values)
                GL.DisableVertexAttribArray(loc);
        }

        public void Use() => GL.UseProgram(Id);

        public static void UseDefault() => GL.UseProgram(0);

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
    }
}