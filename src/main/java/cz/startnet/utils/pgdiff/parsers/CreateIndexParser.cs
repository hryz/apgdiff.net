
using System;
using cz.startnet.utils.pgdiff.schema;

namespace cz.startnet.utils.pgdiff.parsers {




public class CreateIndexParser {

    
    public static void parse(PgDatabase database,
            String statement) {
        Parser parser = new Parser(statement);
        parser.expect("CREATE");

        bool unique = parser.expectOptional("UNIQUE");

        parser.expect("INDEX");
        parser.expectOptional("CONCURRENTLY");

        String indexName =
                ParserUtils.getObjectName(parser.parseIdentifier());

        parser.expect("ON");

        String tableName = parser.parseIdentifier();
        String definition = parser.getRest();
        String schemaName =
                ParserUtils.getSchemaName(tableName, database);
        PgSchema schema = database.getSchema(schemaName);

        if (schema == null) {
            throw new Exception(String.Format(
                    Resources.getString("CannotFindSchema"), schemaName,
                    statement));
        }

        String objectName = ParserUtils.getObjectName(tableName);
        PgTable table = schema.getTable(objectName);

        if (table == null) {
            throw new Exception(String.Format(
                    Resources.getString("CannotFindTable"), tableName,
                    statement));
        }

        PgIndex index = new PgIndex(indexName);
        table.addIndex(index);
        schema.addIndex(index);
        index.setDefinition(definition.trim());
        index.setTableName(table.getName());
        index.setUnique(unique);
    }

    
    private CreateIndexParser() {
    }
}
}