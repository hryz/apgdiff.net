
using System;

namespace pgdiff.parsers {


public class ParserException : Exception {

    
    private static long serialVersionUID = 1L;

    
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