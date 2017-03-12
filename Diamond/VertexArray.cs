using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Diamond.Attributes;
using Diamond.Shaders;
using NLog;
using OpenTK.Graphics.OpenGL4;

namespace Diamond
{
    /// <summary>
    /// Wrap an OpenGL Vertex Array Object
    /// </summary>
    public sealed class VertexArray : GLObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Static

        private static VertexArray _current;

        /// <summary>
        /// The currently bound VAO, or null if no vao is bound.
        /// </summary>
        public static VertexArray Current
        {
            get => _current;
            set => Bind(value);
        }

        /// <summary>
        /// Bind a VAO to the context
        /// </summary>
        /// <param name="vao">The vao to bind, or null to unbind the current vao</param>
        public static void Bind(VertexArray vao)
        {
            GL.BindVertexArray(vao?.Id ?? 0);
            Logger.Trace("Bound {0}", (object) vao ?? "default vao");

            _current = vao;
        }

        #endregion

        #region ctor, Delete()

        /// <inheritdoc />
        internal VertexArray()
            : base(GL.GenVertexArray())
        {
            Logger.Debug("Created {0}", this);
        }

        /// <inheritdoc />
        protected override void Delete()
        {
            Logger.Debug("Disposing {0}", this);
            GL.DeleteVertexArray(Id);
        }

        #endregion

        #region Properties

        #region Queries

        #endregion

        #region Stored

        private Buffer _elementArrayBuffer;

        /// <summary>
        /// The ElementArrayBuffer associated with this VAO
        /// </summary>
        public Buffer ElementArrayBuffer
        {
            get => _elementArrayBuffer;
            set
            {
                Current = this;
                Buffer.ElementArrayBuffer = _elementArrayBuffer = value;
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Attach a VBO to this VAO. Use attributes to infer the 
        /// vertex attrib pointer information
        /// </summary>
        /// <typeparam name="T">The struct to use for vertex attrib information. Must be marked with VertexDataAttribute</typeparam>
        /// <param name="buffer">The buffer to add</param>
        public void Attach<T>(Buffer<T> buffer) where T : struct
        {
            var vda = (VertexDataAttribute) typeof(T)
                .GetCustomAttributes(typeof(VertexDataAttribute), false)
                .FirstOrDefault();

            if (vda == null)
            {
                Logger.Error("Cannot attach buffer {0} to {1}, {2} is missing VertexDataAttribute", buffer, this,
                    typeof(T).Name);
                return;
            }

            Current = this;
            Buffer.ArrayBuffer = buffer;

            var stride = Marshal.SizeOf<T>();
            foreach (var field in typeof(T).GetFields())
            {
                Logger.Debug("Analyzing {0}", field.Name);
                var offset = Marshal.OffsetOf<T>(field.Name);
                foreach (var vai in field.GetCustomAttributes<VertexAttribAttribute>(false))
                {
                    Logger.Debug(
                        $"Enabling {vai.Attribute}, {vai.Size}, {vai.Type}, {vai.Normalized}, {stride}, {offset}");

                    GL.EnableVertexAttribArray(vai.Attribute);
                    GL.VertexAttribPointer(vai.Attribute, vai.Size, vai.Type, vai.Normalized, stride, offset);
                }
            }
        }

        /// <summary>
        /// Bind this vao
        /// </summary>
        public void Bind() => Bind(this);

        #endregion

        #region Factory Methods

        /// <summary>
        /// Create an empty vertex array object. This contains no binding information
        /// by default, and must have any vertex buffer objects attached manually.
        /// </summary>
        /// <returns>A new Vertex Array Object, or null if creation failed</returns>
        public static VertexArray Create()
        {
            return new VertexArray();
        }

        #endregion
    }
}