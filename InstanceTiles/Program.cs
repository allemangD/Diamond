using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace InstanceTiles
{
    internal class Program
    {
        private float[] points =
        {
            +.5f, +.5f, +.5f, -.5f, +.5f, +.5f, -.5f, -.5f, +.5f,
            -.5f, -.5f, +.5f, +.5f, -.5f, +.5f, +.5f, +.5f, +.5f,
            -.5f, +.5f, -.5f, +.5f, +.5f, -.5f, +.5f, -.5f, -.5f,
            +.5f, -.5f, -.5f, -.5f, -.5f, -.5f, -.5f, +.5f, -.5f,
            +.5f, +.5f, -.5f, +.5f, +.5f, +.5f, +.5f, -.5f, +.5f,
            +.5f, -.5f, +.5f, +.5f, -.5f, -.5f, +.5f, +.5f, -.5f,
            -.5f, +.5f, +.5f, -.5f, +.5f, -.5f, -.5f, -.5f, -.5f,
            -.5f, -.5f, -.5f, -.5f, -.5f, +.5f, -.5f, +.5f, +.5f,
            +.5f, +.5f, -.5f, -.5f, +.5f, -.5f, -.5f, +.5f, +.5f,
            -.5f, +.5f, +.5f, +.5f, +.5f, +.5f, +.5f, +.5f, -.5f,
            +.5f, -.5f, +.5f, -.5f, -.5f, +.5f, -.5f, -.5f, -.5f,
            -.5f, -.5f, -.5f, +.5f, -.5f, -.5f, +.5f, -.5f, +.5f,
        };

        private float[] texCoords =
        {
            1.0f, 0.0f, 5.0f, 0.0f, 5.0f, 1.0f,
            0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 5.0f,
            1.0f, 0.5f, 0.0f, 0.5f, 0.0f, 1.0f,
            0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f,
            0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.5f,
            0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.5f,
            0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f,
            0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f,
            0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f,
            0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f,
        };

        private float[] cubePoints =
        {
            0, 0, 0,
            1, 0, 0,
            0, 1, 0,
        };

        private float[] cubeTexCoords =
        {
            0.05f
        };

        private static void Main(string[] args)
        {

        }
    }
}