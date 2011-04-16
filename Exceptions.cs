using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuncitonInterpreter
{
    /// <summary>Indicates that the parser discovered a syntax error in the program.</summary>
    sealed class ParseErrorException : Exception
    {
        /// <summary>Specifies a list of indexes into the relevant source file where the error occurred.</summary>
        public int[] Indexes { get; private set; }
        /// <summary>Constructor.</summary>
        public ParseErrorException(string message, params int[] index) : base(message) { }
    }

    /// <summary>
    /// Represents an internal error in the code. Any place where the code is able
    /// to verify its own consistency is where this exception should be thrown, for
    /// example in “unreachable” code safeguards.
    /// </summary>
    public sealed class InternalErrorException : Exception
    {
        /// <summary>Creates an exception instance with the specified message.</summary>
        public InternalErrorException(string message)
            : base(message)
        { }

        /// <summary>Creates an exception instance with the specified message and inner exception.</summary>
        public InternalErrorException(string message, Exception innerException)
            : base(null, innerException)
        { }
    }
}
