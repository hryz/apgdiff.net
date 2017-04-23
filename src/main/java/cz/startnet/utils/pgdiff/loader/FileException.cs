

using System;

namespace pgdiff.loader {


public class FileException : Exception {

    
    private static long serialVersionUID = 1L;

    
    public FileException() {
    }

    
    public FileException(String msg): base(msg) { }

    
    public FileException(String msg, Exception cause) : base(msg, cause) { }
}
}