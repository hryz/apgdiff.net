
package cz.startnet.utils.pgdiff;

import cz.startnet.utils.pgdiff.schema.PgSchema;
import cz.startnet.utils.pgdiff.schema.PgTable;
import cz.startnet.utils.pgdiff.schema.PgTrigger;
import java.io.PrintWriter;
import java.util.ArrayList;
import java.util.List;


public class PgDiffTriggers {

    
    public static void createTriggers(final PrintWriter writer,
            final PgSchema oldSchema, final PgSchema newSchema,
            final SearchPathHelper searchPathHelper) {
        for (final PgTable newTable : newSchema.getTables()) {
            final PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            // Add new triggers
            for (final PgTrigger trigger : getNewTriggers(oldTable, newTable)) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.println(trigger.getCreationSQL());
            }
        }
    }

    
    public static void dropTriggers(final PrintWriter writer,
            final PgSchema oldSchema, final PgSchema newSchema,
            final SearchPathHelper searchPathHelper) {
        for (final PgTable newTable : newSchema.getTables()) {
            final PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            // Drop triggers that no more exist or are modified
            for (final PgTrigger trigger :
                    getDropTriggers(oldTable, newTable)) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.println(trigger.getDropSQL());
            }
        }
    }

    
    private static List<PgTrigger> getDropTriggers(final PgTable oldTable,
            final PgTable newTable) {
        @SuppressWarnings("CollectionWithoutInitialCapacity")
        final List<PgTrigger> list = new ArrayList<PgTrigger>();

        if (newTable != null && oldTable != null) {
            final List<PgTrigger> newTriggers = newTable.getTriggers();

            for (final PgTrigger oldTrigger : oldTable.getTriggers()) {
                if (!newTriggers.contains(oldTrigger)) {
                    list.add(oldTrigger);
                }
            }
        }

        return list;
    }

    
    private static List<PgTrigger> getNewTriggers(final PgTable oldTable,
            final PgTable newTable) {
        @SuppressWarnings("CollectionWithoutInitialCapacity")
        final List<PgTrigger> list = new ArrayList<PgTrigger>();

        if (newTable != null) {
            if (oldTable == null) {
                list.addAll(newTable.getTriggers());
            } else {
                for (final PgTrigger newTrigger : newTable.getTriggers()) {
                    if (!oldTable.getTriggers().contains(newTrigger)) {
                        list.add(newTrigger);
                    }
                }
            }
        }

        return list;
    }

    
    public static void alterComments(final PrintWriter writer,
            final PgSchema oldSchema, final PgSchema newSchema,
            final SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        for (PgTable oldTable : oldSchema.getTables()) {
            final PgTable newTable = newSchema.getTable(oldTable.getName());

            if (newTable == null) {
                continue;
            }

            for (final PgTrigger oldTrigger : oldTable.getTriggers()) {
                final PgTrigger newTrigger =
                        newTable.getTrigger(oldTrigger.getName());

                if (newTrigger == null) {
                    continue;
                }

                if (oldTrigger.getComment() == null
                        && newTrigger.getComment() != null
                        || oldTrigger.getComment() != null
                        && newTrigger.getComment() != null
                        && !oldTrigger.getComment().equals(
                        newTrigger.getComment())) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.println();
                    writer.print("COMMENT ON TRIGGER ");
                    writer.print(
                            PgDiffUtils.getQuotedName(newTrigger.getName()));
                    writer.print(" ON ");
                    writer.print(PgDiffUtils.getQuotedName(
                            newTrigger.getTableName()));
                    writer.print(" IS ");
                    writer.print(newTrigger.getComment());
                    writer.println(';');
                } else if (oldTrigger.getComment() != null
                        && newTrigger.getComment() == null) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.println();
                    writer.print("COMMENT ON TRIGGER ");
                    writer.print(
                            PgDiffUtils.getQuotedName(newTrigger.getName()));
                    writer.print(" ON ");
                    writer.print(PgDiffUtils.getQuotedName(
                            newTrigger.getTableName()));
                    writer.println(" IS NULL;");
                }
            }
        }
    }

    
    private PgDiffTriggers() {
    }
}
