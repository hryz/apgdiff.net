using System;

namespace pgdiff.loader
{
    public class FileException : Exception
    {
        public FileException()
        {
        }


        public FileException(string msg) : base(msg)
        {
        }


        public FileException(string msg, Exception cause) : base(msg, cause)
        {
        }
    }
}