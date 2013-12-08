using System;
using System.Diagnostics;
using System.Text;

namespace FuncitonInterpreter
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
