using System.Collections.Generic;

namespace BinarySearchTree
{
    public static class ObjectExtensions
    {
        public static IEnumerable<T> Yield<T>(this T obj)
        {
            yield return obj;
        } 
    }
}
