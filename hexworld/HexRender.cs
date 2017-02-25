using System;
using System.Drawing;
using System.IO;
using hexworld.Util;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace hexworld
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 UV;
        public Vector3 Normal;

        public Vertex(Vector3 position, Vector2 uv, Vector3 normal)
        {
            Position = position;
            UV = uv;
            Normal = normal;
        }
    }

    public struct Tile
    {
        public Vector3 Position;

        public Tile(Vector3 position)
        {
            Position = position;
        }
    }

    public class HexRender : GameWindow
    {
        private Program pgm;
        private Texture grass;
        private Texture stone;
        private Texture tex2;

        private Matrix4 view;
        private Matrix4 proj;

        private VBO tileVbo;
        private VBO cubeVbo;

        private readonly Vertex[] cubeVerts =
        {
            // +X
            new Vertex(new Vector3(+.5f, +.5f, -.5f), new Vector2(1.0f, 0.5f), new Vector3(+1, +0, +0)),
            new Vertex(new Vector3(+.5f, +.5f, +.5f), new Vector2(1.0f, 0.0f), new Vector3(+1, +0, +0)),
            new Vertex(new Vector3(+.5f, -.5f, +.5f), new Vector2(0.5f, 0.0f), new Vector3(+1, +0, +0)),
            new Vertex(new Vector3(+.5f, -.5f, +.5f), new Vector2(0.5f, 0.0f), new Vector3(+1, +0, +0)),
            new Vertex(new Vector3(+.5f, -.5f, -.5f), new Vector2(0.5f, 0.5f), new Vector3(+1, +0, +0)),
            new Vertex(new Vector3(+.5f, +.5f, -.5f), new Vector2(1.0f, 0.5f), new Vector3(+1, +0, +0)),
            // -X
            new Vertex(new Vector3(-.5f, +.5f, +.5f), new Vector2(0.5f, 0.0f), new Vector3(-1, +0, +0)),
            new Vertex(new Vector3(-.5f, +.5f, -.5f), new Vector2(0.5f, 0.5f), new Vector3(-1, +0, +0)),
            new Vertex(new Vector3(-.5f, -.5f, -.5f), new Vector2(1.0f, 0.5f), new Vector3(-1, +0, +0)),
            new Vertex(new Vector3(-.5f, -.5f, -.5f), new Vector2(1.0f, 0.5f), new Vector3(-1, +0, +0)),
            new Vertex(new Vector3(-.5f, -.5f, +.5f), new Vector2(1.0f, 0.0f), new Vector3(-1, +0, +0)),
            new Vertex(new Vector3(-.5f, +.5f, +.5f), new Vector2(0.5f, 0.0f), new Vector3(-1, +0, +0)),
            // +Y
            new Vertex(new Vector3(+.5f, +.5f, -.5f), new Vector2(0.5f, 0.5f), new Vector3(+0, +1, +0)),
            new Vertex(new Vector3(-.5f, +.5f, -.5f), new Vector2(1.0f, 0.5f), new Vector3(+0, +1, +0)),
            new Vertex(new Vector3(-.5f, +.5f, +.5f), new Vector2(1.0f, 0.0f), new Vector3(+0, +1, +0)),
            new Vertex(new Vector3(-.5f, +.5f, +.5f), new Vector2(1.0f, 0.0f), new Vector3(+0, +1, +0)),
            new Vertex(new Vector3(+.5f, +.5f, +.5f), new Vector2(0.5f, 0.0f), new Vector3(+0, +1, +0)),
            new Vertex(new Vector3(+.5f, +.5f, -.5f), new Vector2(0.5f, 0.5f), new Vector3(+0, +1, +0)),
            // -Y
            new Vertex(new Vector3(+.5f, -.5f, +.5f), new Vector2(1.0f, 0.0f), new Vector3(+0, -1, +0)),
            new Vertex(new Vector3(-.5f, -.5f, +.5f), new Vector2(0.5f, 0.0f), new Vector3(+0, -1, +0)),
            new Vertex(new Vector3(-.5f, -.5f, -.5f), new Vector2(0.5f, 0.5f), new Vector3(+0, -1, +0)),
            new Vertex(new Vector3(-.5f, -.5f, -.5f), new Vector2(0.5f, 0.5f), new Vector3(+0, -1, +0)),
            new Vertex(new Vector3(+.5f, -.5f, -.5f), new Vector2(1.0f, 0.5f), new Vector3(+0, -1, +0)),
            new Vertex(new Vector3(+.5f, -.5f, +.5f), new Vector2(1.0f, 0.0f), new Vector3(+0, -1, +0)),
            // +Z
            new Vertex(new Vector3(+.5f, +.5f, +.5f), new Vector2(0.5f, 0.0f), new Vector3(+0, +0, +1)),
            new Vertex(new Vector3(-.5f, +.5f, +.5f), new Vector2(0.0f, 0.0f), new Vector3(+0, +0, +1)),
            new Vertex(new Vector3(-.5f, -.5f, +.5f), new Vector2(0.0f, 0.5f), new Vector3(+0, +0, +1)),
            new Vertex(new Vector3(-.5f, -.5f, +.5f), new Vector2(0.0f, 0.5f), new Vector3(+0, +0, +1)),
            new Vertex(new Vector3(+.5f, -.5f, +.5f), new Vector2(0.5f, 0.5f), new Vector3(+0, +0, +1)),
            new Vertex(new Vector3(+.5f, +.5f, +.5f), new Vector2(0.5f, 0.0f), new Vector3(+0, +0, +1)),
            // -Z
            new Vertex(new Vector3(+.5f, +.5f, -.5f), new Vector2(0.5f, 0.5f), new Vector3(+0, +0, -1)),
            new Vertex(new Vector3(-.5f, +.5f, -.5f), new Vector2(0.0f, 0.5f), new Vector3(+0, +0, -1)),
            new Vertex(new Vector3(-.5f, -.5f, -.5f), new Vector2(0.0f, 1.0f), new Vector3(+0, +0, -1)),
            new Vertex(new Vector3(-.5f, -.5f, -.5f), new Vector2(0.0f, 1.0f), new Vector3(+0, +0, -1)),
            new Vertex(new Vector3(+.5f, -.5f, -.5f), new Vector2(0.5f, 1.0f), new Vector3(+0, +0, -1)),
            new Vertex(new Vector3(+.5f, +.5f, -.5f), new Vector2(0.5f, 0.5f), new Vector3(+0, +0, -1)),

            // Plane
            new Vertex(new Vector3(+.5f, +.5f, 0.0f), new Vector2(0.5f, 0.0f), new Vector3(+0, +0, +1)),
            new Vertex(new Vector3(-.5f, +.5f, 0.0f), new Vector2(0.0f, 0.0f), new Vector3(+0, +0, +1)),
            new Vertex(new Vector3(-.5f, -.5f, 0.0f), new Vector2(0.0f, 0.5f), new Vector3(+0, +0, +1)),
            new Vertex(new Vector3(-.5f, -.5f, 0.0f), new Vector2(0.0f, 0.5f), new Vector3(+0, +0, +1)),
            new Vertex(new Vector3(+.5f, -.5f, 0.0f), new Vector2(0.5f, 0.5f), new Vector3(+0, +0, +1)),
            new Vertex(new Vector3(+.5f, +.5f, 0.0f), new Vector2(0.5f, 0.0f), new Vector3(+0, +0, +1)),
        };

        private Tile[] tiles =
        {
            // Grass
            new Tile(new Vector3(-2, -2, 0)),
            new Tile(new Vector3(-2, -1, 0)),
            new Tile(new Vector3(-2, +0, 0)),
            new Tile(new Vector3(-2, +1, 0)),
            new Tile(new Vector3(-2, +2, 0)),
            new Tile(new Vector3(+2, -2, 0)),
            new Tile(new Vector3(+2, -1, 0)),
            new Tile(new Vector3(+2, +0, 0)),
            new Tile(new Vector3(+2, +1, 0)),
            new Tile(new Vector3(+2, +2, 0)),
            new Tile(new Vector3(-1, -2, 0)),
            new Tile(new Vector3(-1, +2, 0)),
            new Tile(new Vector3(+0, -2, 0)),
            new Tile(new Vector3(+0, +2, 0)),
            new Tile(new Vector3(+1, -2, 0)),
            new Tile(new Vector3(+1, +2, 0)),
            // Stone
            new Tile(new Vector3(+0, +0, +1)),
            new Tile(new Vector3(+0, +1, +1)),
            new Tile(new Vector3(+0, -1, +1)),
            new Tile(new Vector3(+1, +0, +1)),
            new Tile(new Vector3(-1, +0, +1)),
        };

        public HexRender(int width, int height) : base(width, height, new GraphicsMode(32, 32, 0, 0))
        {
            Width = width;
            Height = Height;
            X = (DisplayDevice.Default.Width - Width) / 2;
            Y = (DisplayDevice.Default.Height - Height) / 2;
        }

        private Random rand = new Random();
        private double t;

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            t += e.Time;

            view = Matrix4.LookAt(10 * Vector3.One, Vector3.Zero, Vector3.UnitZ);
            proj = Matrix4.CreateOrthographic(Width / 100f, Height / 100f, -100, 100);

            for (var i = 0; i < 16; i++)
            {
                var ti = tiles[i];
                tiles[i].Position.Z = (float) (Math.Sin((t + ti.Position.X - ti.Position.Y / 1.5) / 1.5) * .25);
            }

//            tileVbo.Data(tiles, BufferUsageHint.DynamicDraw);
            tileVbo.Bind();
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr) (0), (IntPtr) (16 * 3 * sizeof(float)), tiles);
            VBO.Unbind();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cubeVbo = new VBO();
            cubeVbo.Data(cubeVerts, BufferUsageHint.StaticDraw);

            tileVbo = new VBO();
            tileVbo.Data(tiles, BufferUsageHint.DynamicDraw);

            var vs = new Shader(ShaderType.VertexShader)
            {
                Source = File.ReadAllText("s.vs.glsl")
            };
            vs.Compile();
            Console.Out.WriteLine(vs.Log);

            var fs = new Shader(ShaderType.FragmentShader)
            {
                Source = File.ReadAllText("s.fs.glsl")
            };
            fs.Compile();
            Console.Out.WriteLine(fs.Log);

            pgm = new Program();
            pgm.Attach(vs);
            pgm.Attach(fs);
            pgm.Link();
            Console.Out.WriteLine(pgm.Log);

            var pos = pgm.GetAttribute("locpos");
            var crd = pgm.GetAttribute("coord");
            var nrm = pgm.GetAttribute("norm");
            var tpos = pgm.GetAttribute("glbpos");

            GL.EnableVertexAttribArray(pos);
            GL.EnableVertexAttribArray(crd);
            GL.EnableVertexAttribArray(nrm);
            GL.EnableVertexAttribArray(tpos);

            cubeVbo.Bind();

            GL.VertexAttribPointer(pos, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float),
                0);
            GL.VertexAttribPointer(crd, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float),
                3 * sizeof(float));
            GL.VertexAttribPointer(nrm, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float),
                5 * sizeof(float));

            tileVbo.Bind();

            GL.VertexAttribPointer(tpos, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.VertexAttribDivisor(tpos, 1);

            VBO.Unbind();

            grass = Texture.FromBitmap(new Bitmap("grass.png"));
            stone = Texture.FromBitmap(new Bitmap("stone.png"));
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Viewport(ClientRectangle);

            GL.ClearColor(0.2392157F,0.5607843F,0.9960784F, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


            pgm.Use();

            grass.Bind(0);
            stone.Bind(1);

            GL.Uniform1(pgm.GetUniform("tex"), 0);
            GL.UniformMatrix4(pgm.GetUniform("view"), false, ref view);
            GL.UniformMatrix4(pgm.GetUniform("proj"), false, ref proj);

            GL.DrawArraysInstancedBaseInstance(PrimitiveType.Triangles, 0, 36, 16, 0);

            GL.Uniform1(pgm.GetUniform("tex"), 1);
            GL.UniformMatrix4(pgm.GetUniform("view"), false, ref view);
            GL.UniformMatrix4(pgm.GetUniform("proj"), false, ref proj);

//            GL.DrawArraysInstancedBaseInstance(PrimitiveType.Triangles, 36, 6, 5, 16);
            GL.DrawArraysInstancedBaseInstance(PrimitiveType.Triangles, 0, 36, 5, 16);

            SwapBuffers();
        }
    }
}