
namespace cz.startnet.utils.pgdiff {

import cz.startnet.utils.pgdiff.schema.PgIndex;
import cz.startnet.utils.pgdiff.schema.PgSchema;
import cz.startnet.utils.pgdiff.schema.PgTable;





public class PgDiffIndexes {

    
    public static void createIndexes(PrintWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        for (PgTable newTable : newSchema.getTables()) {
            String newTableName = newTable.getName();

            // Add new indexes
            if (oldSchema == null) {
                for (PgIndex index : newTable.getIndexes()) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.println();
                    writer.println(index.getCreationSQL());
                }
            } else {
                for (PgIndex index : getNewIndexes(
                        oldSchema.getTable(newTableName), newTable)) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.println();
                    writer.println(index.getCreationSQL());
                }
            }
        }
    }

    
    public static void dropIndexes(PrintWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        for (PgTable newTable : newSchema.getTables()) {
            String newTableName = newTable.getName();
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTableName);
            }

            // Drop indexes that do not exist in new schema or are modified
            for (PgIndex index : getDropIndexes(oldTable, newTable)) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.println(index.getDropSQL());
            }
        }
    }

    
    private static List<PgIndex> getDropIndexes(PgTable oldTable,
            PgTable newTable) {
        @SuppressWarnings("CollectionWithoutInitialCapacity")
        List<PgIndex> list = new ArrayList<PgIndex>();

        if (newTable != null && oldTable != null) {
            for (PgIndex index : oldTable.getIndexes()) {
                if (!newTable.containsIndex(index.getName())
                        || !newTable.getIndex(index.getName()).equals(index)) {
                    list.add(index);
                }
            }
        }

        return list;
    }

    
    private static List<PgIndex> getNewIndexes(PgTable oldTable,
            PgTable newTable) {
        @SuppressWarnings("CollectionWithoutInitialCapacity")
        List<PgIndex> list = new ArrayList<PgIndex>();

        if (newTable != null) {
            if (oldTable == null) {
                for (PgIndex index : newTable.getIndexes()) {
                    list.add(index);
                }
            } else {
                for (PgIndex index : newTable.getIndexes()) {
                    if (!oldTable.containsIndex(index.getName())
                            || !oldTable.getIndex(index.getName()).
                            equals(index)) {
                        list.add(index);
                    }
                }
            }
        }

        return list;
    }

    
    public static void alterComments(PrintWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        for (PgIndex oldIndex : oldSchema.getIndexes()) {
            PgIndex newIndex = newSchema.getIndex(oldIndex.getName());

            if (newIndex == null) {
                continue;
            }

            if (oldIndex.getComment() == null
                    && newIndex.getComment() != null
                    || oldIndex.getComment() != null
                    && newIndex.getComment() != null
                    && !oldIndex.getComment().equals(
                    newIndex.getComment())) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.print("COMMENT ON INDEX ");
                writer.print(
                        PgDiffUtils.getQuotedName(newIndex.getName()));
                writer.print(" IS ");
                writer.print(newIndex.getComment());
                writer.println(';');
            } else if (oldIndex.getComment() != null
                    && newIndex.getComment() == null) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.print("COMMENT ON INDEX ");
                writer.print(
                        PgDiffUtils.getQuotedName(newIndex.getName()));
                writer.println(" IS NULL;");
            }
        }
    }

    
    private PgDiffIndexes() {
    }
}
}