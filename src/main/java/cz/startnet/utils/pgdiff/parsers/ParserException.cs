
package cz.startnet.utils.pgdiff.parsers;


public class ParserException extends RuntimeException {

    
    private static final long serialVersionUID = 1L;

    
    public ParserException() {
    }

    
    public ParserException(final String msg) {
        super(msg);
    }

    
    public ParserException(final String msg, final Throwable cause) {
        super(msg, cause);
    }
}
