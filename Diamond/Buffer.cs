using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using NLog;
using OpenTK.Graphics.OpenGL4;

namespace Diamond
{
    /// <summary>
    /// Wrap an OpenGL buffer object
    /// </summary>
    public class Buffer : GLObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Static

        private static readonly Dictionary<BufferTarget, Buffer> BoundBuffers;

        /// <inheritdoc/>
        static Buffer()
        {
            BoundBuffers = new Dictionary<BufferTarget, Buffer>();
            foreach (var value in Enum.GetValues(typeof(BufferTarget)).Cast<BufferTarget>())
                BoundBuffers[value] = null;
        }

        #region ArrayBuffer

        /// <summary>
        /// The buffer will be used as a source for vertex data, but the connection is only made when
        /// glVertexAttribPointer is called. The pointer field of this function is taken as a byte 
        /// offset from the beginning of whatever buffer is currently bound to this target.
        /// </summary>
        public static Buffer ArrayBuffer
        {
            get => BoundBuffers[BufferTarget.ArrayBuffer];
            set => Bind(BufferTarget.ArrayBuffer, value);
        }

        #endregion

        #region ElementArrayBuffer

        /// <summary>
        /// All rendering functions of the form gl*Draw*Elements* will use the pointer field as a byte
        /// offset from the beginning of the buffer object bound to this target. The indices used for
        /// indexed rendering will be taken from the buffer object. Note that this binding target is
        /// part of a Vertex Array Objects state, so a VAO must be bound before binding a buffer here.
        /// </summary>
        public static Buffer ElementArrayBuffer
        {
            get => BoundBuffers[BufferTarget.ArrayBuffer];
            set => Bind(BufferTarget.ElementArrayBuffer, value);
        }

        #endregion

        #region DrawIndirectBuffer

        /// <summary>
        /// The buffer bound to this target will be used as the source for the indirect data when performing
        ///  indirect rendering. This is only available in core OpenGL 4.0 or with ARB_draw_indirect.
        /// </summary>
        public static Buffer DrawIndirectBuffer
        {
            get => BoundBuffers[BufferTarget.DrawIndirectBuffer];
            set => Bind(BufferTarget.DrawIndirectBuffer, value);
        }

        #endregion

        /// <summary>
        /// Bind a buffer to a target. If buffer is null, unbinds the target.
        /// </summary>
        /// <param name="target">The binding target</param>
        /// <param name="buffer">The buffer to bind, or 0 if null</param>
        public static void Bind(BufferTarget target, Buffer buffer)
        {
            GL.BindBuffer(target, buffer?.Id ?? 0);
            Logger.Debug("Bound {0} to {1}", (object) buffer ?? "default buffer", target);

            BoundBuffers[target] = buffer;
        }

        #endregion

        #region ctor, Delete()

        /// <summary>
        /// Create a buffer object wrapper
        /// </summary>
        internal Buffer()
            : base(GL.GenBuffer())
        {
            Logger.Debug("Created {0}", this);
        }

        /// <inheritdoc />
        protected override void Delete()
        {
            Logger.Debug("Disposing {0}", this);
            GL.DeleteBuffer(Id);
        }

        #endregion

        #region Properties

        #region Queries

        /// <summary>
        /// The usage hint for the current data store
        /// </summary>
        public BufferUsageHint Usage => (BufferUsageHint) Get(BufferParameterName.BufferUsage);

        /// <summary>
        /// The size of the current data store in bytes
        /// </summary>
        public int Size => Get(BufferParameterName.BufferSize);

        #endregion

