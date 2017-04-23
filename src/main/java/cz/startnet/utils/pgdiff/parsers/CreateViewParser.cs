
package cz.startnet.utils.pgdiff.parsers;

import cz.startnet.utils.pgdiff.Resources;
import cz.startnet.utils.pgdiff.schema.PgDatabase;
import cz.startnet.utils.pgdiff.schema.PgSchema;
import cz.startnet.utils.pgdiff.schema.PgView;
import java.text.MessageFormat;
import java.util.ArrayList;
import java.util.List;


public class CreateViewParser {

    
    public static void parse(final PgDatabase database,
            final String statement) {
        final Parser parser = new Parser(statement);
        parser.expect("CREATE");
        parser.expectOptional("OR", "REPLACE");
        parser.expect("VIEW");

        final String viewName = parser.parseIdentifier();

        final boolean columnsExist = parser.expectOptional("(");
        final List<String> columnNames = new ArrayList<String>(10);

        if (columnsExist) {
            while (!parser.expectOptional(")")) {
                columnNames.add(
                        ParserUtils.getObjectName(parser.parseIdentifier()));
                parser.expectOptional(",");
            }
        }

        parser.expect("AS");

        final String query = parser.getRest();

        final PgView view = new PgView(ParserUtils.getObjectName(viewName));
        view.setColumnNames(columnNames);
        view.setQuery(query);

        final String schemaName = ParserUtils.getSchemaName(viewName, database);
        final PgSchema schema = database.getSchema(schemaName);

        if (schema == null) {
            throw new RuntimeException(MessageFormat.format(
                    Resources.getString("CannotFindSchema"), schemaName,
                    statement));
        }

        schema.addView(view);
    }

    
    private CreateViewParser() {
    }
}
