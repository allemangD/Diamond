using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTK;

namespace hexworld
{
    public class Driver
    {
        public static void Main(string[] args)
        {
            using (var gw = new HexRender(1280, 720)) gw.Run();
        }
    }
}