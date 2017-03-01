using System;
using System.Runtime.InteropServices;
using Diamond.Shaders;
using Diamond.Util;
using Diamond.Wrappers;
using NLog;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Buffers
{
    public class Buffer<T> : GLObject where T : struct
    {
        private readonly BufferWrap _buffer;
        private readonly VertexDataInfo _vdi;
        internal override Wrapper Wrapper => _buffer;

        private readonly int _size;

        public BufferTarget Target => _buffer.Target;

        public BufferUsageHint Usage
        {
            get => _buffer.Usage;
            set => _buffer.Usage = value;
        }

        internal Buffer(BufferWrap buffer, string name)
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
                var exception = new InvalidOperationException($"Cannot use type {typeof(T)} to create a Vertex Buffer");
                Logger.Error(exception);
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

        public override string ToString() => Name == null
            ? $"Buffer<{typeof(T).Name}> {Target} ({Id})"
            : $"Buffer<{typeof(T).Name}> {Target} {Name} ({Id})";

        internal static Buffer<T> Empty(BufferTarget target, BufferUsageHint usage, string name)
        {
            var wrapper = new BufferWrap(target, usage);
            var service = new Buffer<T>(wrapper, name);

            Logger.Debug("Created {0}", service);

            return service;
        }

        internal static Buffer<T> FromData(T[] data, BufferTarget target, BufferUsageHint usage, string name = null)
        {
            var service = Empty(target, usage, name);

            service?.Data(data);

            return service;
        }
    }

    public static class Buffer
    {
        public static Buffer<T> Empty<T>(BufferTarget target, BufferUsageHint usage = BufferUsageHint.StaticDraw,
            string name = null) where T : struct => Buffer<T>.Empty(target, usage, name);

        public static Buffer<T> FromData<T>(T[] data, BufferTarget target,
            BufferUsageHint usage = BufferUsageHint.StaticDraw,
            string name = null) where T : struct => Buffer<T>.FromData(data, target, usage, name);
    }
}