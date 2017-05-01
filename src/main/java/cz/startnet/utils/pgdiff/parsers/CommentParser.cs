using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers
{
    public class CommentParser
    {
        private CommentParser()
        {
        }


        public static void Parse(PgDatabase database, string statement, bool outputIgnoredStatements)
        {
            var parser = new Parser(statement);
            parser.Expect("COMMENT", "ON");

            if (parser.ExpectOptional("TABLE")) ParseTable(parser, database);
            else if (parser.ExpectOptional("COLUMN")) ParseColumn(parser, database);
            else if (parser.ExpectOptional("CONSTRAINT")) ParseConstraint(parser, database);
            else if (parser.ExpectOptional("DATABASE")) ParseDatabase(parser, database);
            else if (parser.ExpectOptional("FUNCTION")) ParseFunction(parser, database);
            else if (parser.ExpectOptional("INDEX")) ParseIndex(parser, database);
            else if (parser.ExpectOptional("SCHEMA")) ParseSchema(parser, database);
            else if (parser.ExpectOptional("SEQUENCE")) ParseSequence(parser, database);
            else if (parser.ExpectOptional("TRIGGER")) ParseTrigger(parser, database);
            else if (parser.ExpectOptional("VIEW")) ParseView(parser, database);
            else if (outputIgnoredStatements) database.IgnoredStatements.Add(statement);
        }


        private static void ParseTable(Parser parser, PgDatabase database)
        {
            var tableName = parser.ParseIdentifier();
            var objectName = ParserUtils.GetObjectName(tableName);
            var schemaName = ParserUtils.GetSchemaName(tableName, database);

            var table = database.GetSchema(schemaName).GetTable(objectName);

            parser.Expect("IS");
            table.Comment = GetComment(parser);
            parser.Expect(";");
        }


        private static void ParseConstraint(Parser parser, PgDatabase database)
        {
            var constraintName = ParserUtils.GetObjectName(parser.ParseIdentifier());

            parser.Expect("ON");

            var tableName = parser.ParseIdentifier();
            var objectName = ParserUtils.GetObjectName(tableName);
            var schemaName = ParserUtils.GetSchemaName(constraintName, database);

            var constraint = database.GetSchema(schemaName).GetTable(objectName).GetConstraint(constraintName);

            parser.Expect("IS");
            constraint.Comment = GetComment(parser);
            parser.Expect(";");
        }


        private static void ParseDatabase(Parser parser, PgDatabase database)
        {
            parser.ParseIdentifier();
            parser.Expect("IS");
            database.Comment = GetComment(parser);
            parser.Expect(";");
        }


        private static void ParseIndex(Parser parser, PgDatabase database)
        {
            var indexName = parser.ParseIdentifier();
            var objectName = ParserUtils.GetObjectName(indexName);
            var schemaName = ParserUtils.GetSchemaName(indexName, database);
            var schema = database.GetSchema(schemaName);

            var index = schema.GetIndex(objectName);

            if (index == null)
            {
                var primaryKey = schema.GetPrimaryKey(objectName);
                parser.Expect("IS");
                primaryKey.Comment = GetComment(parser);
                parser.Expect(";");
            }
            else
            {
                parser.Expect("IS");
                index.Comment = GetComment(parser);
                parser.Expect(";");
            }
        }


        private static void ParseSchema(Parser parser, PgDatabase database)
        {
            var schemaName = ParserUtils.GetObjectName(parser.ParseIdentifier());
            var schema = database.GetSchema(schemaName);

            parser.Expect("IS");
            schema.Comment = GetComment(parser);
            parser.Expect(";");
        }


        private static void ParseSequence(Parser parser, PgDatabase database)
        {
            var sequenceName = parser.ParseIdentifier();
            var objectName = ParserUtils.GetObjectName(sequenceName);
            var schemaName = ParserUtils.GetSchemaName(sequenceName, database);

            var sequence = database.GetSchema(schemaName).GetSequence(objectName);

            parser.Expect("IS");
            sequence.Comment = GetComment(parser);
            parser.Expect(";");
        }


        private static void ParseTrigger(Parser parser, PgDatabase database)
        {
            var triggerName = ParserUtils.GetObjectName(parser.ParseIdentifier());

            parser.Expect("ON");

            var tableName = parser.ParseIdentifier();
            var objectName = ParserUtils.GetObjectName(tableName);
            var schemaName = ParserUtils.GetSchemaName(triggerName, database);

            var trigger = database.GetSchema(schemaName).GetTable(objectName).GetTrigger(triggerName);

            parser.Expect("IS");
            trigger.Comment = GetComment(parser);
            parser.Expect(";");
        }


        private static void ParseView(Parser parser, PgDatabase database)
        {
            var viewName = parser.ParseIdentifier();
            var objectName = ParserUtils.GetObjectName(viewName);
            var schemaName = ParserUtils.GetSchemaName(viewName, database);

            var view = database.GetSchema(schemaName).GetView(objectName);

            parser.Expect("IS");
            view.Comment = GetComment(parser);
            parser.Expect(";");
        }


        private static void ParseColumn(Parser parser, PgDatabase database)
        {
            var columnName = parser.ParseIdentifier();
            var objectName = ParserUtils.GetObjectName(columnName);
            var tableName = ParserUtils.GetSecondObjectName(columnName);
            var schemaName = ParserUtils.GetThirdObjectName(columnName);
            var schema = database.GetSchema(schemaName);

            var table = schema.GetTable(tableName);

            if (table == null)
            {
                var view = schema.GetView(tableName);
                parser.Expect("IS");

                var comment = GetComment(parser);

                if (comment == null)
                    view.RemoveColumnComment(objectName);
                else
                    view.AddColumnComment(objectName, comment);
                parser.Expect(";");
            }
            else
            {
                var column = table.GetColumn(objectName);

                if (column == null)
                    throw new ParserException(string.Format(Resources.CannotFindColumnInTable, columnName, table.Name));

                parser.Expect("IS");
                column.Comment = GetComment(parser);
                parser.Expect(";");
            }
        }


        private static void ParseFunction(Parser parser, PgDatabase database)
        {
            var functionName = parser.ParseIdentifier();
            var objectName = ParserUtils.GetObjectName(functionName);
            var schemaName = ParserUtils.GetSchemaName(functionName, database);
            var schema = database.GetSchema(schemaName);

            parser.Expect("(");

            var tmpFunction = new PgFunction();
            tmpFunction.Name = objectName;

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

                if (!parser.ExpectOptional(")") && !parser.ExpectOptional(","))
                {
                    parser.SetPosition(position);
                    argumentName = ParserUtils.GetObjectName(parser.ParseIdentifier());
                    dataType = parser.ParseDataType();
                }
                else
                {
                    parser.SetPosition(position2);
                }

                var argument = new PgFunction.Argument
                {
                    DataType = dataType,
                    Mode = mode,
                    Name = argumentName
                };
                tmpFunction.AddArgument(argument);

                if (parser.ExpectOptional(")"))
                    break;

                parser.Expect(",");
            }

            var function =
                schema.GetFunction(tmpFunction.GetSignature());

            parser.Expect("IS");
            function.Comment = GetComment(parser);
            parser.Expect(";");
        }


        private static string GetComment(Parser parser)
        {
            var comment = parser.ParseString();

            return "null".EqualsIgnoreCase(comment)
                ? null
                : comment;
        }
    }
}