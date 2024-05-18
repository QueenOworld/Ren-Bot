using System;
using System.Collections.Generic;
using System.Linq;

namespace StutterMosher
{
    // Extension methods
    public static class ListExtensions
    {
        // Find the index of an ordered sequence of elements in a list.
        // Returns -1 if sequence is not found.
        public static int IndexOf<T>(this List<T> list, List<T> other)
        {
            for (int i = 0; i < list.Count - other.Count; i++)
            {
                if (list[i].Equals(other.First()) && list.GetRange(i, other.Count).SequenceEqual(other))
                    return i;
            }
            return -1;
        }
    }
}
