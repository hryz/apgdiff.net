
namespace cz.startnet.utils.pgdiff {






public class Main {

    
    public static void main(String[] args)
            throws UnsupportedEncodingException {
        @SuppressWarnings("UseOfSystemOutOrSystemErr")
        PrintWriter writer = new PrintWriter(System.out, true);
        PgDiffArguments arguments = new PgDiffArguments();

        if (arguments.parse(writer, args)) {
            @SuppressWarnings("UseOfSystemOutOrSystemErr")
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