using System;
using System.Collections.Generic;
using System.IO;
using pgdiff.schema;

namespace pgdiff {






public class PgDiffIndexes {

    
    public static void CreateIndexes(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        foreach (PgTable newTable in newSchema.GetTables()) {
            String newTableName = newTable.GetName();

            // Add new indexes
            if (oldSchema == null) {
                foreach (PgIndex index in newTable.GetIndexes()) {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(index.GetCreationSql());
                }
            } else {
                foreach (PgIndex index in GetNewIndexes(
                        oldSchema.GetTable(newTableName), newTable)) {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(index.GetCreationSql());
                }
            }
        }
    }

    
    public static void DropIndexes(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        foreach (PgTable newTable in newSchema.GetTables()) {
            String newTableName = newTable.GetName();
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.GetTable(newTableName);
            }

            // Drop indexes that do not exist in new schema or are modified
            foreach (PgIndex index in GetDropIndexes(oldTable, newTable)) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(index.GetDropSql());
            }
        }
    }

    
    private static List<PgIndex> GetDropIndexes(PgTable oldTable,
            PgTable newTable) {
        
        List<PgIndex> list = new List<PgIndex>();

        if (newTable != null && oldTable != null) {
            foreach (PgIndex index in oldTable.GetIndexes()) {
                if (!newTable.ContainsIndex(index.GetName())
                        || !newTable.GetIndex(index.GetName()).Equals(index)) {
                    list.Add(index);
                }
            }
        }

        return list;
    }

    
    private static List<PgIndex> GetNewIndexes(PgTable oldTable,
            PgTable newTable) {
        
        List<PgIndex> list = new List<PgIndex>();

        if (newTable != null) {
            if (oldTable == null) {
                foreach (PgIndex index in newTable.GetIndexes()) {
                    list.Add(index);
                }
            } else {
                foreach (PgIndex index in newTable.GetIndexes()) {
                    if (!oldTable.ContainsIndex(index.GetName())
                            || !oldTable.GetIndex(index.GetName()).
                            Equals(index)) {
                        list.Add(index);
                    }
                }
            }
        }

        return list;
    }

    
    public static void AlterComments(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        foreach (PgIndex oldIndex in oldSchema.GetIndexes()) {
            PgIndex newIndex = newSchema.GetIndex(oldIndex.GetName());

            if (newIndex == null) {
                continue;
            }

            if (oldIndex.GetComment() == null
                    && newIndex.GetComment() != null
                    || oldIndex.GetComment() != null
                    && newIndex.GetComment() != null
                    && !oldIndex.GetComment().Equals(
                    newIndex.GetComment())) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON INDEX ");
                writer.Write(
                        PgDiffUtils.GetQuotedName(newIndex.GetName()));
                writer.Write(" IS ");
                writer.Write(newIndex.GetComment());
                writer.WriteLine(';');
            } else if (oldIndex.GetComment() != null
                    && newIndex.GetComment() == null) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON INDEX ");
                writer.Write(
                        PgDiffUtils.GetQuotedName(newIndex.GetName()));
                writer.WriteLine(" IS NULL;");
            }
        }
    }

    
    private PgDiffIndexes() {
    }
}
}