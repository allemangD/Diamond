using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace hexworld
{
    public struct Vert
    {
        public static readonly int SizeInBytes = sizeof(float) * 5;

        public Vector3 Point;
        public Vector2 TexCoord;

        public Vert(Vector3 point, Vector2 texCoord)
        {
            Point = point;
            TexCoord = texCoord;
        }

        public Vert(float px, float py, float pz, float tx, float ty)
            : this(new Vector3(px, py, pz), new Vector2(tx, ty))
        {
        }
    }

    public class HexWindow : GameWindow
    {
        private readonly Vert[] _verts =
        {
            // +X
            new Vert(+1, +1, -1, 1.0f, 0.5f), new Vert(+1, +1, +1, 1.0f, 0.0f), new Vert(+1, -1, +1, 0.5f, 0.0f),
            new Vert(+1, -1, +1, 0.5f, 0.0f), new Vert(+1, -1, -1, 0.5f, 0.5f), new Vert(+1, +1, -1, 1.0f, 0.5f),
            // -X
            new Vert(-1, +1, +1, 0.5f, 0.0f), new Vert(-1, +1, -1, 0.5f, 0.5f), new Vert(-1, -1, -1, 1.0f, 0.5f),
            new Vert(-1, -1, -1, 1.0f, 0.5f), new Vert(-1, -1, +1, 1.0f, 0.0f), new Vert(-1, +1, +1, 0.5f, 0.0f),
            // +Y
            new Vert(+1, +1, -1, 0.5f, 0.5f), new Vert(-1, +1, -1, 1.0f, 0.5f), new Vert(-1, +1, +1, 1.0f, 0.0f),
            new Vert(-1, +1, +1, 1.0f, 0.0f), new Vert(+1, +1, +1, 0.5f, 0.0f), new Vert(+1, +1, -1, 0.5f, 0.5f),
            // -Y
            new Vert(+1, -1, +1, 1.0f, 0.0f), new Vert(-1, -1, +1, 0.5f, 0.0f), new Vert(-1, -1, -1, 0.5f, 0.5f),
            new Vert(-1, -1, -1, 0.5f, 0.5f), new Vert(+1, -1, -1, 1.0f, 0.5f), new Vert(+1, -1, +1, 1.0f, 0.0f),
            // +Z
            new Vert(+1, +1, +1, 0.5f, 0.0f), new Vert(-1, +1, +1, 0.0f, 0.0f), new Vert(-1, -1, +1, 0.0f, 0.5f),
            new Vert(-1, -1, +1, 0.0f, 0.5f), new Vert(+1, -1, +1, 0.5f, 0.5f), new Vert(+1, +1, +1, 0.5f, 0.0f),
            // -Z
            new Vert(+1, +1, -1, 0.5f, 0.5f), new Vert(-1, +1, -1, 0.0f, 0.5f), new Vert(-1, -1, -1, 0.0f, 1.0f),
            new Vert(-1, -1, -1, 0.0f, 1.0f), new Vert(+1, -1, -1, 0.5f, 1.0f), new Vert(+1, +1, -1, 0.5f, 0.5f),
        };

        private Matrix4 _view = Matrix4.Identity;
        private Matrix4 _proj = Matrix4.Identity;

        private int _pgm;
        private int _texLoc;
        private int _tex2Loc;

        public HexWindow(int width, int height)
            : base(width, height)
        {
            Width = width;
            Height = height;
            X = (DisplayDevice.Default.Width - Width) / 2;
            Y = (DisplayDevice.Default.Height - Height) / 2;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            const float s = 200f;
            _view = Matrix4.LookAt(10 * Vector3.One, Vector3.Zero, Vector3.UnitZ);
            _proj = Matrix4.CreateOrthographic(Width / s, Height / s, 0, 100);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var vertsVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertsVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (_verts.Length * Vert.SizeInBytes), _verts,
                BufferUsageHint.StaticDraw);

            var vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, File.ReadAllText("s.vs.glsl"));
            GL.CompileShader(vs);
            Console.Out.WriteLine("vs:" + GL.GetShaderInfoLog(vs));

            var fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, File.ReadAllText("s.fs.glsl"));
            GL.CompileShader(fs);
            Console.Out.WriteLine("fs: " + GL.GetShaderInfoLog(fs));

            _pgm = GL.CreateProgram();
            GL.AttachShader(_pgm, vs);
            GL.AttachShader(_pgm, fs);
            GL.LinkProgram(_pgm);
            Console.Out.WriteLine("pgm: " + GL.GetProgramInfoLog(_pgm));

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertsVbo);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vert.SizeInBytes, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vert.SizeInBytes, Vector3.SizeInBytes);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            var texBmp = new Bitmap("tex.png");
            _texLoc = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _texLoc);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Nearest);
            var texData = texBmp.LockBits(new Rectangle(0, 0, texBmp.Width, texBmp.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, texBmp.Width, texBmp.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, texData.Scan0);
            texBmp.UnlockBits(texData);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            var tex2Bmp = new Bitmap("tex2.png");
            _tex2Loc = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _tex2Loc);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Nearest);
            var tex2Data = tex2Bmp.LockBits(new Rectangle(0, 0, tex2Bmp.Width, tex2Bmp.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, tex2Bmp.Width, tex2Bmp.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, tex2Data.Scan0);
            tex2Bmp.UnlockBits(tex2Data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Viewport(ClientRectangle);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            GL.UseProgram(_pgm);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _tex2Loc);
            GL.Uniform1(GL.GetUniformLocation(_pgm, "tex"), 0);

            GL.UniformMatrix4(GL.GetUniformLocation(_pgm, "view"), false, ref _view);
            GL.UniformMatrix4(GL.GetUniformLocation(_pgm, "proj"), false, ref _proj);

            GL.DrawArrays(PrimitiveType.Triangles, 0, _verts.Length);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            SwapBuffers();
        }
    }
}