using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers
{
    public class AlterSequenceParser
    {
        private AlterSequenceParser()
        {
        }

        public static void Parse(PgDatabase database, string statement, bool outputIgnoredStatements)
        {
            var parser = new Parser(statement);

            parser.Expect("ALTER", "SEQUENCE");

            var sequenceName = parser.ParseIdentifier();
            var schemaName = ParserUtils.GetSchemaName(sequenceName, database);
            var schema = database.GetSchema(schemaName);

            if (schema == null)
                throw new Exception(string.Format(Resources.CannotFindSchema, schemaName, statement));

            var objectName = ParserUtils.GetObjectName(sequenceName);
            var sequence = schema.GetSequence(objectName);

            if (sequence == null)
                throw new Exception(string.Format(Resources.CannotFindSequence, sequenceName, statement));


            while (!parser.ExpectOptional(";"))
                if (parser.ExpectOptional("OWNED", "BY"))
                    sequence.OwnedBy = parser.ExpectOptional("NONE") 
                        ? null 
                        : parser.GetExpression();
                else
                    parser.ThrowUnsupportedCommand();
        }
    }
}