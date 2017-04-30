using System.Collections.Generic;
using System.IO;
using System.Linq;
using pgdiff.schema;

namespace pgdiff {






public class PgDiffTriggers {

    
    public static void createTriggers(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        foreach (PgTable newTable in newSchema.getTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            // Add new triggers
            foreach (PgTrigger trigger in getNewTriggers(oldTable, newTable)) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(trigger.getCreationSQL());
            }
        }
    }

    
    public static void dropTriggers(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        foreach (PgTable newTable in newSchema.getTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            // Drop triggers that no more exist or are modified
            foreach (PgTrigger trigger in getDropTriggers(oldTable, newTable)) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(trigger.getDropSQL());
            }
        }
    }

    
    private static List<PgTrigger> getDropTriggers(PgTable oldTable,
            PgTable newTable) {
        
        List<PgTrigger> list = new List<PgTrigger>();

        if (newTable != null && oldTable != null) {
            List<PgTrigger> newTriggers = newTable.getTriggers();

            foreach (PgTrigger oldTrigger in oldTable.getTriggers()) {
                if (newTriggers.All(t => !t.Equals(oldTrigger))) {
                    list.Add(oldTrigger);
                }
            }
        }

        return list;
    }

    
    private static List<PgTrigger> getNewTriggers(PgTable oldTable,
            PgTable newTable) {
        
        List<PgTrigger> list = new List<PgTrigger>();

        if (newTable != null) {
            if (oldTable == null) {
                list.AddRange(newTable.getTriggers());
            } else {
                foreach (PgTrigger newTrigger in newTable.getTriggers()) {
                    if (oldTable.getTriggers().All(t => !t.Equals(newTrigger))) {
                        list.Add(newTrigger);
                    }
                }
            }
        }

        return list;
    }

    
    public static void alterComments(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        foreach (PgTable oldTable in oldSchema.getTables()) {
            PgTable newTable = newSchema.getTable(oldTable.getName());

            if (newTable == null) {
                continue;
            }

            foreach (PgTrigger oldTrigger in oldTable.getTriggers()) {
                PgTrigger newTrigger =
                        newTable.getTrigger(oldTrigger.name);

                if (newTrigger == null) {
                    continue;
                }

                if (oldTrigger.comment == null
                        && newTrigger.comment != null
                        || oldTrigger.comment != null
                        && newTrigger.comment != null
                        && !oldTrigger.comment.Equals(
                        newTrigger.comment)) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON TRIGGER ");
                    writer.Write(
                            PgDiffUtils.getQuotedName(newTrigger.name));
                    writer.Write(" ON ");
                    writer.Write(PgDiffUtils.getQuotedName(
                            newTrigger.tableName));
                    writer.Write(" IS ");
                    writer.Write(newTrigger.comment);
                    writer.WriteLine(';');
                } else if (oldTrigger.comment != null
                        && newTrigger.comment == null) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON TRIGGER ");
                    writer.Write(
                            PgDiffUtils.getQuotedName(newTrigger.name));
                    writer.Write(" ON ");
                    writer.Write(PgDiffUtils.getQuotedName(
                            newTrigger.tableName));
                    writer.WriteLine(" IS NULL;");
                }
            }
        }
    }

    
    private PgDiffTriggers() {
    }
}
}