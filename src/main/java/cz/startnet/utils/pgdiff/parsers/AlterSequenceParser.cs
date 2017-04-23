
package cz.startnet.utils.pgdiff.parsers;

import cz.startnet.utils.pgdiff.Resources;
import cz.startnet.utils.pgdiff.schema.PgDatabase;
import cz.startnet.utils.pgdiff.schema.PgSchema;
import cz.startnet.utils.pgdiff.schema.PgSequence;
import java.text.MessageFormat;


public class AlterSequenceParser {

    
    public static void parse(final PgDatabase database,
            final String statement, final boolean outputIgnoredStatements) {
        final Parser parser = new Parser(statement);

        parser.expect("ALTER", "SEQUENCE");

        final String sequenceName = parser.parseIdentifier();
        final String schemaName =
                ParserUtils.getSchemaName(sequenceName, database);
        final PgSchema schema = database.getSchema(schemaName);

        if (schema == null) {
            throw new RuntimeException(MessageFormat.format(
                    Resources.getString("CannotFindSchema"), schemaName,
                    statement));
        }

        final String objectName = ParserUtils.getObjectName(sequenceName);
        final PgSequence sequence = schema.getSequence(objectName);

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
