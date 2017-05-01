using System;

namespace pgdiff.parsers
{
    public class ParserException : Exception
    {

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