using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers {




public class AlterSequenceParser {

    
    public static void Parse(PgDatabase database,
            String statement, bool outputIgnoredStatements) {
        Parser parser = new Parser(statement);

        parser.Expect("ALTER", "SEQUENCE");

        String sequenceName = parser.ParseIdentifier();
        String schemaName =
                ParserUtils.GetSchemaName(sequenceName, database);
        PgSchema schema = database.GetSchema(schemaName);

        if (schema == null) {
            throw new Exception(String.Format(Resources.CannotFindSchema, schemaName,statement));
        }

        String objectName = ParserUtils.GetObjectName(sequenceName);
        PgSequence sequence = schema.GetSequence(objectName);

        if (sequence == null) {
            throw new Exception(String.Format(Resources.CannotFindSequence, sequenceName,statement));
        }

        while (!parser.ExpectOptional(";")) {

            if (parser.ExpectOptional("OWNED", "BY")) {
                if (parser.ExpectOptional("NONE")) {
                    sequence.SetOwnedBy(null);
                } else {
                    sequence.SetOwnedBy(parser.GetExpression());
                }
            } else {
                parser.ThrowUnsupportedCommand();
            }
        }
    }

    
    private AlterSequenceParser() {
    }
}
}