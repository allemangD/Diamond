using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diamond.Buffers;
using Newtonsoft.Json;
using OpenTK;

namespace hexworld
{
    public class Driver
    {
        public static void Main(string[] args)
        {
            using (var gw = new HexRender(1280, 720)) gw.Run();

//            var s1 = new SubArray<int>(new[] {1, 3, 5, 7, 9});
//            var s2 = new SubArray<int>(new[] {0, 2, 4, 6, 8});
//
//            Console.Out.WriteLine($"s1 = {s1}");
//            Console.Out.WriteLine($"s2 = {s2}");
//
//            var arr = SubArray<int>.Join(s1, s2);
//
//            Console.Out.WriteLine($"arr = {string.Join(", ", arr)}");
//            Console.Out.WriteLine($"s1 = {s1}");
//            Console.Out.WriteLine($"s2 = {s2}");
//
//            Console.ReadKey();
        }
    }
}