
namespace cz.startnet.utils.pgdiff.parsers {

import cz.startnet.utils.pgdiff.Resources;
import cz.startnet.utils.pgdiff.schema.PgDatabase;
import cz.startnet.utils.pgdiff.schema.PgSchema;
import cz.startnet.utils.pgdiff.schema.PgView;



public class AlterViewParser {

    
    public static void parse(PgDatabase database,
            String statement, boolean outputIgnoredStatements) {
        Parser parser = new Parser(statement);
        parser.expect("ALTER", "VIEW");

        String viewName = parser.parseIdentifier();
        String schemaName = ParserUtils.getSchemaName(viewName, database);
        PgSchema schema = database.getSchema(schemaName);

        if (schema == null) {
            throw new RuntimeException(MessageFormat.format(
                    Resources.getString("CannotFindSchema"), schemaName,
                    statement));
        }

        String objectName = ParserUtils.getObjectName(viewName);
        PgView view = schema.getView(objectName);

        if (view == null) {
            throw new RuntimeException(MessageFormat.format(
                    Resources.getString("CannotFindView"), viewName,
                    statement));
        }

        while (!parser.expectOptional(";")) {
            if (parser.expectOptional("ALTER")) {
                parser.expectOptional("COLUMN");

                String columnName =
                        ParserUtils.getObjectName(parser.parseIdentifier());

                if (parser.expectOptional("SET", "DEFAULT")) {
                    String expression = parser.getExpression();
                    view.addColumnDefaultValue(columnName, expression);
                } else if (parser.expectOptional("DROP", "DEFAULT")) {
                    view.removeColumnDefaultValue(columnName);
                } else {
                    parser.throwUnsupportedCommand();
                }
            } else if (parser.expectOptional("OWNER", "TO")) {
                // we do not parse this one so we just consume the identifier
                if (outputIgnoredStatements) {
                    database.addIgnoredStatement("ALTER TABLE " + viewName
                            + " OWNER TO " + parser.parseIdentifier() + ';');
                } else {
                    parser.parseIdentifier();
                }
            } else {
                parser.throwUnsupportedCommand();
            }
        }
    }

    
    private AlterViewParser() {
    }
}
}