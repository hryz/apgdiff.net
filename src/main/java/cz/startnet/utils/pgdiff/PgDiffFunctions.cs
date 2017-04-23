
package cz.startnet.utils.pgdiff;

import cz.startnet.utils.pgdiff.schema.PgFunction;
import cz.startnet.utils.pgdiff.schema.PgSchema;
import java.io.PrintWriter;


public class PgDiffFunctions {

    
    public static void createFunctions(final PrintWriter writer,
            final PgDiffArguments arguments, final PgSchema oldSchema,
            final PgSchema newSchema, final SearchPathHelper searchPathHelper) {
        // Add new functions and replace modified functions
        for (final PgFunction newFunction : newSchema.getFunctions()) {
            final PgFunction oldFunction;

            if (oldSchema == null) {
                oldFunction = null;
            } else {
                oldFunction = oldSchema.getFunction(newFunction.getSignature());
            }

            if ((oldFunction == null) || !newFunction.equals(
                    oldFunction, arguments.isIgnoreFunctionWhitespace())) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.println(newFunction.getCreationSQL());
            }
        }
    }

    
    public static void dropFunctions(final PrintWriter writer,
            final PgDiffArguments arguments, final PgSchema oldSchema,
            final PgSchema newSchema, final SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        // Drop functions that exist no more
        for (final PgFunction oldFunction : oldSchema.getFunctions()) {
            if (!newSchema.containsFunction(oldFunction.getSignature())) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.println(oldFunction.getDropSQL());
            }
        }
    }

    
    public static void alterComments(final PrintWriter writer,
            final PgSchema oldSchema, final PgSchema newSchema,
            final SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        for (final PgFunction oldfunction : oldSchema.getFunctions()) {
            final PgFunction newFunction =
                    newSchema.getFunction(oldfunction.getSignature());

            if (newFunction == null) {
                continue;
            }

            if (oldfunction.getComment() == null
                    && newFunction.getComment() != null
                    || oldfunction.getComment() != null
                    && newFunction.getComment() != null
                    && !oldfunction.getComment().equals(
                    newFunction.getComment())) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.print("COMMENT ON FUNCTION ");
                writer.print(PgDiffUtils.getQuotedName(newFunction.getName()));
                writer.print('(');

                boolean addComma = false;

                for (final PgFunction.Argument argument :
                        newFunction.getArguments()) {
                    if (addComma) {
                        writer.print(", ");
                    } else {
                        addComma = true;
                    }

                    writer.print(argument.getDeclaration(false));
                }

                writer.print(") IS ");
                writer.print(newFunction.getComment());
                writer.println(';');
            } else if (oldfunction.getComment() != null
                    && newFunction.getComment() == null) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.print("COMMENT ON FUNCTION ");
                writer.print(PgDiffUtils.getQuotedName(newFunction.getName()));
                writer.print('(');

                boolean addComma = false;

                for (final PgFunction.Argument argument :
                        newFunction.getArguments()) {
                    if (addComma) {
                        writer.print(", ");
                    } else {
                        addComma = true;
                    }

                    writer.print(argument.getDeclaration(false));
                }

                writer.println(") IS NULL;");
            }
        }
    }

    
    private PgDiffFunctions() {
    }
}
