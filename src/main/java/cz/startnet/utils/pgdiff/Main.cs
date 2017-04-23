
using System;

namespace cz.startnet.utils.pgdiff {






public class Main {

    
    public static void main(String[] args) {
        
        TextWriter writer = new TextWriter(System.out, true);
        PgDiffArguments arguments = new PgDiffArguments();

        if (arguments.parse(writer, args)) {
            
            TextWriter encodedWriter = new TextWriter(
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