using System;
using System.Runtime.InteropServices;
using Diamond.Shaders;
using Diamond.Util;
using NLog;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Buffers
{
    internal class GLBufferWrapper : GLWrapper
    {
        public BufferTarget Target { get; }
        public BufferUsageHint Usage { get; set; }

        internal GLBufferWrapper(BufferTarget target, BufferUsageHint usage)
        {
            Id = GL.GenBuffer();
            Target = target;
            Usage = usage;
        }

        public void Bind() => GL.BindBuffer(Target, Id);

        public void Data<T>(int size, T[] data) where T : struct
        {
            Bind();
            GL.BufferData(Target, (IntPtr) (size * data.Length), data, Usage);
        }

        public void SubData<T>(int size, int offset, int count, T[] data) where T : struct
        {
            Bind();
            GL.BufferSubData(Target, (IntPtr) (offset * size), (IntPtr) (count * size), data);
        }

        public override void GLDelete() => GL.DeleteBuffer(Id);
    }

    public class GLBuffer<T> : GLObject where T : struct
    {
        private GLBufferWrapper _buffer;
        internal override GLWrapper Wrapper => _buffer;

        private int _size;

        public BufferTarget Target => _buffer.Target;

        public BufferUsageHint Usage
        {
            get => _buffer.Usage;
            set => _buffer.Usage = value;
        }

        internal GLBuffer(GLBufferWrapper buffer, string name)
        {
            _buffer = buffer;
            Name = name;
            _size = Marshal.SizeOf<T>();
        }

        public void Data(T[] data) => _buffer.Data(_size, data);

        public void Data(int offset, int count, T[] data) => _buffer.SubData(_size, offset, count, data);

        public void Data(SubArray<T> data) => Data(data.Offset, data.Length, data.Array);

        public override string ToString() => Name == null ? $"{Target} ({Id})" : $"{Target} {Name} ({Id})";
    }

    public static class GLBuffer
    {
        internal static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static GLBuffer<T> Empty<T>(BufferTarget target, BufferUsageHint usage = BufferUsageHint.StaticDraw,
            string name = null) where T : struct
        {
            var wrapper = new GLBufferWrapper(target, usage);
            var service = new GLBuffer<T>(wrapper, name);

            Logger.Debug("Created {0}", service);

            return service;
        }

        public static GLBuffer<T> FromData<T>(T[] data, BufferTarget target,
            BufferUsageHint usage = BufferUsageHint.StaticDraw,
            string name = null) where T : struct
        {
            var service = Empty<T>(target, usage, name);

            service?.Data(data);

            return service;
        }
    }

    public class VertexBuffer<T> : GLBuffer<T> where T : struct
    {
        private readonly VertexDataInfo _vdi;
        private readonly GLBufferWrapper _buffer;

        internal VertexBuffer(GLBufferWrapper buffer, string name)
            : base(buffer, name)
        {
            _vdi = VertexDataInfo.GetInfo<T>();
            _buffer = buffer;
        }

        public void PointTo(Program program)
        {
            _buffer.Bind();
            foreach (var attr in _vdi.Pointers)
            {
                var loc = program.AttributeLocation(attr.Name);
                if (loc.HasValue)
                    GL.VertexAttribPointer((int) loc, attr.Size, attr.Type, attr.Normalized, _vdi.Stride, attr.Offset);
            }
        }
    }

    public static class VertexBuffer
    {
        public static VertexBuffer<T> Empty<T>(BufferTarget target, BufferUsageHint usage = BufferUsageHint.StaticDraw,
            string name = null) where T : struct
        {
            if (typeof(T).GetCustomAttributes(typeof(VertexDataAttribute), false).Length == 0)
            {
                GLBuffer.Logger.Warn("Cannot use type {0} to create a VertexBuffer", typeof(T));
                return null;
            }

            var wrapper = new GLBufferWrapper(target, usage);
            var service = new VertexBuffer<T>(wrapper, name);

            GLBuffer.Logger.Debug("Created {0}", service);

            return service;
        }

        public static VertexBuffer<T> FromData<T>(T[] data, BufferTarget target,
            BufferUsageHint usage = BufferUsageHint.StaticDraw,
            string name = null) where T : struct
        {
            var service = Empty<T>(target, usage, name);

            service?.Data(data);

            return service;
        }
    }
}