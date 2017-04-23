

using System;

namespace cz.startnet.utils.pgdiff.loader {


public class FileException : Exception {

    
    private static long serialVersionUID = 1L;

    
    public FileException() {
    }

    
    public FileException(String msg) {
        super(msg);
    }

    
    public FileException(String msg, Throwable cause) {
        super(msg, cause);
    }
}
}