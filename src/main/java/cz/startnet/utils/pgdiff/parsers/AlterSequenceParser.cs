
using System;
using cz.startnet.utils.pgdiff.schema;

namespace cz.startnet.utils.pgdiff.parsers {




public class AlterSequenceParser {

    
    public static void parse(PgDatabase database,
            String statement, bool outputIgnoredStatements) {
        Parser parser = new Parser(statement);

        parser.expect("ALTER", "SEQUENCE");

        String sequenceName = parser.parseIdentifier();
        String schemaName =
                ParserUtils.getSchemaName(sequenceName, database);
        PgSchema schema = database.getSchema(schemaName);

        if (schema == null) {
            throw new Exception(String.Format(
                    Resources.getString("CannotFindSchema"), schemaName,
                    statement));
        }

        String objectName = ParserUtils.getObjectName(sequenceName);
        PgSequence sequence = schema.getSequence(objectName);

        if (sequence == null) {
            throw new Exception(String.Format(
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