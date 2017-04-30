using System;
using pgdiff.schema;

namespace pgdiff.parsers {



public class CreateTriggerParser {

    
    public static void Parse(PgDatabase database,
            String statement, bool ignoreSlonyTriggers) {
        Parser parser = new Parser(statement);
        parser.Expect("CREATE", "TRIGGER");

        String triggerName = parser.ParseIdentifier();
        String objectName = ParserUtils.GetObjectName(triggerName);

        PgTrigger trigger = new PgTrigger();
        trigger.Name = objectName;

        if (parser.ExpectOptional("BEFORE")) {
            trigger.Before =true;
        } else if (parser.ExpectOptional("AFTER")) {
            trigger.Before =false;
        }

        bool first = true;

        while (true) {
            if (!first && !parser.ExpectOptional("OR")) {
                break;
            } else if (parser.ExpectOptional("INSERT")) {
                trigger.OnInsert =true;
            } else if (parser.ExpectOptional("UPDATE")) {
                trigger.OnUpdate = true;

                if (parser.ExpectOptional("OF")) {
                    do {
                        trigger.AddUpdateColumn(parser.ParseIdentifier());
                    } while (parser.ExpectOptional(","));
                }
            } else if (parser.ExpectOptional("DELETE")) {
                trigger.OnDelete =true;
            } else if (parser.ExpectOptional("TRUNCATE")) {
                trigger.OnTruncate = true;
            } else if (first) {
                break;
            } else {
                parser.ThrowUnsupportedCommand();
            }

            first = false;
        }

        parser.Expect("ON");

        String tableName = parser.ParseIdentifier();

        trigger.TableName = ParserUtils.GetObjectName(tableName);

        if (parser.ExpectOptional("FOR")) {
            parser.ExpectOptional("EACH");

            if (parser.ExpectOptional("ROW")) {
                trigger.ForEachRow = true;
            } else if (parser.ExpectOptional("STATEMENT"))
            {
                trigger.ForEachRow = false;
            } else {
                parser.ThrowUnsupportedCommand();
            }
        }

        if (parser.ExpectOptional("WHEN")) {
            parser.Expect("(");
            trigger.When =parser.GetExpression();
            parser.Expect(")");
        }

        parser.Expect("EXECUTE", "PROCEDURE");
        trigger.Function = parser.GetRest();

        bool ignoreSlonyTrigger = ignoreSlonyTriggers
                && ("_slony_logtrigger".Equals(trigger.Name)
                || "_slony_denyaccess".Equals(trigger.Name));

        if (!ignoreSlonyTrigger) {
            PgSchema tableSchema = database.GetSchema(
                    ParserUtils.GetSchemaName(tableName, database));
            tableSchema.GetTable(trigger.TableName).AddTrigger(trigger);
        }
    }

    
    private CreateTriggerParser() {
    }
}
}