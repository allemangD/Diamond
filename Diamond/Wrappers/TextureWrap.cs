using System;
using OpenTK.Graphics.OpenGL4;

namespace Diamond.Wrappers
{
    /// <summary>
    /// Wrapper class for OpenGL Texture objects
    /// </summary>
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

        /// <summary>
        /// The target for this texture; Texture type.
        /// </summary>
        public TextureTarget Target { get; }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Bind this texture to the currently active TextureUnit (glBindTexture)
        /// </summary>
        public void Bind() => GL.BindTexture(Target, Id);

        /// <summary>
        /// Bind this texture to a particular TextureUnit (glActiveTexture, glBindTexture)
        /// </summary>
        /// <param name="unit">Unit to bind to</param>
        public void Bind(int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + unit);
            Bind();
        }

        /// <summary>
        /// Set a texture parameter (glTexParameter)
        /// </summary>
        /// <param name="parameter">The parameter to set</param>
        /// <param name="value">The value to set</param>
        public void TexParameter(TextureParameterName parameter, int value) => GL.TexParameter(Target, parameter,
            value);

        /// <summary>
        /// Upload data to this texture
        /// </summary>
        /// <param name="internalFormat">The number of color components in the texture</param>
        /// <param name="width">The width of the texture</param>
        /// <param name="height">The height of the texture</param>
        /// <param name="format">The pixel format of the texture</param>
        /// <param name="type">The type of the pixel data</param>
        /// <param name="pixels">Location of the pixel data</param>
        public void Image2D(PixelInternalFormat internalFormat, int width, int height, PixelFormat format,
            PixelType type, IntPtr pixels) =>
            GL.TexImage2D(Target, 0, internalFormat, width, height, 0, format, type, pixels);

        #endregion

        public override string ToString() => $"Texture Wrapper - {Target} ({Id})";
    }
}