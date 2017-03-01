using System.Drawing;
using System.Drawing.Imaging;
using Diamond.Wrappers;
using NLog;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Diamond.Textures
{
    public class Texture : GLObject
    {
        internal static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly TextureWrap _texture;
        internal override Wrapper Wrapper => _texture;

        internal Texture(TextureWrap wrapper, string name)
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
            var wrapper= new TextureWrap(TextureTarget.Texture2D);
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