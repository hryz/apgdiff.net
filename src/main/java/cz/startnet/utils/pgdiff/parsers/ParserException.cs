using System;

namespace pgdiff.parsers
{
    public class ParserException : Exception
    {
        private static long _serialVersionUid = 1L;


        public ParserException()
        {
        }


        public ParserException(string msg) : base(msg)
        {
        }


        public ParserException(string msg, Exception cause) : base(msg, cause)
        {
        }
    }
}