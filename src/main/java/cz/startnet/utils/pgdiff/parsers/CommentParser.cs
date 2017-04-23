
namespace cz.startnet.utils.pgdiff.parsers {

import cz.startnet.utils.pgdiff.Resources;
import cz.startnet.utils.pgdiff.schema.PgColumn;
import cz.startnet.utils.pgdiff.schema.PgConstraint;
import cz.startnet.utils.pgdiff.schema.PgDatabase;
import cz.startnet.utils.pgdiff.schema.PgFunction;
import cz.startnet.utils.pgdiff.schema.PgIndex;
import cz.startnet.utils.pgdiff.schema.PgSchema;
import cz.startnet.utils.pgdiff.schema.PgSequence;
import cz.startnet.utils.pgdiff.schema.PgTable;
import cz.startnet.utils.pgdiff.schema.PgTrigger;
import cz.startnet.utils.pgdiff.schema.PgView;
import java.text.MessageFormat;


public class CommentParser {

    
    public static void parse(PgDatabase database,
            String statement, boolean outputIgnoredStatements) {
        Parser parser = new Parser(statement);
        parser.expect("COMMENT", "ON");

        if (parser.expectOptional("TABLE")) {
            parseTable(parser, database);
        } else if (parser.expectOptional("COLUMN")) {
            parseColumn(parser, database);
        } else if (parser.expectOptional("CONSTRAINT")) {
            parseConstraint(parser, database);
        } else if (parser.expectOptional("DATABASE")) {
            parseDatabase(parser, database);
        } else if (parser.expectOptional("FUNCTION")) {
            parseFunction(parser, database);
        } else if (parser.expectOptional("INDEX")) {
            parseIndex(parser, database);
        } else if (parser.expectOptional("SCHEMA")) {
            parseSchema(parser, database);
        } else if (parser.expectOptional("SEQUENCE")) {
            parseSequence(parser, database);
        } else if (parser.expectOptional("TRIGGER")) {
            parseTrigger(parser, database);
        } else if (parser.expectOptional("VIEW")) {
            parseView(parser, database);
        } else if (outputIgnoredStatements) {
            database.addIgnoredStatement(statement);
        }
    }

    
    private static void parseTable(Parser parser,
            PgDatabase database) {
        String tableName = parser.parseIdentifier();
        String objectName = ParserUtils.getObjectName(tableName);
        String schemaName =
                ParserUtils.getSchemaName(tableName, database);

        PgTable table =
                database.getSchema(schemaName).getTable(objectName);

        parser.expect("IS");
        table.setComment(getComment(parser));
        parser.expect(";");
    }

    
    private static void parseConstraint(Parser parser,
            PgDatabase database) {
        String constraintName =
                ParserUtils.getObjectName(parser.parseIdentifier());

        parser.expect("ON");

        String tableName = parser.parseIdentifier();
        String objectName = ParserUtils.getObjectName(tableName);
        String schemaName =
                ParserUtils.getSchemaName(constraintName, database);

        PgConstraint constraint = database.getSchema(schemaName).
                getTable(objectName).getConstraint(constraintName);

        parser.expect("IS");
        constraint.setComment(getComment(parser));
        parser.expect(";");
    }

    
    private static void parseDatabase(Parser parser,
            PgDatabase database) {
        parser.parseIdentifier();
        parser.expect("IS");
        database.setComment(getComment(parser));
        parser.expect(";");
    }

    
    private static void parseIndex(Parser parser,
            PgDatabase database) {
        String indexName = parser.parseIdentifier();
        String objectName = ParserUtils.getObjectName(indexName);
        String schemaName =
                ParserUtils.getSchemaName(indexName, database);
        PgSchema schema = database.getSchema(schemaName);

        PgIndex index = schema.getIndex(objectName);

        if (index == null) {
            PgConstraint primaryKey = schema.getPrimaryKey(objectName);
            parser.expect("IS");
            primaryKey.setComment(getComment(parser));
            parser.expect(";");
        } else {
            parser.expect("IS");
            index.setComment(getComment(parser));
            parser.expect(";");
        }
    }

    
    private static void parseSchema(Parser parser,
            PgDatabase database) {
        String schemaName =
                ParserUtils.getObjectName(parser.parseIdentifier());
        PgSchema schema = database.getSchema(schemaName);

        parser.expect("IS");
        schema.setComment(getComment(parser));
        parser.expect(";");
    }

    
    private static void parseSequence(Parser parser,
            PgDatabase database) {
        String sequenceName = parser.parseIdentifier();
        String objectName = ParserUtils.getObjectName(sequenceName);
        String schemaName =
                ParserUtils.getSchemaName(sequenceName, database);

        PgSequence sequence =
                database.getSchema(schemaName).getSequence(objectName);

        parser.expect("IS");
        sequence.setComment(getComment(parser));
        parser.expect(";");
    }

    
    private static void parseTrigger(Parser parser,
            PgDatabase database) {
        String triggerName =
                ParserUtils.getObjectName(parser.parseIdentifier());

        parser.expect("ON");

        String tableName = parser.parseIdentifier();
        String objectName = ParserUtils.getObjectName(tableName);
        String schemaName =
                ParserUtils.getSchemaName(triggerName, database);

        PgTrigger trigger = database.getSchema(schemaName).
                getTable(objectName).getTrigger(triggerName);

        parser.expect("IS");
        trigger.setComment(getComment(parser));
        parser.expect(";");
    }

    
    private static void parseView(Parser parser,
            PgDatabase database) {
        String viewName = parser.parseIdentifier();
        String objectName = ParserUtils.getObjectName(viewName);
        String schemaName =
                ParserUtils.getSchemaName(viewName, database);

        PgView view = database.getSchema(schemaName).getView(objectName);

        parser.expect("IS");
        view.setComment(getComment(parser));
        parser.expect(";");
    }

    
    private static void parseColumn(Parser parser,
            PgDatabase database) {
        String columnName = parser.parseIdentifier();
        String objectName = ParserUtils.getObjectName(columnName);
        String tableName = ParserUtils.getSecondObjectName(columnName);
        String schemaName = ParserUtils.getThirdObjectName(columnName);
        PgSchema schema = database.getSchema(schemaName);

        PgTable table = schema.getTable(tableName);

        if (table == null) {
            PgView view = schema.getView(tableName);
            parser.expect("IS");

            String comment = getComment(parser);

            if (comment == null) {
                view.removeColumnComment(objectName);
            } else {
                view.addColumnComment(objectName, comment);
            }
            parser.expect(";");
        } else {
            PgColumn column = table.getColumn(objectName);

            if (column == null) {
                throw new ParserException(MessageFormat.format(
                        Resources.getString("CannotFindColumnInTable"),
                        columnName, table.getName()));
            }

            parser.expect("IS");
            column.setComment(getComment(parser));
            parser.expect(";");
        }
    }

    
    private static void parseFunction(Parser parser,
            PgDatabase database) {
        String functionName = parser.parseIdentifier();
        String objectName = ParserUtils.getObjectName(functionName);
        String schemaName =
                ParserUtils.getSchemaName(functionName, database);
        PgSchema schema = database.getSchema(schemaName);

        parser.expect("(");

        PgFunction tmpFunction = new PgFunction();
        tmpFunction.setName(objectName);

        while (!parser.expectOptional(")")) {
            String mode;

            if (parser.expectOptional("IN")) {
                mode = "IN";
            } else if (parser.expectOptional("OUT")) {
                mode = "OUT";
            } else if (parser.expectOptional("INOUT")) {
                mode = "INOUT";
            } else if (parser.expectOptional("VARIADIC")) {
                mode = "VARIADIC";
            } else {
                mode = null;
            }

            int position = parser.getPosition();
            String argumentName = null;
            String dataType = parser.parseDataType();

            int position2 = parser.getPosition();

            if (!parser.expectOptional(")") && !parser.expectOptional(",")) {
                parser.setPosition(position);
                argumentName =
                        ParserUtils.getObjectName(parser.parseIdentifier());
                dataType = parser.parseDataType();
            } else {
                parser.setPosition(position2);
            }

            PgFunction.Argument argument = new PgFunction.Argument();
            argument.setDataType(dataType);
            argument.setMode(mode);
            argument.setName(argumentName);
            tmpFunction.addArgument(argument);

            if (parser.expectOptional(")")) {
                break;
            } else {
                parser.expect(",");
            }
        }

        PgFunction function =
                schema.getFunction(tmpFunction.getSignature());

        parser.expect("IS");
        function.setComment(getComment(parser));
        parser.expect(";");
    }

    
    private static String getComment(Parser parser) {
        String comment = parser.parseString();

        if ("null".equalsIgnoreCase(comment)) {
            return null;
        }

        return comment;
    }

    
    private CommentParser() {
    }
}
}