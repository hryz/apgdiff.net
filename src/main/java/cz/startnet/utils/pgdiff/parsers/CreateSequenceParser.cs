using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers {




public class CreateSequenceParser {

    
    public static void Parse(PgDatabase database,
            String statement) {
        Parser parser = new Parser(statement);
        parser.Expect("CREATE", "SEQUENCE");

        String sequenceName = parser.ParseIdentifier();
        PgSequence sequence =
                new PgSequence(ParserUtils.GetObjectName(sequenceName));
        String schemaName =
                ParserUtils.GetSchemaName(sequenceName, database);
        PgSchema schema = database.GetSchema(schemaName);

        if (schema == null) {
            throw new Exception(String.Format(Resources.CannotFindSchema, schemaName,statement));
        }

        schema.AddSequence(sequence);

        while (!parser.ExpectOptional(";")) {
            if (parser.ExpectOptional("INCREMENT")) {
                parser.ExpectOptional("BY");
                sequence.SetIncrement(parser.ParseString());
            } else if (parser.ExpectOptional("MINVALUE")) {
                sequence.SetMinValue(parser.ParseString());
            } else if (parser.ExpectOptional("MAXVALUE")) {
                sequence.SetMaxValue(parser.ParseString());
            } else if (parser.ExpectOptional("START")) {
                parser.ExpectOptional("WITH");
                sequence.SetStartWith(parser.ParseString());
            } else if (parser.ExpectOptional("CACHE")) {
                sequence.SetCache(parser.ParseString());
            } else if (parser.ExpectOptional("CYCLE")) {
                sequence.SetCycle(true);
            } else if (parser.ExpectOptional("OWNED", "BY")) {
                if (parser.ExpectOptional("NONE")) {
                    sequence.SetOwnedBy(null);
                } else {
                    sequence.SetOwnedBy(ParserUtils.GetObjectName(
                            parser.ParseIdentifier()));
                }
            } else if (parser.ExpectOptional("NO")) {
                if (parser.ExpectOptional("MINVALUE")) {
                    sequence.SetMinValue(null);
                } else if (parser.ExpectOptional("MAXVALUE")) {
                    sequence.SetMaxValue(null);
                } else if (parser.ExpectOptional("CYCLE")) {
                    sequence.SetCycle(false);
                } else {
                    parser.ThrowUnsupportedCommand();
                }
            } else {
                parser.ThrowUnsupportedCommand();
            }
        }
    }

    
    private CreateSequenceParser() {
    }
}
}