using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers
{
    public class CreateSequenceParser
    {
        private CreateSequenceParser()
        {
        }


        public static void Parse(PgDatabase database, string statement)
        {
            var parser = new Parser(statement);
            parser.Expect("CREATE", "SEQUENCE");

            var sequenceName = parser.ParseIdentifier();
            var sequence = new PgSequence(ParserUtils.GetObjectName(sequenceName));
            var schemaName = ParserUtils.GetSchemaName(sequenceName, database);
            var schema = database.GetSchema(schemaName);

            if (schema == null)
                throw new Exception(string.Format(Resources.CannotFindSchema, schemaName, statement));

            schema.AddSequence(sequence);

            while (!parser.ExpectOptional(";"))
                if (parser.ExpectOptional("INCREMENT"))
                {
                    parser.ExpectOptional("BY");
                    sequence.Increment = parser.ParseString();
                }
                else if (parser.ExpectOptional("MINVALUE"))
                {
                    sequence.MinValue = parser.ParseString();
                }
                else if (parser.ExpectOptional("MAXVALUE"))
                {
                    sequence.MaxValue = parser.ParseString();
                }
                else if (parser.ExpectOptional("START"))
                {
                    parser.ExpectOptional("WITH");
                    sequence.StartWith = parser.ParseString();
                }
                else if (parser.ExpectOptional("CACHE"))
                {
                    sequence.Cache = parser.ParseString();
                }
                else if (parser.ExpectOptional("CYCLE"))
                {
                    sequence.Cycle = true;
                }
                else if (parser.ExpectOptional("OWNED", "BY"))
                {
                    sequence.OwnedBy = parser.ExpectOptional("NONE")
                        ? null
                        : ParserUtils.GetObjectName(parser.ParseIdentifier());
                }
                else if (parser.ExpectOptional("NO"))
                {
                    if (parser.ExpectOptional("MINVALUE"))
                        sequence.MinValue = null;
                    else if (parser.ExpectOptional("MAXVALUE"))
                        sequence.MaxValue = null;
                    else if (parser.ExpectOptional("CYCLE"))
                        sequence.Cycle = false;
                    else
                        parser.ThrowUnsupportedCommand();
                }
                else
                {
                    parser.ThrowUnsupportedCommand();
                }
        }
    }
}