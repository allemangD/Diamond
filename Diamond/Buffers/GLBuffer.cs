using System;
using System.Runtime.InteropServices;
using Diamond.Shaders;
using Diamond.Util;
using Diamond.Wrappers;
using NLog;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Buffers
{
    public class GLBuffer<T> : GLObject where T : struct
    {
        private readonly GLBufferWrapper _buffer;
        private readonly VertexDataInfo _vdi;
        internal override GLWrapper Wrapper => _buffer;

        private readonly int _size;

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
            _vdi = VertexDataInfo.GetInfo<T>();
        }

        public void Data(T[] data) => _buffer.Data(_size, data);

        public void Data(int offset, int count, T[] data) => _buffer.SubData(_size, offset, count, data);

        public void Data(SubArray<T> data) => Data(data.Offset, data.Length, data.Array);

        public void PointTo(Program program)
        {
            if (_vdi == null)
            {
                var exception = new InvalidOperationException($"Cannot use type {typeof(T)} to create a VertexBuffer");
                GLBuffer.Logger.Error(exception);
                throw exception;
            }

            _buffer.Bind();
            foreach (var attr in _vdi.Pointers)
            {
                var loc = program.AttributeLocation(attr.Name);
                if (loc.HasValue)
                    GL.VertexAttribPointer((int) loc, attr.Size, attr.Type, attr.Normalized, _vdi.Stride, attr.Offset);
            }
        }

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
}