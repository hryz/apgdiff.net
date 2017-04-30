using System.Collections.Generic;
using System.IO;
using System.Linq;
using pgdiff.schema;

namespace pgdiff
{
    public class PgDiffTriggers
    {
        private PgDiffTriggers()
        {
        }


        public static void CreateTriggers(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            foreach (var newTable in newSchema.GetTables())
            {
                var oldTable = oldSchema?.GetTable(newTable.Name);

                // Add new triggers
                foreach (var trigger in GetNewTriggers(oldTable, newTable))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(trigger.GetCreationSql());
                }
            }
        }


        public static void DropTriggers(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            foreach (var newTable in newSchema.GetTables())
            {
                var oldTable = oldSchema?.GetTable(newTable.Name);

                // Drop triggers that no more exist or are modified
                foreach (var trigger in GetDropTriggers(oldTable, newTable))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(trigger.GetDropSql());
                }
            }
        }


        private static List<PgTrigger> GetDropTriggers(PgTable oldTable, PgTable newTable)
        {
            var list = new List<PgTrigger>();

            if (newTable != null && oldTable != null)
            {
                var newTriggers = newTable.GetTriggers();

                foreach (var oldTrigger in oldTable.GetTriggers())
                    if (newTriggers.All(t => !t.Equals(oldTrigger))) list.Add(oldTrigger);
            }

            return list;
        }


        private static List<PgTrigger> GetNewTriggers(PgTable oldTable, PgTable newTable)
        {
            var list = new List<PgTrigger>();

            if (newTable != null)
                if (oldTable == null) list.AddRange(newTable.GetTriggers());
                else
                    foreach (var newTrigger in newTable.GetTriggers())
                        if (oldTable.GetTriggers().All(t => !t.Equals(newTrigger))) list.Add(newTrigger);

            return list;
        }


        public static void AlterComments(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            if (oldSchema == null) return;

            foreach (var oldTable in oldSchema.GetTables())
            {
                var newTable = newSchema.GetTable(oldTable.Name);

                if (newTable == null) continue;

                foreach (var oldTrigger in oldTable.GetTriggers())
                {
                    var newTrigger =
                        newTable.GetTrigger(oldTrigger.Name);

                    if (newTrigger == null) continue;

                    if (oldTrigger.Comment == null
                        && newTrigger.Comment != null
                        || oldTrigger.Comment != null
                        && newTrigger.Comment != null
                        && !oldTrigger.Comment.Equals(
                            newTrigger.Comment))
                    {
                        searchPathHelper.OutputSearchPath(writer);
                        writer.WriteLine();
                        writer.Write("COMMENT ON TRIGGER ");
                        writer.Write(
                            PgDiffUtils.GetQuotedName(newTrigger.Name));
                        writer.Write(" ON ");
                        writer.Write(PgDiffUtils.GetQuotedName(
                            newTrigger.TableName));
                        writer.Write(" IS ");
                        writer.Write(newTrigger.Comment);
                        writer.WriteLine(';');
                    }
                    else if (oldTrigger.Comment != null
                             && newTrigger.Comment == null)
                    {
                        searchPathHelper.OutputSearchPath(writer);
                        writer.WriteLine();
                        writer.Write("COMMENT ON TRIGGER ");
                        writer.Write(
                            PgDiffUtils.GetQuotedName(newTrigger.Name));
                        writer.Write(" ON ");
                        writer.Write(PgDiffUtils.GetQuotedName(
                            newTrigger.TableName));
                        writer.WriteLine(" IS NULL;");
                    }
                }
            }
        }
    }
}