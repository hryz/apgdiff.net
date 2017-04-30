using pgdiff.schema;

namespace pgdiff.parsers
{
    public class CreateSchemaParser
    {
        private CreateSchemaParser()
        {
        }


        public static void Parse(PgDatabase database, string statement)
        {
            var parser = new Parser(statement);
            parser.Expect("CREATE", "SCHEMA");

            if (parser.ExpectOptional("AUTHORIZATION"))
            {
                var schema = new PgSchema(ParserUtils.GetObjectName(parser.ParseIdentifier()));
                database.Schemas.Add(schema);
                schema.Authorization = schema.Name;

                var definition = parser.GetRest();

                if (!string.IsNullOrEmpty(definition))
                    schema.Definition = definition;
            }
            else
            {
                var schema = new PgSchema(ParserUtils.GetObjectName(parser.ParseIdentifier()));
                database.Schemas.Add(schema);

                if (parser.ExpectOptional("AUTHORIZATION"))
                    schema.Authorization = ParserUtils.GetObjectName(parser.ParseIdentifier());

                var definition = parser.GetRest();

                if (!string.IsNullOrEmpty(definition))
                    schema.Definition = definition;
            }
        }
    }
}