using System;
using System.Diagnostics;

namespace FuncitonInterpreter
{
    static class Helpers
    {
        /// <summary>Throws the specified exception.</summary>
        /// <typeparam name="TResult">The type to return.</typeparam>
        /// <param name="exception">The exception to throw.</param>
        /// <returns>This method never returns a value. It always throws.</returns>
        public static TResult Throw<TResult>(Exception exception)
        {
            throw exception;
        }

        /// <summary>Checks the specified condition and causes the debugger to break if it is false. Throws an <see cref="InternalErrorException"/> afterwards.</summary>
        public static void Assert(bool assertion)
        {
            if (!assertion)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw new InternalErrorException("Assertion failure");
            }
        }
    }
}
