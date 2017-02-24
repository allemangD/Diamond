using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hexworld
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var gw = new HexWindow(1920, 1080))
            {
                gw.Run();
            }
        }
    }
}