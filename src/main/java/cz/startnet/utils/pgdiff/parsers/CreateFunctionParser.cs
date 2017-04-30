using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers {




public class CreateFunctionParser {

    
    public static void Parse(PgDatabase database,
            String statement) {
        Parser parser = new Parser(statement);
        parser.Expect("CREATE");
        parser.ExpectOptional("OR", "REPLACE");
        parser.Expect("FUNCTION");

        String functionName = parser.ParseIdentifier();
        String schemaName =
                ParserUtils.GetSchemaName(functionName, database);
        PgSchema schema = database.GetSchema(schemaName);

        if (schema == null) {
            throw new Exception(String.Format(Resources.CannotFindSchema, schemaName,statement));
        }

        PgFunction function = new PgFunction();
        function.SetName(ParserUtils.GetObjectName(functionName));
        schema.AddFunction(function);

        parser.Expect("(");

        while (!parser.ExpectOptional(")")) {
            String mode;

            if (parser.ExpectOptional("IN")) {
                mode = "IN";
            } else if (parser.ExpectOptional("OUT")) {
                mode = "OUT";
            } else if (parser.ExpectOptional("INOUT")) {
                mode = "INOUT";
            } else if (parser.ExpectOptional("VARIADIC")) {
                mode = "VARIADIC";
            } else {
                mode = null;
            }

            int position = parser.GetPosition();
            String argumentName = null;
            String dataType = parser.ParseDataType();

            int position2 = parser.GetPosition();

            if (!parser.ExpectOptional(")") && !parser.ExpectOptional(",")
                    && !parser.ExpectOptional("=")
                    && !parser.ExpectOptional("DEFAULT")) {
                parser.SetPosition(position);
                argumentName =
                        ParserUtils.GetObjectName(parser.ParseIdentifier());
                dataType = parser.ParseDataType();
            } else {
                parser.SetPosition(position2);
            }

            String defaultExpression;

            if (parser.ExpectOptional("=")
                    || parser.ExpectOptional("DEFAULT")) {
                defaultExpression = parser.GetExpression();
            } else {
                defaultExpression = null;
            }

            PgFunction.Argument argument = new PgFunction.Argument();
            argument.SetDataType(dataType);
            argument.SetDefaultExpression(defaultExpression);
            argument.SetMode(mode);
            argument.SetName(argumentName);
            function.AddArgument(argument);

            if (parser.ExpectOptional(")")) {
                break;
            } else {
                parser.Expect(",");
            }
        }

        function.SetBody(parser.GetRest());
    }

    
    private CreateFunctionParser() {
    }
}
}