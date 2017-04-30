using System;
using pgdiff.schema;

namespace pgdiff.parsers {



public class CreateSchemaParser {

    
    public static void Parse(PgDatabase database,
            String statement) {
        Parser parser = new Parser(statement);
        parser.Expect("CREATE", "SCHEMA");

        if (parser.ExpectOptional("AUTHORIZATION")) {
            PgSchema schema = new PgSchema(
                    ParserUtils.GetObjectName(parser.ParseIdentifier()));
            database.AddSchema(schema);
            schema.SetAuthorization(schema.GetName());

            String definition = parser.GetRest();

            if (!String.IsNullOrEmpty(definition)) {
                schema.SetDefinition(definition);
            }
        } else {
            PgSchema schema = new PgSchema(
                    ParserUtils.GetObjectName(parser.ParseIdentifier()));
            database.AddSchema(schema);

            if (parser.ExpectOptional("AUTHORIZATION")) {
                schema.SetAuthorization(
                        ParserUtils.GetObjectName(parser.ParseIdentifier()));
            }

            String definition = parser.GetRest();

            if (!String.IsNullOrEmpty(definition)) {
                schema.SetDefinition(definition);
            }
        }
    }

    
    private CreateSchemaParser() {
    }
}
}