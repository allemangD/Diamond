using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace hexworld
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Cube
    {
        public Vector3 Position;
        public Vector3 Color;

        public Cube(Vector3 position, Vector3 color)
        {
            Position = position;
            Color = color;
        }
    }
}
