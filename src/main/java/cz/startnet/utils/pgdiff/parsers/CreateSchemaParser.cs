
package cz.startnet.utils.pgdiff.parsers;

import cz.startnet.utils.pgdiff.schema.PgDatabase;
import cz.startnet.utils.pgdiff.schema.PgSchema;


public class CreateSchemaParser {

    
    public static void parse(final PgDatabase database,
            final String statement) {
        final Parser parser = new Parser(statement);
        parser.expect("CREATE", "SCHEMA");

        if (parser.expectOptional("AUTHORIZATION")) {
            final PgSchema schema = new PgSchema(
                    ParserUtils.getObjectName(parser.parseIdentifier()));
            database.addSchema(schema);
            schema.setAuthorization(schema.getName());

            final String definition = parser.getRest();

            if (definition != null && !definition.isEmpty()) {
                schema.setDefinition(definition);
            }
        } else {
            final PgSchema schema = new PgSchema(
                    ParserUtils.getObjectName(parser.parseIdentifier()));
            database.addSchema(schema);

            if (parser.expectOptional("AUTHORIZATION")) {
                schema.setAuthorization(
                        ParserUtils.getObjectName(parser.parseIdentifier()));
            }

            final String definition = parser.getRest();

            if (definition != null && !definition.isEmpty()) {
                schema.setDefinition(definition);
            }
        }
    }

    
    private CreateSchemaParser() {
    }
}
