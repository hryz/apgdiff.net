using System;
using System.Collections.Generic;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers {






public class CreateViewParser {

    
    public static void parse(PgDatabase database,
            String statement) {
        Parser parser = new Parser(statement);
        parser.expect("CREATE");
        parser.expectOptional("OR", "REPLACE");
        parser.expect("VIEW");

        String viewName = parser.parseIdentifier();

        bool columnsExist = parser.expectOptional("(");
        List<String> columnNames = new List<string>();

        if (columnsExist) {
            while (!parser.expectOptional(")")) {
                columnNames.Add(ParserUtils.getObjectName(parser.parseIdentifier()));
                parser.expectOptional(",");
            }
        }

        parser.expect("AS");

        String query = parser.getRest();

        PgView view = new PgView(ParserUtils.getObjectName(viewName));
        view.setColumnNames(columnNames);
        view.setQuery(query);

        String schemaName = ParserUtils.getSchemaName(viewName, database);
        PgSchema schema = database.getSchema(schemaName);

        if (schema == null) {
            throw new Exception(String.Format(Resources.CannotFindSchema, schemaName,statement));
        }

        schema.addView(view);
    }

    
    private CreateViewParser() {
    }
}
}