
package cz.startnet.utils.pgdiff.loader;


public class FileException extends RuntimeException {

    
    private static final long serialVersionUID = 1L;

    
    public FileException() {
    }

    
    public FileException(final String msg) {
        super(msg);
    }

    
    public FileException(final String msg, final Throwable cause) {
        super(msg, cause);
    }
}
