using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace WhatTheTexture
{
    internal class Program : GameWindow
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Width = 1920;
            Height = 1080;
            X = (DisplayDevice.Default.Width - Width) / 2;
            Y = (DisplayDevice.Default.Height - Height) / 2;

            GL.Enable(EnableCap.Texture2D);

            var image = new Bitmap("tex.png");

            var texID = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texID);
            var bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Nearest);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);

            image.UnlockBits(bitmapData);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.Viewport(ClientRectangle);

            GL.Begin(PrimitiveType.Triangles);

            GL.TexCoord2(0, 0);
            GL.Vertex2(-.8, -.8);
            GL.TexCoord2(1, 0);
            GL.Vertex2(.8, -.8);
            GL.TexCoord2(0, 1);
            GL.Vertex2(-.8, .8);

            GL.TexCoord2(1, 1);
            GL.Vertex2(.8, .8);
            GL.TexCoord2(1, 0);
            GL.Vertex2(.8, -.8);
            GL.TexCoord2(0, 1);
            GL.Vertex2(-.8, .8);

            GL.End();

            SwapBuffers();
        }

        private static void Main(string[] args)
        {
            using (var p = new Program())
            {
                p.Run();
            }
        }
    }
}