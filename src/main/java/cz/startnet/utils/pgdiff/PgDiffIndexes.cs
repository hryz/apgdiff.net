
using System;
using System.Collections.Generic;
using cz.startnet.utils.pgdiff.schema;

namespace cz.startnet.utils.pgdiff {






public class PgDiffIndexes {

    
    public static void createIndexes(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        for (PgTable newTable : newSchema.getTables()) {
            String newTableName = newTable.getName();

            // Add new indexes
            if (oldSchema == null) {
                for (PgIndex index : newTable.getIndexes()) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(index.getCreationSQL());
                }
            } else {
                for (PgIndex index : getNewIndexes(
                        oldSchema.getTable(newTableName), newTable)) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(index.getCreationSQL());
                }
            }
        }
    }

    
    public static void dropIndexes(TextWriter writer,
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
                writer.WriteLine();
                writer.WriteLine(index.getDropSQL());
            }
        }
    }

    
    private static List<PgIndex> getDropIndexes(PgTable oldTable,
            PgTable newTable) {
        
        List<PgIndex> list = new ArrayList<PgIndex>();

        if (newTable != null && oldTable != null) {
            for (PgIndex index : oldTable.getIndexes()) {
                if (!newTable.containsIndex(index.getName())
                        || !newTable.getIndex(index.getName()).Equals(index)) {
                    list.add(index);
                }
            }
        }

        return list;
    }

    
    private static List<PgIndex> getNewIndexes(PgTable oldTable,
            PgTable newTable) {
        
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
                            Equals(index)) {
                        list.add(index);
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

        for (PgIndex oldIndex : oldSchema.getIndexes()) {
            PgIndex newIndex = newSchema.getIndex(oldIndex.getName());

            if (newIndex == null) {
                continue;
            }

            if (oldIndex.getComment() == null
                    && newIndex.getComment() != null
                    || oldIndex.getComment() != null
                    && newIndex.getComment() != null
                    && !oldIndex.getComment().Equals(
                    newIndex.getComment())) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON INDEX ");
                writer.Write(
                        PgDiffUtils.getQuotedName(newIndex.getName()));
                writer.Write(" IS ");
                writer.Write(newIndex.getComment());
                writer.WriteLine(';');
            } else if (oldIndex.getComment() != null
                    && newIndex.getComment() == null) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON INDEX ");
                writer.Write(
                        PgDiffUtils.getQuotedName(newIndex.getName()));
                writer.WriteLine(" IS NULL;");
            }
        }
    }

    
    private PgDiffIndexes() {
    }
}
}