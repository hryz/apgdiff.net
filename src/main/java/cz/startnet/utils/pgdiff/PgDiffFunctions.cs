using System.IO;
using pgdiff.schema;

namespace pgdiff {




public class PgDiffFunctions {

    
    public static void CreateFunctions(TextWriter writer,
            PgDiffArguments arguments, PgSchema oldSchema,
            PgSchema newSchema, SearchPathHelper searchPathHelper) {
        // Add new functions and replace modified functions
        foreach (PgFunction newFunction in newSchema.GetFunctions()) {
            PgFunction oldFunction;

            if (oldSchema == null) {
                oldFunction = null;
            } else {
                oldFunction = oldSchema.GetFunction(newFunction.GetSignature());
            }

            if ((oldFunction == null) || !newFunction.Equals(
                    oldFunction, arguments.IsIgnoreFunctionWhitespace())) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(newFunction.GetCreationSql());
            }
        }
    }

    
    public static void DropFunctions(TextWriter writer,
            PgDiffArguments arguments, PgSchema oldSchema,
            PgSchema newSchema, SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        // Drop functions that exist no more
        foreach (PgFunction oldFunction in oldSchema.GetFunctions()) {
            if (!newSchema.ContainsFunction(oldFunction.GetSignature())) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(oldFunction.GetDropSql());
            }
        }
    }

    
    public static void AlterComments(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        foreach (PgFunction oldfunction in oldSchema.GetFunctions()) {
            PgFunction newFunction =
                    newSchema.GetFunction(oldfunction.GetSignature());

            if (newFunction == null) {
                continue;
            }

            if (oldfunction.GetComment() == null
                    && newFunction.GetComment() != null
                    || oldfunction.GetComment() != null
                    && newFunction.GetComment() != null
                    && !oldfunction.GetComment().Equals(
                    newFunction.GetComment())) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON FUNCTION ");
                writer.Write(PgDiffUtils.GetQuotedName(newFunction.GetName()));
                writer.Write('(');

                bool addComma = false;

                foreach (PgFunction.Argument argument in newFunction.GetArguments()) {
                    if (addComma) {
                        writer.Write(", ");
                    } else {
                        addComma = true;
                    }

                    writer.Write(argument.GetDeclaration(false));
                }

                writer.Write(") IS ");
                writer.Write(newFunction.GetComment());
                writer.WriteLine(';');
            } else if (oldfunction.GetComment() != null
                    && newFunction.GetComment() == null) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON FUNCTION ");
                writer.Write(PgDiffUtils.GetQuotedName(newFunction.GetName()));
                writer.Write('(');

                bool addComma = false;

                foreach (PgFunction.Argument argument in newFunction.GetArguments()) {
                    if (addComma) {
                        writer.Write(", ");
                    } else {
                        addComma = true;
                    }

                    writer.Write(argument.GetDeclaration(false));
                }

                writer.WriteLine(") IS NULL;");
            }
        }
    }

    
    private PgDiffFunctions() {
    }
}
}