using System;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Wrappers
{
    internal sealed class TextureWrap : Wrapper
    {
        #region Constructor, GLDelete()

        internal TextureWrap(TextureTarget target)
            : base(GL.GenTexture())
        {
            Target = target;
        }

        protected override void GLDelete() => GL.DeleteTexture(Id);

        #endregion

        #region Properties

        #region Stored

        public TextureTarget Target { get; }

        #endregion

        #endregion

        #region Methods

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

        #endregion

        public override string ToString() => $"Texture Wrapper - {Target} ({Id})";
    }
}