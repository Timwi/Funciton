using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuncitonInterpreter
{
    static class ExtensionMethods
    {
        /// <summary>
        ///     Similar to <see cref="string.Substring(int,int)"/>, only for arrays. Returns a new array containing <paramref
        ///     name="length"/> items from the specified <paramref name="startIndex"/> onwards.</summary>
        public static T[] Subarray<T>(this T[] array, int startIndex, int length)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "startIndex cannot be negative.");
            if (length < 0 || startIndex + length > array.Length)
                throw new ArgumentOutOfRangeException("length", "length cannot be negative or extend beyond the end of the array.");
            T[] result = new T[length];
            Array.Copy(array, startIndex, result, 0, length);
            return result;
        }

        /// <summary>Efficient union for very small arrays.</summary>
        public static T[] ArrayUnion<T>(this T[] array, T[] otherArray)
        {
            if (array.Length == 0)
                return otherArray;
            if (otherArray.Length == 0)
                return array;
            var list = new List<T>(array);
            foreach (var item in otherArray)
                if (Array.IndexOf(array, item) == -1)
                    list.Add(item);
            return list.ToArray();
        }

        /// <summary>Retrieve a value from a dictionary, but return a default value if the key is not in the dictionary.</summary>
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue @default)
        {
            TValue val;
            if (dic.TryGetValue(key, out val))
                return val;
            return @default;
        }

        /// <summary>Formats a string using <see cref="string.Format(string, object[])"/>.</summary>
        public static string Fmt(this string formatString, params object[] args)
        {
            return string.Format(formatString, args);
        }

        /// <summary>Formats a string using <see cref="string.Format(string, object)"/>.</summary>
        public static string Fmt(this string formatString, object arg0)
        {
            return string.Format(formatString, arg0);
        }

        /// <summary>Formats a string using <see cref="string.Format(string, object, object)"/>.</summary>
        public static string Fmt(this string formatString, object arg0, object arg1)
        {
            return string.Format(formatString, arg0, arg1);
        }

        /// <summary>Formats a string using <see cref="string.Format(string, object, object, object)"/>.</summary>
        public static string Fmt(this string formatString, object arg0, object arg1, object arg2)
        {
            return string.Format(formatString, arg0, arg1, arg2);
        }
    }
}
