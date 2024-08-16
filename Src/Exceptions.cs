using System;

namespace Funciton
{
    /// <summary>Represents a parse error, including location in the source.</summary>
    sealed class ParseError(string message, int? character = null, int? line = null, string sourceFile = null)
    {
        public string SourceFile { get; private set; } = sourceFile;
        public int? Character { get; private set; } = character;
        public int? Line { get; private set; } = line;
        public string Message { get; private set; } = message;
    }

    /// <summary>Indicates that the parser discovered a syntax error in the program.</summary>
    sealed class ParseErrorException(params ParseError[] errors) : Exception()
    {
        /// <summary>Specifies a list of errors that occurred.</summary>
        public ParseError[] Errors { get; private set; } = errors;
    }

    /// <summary>
    ///     Represents an internal error in the code. Any place where the code is able to verify its own consistency is where
    ///     this exception should be thrown, for example in “unreachable” code safeguards.</summary>
    public sealed class InternalErrorException : Exception
    {
        public InternalErrorException(string message)
            : base(message)
        { }

        /// <summary>Creates an exception instance with the specified message and inner exception.</summary>
        public InternalErrorException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