        #region Stored

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Get a property of this buffer
        /// </summary>
        /// <param name="param">The property to get</param>
        /// <returns>The int value of the property</returns>
        public int Get(BufferParameterName param)
        {
            ArrayBuffer = this;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, param, out int res);
            return res;
        }

        /// <summary>
        /// Bind this buffer to a target
        /// </summary>
        /// <param name="target">The binding target</param>
        public void Bind(BufferTarget target) => Bind(target, this);

        /// <inheritdoc />
        public override string ToString() =>
            $"'Buffer {Id}'";

        /// <summary>
        /// Upload data to this buffer. Deletes the existing data store and creates a new one
        /// from the marshalled size of T.
        /// </summary>
        /// <remarks>
        /// BufferUsageHint should be based off of what the user will be doing with the buffer.
        /// 
        /// DRAW: The user will be writing data to the buffer, but the user will not read it.
        /// READ: The user will not be writing data, but the user will be reading it back.
        /// COPY: The user will be neither writing nor reading the data.
        /// 
        /// STATIC: The user will set the data once.
        /// DYNAMIC: The user will set the data occasionally.
        /// STREAM: The user will be changing the data after every use.Or almost every use.
        /// </remarks>
        /// <typeparam name="T">Data element type, used with Marshal.SizeOf to calculate size of data store</typeparam>
        /// <param name="data">Values to upload</param>
        /// <param name="usage">Usage hint for for this data.</param>
        public void Upload<T>(T[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw) where T : struct
        {
            Logger.Debug("Updating {0} data (replacing store)", this);
            ArrayBuffer = this;
            GL.BufferData(BufferTarget.ArrayBuffer, data.SizeInBytes(), data, usage);
        }

        /// <summary>
        /// Upload the data to this buffer within a range. Does not delete the existing data store.
        /// </summary>
        /// <param name="data">The data to upload. The range is also applied to this array.</param>
        /// <param name="offset">The offset index of data to begin uploading</param>
        /// <param name="count">The number of T elements to upload</param>
        /// <typeparam name="T">Data element type. Offsets are applied according to the size of this in bytes.</typeparam>
        public void Upload<T>(T[] data, int offset, int count) where T : struct
        {
            Logger.Debug("Updating {0} data range ({1} for {2}] from type {3}",
                this, offset, count, typeof(T).Name);
            ArrayBuffer = this;
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr) offset, count, data);
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Creates a buffer, initialized with an array
        /// </summary>
        /// <remarks>
        /// BufferUsageHint should be based off of what the user will be doing with the buffer.
        /// 
        /// DRAW: The user will be writing data to the buffer, but the user will not read it.
        /// READ: The user will not be writing data, but the user will be reading it back.
        /// COPY: The user will be neither writing nor reading the data.
        /// 
        /// STATIC: The user will set the data once.
        /// DYNAMIC: The user will set the data occasionally.
        /// STREAM: The user will be changing the data after every use.Or almost every use.
        /// </remarks>
        /// <typeparam name="T">Data element type.</typeparam>
        /// <param name="data">Data used to initialize the buffer</param>
        /// <param name="usage">The usage hint for this buffer</param>
        /// <returns>The initialized buffer, or null if initialization failed</returns>
        public static Buffer<T> FromData<T>(T[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
            where T : struct
        {
            var buffer = new Buffer<T>();

            buffer.Upload(data, usage);

            return buffer;
        }

        #endregion
    }

    /// <summary>
    /// Generic overload for Buffer. Caches properties like data, usage, and size.
    /// </summary>
    /// <typeparam name="T">Data type used for all operations</typeparam>
    public sealed class Buffer<T> : Buffer where T : struct
    {
        /// <summary>
        /// Cached usage hint for the buffer's storage
        /// </summary>
        public new BufferUsageHint Usage { get; private set; }

        /// <summary>
        /// Size of the buffer data storage in T units.
        /// </summary>
        public new int Size { get; private set; }

        /// <summary>
        /// The data in this buffer. Not copied to the buffer until Upload() is called.
        /// </summary>
        public T[] Data { get; private set; }

        /// <summary>
        /// Upload data to this buffer. Deletes the existing data store and creates a new one.
        /// </summary>
        /// <remarks>
        /// BufferUsageHint should be based off of what the user will be doing with the buffer.
        /// 
        /// DRAW: The user will be writing data to the buffer, but the user will not read it.
        /// READ: The user will not be writing data, but the user will be reading it back.
        /// COPY: The user will be neither writing nor reading the data.
        /// 
        /// STATIC: The user will set the data once.
        /// DYNAMIC: The user will set the data occasionally.
        /// STREAM: The user will be changing the data after every use.Or almost every use.
        /// </remarks>
        /// <param name="data">Values to upload</param>
        /// <param name="usage">Usage hint for for this data.</param>
        public void Upload(T[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
        {
            base.Upload<T>(data, usage);
            Data = data;
            Usage = usage;
            Size = data.Length;
        }

        /// <summary>
        /// Upload the data to this buffer within a range. Does not delete the existing data store.
        /// </summary>
        /// <param name="offset">The offset index of data to begin uploading</param>
        /// <param name="count">The number of T elements to upload</param>
        public void Upload(int offset, int count)
        {
            base.Upload<T>(Data, offset, count);
        }
    }
}