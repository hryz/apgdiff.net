
package cz.startnet.utils.pgdiff.parsers;

import cz.startnet.utils.pgdiff.schema.PgDatabase;
import cz.startnet.utils.pgdiff.schema.PgSchema;
import cz.startnet.utils.pgdiff.schema.PgTrigger;


public class CreateTriggerParser {

    
    public static void parse(PgDatabase database,
            String statement, boolean ignoreSlonyTriggers) {
        Parser parser = new Parser(statement);
        parser.expect("CREATE", "TRIGGER");

        String triggerName = parser.parseIdentifier();
        String objectName = ParserUtils.getObjectName(triggerName);

        PgTrigger trigger = new PgTrigger();
        trigger.setName(objectName);

        if (parser.expectOptional("BEFORE")) {
            trigger.setBefore(true);
        } else if (parser.expectOptional("AFTER")) {
            trigger.setBefore(false);
        }

        boolean first = true;

        while (true) {
            if (!first && !parser.expectOptional("OR")) {
                break;
            } else if (parser.expectOptional("INSERT")) {
                trigger.setOnInsert(true);
            } else if (parser.expectOptional("UPDATE")) {
                trigger.setOnUpdate(true);

                if (parser.expectOptional("OF")) {
                    do {
                        trigger.addUpdateColumn(parser.parseIdentifier());
                    } while (parser.expectOptional(","));
                }
            } else if (parser.expectOptional("DELETE")) {
                trigger.setOnDelete(true);
            } else if (parser.expectOptional("TRUNCATE")) {
                trigger.setOnTruncate(true);
            } else if (first) {
                break;
            } else {
                parser.throwUnsupportedCommand();
            }

            first = false;
        }

        parser.expect("ON");

        String tableName = parser.parseIdentifier();

        trigger.setTableName(ParserUtils.getObjectName(tableName));

        if (parser.expectOptional("FOR")) {
            parser.expectOptional("EACH");

            if (parser.expectOptional("ROW")) {
                trigger.setForEachRow(true);
            } else if (parser.expectOptional("STATEMENT")) {
                trigger.setForEachRow(false);
            } else {
                parser.throwUnsupportedCommand();
            }
        }

        if (parser.expectOptional("WHEN")) {
            parser.expect("(");
            trigger.setWhen(parser.getExpression());
            parser.expect(")");
        }

        parser.expect("EXECUTE", "PROCEDURE");
        trigger.setFunction(parser.getRest());

        boolean ignoreSlonyTrigger = ignoreSlonyTriggers
                && ("_slony_logtrigger".equals(trigger.getName())
                || "_slony_denyaccess".equals(trigger.getName()));

        if (!ignoreSlonyTrigger) {
            PgSchema tableSchema = database.getSchema(
                    ParserUtils.getSchemaName(tableName, database));
            tableSchema.getTable(trigger.getTableName()).addTrigger(trigger);
        }
    }

    
    private CreateTriggerParser() {
    }
}
