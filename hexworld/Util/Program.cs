using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace hexworld.Util
{
    public class Program : GLObject
    {
        private Dictionary<string, int> uniforms = new Dictionary<string, int>();
        private Dictionary<string, int> attributes = new Dictionary<string, int>();

        public string Log => GL.GetProgramInfoLog((int) Id);

        public Program()
            : base((uint) GL.CreateProgram())
        {
        }

        public void Attach(Shader shader) => GL.AttachShader(Id, shader.Id);

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
    }
}