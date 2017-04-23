
package cz.startnet.utils.pgdiff;

import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.io.UnsupportedEncodingException;


public class Main {

    
    public static void main(final String[] args)
            throws UnsupportedEncodingException {
        @SuppressWarnings("UseOfSystemOutOrSystemErr")
        final PrintWriter writer = new PrintWriter(System.out, true);
        final PgDiffArguments arguments = new PgDiffArguments();

        if (arguments.parse(writer, args)) {
            @SuppressWarnings("UseOfSystemOutOrSystemErr")
            final PrintWriter encodedWriter = new PrintWriter(
                    new OutputStreamWriter(
                    System.out, arguments.getOutCharsetName()));
            PgDiff.createDiff(encodedWriter, arguments);
            encodedWriter.close();
        }

        writer.close();
    }

    
    private Main() {
    }
}
