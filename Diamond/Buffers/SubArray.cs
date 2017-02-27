using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Diamond.Buffers
{
    public class SubArray<T> : IEnumerable<T>
    {
        public T[] Array;
        public int Offset;
        public int Length;

        public ref T this[int i]
        {
            get
            {
                if (i < 0 || i >= Length)
                    throw new IndexOutOfRangeException("Index out of bounds of subarray");
                return ref Array[i + Offset];
            }
        }

        public SubArray(T[] array) : this(array, 0, array.Length)
        {
        }

        public SubArray(T[] array, int offset, int length)
        {
            Array = array;
            Offset = offset;
            Length = length;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Length; i++)
                yield return Array[Offset + i];
        }

        public T[] ToArray()
        {
            var arr = new T[Length];
            System.Array.ConstrainedCopy(Array, Offset, arr, 0, Length);
            return arr;
        }

        public override string ToString()
        {
            if (Length == 0)
                return "[ ]";

            return $"[{string.Join(", ", this)}]";
        }
    }

    public static class SubArray
    {
        public static T[] Join<T>(params SubArray<T>[] subArrays) => Join((IEnumerable<SubArray<T>>)subArrays);

        public static T[] Join<T>(IEnumerable<SubArray<T>> subArrays)
        {
            HashSet<T[]> uniqueArrays = new HashSet<T[]>();
            foreach (var subArray in subArrays)
            {
                uniqueArrays.Add(subArray.Array);
            }

            if (uniqueArrays.Count == 0) return new T[0];
            if (uniqueArrays.Count == 1) return uniqueArrays.ToArray()[0];

            var length = 0;
            var offsets = new Dictionary<T[], int>();
            foreach (var uniqueArray in uniqueArrays)
            {
                offsets[uniqueArray] = length;
                length += uniqueArray.Length;
            }

            var array = new T[length];
            foreach (var uniqueArray in uniqueArrays)
            {
                System.Array.ConstrainedCopy(uniqueArray, 0, array, offsets[uniqueArray], uniqueArray.Length);
            }

            foreach (var subArray in subArrays)
            {
                subArray.Offset = offsets[subArray.Array];
                subArray.Array = array;
            }

            return array;
        }

    }
}