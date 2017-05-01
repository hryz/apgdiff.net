using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers
{
    public class AlterViewParser
    {
        private AlterViewParser()
        {
        }

        public static void Parse(PgDatabase database, string statement, bool outputIgnoredStatements)
        {
            var parser = new Parser(statement);
            parser.Expect("ALTER", "VIEW");

            var viewName = parser.ParseIdentifier();
            var schemaName = ParserUtils.GetSchemaName(viewName, database);
            var schema = database.GetSchema(schemaName);

            if (schema == null)
                throw new Exception(string.Format(Resources.CannotFindSchema, schemaName, statement));


            var objectName = ParserUtils.GetObjectName(viewName);
            var view = schema.GetView(objectName);

            if (view == null)
                throw new Exception(string.Format(Resources.CannotFindView, viewName, statement));

            while (!parser.ExpectOptional(";"))
                if (parser.ExpectOptional("ALTER"))
                {
                    parser.ExpectOptional("COLUMN");

                    var columnName = ParserUtils.GetObjectName(parser.ParseIdentifier());

                    if (parser.ExpectOptional("SET", "DEFAULT"))
                    {
                        var expression = parser.GetExpression();
                        view.AddColumnDefaultValue(columnName, expression);
                    }
                    else if (parser.ExpectOptional("DROP", "DEFAULT"))
                    {
                        view.RemoveColumnDefaultValue(columnName);
                    }
                    else
                    {
                        parser.ThrowUnsupportedCommand();
                    }
                }
                else if (parser.ExpectOptional("OWNER", "TO"))
                {
                    // we do not parse this one so we just consume the identifier
                    if (outputIgnoredStatements)
                        database.IgnoredStatements.Add($"ALTER TABLE {viewName} OWNER TO {parser.ParseIdentifier()};");
                    else
                        parser.ParseIdentifier();
                }
                else
                {
                    parser.ThrowUnsupportedCommand();
                }
        }
    }
}