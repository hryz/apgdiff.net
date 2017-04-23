using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers {




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
            throw new Exception(String.Format(Resources.CannotFindSchema, schemaName,statement));
        }

        String objectName = ParserUtils.getObjectName(sequenceName);
        PgSequence sequence = schema.getSequence(objectName);

        if (sequence == null) {
            throw new Exception(String.Format(Resources.CannotFindSequence, sequenceName,statement));
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