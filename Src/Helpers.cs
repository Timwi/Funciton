using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Funciton
{
    static class Helpers
    {
        /// <summary>
        ///     Throws the specified exception.</summary>
        /// <typeparam name="TResult">
        ///     The type to return.</typeparam>
        /// <param name="exception">
        ///     The exception to throw.</param>
        /// <returns>
        ///     This method never returns a value. It always throws.</returns>
        public static TResult Throw<TResult>(Exception exception)
        {
            throw exception;
        }

        /// <summary>
        ///     Checks the specified condition and causes the debugger to break if it is false. Throws an <see
        ///     cref="InternalErrorException"/> afterwards.</summary>
        public static void Assert(bool assertion)
        {
            if (!assertion)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw new InternalErrorException("Assertion failure");
            }
        }

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
            if (array == null || array.Length == 0)
                return otherArray;
            if (otherArray == null || otherArray.Length == 0)
                return array;
            var hashSet = new HashSet<T>(array);
            foreach (var item in otherArray)
                hashSet.Add(item);
            return hashSet.ToArray();
        }

        /// <summary>Adds a single element to an array if it’s not already in it.</summary>
        public static T[] ArrayUnion<T>(this T[] array, T element)
        {
            if (array == null || array.Length == 0)
                return new[] { element };
            if (array.Contains(element))
                return array;
            var newArray = new T[array.Length + 1];
            Array.Copy(array, newArray, array.Length);
            newArray[array.Length] = element;
            return newArray;
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

        /// <summary>Escapes a string according to C-like string escaping rules.</summary>
        public static string CLiteralEscape(this string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var result = new StringBuilder(value.Length + value.Length / 2);

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '\0': result.Append(@"\0"); break;
                    case '\a': result.Append(@"\a"); break;
                    case '\b': result.Append(@"\b"); break;
                    case '\t': result.Append(@"\t"); break;
                    case '\n': result.Append(@"\n"); break;
                    case '\v': result.Append(@"\v"); break;
                    case '\f': result.Append(@"\f"); break;
                    case '\r': result.Append(@"\r"); break;
                    case '\\': result.Append(@"\\"); break;
                    case '"': result.Append(@"\"""); break;
                    default:
                        if (c >= ' ')
                            result.Append(c);
                        else
                            result.AppendFormat(@"\x{0:X2}", (int) c);
                        break;
                }
            }

            return result.ToString();
        }
    }
}
