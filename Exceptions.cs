using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuncitonInterpreter
{
    /// <summary>Represents a parse error, including location in the source.</summary>
    sealed class ParseError
    {
        public string SourceFile { get; set; }
        public int? Character { get; private set; }
        public int? Line { get; private set; }
        public string Message { get; private set; }
        public ParseError(string message, int? character = null, int? line = null, string sourceFile = null) { Line = line; Character = character; Message = message; SourceFile = sourceFile; }
    }

    /// <summary>Indicates that the parser discovered a syntax error in the program.</summary>
    sealed class ParseErrorException : Exception
    {
        /// <summary>Specifies a list of errors that occurred.</summary>
        public ParseError[] Errors { get; private set; }
        /// <summary>Constructor.</summary>
        public ParseErrorException(params ParseError[] errors) : base() { Errors = errors; }
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
