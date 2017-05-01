using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers
{
    public class CreateIndexParser
    {
        private CreateIndexParser()
        {
        }


        public static void Parse(PgDatabase database, string statement)
        {
            var parser = new Parser(statement);
            parser.Expect("CREATE");

            var unique = parser.ExpectOptional("UNIQUE");

            parser.Expect("INDEX");
            parser.ExpectOptional("CONCURRENTLY");

            var indexName = ParserUtils.GetObjectName(parser.ParseIdentifier());

            parser.Expect("ON");

            var tableName = parser.ParseIdentifier();
            var definition = parser.GetRest();
            var schemaName = ParserUtils.GetSchemaName(tableName, database);
            var schema = database.GetSchema(schemaName);

            if (schema == null)
                throw new Exception(string.Format(Resources.CannotFindSchema, schemaName, statement));

            var objectName = ParserUtils.GetObjectName(tableName);
            var table = schema.GetTable(objectName);

            if (table == null)
                throw new Exception(string.Format(Resources.CannotFindTable, tableName, statement));

            var index = new PgIndex(indexName);
            table.AddIndex(index);
            schema.AddIndex(index);
            index.Definition = definition.Trim();
            index.TableName = table.Name;
            index.Unique = unique;
        }
    }
}