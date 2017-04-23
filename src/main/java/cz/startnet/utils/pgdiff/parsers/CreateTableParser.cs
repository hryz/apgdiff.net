
using System;
using cz.startnet.utils.pgdiff.schema;

namespace cz.startnet.utils.pgdiff.parsers {




public class CreateTableParser {

    
    public static void parse(PgDatabase database,
            String statement) {
        Parser parser = new Parser(statement);
        parser.expect("CREATE", "TABLE");

        // Optional IF NOT EXISTS, irrelevant for our purposes
        parser.expectOptional("IF", "NOT", "EXISTS");

        String tableName = parser.parseIdentifier();
        PgTable table = new PgTable(ParserUtils.getObjectName(tableName));
        String schemaName =
                ParserUtils.getSchemaName(tableName, database);
        PgSchema schema = database.getSchema(schemaName);

        if (schema == null) {
            throw new Exception(String.Format(
                    Resources.getString("CannotFindSchema"), schemaName,
                    statement));
        }

        schema.addTable(table);

        parser.expect("(");

        while (!parser.expectOptional(")")) {
            if (parser.expectOptional("CONSTRAINT")) {
                parseConstraint(parser, table);
            } else {
                parseColumn(parser, table);
            }

            if (parser.expectOptional(")")) {
                break;
            } else {
                parser.expect(",");
            }
        }

        while (!parser.expectOptional(";")) {
            if (parser.expectOptional("INHERITS")) {
                parseInherits(parser, table);
            } else if (parser.expectOptional("WITHOUT")) {
                table.setWith("OIDS=false");
            } else if (parser.expectOptional("WITH")) {
                if (parser.expectOptional("OIDS")
                        || parser.expectOptional("OIDS=true")) {
                    table.setWith("OIDS=true");
                } else if (parser.expectOptional("OIDS=false")) {
                    table.setWith("OIDS=false");
                } else {
                    table.setWith(parser.getExpression());
                }
            } else if (parser.expectOptional("TABLESPACE")) {
                table.setTablespace(parser.parseString());
            } else {
                parser.throwUnsupportedCommand();
            }
        }
    }

    
    private static void parseInherits(Parser parser,
            PgTable table) {
        parser.expect("(");

        while (!parser.expectOptional(")")) {
            table.addInherits(
                    ParserUtils.getObjectName(parser.parseIdentifier()));

            if (parser.expectOptional(")")) {
                break;
            } else {
                parser.expect(",");
            }
        }
    }

    
    private static void parseConstraint(Parser parser,
            PgTable table) {
        PgConstraint constraint = new PgConstraint(
                ParserUtils.getObjectName(parser.parseIdentifier()));
        table.addConstraint(constraint);
        constraint.setDefinition(parser.getExpression());
        constraint.setTableName(table.getName());
    }

    
    private static void parseColumn(Parser parser, PgTable table) {
        PgColumn column = new PgColumn(
                ParserUtils.getObjectName(parser.parseIdentifier()));
        table.addColumn(column);
        column.parseDefinition(parser.getExpression());
    }

    
    private CreateTableParser() {
    }
}
}