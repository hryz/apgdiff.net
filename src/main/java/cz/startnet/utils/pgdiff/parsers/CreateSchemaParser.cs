
namespace cz.startnet.utils.pgdiff.parsers {



public class CreateSchemaParser {

    
    public static void parse(PgDatabase database,
            String statement) {
        Parser parser = new Parser(statement);
        parser.expect("CREATE", "SCHEMA");

        if (parser.expectOptional("AUTHORIZATION")) {
            PgSchema schema = new PgSchema(
                    ParserUtils.getObjectName(parser.parseIdentifier()));
            database.addSchema(schema);
            schema.setAuthorization(schema.getName());

            String definition = parser.getRest();

            if (definition != null && !definition.isEmpty()) {
                schema.setDefinition(definition);
            }
        } else {
            PgSchema schema = new PgSchema(
                    ParserUtils.getObjectName(parser.parseIdentifier()));
            database.addSchema(schema);

            if (parser.expectOptional("AUTHORIZATION")) {
                schema.setAuthorization(
                        ParserUtils.getObjectName(parser.parseIdentifier()));
            }

            String definition = parser.getRest();

            if (definition != null && !definition.isEmpty()) {
                schema.setDefinition(definition);
            }
        }
    }

    
    private CreateSchemaParser() {
    }
}
}