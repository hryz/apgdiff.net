using System;
using pgdiff.schema;

namespace pgdiff.parsers {



public class CreateTriggerParser {

    
    public static void parse(PgDatabase database,
            String statement, bool ignoreSlonyTriggers) {
        Parser parser = new Parser(statement);
        parser.expect("CREATE", "TRIGGER");

        String triggerName = parser.parseIdentifier();
        String objectName = ParserUtils.getObjectName(triggerName);

        PgTrigger trigger = new PgTrigger();
        trigger.name = objectName;

        if (parser.expectOptional("BEFORE")) {
            trigger.before =true;
        } else if (parser.expectOptional("AFTER")) {
            trigger.before =false;
        }

        bool first = true;

        while (true) {
            if (!first && !parser.expectOptional("OR")) {
                break;
            } else if (parser.expectOptional("INSERT")) {
                trigger.onInsert =true;
            } else if (parser.expectOptional("UPDATE")) {
                trigger.onUpdate = true;

                if (parser.expectOptional("OF")) {
                    do {
                        trigger.addUpdateColumn(parser.parseIdentifier());
                    } while (parser.expectOptional(","));
                }
            } else if (parser.expectOptional("DELETE")) {
                trigger.onDelete =true;
            } else if (parser.expectOptional("TRUNCATE")) {
                trigger.onTruncate = true;
            } else if (first) {
                break;
            } else {
                parser.throwUnsupportedCommand();
            }

            first = false;
        }

        parser.expect("ON");

        String tableName = parser.parseIdentifier();

        trigger.tableName = ParserUtils.getObjectName(tableName);

        if (parser.expectOptional("FOR")) {
            parser.expectOptional("EACH");

            if (parser.expectOptional("ROW")) {
                trigger.forEachRow = true;
            } else if (parser.expectOptional("STATEMENT"))
            {
                trigger.forEachRow = false;
            } else {
                parser.throwUnsupportedCommand();
            }
        }

        if (parser.expectOptional("WHEN")) {
            parser.expect("(");
            trigger.when =parser.getExpression();
            parser.expect(")");
        }

        parser.expect("EXECUTE", "PROCEDURE");
        trigger.function = parser.getRest();

        bool ignoreSlonyTrigger = ignoreSlonyTriggers
                && ("_slony_logtrigger".Equals(trigger.name)
                || "_slony_denyaccess".Equals(trigger.name));

        if (!ignoreSlonyTrigger) {
            PgSchema tableSchema = database.getSchema(
                    ParserUtils.getSchemaName(tableName, database));
            tableSchema.getTable(trigger.tableName).addTrigger(trigger);
        }
    }

    
    private CreateTriggerParser() {
    }
}
}