using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Diamond
{
    internal static class Extensions
    {
        private static readonly Dictionary<Type, int> ByteSizes = new Dictionary<Type, int>();

        public static int SizeInBytes(this Type type)
        {
            if (!ByteSizes.ContainsKey(type))
                ByteSizes[type] = Marshal.SizeOf(type);
            return ByteSizes[type];
        }

        public static int SizeInBytes<T>(this T[] arr) where T : struct
        {
            return typeof(T).SizeInBytes() * arr.Length;
        }
    }
}