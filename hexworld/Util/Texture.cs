using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace hexworld.Util
{
    public class Texture : GLObject
    {
        public TextureTarget Target;

        public Texture(TextureTarget target = TextureTarget.Texture2D)
            : base((uint) GL.GenTexture())
        {
            Target = target;
        }

        public void Bind()
        {
            GL.BindTexture(Target, Id);
        }

        public void Bind(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            Bind();
        }

        public void Bind(int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + unit);
            Bind();
        }

        public void Unbind()
        {
            GL.BindTexture(Target, 0);
        }

        public void Unbind(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            Unbind();
        }

        public void Unbind(int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + unit);
            Unbind();
        }

        public static Texture FromBitmap(Bitmap bmp)
        {
            var tex = new Texture(TextureTarget.Texture2D);
            tex.Bind();
            GL.TexParameter(tex.Target, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            GL.TexParameter(tex.Target, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(tex.Target, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);
            tex.Unbind();
            return tex;
        }
    }
}