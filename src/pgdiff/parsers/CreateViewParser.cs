using System;
using System.Collections.Generic;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers
{
    public class CreateViewParser
    {
        private CreateViewParser()
        {
        }


        public static void Parse(PgDatabase database, string statement)
        {
            var parser = new Parser(statement);
            parser.Expect("CREATE");
            parser.ExpectOptional("OR", "REPLACE");
            parser.Expect("VIEW");

            var viewName = parser.ParseIdentifier();

            var columnsExist = parser.ExpectOptional("(");
            var columnNames = new List<string>();

            if (columnsExist)
                while (!parser.ExpectOptional(")"))
                {
                    columnNames.Add(ParserUtils.GetObjectName(parser.ParseIdentifier()));
                    parser.ExpectOptional(",");
                }

            parser.Expect("AS");

            var query = parser.GetRest();

            var view = new PgView(ParserUtils.GetObjectName(viewName));
            view.ColumnNames = columnNames;
            view.Query = query;

            var schemaName = ParserUtils.GetSchemaName(viewName, database);
            var schema = database.GetSchema(schemaName);

            if (schema == null)
                throw new Exception(string.Format(Resources.CannotFindSchema, schemaName, statement));

            schema.AddView(view);
        }
    }
}