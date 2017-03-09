using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Diamond.Shaders;
using Diamond.Util;
using Newtonsoft.Json.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace hexworld
{
    public class HexRender : GameWindow
    {
        public HexRender(int width, int height)
            : base(width, height, new GraphicsMode(32, 24, 0, 0))
        {
            Width = width;
            Height = height;
            X = (DisplayDevice.Default.Width - Width) / 2;
            Y = (DisplayDevice.Default.Height - Height) / 2;
        }

        private Program _pgm;

        /// <inheritdoc />
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            _pgm.Dispose();
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _pgm = Program.FromFiles("res/obj.fs.glsl", "res/obj.vs.glsl");
        }
    }
}