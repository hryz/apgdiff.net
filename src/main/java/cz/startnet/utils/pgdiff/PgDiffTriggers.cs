
using System.Collections.Generic;
using cz.startnet.utils.pgdiff.schema;

namespace cz.startnet.utils.pgdiff {






public class PgDiffTriggers {

    
    public static void createTriggers(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        for (PgTable newTable : newSchema.getTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            // Add new triggers
            for (PgTrigger trigger : getNewTriggers(oldTable, newTable)) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(trigger.getCreationSQL());
            }
        }
    }

    
    public static void dropTriggers(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        for (PgTable newTable : newSchema.getTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            // Drop triggers that no more exist or are modified
            for (PgTrigger trigger :
                    getDropTriggers(oldTable, newTable)) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(trigger.getDropSQL());
            }
        }
    }

    
    private static List<PgTrigger> getDropTriggers(PgTable oldTable,
            PgTable newTable) {
        
        List<PgTrigger> list = new ArrayList<PgTrigger>();

        if (newTable != null && oldTable != null) {
            List<PgTrigger> newTriggers = newTable.getTriggers();

            for (PgTrigger oldTrigger : oldTable.getTriggers()) {
                if (!newTriggers.contains(oldTrigger)) {
                    list.add(oldTrigger);
                }
            }
        }

        return list;
    }

    
    private static List<PgTrigger> getNewTriggers(PgTable oldTable,
            PgTable newTable) {
        
        List<PgTrigger> list = new ArrayList<PgTrigger>();

        if (newTable != null) {
            if (oldTable == null) {
                list.addAll(newTable.getTriggers());
            } else {
                for (PgTrigger newTrigger : newTable.getTriggers()) {
                    if (!oldTable.getTriggers().contains(newTrigger)) {
                        list.add(newTrigger);
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

        for (PgTable oldTable : oldSchema.getTables()) {
            PgTable newTable = newSchema.getTable(oldTable.getName());

            if (newTable == null) {
                continue;
            }

            for (PgTrigger oldTrigger : oldTable.getTriggers()) {
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