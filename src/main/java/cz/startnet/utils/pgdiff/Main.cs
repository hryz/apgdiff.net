
using System;

namespace cz.startnet.utils.pgdiff {






public class Main {

    
    public static void main(String[] args) {
        
        PrintWriter writer = new PrintWriter(System.out, true);
        PgDiffArguments arguments = new PgDiffArguments();

        if (arguments.parse(writer, args)) {
            
            PrintWriter encodedWriter = new PrintWriter(
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
}