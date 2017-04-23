
namespace cz.startnet.utils.pgdiff.parsers {

using cz.startnet.utils.pgdiff.Resources;
using cz.startnet.utils.pgdiff.schema.PgDatabase;
using cz.startnet.utils.pgdiff.schema.PgFunction;
using cz.startnet.utils.pgdiff.schema.PgSchema;



public class CreateFunctionParser {

    
    public static void parse(PgDatabase database,
            String statement) {
        Parser parser = new Parser(statement);
        parser.expect("CREATE");
        parser.expectOptional("OR", "REPLACE");
        parser.expect("FUNCTION");

        String functionName = parser.parseIdentifier();
        String schemaName =
                ParserUtils.getSchemaName(functionName, database);
        PgSchema schema = database.getSchema(schemaName);

        if (schema == null) {
            throw new RuntimeException(MessageFormat.format(
                    Resources.getString("CannotFindSchema"), schemaName,
                    statement));
        }

        PgFunction function = new PgFunction();
        function.setName(ParserUtils.getObjectName(functionName));
        schema.addFunction(function);

        parser.expect("(");

        while (!parser.expectOptional(")")) {
            String mode;

            if (parser.expectOptional("IN")) {
                mode = "IN";
            } else if (parser.expectOptional("OUT")) {
                mode = "OUT";
            } else if (parser.expectOptional("INOUT")) {
                mode = "INOUT";
            } else if (parser.expectOptional("VARIADIC")) {
                mode = "VARIADIC";
            } else {
                mode = null;
            }

            int position = parser.getPosition();
            String argumentName = null;
            String dataType = parser.parseDataType();

            int position2 = parser.getPosition();

            if (!parser.expectOptional(")") && !parser.expectOptional(",")
                    && !parser.expectOptional("=")
                    && !parser.expectOptional("DEFAULT")) {
                parser.setPosition(position);
                argumentName =
                        ParserUtils.getObjectName(parser.parseIdentifier());
                dataType = parser.parseDataType();
            } else {
                parser.setPosition(position2);
            }

            String defaultExpression;

            if (parser.expectOptional("=")
                    || parser.expectOptional("DEFAULT")) {
                defaultExpression = parser.getExpression();
            } else {
                defaultExpression = null;
            }

            PgFunction.Argument argument = new PgFunction.Argument();
            argument.setDataType(dataType);
            argument.setDefaultExpression(defaultExpression);
            argument.setMode(mode);
            argument.setName(argumentName);
            function.addArgument(argument);

            if (parser.expectOptional(")")) {
                break;
            } else {
                parser.expect(",");
            }
        }

        function.setBody(parser.getRest());
    }

    
    private CreateFunctionParser() {
    }
}
}