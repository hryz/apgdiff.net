using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers
{
    public class CreateFunctionParser
    {
        private CreateFunctionParser()
        {
        }


        public static void Parse(PgDatabase database, string statement)
        {
            var parser = new Parser(statement);
            parser.Expect("CREATE");
            parser.ExpectOptional("OR", "REPLACE");
            parser.Expect("FUNCTION");

            var functionName = parser.ParseIdentifier();
            var schemaName = ParserUtils.GetSchemaName(functionName, database);
            var schema = database.GetSchema(schemaName);

            if (schema == null)
                throw new Exception(string.Format(Resources.CannotFindSchema, schemaName, statement));

            var function = new PgFunction();
            function.Name = ParserUtils.GetObjectName(functionName);
            schema.AddFunction(function);

            parser.Expect("(");

            while (!parser.ExpectOptional(")"))
            {
                string mode;

                if (parser.ExpectOptional("IN"))
                    mode = "IN";
                else if (parser.ExpectOptional("OUT"))
                    mode = "OUT";
                else if (parser.ExpectOptional("INOUT"))
                    mode = "INOUT";
                else if (parser.ExpectOptional("VARIADIC"))
                    mode = "VARIADIC";
                else
                    mode = null;

                var position = parser.GetPosition();
                string argumentName = null;
                var dataType = parser.ParseDataType();

                var position2 = parser.GetPosition();

                if (!parser.ExpectOptional(")") && !parser.ExpectOptional(",")
                    && !parser.ExpectOptional("=")
                    && !parser.ExpectOptional("DEFAULT"))
                {
                    parser.SetPosition(position);
                    argumentName = ParserUtils.GetObjectName(parser.ParseIdentifier());
                    dataType = parser.ParseDataType();
                }
                else
                {
                    parser.SetPosition(position2);
                }

                string defaultExpression;

                if (parser.ExpectOptional("=") || parser.ExpectOptional("DEFAULT"))
                    defaultExpression = parser.GetExpression();
                else
                    defaultExpression = null;

                var argument = new PgFunction.Argument
                {
                    DataType = dataType,
                    DefaultExpression = defaultExpression,
                    Mode = mode,
                    Name = argumentName
                };
                function.AddArgument(argument);

                if (parser.ExpectOptional(")"))
                    break;

                parser.Expect(",");
            }

            function.Body = parser.GetRest();
        }
    }
}