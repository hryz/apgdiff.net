
namespace cz.startnet.utils.pgdiff.parsers {

import cz.startnet.utils.pgdiff.Resources;
import cz.startnet.utils.pgdiff.schema.PgDatabase;
import cz.startnet.utils.pgdiff.schema.PgSchema;
import cz.startnet.utils.pgdiff.schema.PgSequence;



public class AlterSequenceParser {

    
    public static void parse(PgDatabase database,
            String statement, boolean outputIgnoredStatements) {
        Parser parser = new Parser(statement);

        parser.expect("ALTER", "SEQUENCE");

        String sequenceName = parser.parseIdentifier();
        String schemaName =
                ParserUtils.getSchemaName(sequenceName, database);
        PgSchema schema = database.getSchema(schemaName);

        if (schema == null) {
            throw new RuntimeException(MessageFormat.format(
                    Resources.getString("CannotFindSchema"), schemaName,
                    statement));
        }

        String objectName = ParserUtils.getObjectName(sequenceName);
        PgSequence sequence = schema.getSequence(objectName);

        if (sequence == null) {
            throw new RuntimeException(MessageFormat.format(
                    Resources.getString("CannotFindSequence"), sequenceName,
                    statement));
        }

        while (!parser.expectOptional(";")) {

            if (parser.expectOptional("OWNED", "BY")) {
                if (parser.expectOptional("NONE")) {
                    sequence.setOwnedBy(null);
                } else {
                    sequence.setOwnedBy(parser.getExpression());
                }
            } else {
                parser.throwUnsupportedCommand();
            }
        }
    }

    
    private AlterSequenceParser() {
    }
}
}