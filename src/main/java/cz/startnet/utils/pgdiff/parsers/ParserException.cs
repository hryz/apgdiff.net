
using System;

namespace pgdiff.parsers {


public class ParserException : Exception {

    
    private static long _serialVersionUid = 1L;

    
    public ParserException() {
    }

    
    public ParserException(String msg) : base(msg)
    {
        
    }

    
    public ParserException(String msg, Exception cause) : base(msg, cause)
    {
        
    }
}
}