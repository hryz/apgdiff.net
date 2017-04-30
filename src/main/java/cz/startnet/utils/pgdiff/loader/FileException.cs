using System;

namespace pgdiff.loader
{
    public class FileException : Exception
    {
        private static long _serialVersionUid = 1L;


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