using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers
{
    public class CreateTableParser
    {
        private CreateTableParser()
        {
        }


        public static void Parse(PgDatabase database, string statement)
        {
            var parser = new Parser(statement);
            parser.Expect("CREATE", "TABLE");

            // Optional IF NOT EXISTS, irrelevant for our purposes
            parser.ExpectOptional("IF", "NOT", "EXISTS");

            var tableName = parser.ParseIdentifier();
            var table = new PgTable(ParserUtils.GetObjectName(tableName));
            var schemaName = ParserUtils.GetSchemaName(tableName, database);
            var schema = database.GetSchema(schemaName);

            if (schema == null)
                throw new Exception(string.Format(Resources.CannotFindSchema, schemaName, statement));

            schema.AddTable(table);

            parser.Expect("(");

            while (!parser.ExpectOptional(")"))
            {
                if (parser.ExpectOptional("CONSTRAINT"))
                    ParseConstraint(parser, table);
                else
                    ParseColumn(parser, table);

                if (parser.ExpectOptional(")"))
                    break;

                parser.Expect(",");
            }

            while (!parser.ExpectOptional(";"))
                if (parser.ExpectOptional("INHERITS"))
                    ParseInherits(parser, table);
                else if (parser.ExpectOptional("WITHOUT"))
                    table.With = "OIDS=false";
                else if (parser.ExpectOptional("WITH"))
                    if (parser.ExpectOptional("OIDS") || parser.ExpectOptional("OIDS=true"))
                        table.With = "OIDS=true";
                    else if (parser.ExpectOptional("OIDS=false"))
                        table.With = "OIDS=false";
                    else
                        table.With = parser.GetExpression();
                else if (parser.ExpectOptional("TABLESPACE"))
                    table.Tablespace = parser.ParseString();
                else
                    parser.ThrowUnsupportedCommand();
        }


        private static void ParseInherits(Parser parser, PgTable table)
        {
            parser.Expect("(");

            while (!parser.ExpectOptional(")"))
            {
                table.AddInherits(ParserUtils.GetObjectName(parser.ParseIdentifier()));
                if (parser.ExpectOptional(")"))
                    break;

                parser.Expect(",");
            }
        }


        private static void ParseConstraint(Parser parser, PgTable table)
        {
            var constraint = new PgConstraint(ParserUtils.GetObjectName(parser.ParseIdentifier()));
            table.AddConstraint(constraint);
            constraint.Definition = parser.GetExpression();
            constraint.TableName = table.Name;
        }


        private static void ParseColumn(Parser parser, PgTable table)
        {
            var column = new PgColumn(ParserUtils.GetObjectName(parser.ParseIdentifier()));
            table.AddColumn(column);
            column.ParseDefinition(parser.GetExpression());
        }
    }
}