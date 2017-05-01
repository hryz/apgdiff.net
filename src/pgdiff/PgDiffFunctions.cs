using System.IO;
using pgdiff.schema;

namespace pgdiff
{
    public class PgDiffFunctions
    {
        private PgDiffFunctions()
        {
        }

        public static void CreateFunctions(TextWriter writer, PgDiffArguments arguments, 
            PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            // Add new functions and replace modified functions
            foreach (var newFunction in newSchema.GetFunctions())
            {
                var oldFunction = oldSchema?.GetFunction(newFunction.GetSignature());

                if (oldFunction == null || !newFunction.Equals(oldFunction, arguments.IgnoreFunctionWhitespace))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(newFunction.GetCreationSql());
                }
            }
        }


        public static void DropFunctions(TextWriter writer, PgDiffArguments arguments, 
            PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            if (oldSchema == null)
                return;

            // Drop functions that exist no more
            foreach (var oldFunction in oldSchema.GetFunctions())
                if (!newSchema.ContainsFunction(oldFunction.GetSignature()))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(oldFunction.GetDropSql());
                }
        }


        public static void AlterComments(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            if (oldSchema == null)
                return;

            foreach (var oldFun in oldSchema.GetFunctions())
            {
                var newFunction = newSchema.GetFunction(oldFun.GetSignature());

                if (newFunction == null)
                    continue;

                if (oldFun.Comment == null
                    && newFunction.Comment != null
                    || oldFun.Comment != null
                    && newFunction.Comment != null
                    && !oldFun.Comment.Equals(
                        newFunction.Comment))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON FUNCTION ");
                    writer.Write(PgDiffUtils.GetQuotedName(newFunction.Name));
                    writer.Write('(');

                    var addComma = false;

                    foreach (var argument in newFunction.GetArguments())
                    {
                        if (addComma)
                            writer.Write(", ");
                        else
                            addComma = true;

                        writer.Write(argument.GetDeclaration(false));
                    }

                    writer.Write(") IS ");
                    writer.Write(newFunction.Comment);
                    writer.WriteLine(';');
                }
                else if (oldFun.Comment != null && newFunction.Comment == null)
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON FUNCTION ");
                    writer.Write(PgDiffUtils.GetQuotedName(newFunction.Name));
                    writer.Write('(');

                    var addComma = false;

                    foreach (var argument in newFunction.GetArguments())
                    {
                        if (addComma)
                            writer.Write(", ");
                        else
                            addComma = true;

                        writer.Write(argument.GetDeclaration(false));
                    }

                    writer.WriteLine(") IS NULL;");
                }
            }
        }
    }
}