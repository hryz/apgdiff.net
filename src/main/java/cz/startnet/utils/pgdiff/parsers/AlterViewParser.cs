using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers {




public class AlterViewParser {

    
    public static void Parse(PgDatabase database,
            String statement, bool outputIgnoredStatements) {
        Parser parser = new Parser(statement);
        parser.Expect("ALTER", "VIEW");

        String viewName = parser.ParseIdentifier();
        String schemaName = ParserUtils.GetSchemaName(viewName, database);
        PgSchema schema = database.GetSchema(schemaName);

        if (schema == null) {
            throw new Exception(String.Format(Resources.CannotFindSchema, schemaName,statement));
        }

        String objectName = ParserUtils.GetObjectName(viewName);
        PgView view = schema.GetView(objectName);

        if (view == null) {
            throw new Exception(String.Format(Resources.CannotFindView, viewName,statement));
        }

        while (!parser.ExpectOptional(";")) {
            if (parser.ExpectOptional("ALTER")) {
                parser.ExpectOptional("COLUMN");

                String columnName =
                        ParserUtils.GetObjectName(parser.ParseIdentifier());

                if (parser.ExpectOptional("SET", "DEFAULT")) {
                    String expression = parser.GetExpression();
                    view.AddColumnDefaultValue(columnName, expression);
                } else if (parser.ExpectOptional("DROP", "DEFAULT")) {
                    view.RemoveColumnDefaultValue(columnName);
                } else {
                    parser.ThrowUnsupportedCommand();
                }
            } else if (parser.ExpectOptional("OWNER", "TO")) {
                // we do not parse this one so we just consume the identifier
                if (outputIgnoredStatements) {
                    database.AddIgnoredStatement("ALTER TABLE " + viewName
                            + " OWNER TO " + parser.ParseIdentifier() + ';');
                } else {
                    parser.ParseIdentifier();
                }
            } else {
                parser.ThrowUnsupportedCommand();
            }
        }
    }

    
    private AlterViewParser() {
    }
}
}