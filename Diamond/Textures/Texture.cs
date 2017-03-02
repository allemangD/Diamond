using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Diamond.Wrappers;
using NLog;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Diamond.Textures
{
    /// <summary>
    /// Manages a OpenGL Texture object
    /// </summary>
    public class Texture : GLObject
    {
        internal readonly TextureWrap Wrapper;

        internal Texture(TextureWrap wrapper, string name)
        {
            Wrapper = wrapper;
            Name = name;
        }

        /// <summary>
        /// This textures target; how it is used
        /// </summary>
        public TextureTarget Target => Wrapper.Target;

        /// <summary>
        /// Bind this texture to a particular unit
        /// </summary>
        public void Bind(int unit) => Wrapper.Bind(unit);

        public override string ToString() => Name == null
            ? $"{Wrapper}"
            : $"{Wrapper} \'{Name}\'";

        public override void Dispose()
        {
            Logger.Debug("Disposing {0}", this);
            Wrapper.Dispose();
        }

        #region Factory Methods

        /// <summary>
        /// Create a texture object and upload bitmap data to it
        /// </summary>
        /// <param name="bmp">The image to upload</param>
        /// <param name="name">The name of this GLObject</param>
        /// <returns>The initialized Texture, or null if initialsation failed</returns>
        public static Texture FromBitmap(Bitmap bmp, string name = null)
        {
            var wrapper = new TextureWrap(TextureTarget.Texture2D);
            var service = new Texture(wrapper, null);

            Logger.Debug("Created Texture {0}", service);

            wrapper.Bind();

            // todo: expose texture parameters to enable setting different filters
            wrapper.TexParameter(TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            wrapper.TexParameter(TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);

            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            wrapper.Image2D(PixelInternalFormat.Rgba, bmp.Width, bmp.Height, PixelFormat.Bgra, PixelType.UnsignedByte,
                data.Scan0);
            bmp.UnlockBits(data);

            return service;
        }

        /// <summary>
        /// Create a texture and upload the contents of an image file to it
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="name">The name of this GLObject</param>
        /// <returns>The initialized Texture, or null if instantiation failed</returns>
        public static Texture FromFile(string path, string name = null)
        {
            if (name == null)
                name = Path.GetFileNameWithoutExtension(path);

            return FromBitmap(new Bitmap(path), name);
        }

        #endregion
    }
}