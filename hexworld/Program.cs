using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace hexworld
{
    internal class Game : GameWindow
    {
        #region Constructors

        public Game()
            : this(1920, 1080)
        {
        }

        public Game(int width, int height)
            : base(width, height)
        {
            Width = width;
            Height = height;
            X = (DisplayDevice.Default.Width - Width) / 2;
            Y = (DisplayDevice.Default.Height - Height) / 2;
        }

        #endregion

        #region Shaders

        private int _pgmId;
        private int _vsId;
        private int _fsId;

        #region Handles

        #endregion

        private void LoadShader(string filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (var sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        #endregion

        #region Buffers

        private int _vboPosition;
        private int _cubeVbo;
        private int _ibo;


        #endregion

        #region Data

        private float[] _vertdata;
        private Cube[] _cubes;
        private int[] _indices;

        private Matrix4 _view;
        private Matrix4 _projection;

        #endregion

        private void InitProgram()
        {
            _pgmId = GL.CreateProgram();

            LoadShader("vs.glsl", ShaderType.VertexShader, _pgmId, out _vsId);
            LoadShader("fs.glsl", ShaderType.FragmentShader, _pgmId, out _fsId);

            GL.LinkProgram(_pgmId);
            Console.WriteLine(GL.GetProgramInfoLog(_pgmId));

            GL.GenBuffers(1, out _vboPosition);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboPosition);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (_vertdata.Length * sizeof(float)), _vertdata,
                BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.GenBuffers(1, out _cubeVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _cubeVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (_cubes.Length * sizeof(float) * 6), _cubes,
                BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribDivisor(1, 1);

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribDivisor(2, 1);

            GL.GenBuffers(1, out _ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr) (_indices.Length * sizeof(int)), _indices,
                BufferUsageHint.StaticDraw);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);


            _vertdata = new[]
            {
                -.5f, -.5f, -.5f,
                -.5f, -.5f, .5f,
                -.5f, .5f, -.5f,
                -.5f, .5f, .5f,
                .5f, -.5f, -.5f,
                .5f, -.5f, .5f,
                .5f, .5f, -.5f,
                .5f, .5f, .5f,
            };

            _indices = new[]
            {
                0, 1, 2, 1, 2, 3,
                4, 5, 6, 5, 6, 7,
                0, 1, 4, 1, 4, 5,
                2, 3, 6, 3, 6, 7,
                0, 2, 4, 2, 4, 6,
                1, 3, 5, 3, 5, 7
            };

            _cubes = new[]
            {
                new Cube(new Vector3(2, 2, 0), new Vector3(0.9f, 0.1f, 0.1f)),
                new Cube(new Vector3(2, 1, 0), new Vector3(0.1f, 0.1f, 0.1f)),
                new Cube(new Vector3(2, 0, 0), new Vector3(0.1f, 0.9f, 0.1f)),
                new Cube(new Vector3(2, -1, 0), new Vector3(0.1f, 0.1f, 0.1f)),
                new Cube(new Vector3(2, -2, 0), new Vector3(0.9f, 0.1f, 0.1f)),
                new Cube(new Vector3(1, 2, 0), new Vector3(0.1f, 0.1f, 0.1f)),
                new Cube(new Vector3(1, -2, 0), new Vector3(0.1f, 0.1f, 0.1f)),
                new Cube(new Vector3(0, 2, 0), new Vector3(0.1f, 0.1f, 0.9f)),
                new Cube(new Vector3(0, -2, 0), new Vector3(0.1f, 0.1f, 0.9f)),
                new Cube(new Vector3(-1, 2, 0), new Vector3(0.1f, 0.1f, 0.1f)),
                new Cube(new Vector3(-1, -2, 0), new Vector3(0.1f, 0.1f, 0.1f)),
                new Cube(new Vector3(-2, 2, 0), new Vector3(0.9f, 0.1f, 0.1f)),
                new Cube(new Vector3(-2, 1, 0), new Vector3(0.1f, 0.1f, 0.1f)),
                new Cube(new Vector3(-2, 0, 0), new Vector3(0.1f, 0.9f, 0.1f)),
                new Cube(new Vector3(-2, -1, 0), new Vector3(0.1f, 0.1f, 0.1f)),
                new Cube(new Vector3(-2, -2, 0), new Vector3(0.9f, 0.1f, 0.1f)),
            };

            InitProgram();

            _view = _projection = Matrix4.Identity;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Viewport(ClientRectangle);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);


            GL.DrawElementsInstanced(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero,
                _cubes.Length);

            GL.Flush();
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            _view = Matrix4.LookAt(Vector3.Zero, -Vector3.One, Vector3.UnitZ);

            GL.UniformMatrix4(0, false, ref _view);
            GL.UniformMatrix4(1, false, ref _projection);

            GL.UseProgram(_pgmId);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            float size = 100;
            _projection = Matrix4.CreateOrthographic(Width / size, Height / size, -50, 50);
        }

        public static void Main(string[] args)
        {
            Game gw;
            using (gw = new Game())
            {
                gw.Run();
            }
        }
    }
}