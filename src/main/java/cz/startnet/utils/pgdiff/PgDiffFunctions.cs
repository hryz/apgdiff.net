
using cz.startnet.utils.pgdiff.schema;

namespace cz.startnet.utils.pgdiff {




public class PgDiffFunctions {

    
    public static void createFunctions(TextWriter writer,
            PgDiffArguments arguments, PgSchema oldSchema,
            PgSchema newSchema, SearchPathHelper searchPathHelper) {
        // Add new functions and replace modified functions
        for (PgFunction newFunction : newSchema.getFunctions()) {
            PgFunction oldFunction;

            if (oldSchema == null) {
                oldFunction = null;
            } else {
                oldFunction = oldSchema.getFunction(newFunction.getSignature());
            }

            if ((oldFunction == null) || !newFunction.Equals(
                    oldFunction, arguments.isIgnoreFunctionWhitespace())) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(newFunction.getCreationSQL());
            }
        }
    }

    
    public static void dropFunctions(TextWriter writer,
            PgDiffArguments arguments, PgSchema oldSchema,
            PgSchema newSchema, SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        // Drop functions that exist no more
        for (PgFunction oldFunction : oldSchema.getFunctions()) {
            if (!newSchema.containsFunction(oldFunction.getSignature())) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(oldFunction.getDropSQL());
            }
        }
    }

    
    public static void alterComments(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        for (PgFunction oldfunction : oldSchema.getFunctions()) {
            PgFunction newFunction =
                    newSchema.getFunction(oldfunction.getSignature());

            if (newFunction == null) {
                continue;
            }

            if (oldfunction.getComment() == null
                    && newFunction.getComment() != null
                    || oldfunction.getComment() != null
                    && newFunction.getComment() != null
                    && !oldfunction.getComment().Equals(
                    newFunction.getComment())) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON FUNCTION ");
                writer.Write(PgDiffUtils.getQuotedName(newFunction.getName()));
                writer.Write('(');

                bool addComma = false;

                for (PgFunction.Argument argument :
                        newFunction.getArguments()) {
                    if (addComma) {
                        writer.Write(", ");
                    } else {
                        addComma = true;
                    }

                    writer.Write(argument.getDeclaration(false));
                }

                writer.Write(") IS ");
                writer.Write(newFunction.getComment());
                writer.WriteLine(';');
            } else if (oldfunction.getComment() != null
                    && newFunction.getComment() == null) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON FUNCTION ");
                writer.Write(PgDiffUtils.getQuotedName(newFunction.getName()));
                writer.Write('(');

                bool addComma = false;

                for (PgFunction.Argument argument :
                        newFunction.getArguments()) {
                    if (addComma) {
                        writer.Write(", ");
                    } else {
                        addComma = true;
                    }

                    writer.Write(argument.getDeclaration(false));
                }

                writer.WriteLine(") IS NULL;");
            }
        }
    }

    
    private PgDiffFunctions() {
    }
}
}