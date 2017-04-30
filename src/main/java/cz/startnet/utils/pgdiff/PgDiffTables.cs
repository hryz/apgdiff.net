using System;
using System.Collections.Generic;
using System.IO;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff {









public class PgDiffTables {

    
    public static void DropClusters(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        foreach (PgTable newTable in newSchema.GetTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.GetTable(newTable.GetName());
            }

            String oldCluster;

            if (oldTable == null) {
                oldCluster = null;
            } else {
                oldCluster = oldTable.GetClusterIndexName();
            }

            String newCluster = newTable.GetClusterIndexName();

            if (oldCluster != null && newCluster == null
                    && newTable.ContainsIndex(oldCluster)) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("ALTER TABLE ");
                writer.Write(PgDiffUtils.GetQuotedName(newTable.GetName()));
                writer.WriteLine(" SET WITHOUT CLUSTER;");
            }
        }
    }

    
    public static void CreateClusters(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        foreach (PgTable newTable in newSchema.GetTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.GetTable(newTable.GetName());
            }

            String oldCluster;

            if (oldTable == null) {
                oldCluster = null;
            } else {
                oldCluster = oldTable.GetClusterIndexName();
            }

            String newCluster = newTable.GetClusterIndexName();

            if ((oldCluster == null && newCluster != null)
                    || (oldCluster != null && newCluster != null
                    && newCluster.CompareTo(oldCluster) != 0)) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("ALTER TABLE ");
                writer.Write(PgDiffUtils.GetQuotedName(newTable.GetName()));
                writer.Write(" CLUSTER ON ");
                writer.Write(PgDiffUtils.GetQuotedName(newCluster));
                writer.WriteLine(';');
            }
        }
    }

    
    public static void AlterTables(TextWriter writer,
            PgDiffArguments arguments, PgSchema oldSchema,
            PgSchema newSchema, SearchPathHelper searchPathHelper) {
        foreach (PgTable newTable in newSchema.GetTables()) {
            if (oldSchema == null
                    || !oldSchema.ContainsTable(newTable.GetName())) {
                continue;
            }

            PgTable oldTable = oldSchema.GetTable(newTable.GetName());
            UpdateTableColumns(
                    writer, arguments, oldTable, newTable, searchPathHelper);
            CheckWithOids(writer, oldTable, newTable, searchPathHelper);
            CheckInherits(writer, oldTable, newTable, searchPathHelper);
            CheckTablespace(writer, oldTable, newTable, searchPathHelper);
            AddAlterStatistics(writer, oldTable, newTable, searchPathHelper);
            AddAlterStorage(writer, oldTable, newTable, searchPathHelper);
            AlterComments(writer, oldTable, newTable, searchPathHelper);
        }
    }

    
    private static void AddAlterStatistics(TextWriter writer,
            PgTable oldTable, PgTable newTable,
            SearchPathHelper searchPathHelper) {
        
        Dictionary<string,int?> stats = new Dictionary<string, int?>();

        foreach (PgColumn newColumn in newTable.GetColumns()) {
            PgColumn oldColumn = oldTable.GetColumn(newColumn.GetName());

            if (oldColumn != null) {
                int? oldStat = oldColumn.GetStatistics();
                int? newStat = newColumn.GetStatistics();
                int? newStatValue = null;

                if (newStat != null && (oldStat == null || !newStat.Equals(oldStat))) {
                    newStatValue = newStat;
                } else if (oldStat != null && newStat == null)
                {
                    newStatValue = -1;
                }

                if (newStatValue != null) {
                    stats.Add(newColumn.GetName(), newStatValue);
                }
            }
        }

        foreach (var entry in stats) {
            searchPathHelper.OutputSearchPath(writer);
            writer.WriteLine();
            writer.Write("ALTER TABLE ONLY ");
            writer.Write(PgDiffUtils.GetQuotedName(newTable.GetName()));
            writer.Write(" ALTER COLUMN ");
            writer.Write(PgDiffUtils.GetQuotedName(entry.Key));
            writer.Write(" SET STATISTICS ");
            writer.Write(entry.Value);
            writer.WriteLine(';');
        }
    }

    
    private static void AddAlterStorage(TextWriter writer,
            PgTable oldTable, PgTable newTable,
            SearchPathHelper searchPathHelper) {
        foreach (PgColumn newColumn in newTable.GetColumns()) {
            PgColumn oldColumn = oldTable.GetColumn(newColumn.GetName());
            String oldStorage = (oldColumn == null
                    || oldColumn.GetStorage() == null
                    || String.IsNullOrEmpty(oldColumn.GetStorage())) ? null
                    : oldColumn.GetStorage();
            String newStorage = (newColumn.GetStorage() == null
                    || String.IsNullOrEmpty(newColumn.GetStorage())) ? null
                    : newColumn.GetStorage();

            if (newStorage == null && oldStorage != null) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(Resources.WarningUnableToDetermineStorageType, newTable.GetName() + '.' + newColumn.GetName());

                continue;
            }

            if (newStorage == null || newStorage.Equals(oldStorage,StringComparison.InvariantCultureIgnoreCase)) {
                continue;
            }

            searchPathHelper.OutputSearchPath(writer);
            writer.WriteLine();
            writer.Write("ALTER TABLE ONLY ");
            writer.Write(PgDiffUtils.GetQuotedName(newTable.GetName()));
            writer.Write(" ALTER COLUMN ");
            writer.Write(PgDiffUtils.GetQuotedName(newColumn.GetName()));
            writer.Write(" SET STORAGE ");
            writer.Write(newStorage);
            writer.Write(';');
        }
    }

    
    private static void AddCreateTableColumns(List<String> statements,
            PgDiffArguments arguments, PgTable oldTable,
            PgTable newTable, List<PgColumn> dropDefaultsColumns) {
        foreach (PgColumn column in newTable.GetColumns()) {
            if (!oldTable.ContainsColumn(column.GetName())) {
                statements.Add("\tADD COLUMN " + column.GetFullDefinition(arguments.IsAddDefaults()));

                if (arguments.IsAddDefaults() && !column.GetNullValue()
                        && (column.GetDefaultValue() == null
                        || String.IsNullOrEmpty(column.GetDefaultValue()))) {
                    dropDefaultsColumns.Add(column);
                }
            }
        }
    }

    
    private static void AddDropTableColumns(List<String> statements,
            PgTable oldTable, PgTable newTable) {
        foreach (PgColumn column in oldTable.GetColumns()) {
            if (!newTable.ContainsColumn(column.GetName())) {
                statements.Add("\tDROP COLUMN " + PgDiffUtils.GetQuotedName(column.GetName()));
            }
        }
    }

    
    private static void AddModifyTableColumns(List<String> statements,
            PgDiffArguments arguments, PgTable oldTable,
            PgTable newTable, List<PgColumn> dropDefaultsColumns) {
        foreach (PgColumn newColumn in newTable.GetColumns()) {
            if (!oldTable.ContainsColumn(newColumn.GetName())) {
                continue;
            }

            PgColumn oldColumn = oldTable.GetColumn(newColumn.GetName());
            String newColumnName =
                    PgDiffUtils.GetQuotedName(newColumn.GetName());

            if (!oldColumn.getType().Equals(newColumn.getType())) {
                statements.Add("\tALTER COLUMN " + newColumnName + " TYPE "
                        + newColumn.getType() + " /* "
                        + String.Format(
                        Resources.TypeParameterChange,
                        newTable.GetName(), oldColumn.getType(),
                        newColumn.getType()) + " */");
            }

            String oldDefault = (oldColumn.GetDefaultValue() == null) ? ""
                    : oldColumn.GetDefaultValue();
            String newDefault = (newColumn.GetDefaultValue() == null) ? ""
                    : newColumn.GetDefaultValue();

            if (!oldDefault.Equals(newDefault)) {
                if (newDefault.Length == 0) {
                    statements.Add("\tALTER COLUMN " + newColumnName
                            + " DROP DEFAULT");
                } else {
                    statements.Add("\tALTER COLUMN " + newColumnName
                            + " SET DEFAULT " + newDefault);
                }
            }

            if (oldColumn.GetNullValue() != newColumn.GetNullValue()) {
                if (newColumn.GetNullValue()) {
                    statements.Add("\tALTER COLUMN " + newColumnName
                            + " DROP NOT NULL");
                } else {
                    if (arguments.IsAddDefaults()) {
                        String defaultValue =
                                PgColumnUtils.GetDefaultValue(
                                newColumn.getType());

                        if (defaultValue != null) {
                            statements.Add("\tALTER COLUMN " + newColumnName
                                    + " SET DEFAULT " + defaultValue);
                            dropDefaultsColumns.Add(newColumn);
                        }
                    }

                    statements.Add("\tALTER COLUMN " + newColumnName
                            + " SET NOT NULL");
                }
            }
        }
    }

    
    private static void CheckInherits(TextWriter writer,
            PgTable oldTable, PgTable newTable,
            SearchPathHelper searchPathHelper) {
        foreach (String tableName in oldTable.GetInherits()) {
            if (!newTable.GetInherits().Contains(tableName)) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine("ALTER TABLE "
                        + PgDiffUtils.GetQuotedName(newTable.GetName()));
                writer.WriteLine("\tNO INHERIT "
                        + PgDiffUtils.GetQuotedName(tableName) + ';');
            }
        }

        foreach (String tableName in newTable.GetInherits()) {
            if (!oldTable.GetInherits().Contains(tableName)) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine("ALTER TABLE "
                        + PgDiffUtils.GetQuotedName(newTable.GetName()));
                writer.WriteLine("\tINHERIT "
                        + PgDiffUtils.GetQuotedName(tableName) + ';');
            }
        }
    }

    
    private static void CheckWithOids(TextWriter writer,
            PgTable oldTable, PgTable newTable,
            SearchPathHelper searchPathHelper) {
        if (oldTable.GetWith() == null && newTable.GetWith() == null
                || oldTable.GetWith() != null
                && oldTable.GetWith().Equals(newTable.GetWith())) {
            return;
        }

        searchPathHelper.OutputSearchPath(writer);
        writer.WriteLine();
        writer.WriteLine("ALTER TABLE "
                + PgDiffUtils.GetQuotedName(newTable.GetName()));

        if (newTable.GetWith() == null
                || "OIDS=false".Equals(newTable.GetWith(),StringComparison.InvariantCultureIgnoreCase)) {
            writer.WriteLine("\tSET WITHOUT OIDS;");
        } else if ("OIDS".Equals(newTable.GetWith(),StringComparison.InvariantCultureIgnoreCase)
                || "OIDS=true".Equals(newTable.GetWith(),StringComparison.InvariantCultureIgnoreCase)) {
            writer.WriteLine("\tSET WITH OIDS;");
        } else {
            writer.WriteLine("\tSET " + newTable.GetWith() + ";");
        }
    }

    
    private static void CheckTablespace(TextWriter writer,
            PgTable oldTable, PgTable newTable,
            SearchPathHelper searchPathHelper) {
        if (oldTable.GetTablespace() == null && newTable.GetTablespace() == null
                || oldTable.GetTablespace() != null
                && oldTable.GetTablespace().Equals(newTable.GetTablespace())) {
            return;
        }

        searchPathHelper.OutputSearchPath(writer);
        writer.WriteLine();
        writer.WriteLine("ALTER TABLE "
                + PgDiffUtils.GetQuotedName(newTable.GetName()));
        writer.WriteLine("\tTABLESPACE " + newTable.GetTablespace() + ';');
    }

    
    public static void CreateTables(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        foreach (PgTable table in newSchema.GetTables()) {
            if (oldSchema == null
                    || !oldSchema.ContainsTable(table.GetName())) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(table.GetCreationSql());
            }
        }
    }

    
    public static void DropTables(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        foreach (PgTable table in oldSchema.GetTables()) {
            if (!newSchema.ContainsTable(table.GetName())) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(table.GetDropSql());
            }
        }
    }

    
    private static void UpdateTableColumns(TextWriter writer,
            PgDiffArguments arguments, PgTable oldTable,
            PgTable newTable, SearchPathHelper searchPathHelper) {
        
        List<String> statements = new List<string>();
        
        List<PgColumn> dropDefaultsColumns = new List<PgColumn>();
        AddDropTableColumns(statements, oldTable, newTable);
        AddCreateTableColumns(
                statements, arguments, oldTable, newTable, dropDefaultsColumns);
        AddModifyTableColumns(
                statements, arguments, oldTable, newTable, dropDefaultsColumns);

        if (statements.Count > 0) {
            String quotedTableName =
                    PgDiffUtils.GetQuotedName(newTable.GetName());
            searchPathHelper.OutputSearchPath(writer);
            writer.WriteLine();
            writer.WriteLine("ALTER TABLE " + quotedTableName);

            for (int i = 0; i < statements.Count; i++) {
                writer.Write(statements[i]);
                writer.WriteLine((i + 1) < statements.Count ? "," : ";");
            }

            if (dropDefaultsColumns.Count > 0) {
                writer.WriteLine();
                writer.WriteLine("ALTER TABLE " + quotedTableName);

                for (int i = 0; i < dropDefaultsColumns.Count; i++) {
                    writer.Write("\tALTER COLUMN ");
                    writer.Write(PgDiffUtils.GetQuotedName(
                            dropDefaultsColumns[i].GetName()));
                    writer.Write(" DROP DEFAULT");
                    writer.WriteLine((i + 1) < dropDefaultsColumns.Count ? "," : ";");
                }
            }
        }
    }

    
    private static void AlterComments(TextWriter writer,
            PgTable oldTable, PgTable newTable,
            SearchPathHelper searchPathHelper) {
        if (oldTable.GetComment() == null
                && newTable.GetComment() != null
                || oldTable.GetComment() != null
                && newTable.GetComment() != null
                && !oldTable.GetComment().Equals(newTable.GetComment())) {
            searchPathHelper.OutputSearchPath(writer);
            writer.WriteLine();
            writer.Write("COMMENT ON TABLE ");
            writer.Write(PgDiffUtils.GetQuotedName(newTable.GetName()));
            writer.Write(" IS ");
            writer.Write(newTable.GetComment());
            writer.WriteLine(';');
        } else if (oldTable.GetComment() != null
                && newTable.GetComment() == null) {
            searchPathHelper.OutputSearchPath(writer);
            writer.WriteLine();
            writer.Write("COMMENT ON TABLE ");
            writer.Write(PgDiffUtils.GetQuotedName(newTable.GetName()));
            writer.WriteLine(" IS NULL;");
        }

        foreach (PgColumn newColumn in newTable.GetColumns()) {
            PgColumn oldColumn = oldTable.GetColumn(newColumn.GetName());
            String oldComment =
                    oldColumn == null ? null : oldColumn.GetComment();
            String newComment = newColumn.GetComment();

            if (newComment != null && (oldComment == null ? newComment != null
                    : !oldComment.Equals(newComment))) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON COLUMN ");
                writer.Write(PgDiffUtils.GetQuotedName(newTable.GetName()));
                writer.Write('.');
                writer.Write(PgDiffUtils.GetQuotedName(newColumn.GetName()));
                writer.Write(" IS ");
                writer.Write(newColumn.GetComment());
                writer.WriteLine(';');
            } else if (oldComment != null && newComment == null) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON COLUMN ");
                writer.Write(PgDiffUtils.GetQuotedName(newTable.GetName()));
                writer.Write('.');
                writer.Write(PgDiffUtils.GetQuotedName(newColumn.GetName()));
                writer.WriteLine(" IS NULL;");
            }
        }
    }

    
    private PgDiffTables() {
    }
}
}