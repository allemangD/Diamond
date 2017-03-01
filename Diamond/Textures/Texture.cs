using System;
using System.Drawing;
using System.Drawing.Imaging;
using NLog;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Diamond.Textures
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

    public class Texture : GLObject
    {
        internal static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly TextureWrapper _texture;
        internal override GLWrapper Wrapper => _texture;

        internal Texture(TextureWrapper wrapper, string name)
        {
            _texture = wrapper;
            Name = name;
        }

        public TextureTarget Target => _texture.Target;

        public void Bind() => _texture.Bind();
        public void Bind(int unit) => _texture.Bind(unit);

        public override string ToString() => Name == null ? $"{Target} ({Id})" : $"{Target} \'{Name}\' ({Id})";

        public static Texture FromBitmap(Bitmap bmp, string name = null)
        {
            var wrapper= new TextureWrapper(TextureTarget.Texture2D);
            var service = new Texture(wrapper, null);

            Logger.Debug("Created texture {0}", service);

            wrapper.Bind();
            wrapper.TexParameter(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            wrapper.TexParameter(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            wrapper.Image2D(PixelInternalFormat.Rgba, bmp.Width,bmp.Height,PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            return service;
        }


        public static Texture FromFile(string path)
        {
            return FromBitmap(new Bitmap(path));
        }
    }
}