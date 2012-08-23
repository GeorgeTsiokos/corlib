using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Corlib {
    /// <summary>
    /// System.Char[] extension methods 
    /// </summary>
    public static class CharArrayExtensions {
        /// <summary>
        /// Single pass through <paramref name="source"/> looking for each occurence of any of the <paramref name="values"/>
        /// from <paramref name="startIndex"/> until <paramref name="count"/>
        /// </summary>
        /// <param name="source">the char[] to search</param>
        /// <param name="values">one or more char[] to search <paramref name="source"/> for</param>
        /// <param name="startIndex">where to start in <paramref name="source"/></param>
        /// <param name="count">how many chars to eval in <paramref name="source"/></param>
        /// <returns>the char[] that was found, and the index</returns>
        public static IEnumerable<Tuple<char[], int>> IndexOfAll (this char[] source, char[][] values, int startIndex, int count) {
            Contract.Requires (null != source);
            Contract.Requires (null != values);
            Contract.Requires (startIndex > -1);
            Contract.Requires (startIndex > 0);

            int valuesLength = values.Length;
            if (source.Length < 1 || valuesLength < 1)
                yield break;

            bool result = false;
            for (int i = startIndex; i < count; i++) {
                for (int v = 0; v < valuesLength; v++) {
                    int valueLength = values[v].Length;
                    for (int vi = 0; vi < valueLength && i + vi < count; vi++) {
                        result = source[i + vi].Equals (values[v][vi]);
                        if (!result)
                            break;
                    }
                    if (result)
                        yield return new Tuple<char[], int> (values[v], i);
                }
            }
        }
    }
}