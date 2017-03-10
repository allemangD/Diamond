using System;
using System.Collections;
using System.Collections.Generic;

namespace Diamond.Util
{
    /// <summary>
    /// Provices access to a subset of an array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SubArray<T> : IEnumerable<T>
    {
        /// <summary>
        /// The array that this references
        /// </summary>
        public T[] Array { get; set; }

        /// <summary>
        /// The offset of this subarray
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// The length of this subarray
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// By-ref access to the underlying array
        /// </summary>
        /// <param name="i">The index of the subarray to access</param>
        /// <returns>A reference to the offset position </returns>
        public ref T this[int i]
        {
            get
            {
                if (i < 0 || i >= Length)
                    throw new IndexOutOfRangeException("Index out of bounds of subarray");
                return ref Array[i + Offset];
            }
        }

        /// <summary>
        /// Create a subarray covering an entire array
        /// </summary>
        /// <param name="array">The array to cover</param>
        public SubArray(T[] array)
            : this(array, 0, array.Length)
        {
        }

        /// <summary>
        /// Create a subarray from an array
        /// </summary>
        /// <param name="array">The array to cover</param>
        /// <param name="offset">The offset of the subarray</param>
        /// <param name="length">The length of the subarray</param>
        public SubArray(T[] array, int offset, int length)
        {
            if (offset + length > array.Length)
                throw new IndexOutOfRangeException($"Cannot create subarray with length {length}) of " +
                                                   $"array with length {array.Length} at index {offset}");

            Array = array;
            Offset = offset;
            Length = length;
        }

        /// <summary>
        /// Create a copy of the array within the bounds of the subarray
        /// </summary>
        /// <returns>A copied array from this subarray</returns>
        public T[] ToArray()
        {
            var arr = new T[Length];
            System.Array.ConstrainedCopy(Array, Offset, arr, 0, Length);
            return arr;
        }

        /// <summary>
        /// Create a copy of the array within the bounds of the subarray, and make this subarray cover that copy
        /// </summary>
        /// <returns>The new array that this subarray covers</returns>
        public T[] Extract()
        {
            var arr = ToArray();
            Offset = 0;
            Array = arr;
            return arr;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Length == 0)
                return "[ ]";

            return $"[{string.Join(", ", this)}]";
        }

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Enumerate over the array within the bounds of the subarray
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Length; i++)
                yield return Array[Offset + i];
        }

        #endregion
    }
}