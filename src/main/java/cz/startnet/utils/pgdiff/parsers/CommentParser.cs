using System;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff.parsers {




public class CommentParser {

    
    public static void Parse(PgDatabase database,
            String statement, bool outputIgnoredStatements) {
        Parser parser = new Parser(statement);
        parser.Expect("COMMENT", "ON");

        if (parser.ExpectOptional("TABLE")) {
            ParseTable(parser, database);
        } else if (parser.ExpectOptional("COLUMN")) {
            ParseColumn(parser, database);
        } else if (parser.ExpectOptional("CONSTRAINT")) {
            ParseConstraint(parser, database);
        } else if (parser.ExpectOptional("DATABASE")) {
            ParseDatabase(parser, database);
        } else if (parser.ExpectOptional("FUNCTION")) {
            ParseFunction(parser, database);
        } else if (parser.ExpectOptional("INDEX")) {
            ParseIndex(parser, database);
        } else if (parser.ExpectOptional("SCHEMA")) {
            ParseSchema(parser, database);
        } else if (parser.ExpectOptional("SEQUENCE")) {
            ParseSequence(parser, database);
        } else if (parser.ExpectOptional("TRIGGER")) {
            ParseTrigger(parser, database);
        } else if (parser.ExpectOptional("VIEW")) {
            ParseView(parser, database);
        } else if (outputIgnoredStatements) {
            database.AddIgnoredStatement(statement);
        }
    }

    
    private static void ParseTable(Parser parser,
            PgDatabase database) {
        String tableName = parser.ParseIdentifier();
        String objectName = ParserUtils.GetObjectName(tableName);
        String schemaName =
                ParserUtils.GetSchemaName(tableName, database);

        PgTable table =
                database.GetSchema(schemaName).GetTable(objectName);

        parser.Expect("IS");
        table.SetComment(GetComment(parser));
        parser.Expect(";");
    }

    
    private static void ParseConstraint(Parser parser,
            PgDatabase database) {
        String constraintName =
                ParserUtils.GetObjectName(parser.ParseIdentifier());

        parser.Expect("ON");

        String tableName = parser.ParseIdentifier();
        String objectName = ParserUtils.GetObjectName(tableName);
        String schemaName =
                ParserUtils.GetSchemaName(constraintName, database);

        PgConstraint constraint = database.GetSchema(schemaName).
                GetTable(objectName).GetConstraint(constraintName);

        parser.Expect("IS");
        constraint.SetComment(GetComment(parser));
        parser.Expect(";");
    }

    
    private static void ParseDatabase(Parser parser,
            PgDatabase database) {
        parser.ParseIdentifier();
        parser.Expect("IS");
        database.SetComment(GetComment(parser));
        parser.Expect(";");
    }

    
    private static void ParseIndex(Parser parser,
            PgDatabase database) {
        String indexName = parser.ParseIdentifier();
        String objectName = ParserUtils.GetObjectName(indexName);
        String schemaName =
                ParserUtils.GetSchemaName(indexName, database);
        PgSchema schema = database.GetSchema(schemaName);

        PgIndex index = schema.GetIndex(objectName);

        if (index == null) {
            PgConstraint primaryKey = schema.GetPrimaryKey(objectName);
            parser.Expect("IS");
            primaryKey.SetComment(GetComment(parser));
            parser.Expect(";");
        } else {
            parser.Expect("IS");
            index.SetComment(GetComment(parser));
            parser.Expect(";");
        }
    }

    
    private static void ParseSchema(Parser parser,
            PgDatabase database) {
        String schemaName =
                ParserUtils.GetObjectName(parser.ParseIdentifier());
        PgSchema schema = database.GetSchema(schemaName);

        parser.Expect("IS");
        schema.SetComment(GetComment(parser));
        parser.Expect(";");
    }

    
    private static void ParseSequence(Parser parser,
            PgDatabase database) {
        String sequenceName = parser.ParseIdentifier();
        String objectName = ParserUtils.GetObjectName(sequenceName);
        String schemaName =
                ParserUtils.GetSchemaName(sequenceName, database);

        PgSequence sequence =
                database.GetSchema(schemaName).GetSequence(objectName);

        parser.Expect("IS");
        sequence.SetComment(GetComment(parser));
        parser.Expect(";");
    }

    
    private static void ParseTrigger(Parser parser,
            PgDatabase database) {
        String triggerName =
                ParserUtils.GetObjectName(parser.ParseIdentifier());

        parser.Expect("ON");

        String tableName = parser.ParseIdentifier();
        String objectName = ParserUtils.GetObjectName(tableName);
        String schemaName =
                ParserUtils.GetSchemaName(triggerName, database);

        PgTrigger trigger = database.GetSchema(schemaName).
                GetTable(objectName).GetTrigger(triggerName);

        parser.Expect("IS");
        trigger.Comment = GetComment(parser);
        parser.Expect(";");
    }

    
    private static void ParseView(Parser parser,
            PgDatabase database) {
        String viewName = parser.ParseIdentifier();
        String objectName = ParserUtils.GetObjectName(viewName);
        String schemaName =
                ParserUtils.GetSchemaName(viewName, database);

        PgView view = database.GetSchema(schemaName).GetView(objectName);

        parser.Expect("IS");
        view.SetComment(GetComment(parser));
        parser.Expect(";");
    }

    
    private static void ParseColumn(Parser parser,
            PgDatabase database) {
        String columnName = parser.ParseIdentifier();
        String objectName = ParserUtils.GetObjectName(columnName);
        String tableName = ParserUtils.GetSecondObjectName(columnName);
        String schemaName = ParserUtils.GetThirdObjectName(columnName);
        PgSchema schema = database.GetSchema(schemaName);

        PgTable table = schema.GetTable(tableName);

        if (table == null) {
            PgView view = schema.GetView(tableName);
            parser.Expect("IS");

            String comment = GetComment(parser);

            if (comment == null) {
                view.RemoveColumnComment(objectName);
            } else {
                view.AddColumnComment(objectName, comment);
            }
            parser.Expect(";");
        } else {
            PgColumn column = table.GetColumn(objectName);

            if (column == null) {
                throw new ParserException(String.Format( Resources.CannotFindColumnInTable, columnName, table.GetName()));
            }

            parser.Expect("IS");
            column.SetComment(GetComment(parser));
            parser.Expect(";");
        }
    }

    
    private static void ParseFunction(Parser parser,
            PgDatabase database) {
        String functionName = parser.ParseIdentifier();
        String objectName = ParserUtils.GetObjectName(functionName);
        String schemaName =
                ParserUtils.GetSchemaName(functionName, database);
        PgSchema schema = database.GetSchema(schemaName);

        parser.Expect("(");

        PgFunction tmpFunction = new PgFunction();
        tmpFunction.SetName(objectName);

        while (!parser.ExpectOptional(")")) {
            String mode;

            if (parser.ExpectOptional("IN")) {
                mode = "IN";
            } else if (parser.ExpectOptional("OUT")) {
                mode = "OUT";
            } else if (parser.ExpectOptional("INOUT")) {
                mode = "INOUT";
            } else if (parser.ExpectOptional("VARIADIC")) {
                mode = "VARIADIC";
            } else {
                mode = null;
            }

            int position = parser.GetPosition();
            String argumentName = null;
            String dataType = parser.ParseDataType();

            int position2 = parser.GetPosition();

            if (!parser.ExpectOptional(")") && !parser.ExpectOptional(",")) {
                parser.SetPosition(position);
                argumentName =
                        ParserUtils.GetObjectName(parser.ParseIdentifier());
                dataType = parser.ParseDataType();
            } else {
                parser.SetPosition(position2);
            }

            PgFunction.Argument argument = new PgFunction.Argument();
            argument.SetDataType(dataType);
            argument.SetMode(mode);
            argument.SetName(argumentName);
            tmpFunction.AddArgument(argument);

            if (parser.ExpectOptional(")")) {
                break;
            } else {
                parser.Expect(",");
            }
        }

        PgFunction function =
                schema.GetFunction(tmpFunction.GetSignature());

        parser.Expect("IS");
        function.SetComment(GetComment(parser));
        parser.Expect(";");
    }

    
    private static String GetComment(Parser parser) {
        String comment = parser.ParseString();

        if ("null".Equals(comment,StringComparison.InvariantCultureIgnoreCase)) {
            return null;
        }

        return comment;
    }

    
    private CommentParser() {
    }
}
}