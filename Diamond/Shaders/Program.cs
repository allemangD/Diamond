﻿using System.Collections.Generic;
using System.Linq;
using Diamond.Wrappers;
using NLog;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Shaders
{
    public class Program : GLObject
    {
        private readonly ProgramWrap _program;
        internal override Wrapper Wrapper => _program;

        public static Program Current { get; private set; }

        private readonly Dictionary<string, int> _uniforms = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _attributes = new Dictionary<string, int>();

        internal Program(ProgramWrap program, string name)
        {
            _program = program;
            Name = name;
        }

        public int? UniformLocation(string name)
        {
            if (_uniforms.ContainsKey(name)) return _uniforms[name];
            return null;
        }

        public int? AttributeLocation(string name)
        {
            if (_attributes.ContainsKey(name)) return _attributes[name];
            return null;
        }

        public void Use()
        {
            GL.UseProgram(Id);
            Current = this;
        }

        //? Could create static Program instance which wraps the default shader
        // ie Shader.Default.Use()
        // would also allow sending arrays to the default attribs like gl_Vertex etc.
        public static void UseDefault()
        {
            GL.UseProgram(0);
            Current = null;
        }

        private bool Link()
        {
            _uniforms.Clear();
            _attributes.Clear();

            _program.Link();

            if (!_program.Linked)
                return false;

            for (var i = 0; i < _program.ActiveUniforms; i++)
                _uniforms[_program.UniformName(i)] = i;

            for (var i = 0; i < _program.ActiveAttributes; i++)
                _attributes[_program.AttributeName(i)] = i;

            return true;
        }

        private void Attach(Shader shader)
        {
            _program.Attach((ShaderWrap) shader.Wrapper);
        }

        public override string ToString() => $"Program \'{Name}\' ({Id})";

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Factory Methods

        public static Program FromShaders(string name, params Shader[] shaders) => FromShaders(name,
            (IEnumerable<Shader>) shaders);

        public static Program FromShaders(string name, IEnumerable<Shader>shaders)
        {
            if (shaders == null)
            {
                Logger.Error("Cannot create program {0} with no shaders.", name);
                return null;
            }

            var wrapper = new ProgramWrap();
            var service = new Program(wrapper, name);

            Logger.Debug("Created {0}", service);

            foreach (var shader in shaders)
            {
                if (shader == null)
                {
                    Logger.Error("One or more shaders failed to compile - cannot create program {0}", name);
                    service.Dispose();
                    return null;
                }

                service.Attach(shader);
            }

            service.Link();

            if (!wrapper.Linked)
            {
                Logger.Warn("Failed to link {0}", service);
                Logger.Debug("InfoLog for {0}", service);
                wrapper.Dispose();
                return null;
            }

            Logger.Debug("Successfully linked {0}", service);

            return service;
        }


        public static Program FromShaders(params Shader[] shaders) => FromShaders((IEnumerable<Shader>) shaders);

        public static Program FromShaders(IEnumerable<Shader> shaders)
        {
            var shaderList = shaders.ToList(); // prevent multiple enumeration
            var shaderNames = shaderList.Select(s => s.Name);
            string name = $"[{string.Join(", ", shaderNames)}]";
            return FromShaders(name, shaderList);
        }

        public static Program FromFiles(params string[] paths)
        {
            var shaders = paths.Select(Shader.FromFile).ToList();

            var program = FromShaders(shaders);

            foreach (var shader in shaders)
                shader.Dispose();

            return program;
        }

        #endregion
    }
}