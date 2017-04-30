using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers {




public class CreateTableParser {

    
    public static void Parse(PgDatabase database,
            String statement) {
        Parser parser = new Parser(statement);
        parser.Expect("CREATE", "TABLE");

        // Optional IF NOT EXISTS, irrelevant for our purposes
        parser.ExpectOptional("IF", "NOT", "EXISTS");

        String tableName = parser.ParseIdentifier();
        PgTable table = new PgTable(ParserUtils.GetObjectName(tableName));
        String schemaName =
                ParserUtils.GetSchemaName(tableName, database);
        PgSchema schema = database.GetSchema(schemaName);

        if (schema == null) {
            throw new Exception(String.Format(Resources.CannotFindSchema, schemaName,statement));
        }

        schema.AddTable(table);

        parser.Expect("(");

        while (!parser.ExpectOptional(")")) {
            if (parser.ExpectOptional("CONSTRAINT")) {
                ParseConstraint(parser, table);
            } else {
                ParseColumn(parser, table);
            }

            if (parser.ExpectOptional(")")) {
                break;
            } else {
                parser.Expect(",");
            }
        }

        while (!parser.ExpectOptional(";")) {
            if (parser.ExpectOptional("INHERITS")) {
                ParseInherits(parser, table);
            } else if (parser.ExpectOptional("WITHOUT")) {
                table.SetWith("OIDS=false");
            } else if (parser.ExpectOptional("WITH")) {
                if (parser.ExpectOptional("OIDS")
                        || parser.ExpectOptional("OIDS=true")) {
                    table.SetWith("OIDS=true");
                } else if (parser.ExpectOptional("OIDS=false")) {
                    table.SetWith("OIDS=false");
                } else {
                    table.SetWith(parser.GetExpression());
                }
            } else if (parser.ExpectOptional("TABLESPACE")) {
                table.SetTablespace(parser.ParseString());
            } else {
                parser.ThrowUnsupportedCommand();
            }
        }
    }

    
    private static void ParseInherits(Parser parser,
            PgTable table) {
        parser.Expect("(");

        while (!parser.ExpectOptional(")")) {
            table.AddInherits(
                    ParserUtils.GetObjectName(parser.ParseIdentifier()));

            if (parser.ExpectOptional(")")) {
                break;
            } else {
                parser.Expect(",");
            }
        }
    }

    
    private static void ParseConstraint(Parser parser,
            PgTable table) {
        PgConstraint constraint = new PgConstraint(
                ParserUtils.GetObjectName(parser.ParseIdentifier()));
        table.AddConstraint(constraint);
        constraint.SetDefinition(parser.GetExpression());
        constraint.SetTableName(table.GetName());
    }

    
    private static void ParseColumn(Parser parser, PgTable table) {
        PgColumn column = new PgColumn(
                ParserUtils.GetObjectName(parser.ParseIdentifier()));
        table.AddColumn(column);
        column.ParseDefinition(parser.GetExpression());
    }

    
    private CreateTableParser() {
    }
}
}