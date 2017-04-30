using System;
using System.Collections.Generic;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers {






public class CreateViewParser {

    
    public static void Parse(PgDatabase database,
            String statement) {
        Parser parser = new Parser(statement);
        parser.Expect("CREATE");
        parser.ExpectOptional("OR", "REPLACE");
        parser.Expect("VIEW");

        String viewName = parser.ParseIdentifier();

        bool columnsExist = parser.ExpectOptional("(");
        List<String> columnNames = new List<string>();

        if (columnsExist) {
            while (!parser.ExpectOptional(")")) {
                columnNames.Add(ParserUtils.GetObjectName(parser.ParseIdentifier()));
                parser.ExpectOptional(",");
            }
        }

        parser.Expect("AS");

        String query = parser.GetRest();

        PgView view = new PgView(ParserUtils.GetObjectName(viewName));
        view.SetColumnNames(columnNames);
        view.SetQuery(query);

        String schemaName = ParserUtils.GetSchemaName(viewName, database);
        PgSchema schema = database.GetSchema(schemaName);

        if (schema == null) {
            throw new Exception(String.Format(Resources.CannotFindSchema, schemaName,statement));
        }

        schema.AddView(view);
    }

    
    private CreateViewParser() {
    }
}
}