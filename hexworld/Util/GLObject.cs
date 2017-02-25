using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace hexworld.Util
{
    public abstract class GLObject
    {
        public uint Id { get; protected set; }

        protected GLObject(uint id)
        {
            Id = id;
        }

        public static explicit operator uint(GLObject o) => o.Id;
        public static explicit operator int(GLObject o) => (int) o.Id;
    }
}