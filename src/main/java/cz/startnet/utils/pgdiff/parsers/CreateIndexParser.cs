using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers {




public class CreateIndexParser {

    
    public static void Parse(PgDatabase database,
            String statement) {
        Parser parser = new Parser(statement);
        parser.Expect("CREATE");

        bool unique = parser.ExpectOptional("UNIQUE");

        parser.Expect("INDEX");
        parser.ExpectOptional("CONCURRENTLY");

        String indexName =
                ParserUtils.GetObjectName(parser.ParseIdentifier());

        parser.Expect("ON");

        String tableName = parser.ParseIdentifier();
        String definition = parser.GetRest();
        String schemaName =
                ParserUtils.GetSchemaName(tableName, database);
        PgSchema schema = database.GetSchema(schemaName);

        if (schema == null) {
            throw new Exception(String.Format(Resources.CannotFindSchema, schemaName,statement));
        }

        String objectName = ParserUtils.GetObjectName(tableName);
        PgTable table = schema.GetTable(objectName);

        if (table == null) {
            throw new Exception(String.Format(Resources.CannotFindTable, tableName,statement));
        }

        PgIndex index = new PgIndex(indexName);
        table.AddIndex(index);
        schema.AddIndex(index);
        index.SetDefinition(definition.Trim());
        index.SetTableName(table.GetName());
        index.SetUnique(unique);
    }

    
    private CreateIndexParser() {
    }
}
}