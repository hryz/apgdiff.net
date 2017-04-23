using System.Collections.Generic;
using System.IO;
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
                if (!newTriggers.Contains(oldTrigger)) {
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
                    if (!oldTable.getTriggers().Contains(newTrigger)) {
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
                        newTable.getTrigger(oldTrigger.getName());

                if (newTrigger == null) {
                    continue;
                }

                if (oldTrigger.getComment() == null
                        && newTrigger.getComment() != null
                        || oldTrigger.getComment() != null
                        && newTrigger.getComment() != null
                        && !oldTrigger.getComment().Equals(
                        newTrigger.getComment())) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON TRIGGER ");
                    writer.Write(
                            PgDiffUtils.getQuotedName(newTrigger.getName()));
                    writer.Write(" ON ");
                    writer.Write(PgDiffUtils.getQuotedName(
                            newTrigger.getTableName()));
                    writer.Write(" IS ");
                    writer.Write(newTrigger.getComment());
                    writer.WriteLine(';');
                } else if (oldTrigger.getComment() != null
                        && newTrigger.getComment() == null) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON TRIGGER ");
                    writer.Write(
                            PgDiffUtils.getQuotedName(newTrigger.getName()));
                    writer.Write(" ON ");
                    writer.Write(PgDiffUtils.getQuotedName(
                            newTrigger.getTableName()));
                    writer.WriteLine(" IS NULL;");
                }
            }
        }
    }

    
    private PgDiffTriggers() {
    }
}
}