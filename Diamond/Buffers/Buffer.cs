using System;
using System.Runtime.InteropServices;
using Diamond.Shaders;
using Diamond.Util;
using Diamond.Wrappers;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Buffers
{
    /// <summary>
    /// Manages an OpenGL Buffer object
    /// </summary>
    /// <typeparam name="T">The type of data used for this buffer</typeparam>
    public class Buffer<T> : GLObject where T : struct
    {
        internal readonly BufferWrap Wrapper;

        private readonly VertexDataInfo _vdi;

        /// <summary>
        /// The target for this buffer; its type
        /// </summary>
        public BufferTarget Target => Wrapper.Target;

        /// <summary>
        /// The usage hint for this buffer. Use StaticDraw for one-time uploads to 
        /// vertex buffers, and DynamicDraw for repeated uploads to vertex buffers.
        /// </summary>
        public BufferUsageHint Usage
        {
            get => Wrapper.Usage;
            set => Wrapper.Usage = value;
        }

        internal Buffer(BufferWrap wrapper, string name)
        {
            Wrapper = wrapper;
            Name = name;
            _vdi = VertexDataInfo.GetInfo<T>();
        }

        /// <summary>
        /// Upload data to this buffer
        /// </summary>
        /// <param name="data">The data to upload</param>
        public void Data(T[] data) => Wrapper.Data(_vdi.Stride, data);

        /// <summary>
        /// Upload a range of data to this buffer
        /// </summary>
        /// <param name="offset">The range offset</param>
        /// <param name="count">The range length</param>
        /// <param name="data">The data to upload, offset and length apply to both this and the target</param>
        public void Data(int offset, int count, T[] data) => Wrapper.SubData(_vdi.Stride, offset, count, data);

        /// <summary>
        /// Upload a range of data to this buffer
        /// </summary>
        /// <param name="data">The data to upload</param>
        public void Data(SubArray<T> data) => Data(data.Offset, data.Length, data.Array);

        /// <summary>
        /// Point this buffer to a program's vertex attributes. T must have [VertexDataAttribute], and all fields
        /// of T must have [VertexPointerAttribute] to infer vertex pointer locations.
        /// </summary>
        /// <param name="program">The program to point this buffer to</param>
        public void PointTo(Program program)
        {
            if (_vdi == null)
            {
                var exception = new InvalidOperationException($"Cannot use type {typeof(T)} to create a Vertex Buffer");
                Logger.Error(exception);
                throw exception;
            }

            Wrapper.Bind();
            foreach (var attr in _vdi.Pointers)
            {
                if (program.HasAttribute(attr.Name))
                    GL.VertexAttribPointer(program.AttributeLocation(attr.Name), attr.Size, attr.Type, attr.Normalized,
                        _vdi.Stride, attr.Offset);
            }
        }

        public override string ToString() => Name == null
            ? $"Buffer<{typeof(T).Name}> {Wrapper}"
            : $"Buffer<{typeof(T).Name}> {Wrapper} \'{Name}\'";

        public override void Dispose()
        {
            Logger.Debug("Disposing {0}", this);
            Wrapper.Dispose();
        }

        #region Factory Methods

        /// <summary>
        /// Create an empty buffer of this type
        /// </summary>
        /// <param name="target">The buffer target</param>
        /// <param name="usage">The initial usage hint</param>
        /// <param name="name">The name of this GLObject</param>
        /// <returns>The buffer, or null if initialization failed</returns>
        internal static Buffer<T> Empty(BufferTarget target, BufferUsageHint usage, string name)
        {
            var wrapper = new BufferWrap(target, usage);
            var service = new Buffer<T>(wrapper, name);

            Logger.Debug("Created {0}", service);

            return service;
        }

        /// <summary>
        /// Create a buffer of this type and upload data
        /// </summary>
        /// <param name="data">The data to upload</param>
        /// <param name="target">The buffer target</param>
        /// <param name="usage">The initial usage hint</param>
        /// <param name="name">The name of this GLObject</param>
        /// <returns>The buffer, or null if initialization failed</returns>
        internal static Buffer<T> FromData(T[] data, BufferTarget target, BufferUsageHint usage, string name = null)
        {
            var service = Empty(target, usage, name);

            service?.Data(data);

            return service;
        }

        #endregion
    }

    /// <summary>
    /// Class for static Buffer operations and public factory methods
    /// </summary>
    public static class Buffer
    {
        /// <summary>
        /// Create an empty buffer of this type
        /// </summary>
        /// <param name="target">The buffer target</param>
        /// <param name="usage">The initial usage hint</param>
        /// <param name="name">The name of this GLObject</param>
        /// <returns>The buffer, or null if initialization failed</returns>
        public static Buffer<T> Empty<T>(BufferTarget target, BufferUsageHint usage = BufferUsageHint.StaticDraw,
            string name = null) where T : struct => Buffer<T>.Empty(target, usage, name);

        /// <summary>
        /// Create a buffer of this type and upload data
        /// </summary>
        /// <param name="data">The data to upload</param>
        /// <param name="target">The buffer target</param>
        /// <param name="usage">The initial usage hint</param>
        /// <param name="name">The name of this GLObject</param>
        /// <returns>The buffer, or null if initialization failed</returns>
        public static Buffer<T> FromData<T>(T[] data, BufferTarget target,
            BufferUsageHint usage = BufferUsageHint.StaticDraw,
            string name = null) where T : struct => Buffer<T>.FromData(data, target, usage, name);
    }
}