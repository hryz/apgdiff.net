using System;
using System.Collections.Generic;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers {






public class AlterTableParser {

    
    public static void Parse(PgDatabase database,
            String statement, bool outputIgnoredStatements) {
        Parser parser = new Parser(statement);
        parser.Expect("ALTER", "TABLE");
        parser.ExpectOptional("ONLY");

        String tableName = parser.ParseIdentifier();
        String schemaName =
                ParserUtils.GetSchemaName(tableName, database);
        PgSchema schema = database.GetSchema(schemaName);

        if (schema == null) {
            throw new Exception(String.Format(Resources.CannotFindSchema, schemaName,statement));
        }

        String objectName = ParserUtils.GetObjectName(tableName);
        PgTable table = schema.GetTable(objectName);

        if (table == null) {
            PgView view = schema.GetView(objectName);

            if (view != null) {
                ParseView(parser, view, outputIgnoredStatements, tableName,
                        database);
                return;
            }

            PgSequence sequence = schema.GetSequence(objectName);

            if (sequence != null) {
                ParseSequence(parser, sequence, outputIgnoredStatements,
                        tableName, database);
                return;
            }

            throw new Exception(String.Format(Resources.CannotFindObject, tableName,statement));
        }

        while (!parser.ExpectOptional(";")) {
            if (parser.ExpectOptional("ALTER")) {
                ParseAlterColumn(parser, table);
            } else if (parser.ExpectOptional("CLUSTER", "ON")) {
                table.SetClusterIndexName(
                        ParserUtils.GetObjectName(parser.ParseIdentifier()));
            } else if (parser.ExpectOptional("OWNER", "TO")) {
                // we do not parse this one so we just consume the identifier
                if (outputIgnoredStatements) {
                    database.AddIgnoredStatement("ALTER TABLE " + tableName
                            + " OWNER TO " + parser.ParseIdentifier() + ';');
                } else {
                    parser.ParseIdentifier();
                }
            } else if (parser.ExpectOptional("ADD")) {
                if (parser.ExpectOptional("FOREIGN", "KEY")) {
                    ParseAddForeignKey(parser, table);
                } else if (parser.ExpectOptional("CONSTRAINT")) {
                    ParseAddConstraint(parser, table, schema);
                } else {
                    parser.ThrowUnsupportedCommand();
                }
            } else if (parser.ExpectOptional("ENABLE")) {
                ParseEnable(
                        parser, outputIgnoredStatements, tableName, database);
            } else if (parser.ExpectOptional("DISABLE")) {
                ParseDisable(
                        parser, outputIgnoredStatements, tableName, database);
            } else {
                parser.ThrowUnsupportedCommand();
            }

            if (parser.ExpectOptional(";")) {
                break;
            } else {
                parser.Expect(",");
            }
        }
    }

    
    private static void ParseEnable(Parser parser,
            bool outputIgnoredStatements, String tableName,
            PgDatabase database) {
        if (parser.ExpectOptional("REPLICA")) {
            if (parser.ExpectOptional("TRIGGER")) {
                if (outputIgnoredStatements) {
                    database.AddIgnoredStatement("ALTER TABLE " + tableName
                            + " ENABLE REPLICA TRIGGER "
                            + parser.ParseIdentifier() + ';');
                } else {
                    parser.ParseIdentifier();
                }
            } else if (parser.ExpectOptional("RULE")) {
                if (outputIgnoredStatements) {
                    database.AddIgnoredStatement("ALTER TABLE " + tableName
                            + " ENABLE REPLICA RULE "
                            + parser.ParseIdentifier() + ';');
                } else {
                    parser.ParseIdentifier();
                }
            } else {
                parser.ThrowUnsupportedCommand();
            }
        } else if (parser.ExpectOptional("ALWAYS")) {
            if (parser.ExpectOptional("TRIGGER")) {
                if (outputIgnoredStatements) {
                    database.AddIgnoredStatement("ALTER TABLE " + tableName
                            + " ENABLE ALWAYS TRIGGER "
                            + parser.ParseIdentifier() + ';');
                } else {
                    parser.ParseIdentifier();
                }
            } else if (parser.ExpectOptional("RULE")) {
                if (outputIgnoredStatements) {
                    database.AddIgnoredStatement("ALTER TABLE " + tableName
                            + " ENABLE RULE " + parser.ParseIdentifier() + ';');
                } else {
                    parser.ParseIdentifier();
                }
            } else {
                parser.ThrowUnsupportedCommand();
            }
        }
    }

    
    private static void ParseDisable(Parser parser,
            bool outputIgnoredStatements, String tableName,
            PgDatabase database) {
        if (parser.ExpectOptional("TRIGGER")) {
            if (outputIgnoredStatements) {
                database.AddIgnoredStatement("ALTER TABLE " + tableName
                        + " DISABLE TRIGGER " + parser.ParseIdentifier() + ';');
            } else {
                parser.ParseIdentifier();
            }
        } else if (parser.ExpectOptional("RULE")) {
            if (outputIgnoredStatements) {
                database.AddIgnoredStatement("ALTER TABLE " + tableName
                        + " DISABLE RULE " + parser.ParseIdentifier() + ';');
            } else {
                parser.ParseIdentifier();
            }
        } else {
            parser.ThrowUnsupportedCommand();
        }
    }

    
    private static void ParseAddConstraint(Parser parser,
            PgTable table, PgSchema schema) {
        String constraintName =
                ParserUtils.GetObjectName(parser.ParseIdentifier());
        PgConstraint constraint = new PgConstraint(constraintName);
        constraint.SetTableName(table.GetName());
        table.AddConstraint(constraint);

        if (parser.ExpectOptional("PRIMARY", "KEY")) {
            schema.AddPrimaryKey(constraint);
            constraint.SetDefinition("PRIMARY KEY " + parser.GetExpression());
        } else {
            constraint.SetDefinition(parser.GetExpression());
        }
    }

    
    private static void ParseAlterColumn(Parser parser,
            PgTable table) {
        parser.ExpectOptional("COLUMN");

        String columnName =
                ParserUtils.GetObjectName(parser.ParseIdentifier());

        if (parser.ExpectOptional("SET")) {
            if (parser.ExpectOptional("STATISTICS")) {
                PgColumn column = table.GetColumn(columnName);

                if (column == null) {
                    throw new Exception(String.Format(Resources.CannotFindTableColumn, columnName, table.GetName(), parser.GetString()));
                }

                column.SetStatistics(parser.ParseInteger());
            } else if (parser.ExpectOptional("DEFAULT")) {
                String defaultValue = parser.GetExpression();

                if (table.ContainsColumn(columnName)) {
                    PgColumn column = table.GetColumn(columnName);

                    if (column == null) {
                        throw new Exception(String.Format(Resources.CannotFindTableColumn, columnName, table.GetName(), parser.GetString()));
                    }

                    column.SetDefaultValue(defaultValue);
                } else {
                    throw new ParserException(String.Format( Resources.CannotFindColumnInTable, columnName, table.GetName()));
                }
            } else if (parser.ExpectOptional("STORAGE")) {
                PgColumn column = table.GetColumn(columnName);

                if (column == null) {
                    throw new Exception(String.Format( Resources.CannotFindTableColumn,columnName, table.GetName(), parser.GetString()));
                }

                if (parser.ExpectOptional("PLAIN")) {
                    column.SetStorage("PLAIN");
                } else if (parser.ExpectOptional("EXTERNAL")) {
                    column.SetStorage("EXTERNAL");
                } else if (parser.ExpectOptional("EXTENDED")) {
                    column.SetStorage("EXTENDED");
                } else if (parser.ExpectOptional("MAIN")) {
                    column.SetStorage("MAIN");
                } else {
                    parser.ThrowUnsupportedCommand();
                }
            } else {
                parser.ThrowUnsupportedCommand();
            }
        } else {
            parser.ThrowUnsupportedCommand();
        }
    }

    
    private static void ParseAddForeignKey(Parser parser,
            PgTable table) {
        List<String> columnNames = new List<string>();
        parser.Expect("(");

        while (!parser.ExpectOptional(")")) {
            columnNames.Add(ParserUtils.GetObjectName(parser.ParseIdentifier()));

            if (parser.ExpectOptional(")")) {
                break;
            } else {
                parser.Expect(",");
            }
        }

        String constraintName = ParserUtils.GenerateName(
                table.GetName() + "_", columnNames, "_fkey");
        PgConstraint constraint =
                new PgConstraint(constraintName);
        table.AddConstraint(constraint);
        constraint.SetDefinition(parser.GetExpression());
        constraint.SetTableName(table.GetName());
    }

    
    private static void ParseView(Parser parser, PgView view,
            bool outputIgnoredStatements, String viewName,
            PgDatabase database) {
        while (!parser.ExpectOptional(";")) {
            if (parser.ExpectOptional("ALTER")) {
                parser.ExpectOptional("COLUMN");

                String columnName =
                        ParserUtils.GetObjectName(parser.ParseIdentifier());

                if (parser.ExpectOptional("SET", "DEFAULT")) {
                    String expression = parser.GetExpression();
                    view.AddColumnDefaultValue(columnName, expression);
                } else if (parser.ExpectOptional("DROP", "DEFAULT")) {
                    view.RemoveColumnDefaultValue(columnName);
                } else {
                    parser.ThrowUnsupportedCommand();
                }
            } else if (parser.ExpectOptional("OWNER", "TO")) {
                // we do not parse this one so we just consume the identifier
                if (outputIgnoredStatements) {
                    database.AddIgnoredStatement("ALTER TABLE " + viewName
                            + " OWNER TO " + parser.ParseIdentifier() + ';');
                } else {
                    parser.ParseIdentifier();
                }
            } else {
                parser.ThrowUnsupportedCommand();
            }
        }
    }

    
    private static void ParseSequence(Parser parser,
            PgSequence sequence, bool outputIgnoredStatements,
            String sequenceName, PgDatabase database) {
        while (!parser.ExpectOptional(";")) {
            if (parser.ExpectOptional("OWNER", "TO")) {
                // we do not parse this one so we just consume the identifier
                if (outputIgnoredStatements) {
                    database.AddIgnoredStatement("ALTER TABLE " + sequenceName
                            + " OWNER TO " + parser.ParseIdentifier() + ';');
                } else {
                    parser.ParseIdentifier();
                }
            } else {
                parser.ThrowUnsupportedCommand();
            }
        }
    }

    
    private AlterTableParser() {
    }
}
}