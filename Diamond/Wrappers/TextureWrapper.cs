using System;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Wrappers
{
    internal class TextureWrapper : GLWrapper
    {
        internal TextureWrapper(TextureTarget target)
        {
            Id = GL.GenTexture();
            Target = target;
        }

        public readonly TextureTarget Target;

        public void Bind() => GL.BindTexture(Target, Id);

        public void Bind(int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + unit);
            Bind();
        }

        public void TexParameter(TextureParameterName parameter, int value) => GL.TexParameter(Target, parameter,
            value);

        public void Image2D(PixelInternalFormat internalFormat, int width, int height, PixelFormat format,
            PixelType type, IntPtr pixels) =>
            GL.TexImage2D(Target, 0, internalFormat, width, height, 0, format, type, pixels);

        public override void GLDelete() => GL.DeleteTexture(Id);
    }
}