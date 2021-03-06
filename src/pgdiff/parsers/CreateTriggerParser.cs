using pgdiff.schema;

namespace pgdiff.parsers
{
    public class CreateTriggerParser
    {
        private CreateTriggerParser()
        {
        }


        public static void Parse(PgDatabase database, string statement, bool ignoreSlonyTriggers)
        {
            var parser = new Parser(statement);
            parser.Expect("CREATE", "TRIGGER");

            var triggerName = parser.ParseIdentifier();
            var objectName = ParserUtils.GetObjectName(triggerName);

            var trigger = new PgTrigger();
            trigger.Name = objectName;

            if (parser.ExpectOptional("BEFORE"))
                trigger.Before = true;
            else if (parser.ExpectOptional("AFTER"))
                trigger.Before = false;

            var first = true;

            while (true)
            {
                if (!first && !parser.ExpectOptional("OR"))
                    break;

                if (parser.ExpectOptional("INSERT"))
                {
                    trigger.OnInsert = true;
                }
                else if (parser.ExpectOptional("UPDATE"))
                {
                    trigger.OnUpdate = true;

                    if (parser.ExpectOptional("OF"))
                        do
                        {
                            trigger.AddUpdateColumn(parser.ParseIdentifier());
                        }
                        while (parser.ExpectOptional(","));
                }
                else if (parser.ExpectOptional("DELETE"))
                {
                    trigger.OnDelete = true;
                }
                else if (parser.ExpectOptional("TRUNCATE"))
                {
                    trigger.OnTruncate = true;
                }
                else if (first)
                {
                    break;
                }
                else
                {
                    parser.ThrowUnsupportedCommand();
                }

                first = false;
            }

            parser.Expect("ON");

            var tableName = parser.ParseIdentifier();

            trigger.TableName = ParserUtils.GetObjectName(tableName);

            if (parser.ExpectOptional("FOR"))
            {
                parser.ExpectOptional("EACH");

                if (parser.ExpectOptional("ROW"))
                    trigger.ForEachRow = true;
                else if (parser.ExpectOptional("STATEMENT"))
                    trigger.ForEachRow = false;
                else
                    parser.ThrowUnsupportedCommand();
            }

            if (parser.ExpectOptional("WHEN"))
            {
                parser.Expect("(");
                trigger.When = parser.GetExpression();
                parser.Expect(")");
            }

            parser.Expect("EXECUTE", "PROCEDURE");
            trigger.Function = parser.GetRest();

            var ignoreSlonyTrigger = ignoreSlonyTriggers
                                     && ("_slony_logtrigger".Equals(trigger.Name)
                                         || "_slony_denyaccess".Equals(trigger.Name));

            if (!ignoreSlonyTrigger)
            {
                var tableSchema = database.GetSchema(ParserUtils.GetSchemaName(tableName, database));
                tableSchema.GetTable(trigger.TableName).AddTrigger(trigger);
            }
        }
    }
}