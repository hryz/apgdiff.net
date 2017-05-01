using System;
using System.Collections.Generic;
using System.IO;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff
{
    public class PgDiffTables
    {
        private PgDiffTables()
        {
        }


        public static void DropClusters(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            foreach (var newTable in newSchema.GetTables())
            {
                var oldTable = oldSchema?.GetTable(newTable.Name);

                var oldCluster = oldTable?.ClusterIndexName;

                var newCluster = newTable.ClusterIndexName;

                if (oldCluster != null && newCluster == null && newTable.ContainsIndex(oldCluster))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("ALTER TABLE ");
                    writer.Write(PgDiffUtils.GetQuotedName(newTable.Name));
                    writer.WriteLine(" SET WITHOUT CLUSTER;");
                }
            }
        }


        public static void CreateClusters(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            foreach (var newTable in newSchema.GetTables())
            {
                var oldTable = oldSchema?.GetTable(newTable.Name);
                var oldCluster = oldTable?.ClusterIndexName;

                var newCluster = newTable.ClusterIndexName;

                if (oldCluster == null && newCluster != null
                    || oldCluster != null && newCluster != null
                    && newCluster.CompareTo(oldCluster) != 0)
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("ALTER TABLE ");
                    writer.Write(PgDiffUtils.GetQuotedName(newTable.Name));
                    writer.Write(" CLUSTER ON ");
                    writer.Write(PgDiffUtils.GetQuotedName(newCluster));
                    writer.WriteLine(';');
                }
            }
        }


        public static void AlterTables(TextWriter writer, PgDiffArguments arguments, 
            PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            foreach (var newTable in newSchema.GetTables())
            {
                if (oldSchema == null
                    || !oldSchema.ContainsTable(newTable.Name))
                    continue;

                var oldTable = oldSchema.GetTable(newTable.Name);
                UpdateTableColumns(writer, arguments, oldTable, newTable, searchPathHelper);
                CheckWithOids(writer, oldTable, newTable, searchPathHelper);
                CheckInherits(writer, oldTable, newTable, searchPathHelper);
                CheckTablespace(writer, oldTable, newTable, searchPathHelper);
                AddAlterStatistics(writer, oldTable, newTable, searchPathHelper);
                AddAlterStorage(writer, oldTable, newTable, searchPathHelper);
                AlterComments(writer, oldTable, newTable, searchPathHelper);
            }
        }


        private static void AddAlterStatistics(TextWriter writer, PgTable oldTable, PgTable newTable, SearchPathHelper searchPathHelper)
        {
            var stats = new Dictionary<string, int?>();

            foreach (var newColumn in newTable.Columns)
            {
                var oldColumn = oldTable.GetColumn(newColumn.Name);

                if (oldColumn != null)
                {
                    var oldStat = oldColumn.Statistics;
                    var newStat = newColumn.Statistics;
                    int? newStatValue = null;

                    if (newStat != null && (oldStat == null || !newStat.Equals(oldStat)))
                        newStatValue = newStat;
                    else if (oldStat != null && newStat == null)
                        newStatValue = -1;

                    if (newStatValue != null)
                        stats.Add(newColumn.Name, newStatValue);
                }
            }

            foreach (var entry in stats)
            {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("ALTER TABLE ONLY ");
                writer.Write(PgDiffUtils.GetQuotedName(newTable.Name));
                writer.Write(" ALTER COLUMN ");
                writer.Write(PgDiffUtils.GetQuotedName(entry.Key));
                writer.Write(" SET STATISTICS ");
                writer.Write(entry.Value);
                writer.WriteLine(';');
            }
        }


        private static void AddAlterStorage(TextWriter writer, PgTable oldTable, PgTable newTable, SearchPathHelper searchPathHelper)
        {
            foreach (var newColumn in newTable.Columns)
            {
                var oldColumn = oldTable.GetColumn(newColumn.Name);
                var oldStorage = string.IsNullOrEmpty(oldColumn?.Storage)
                    ? null
                    : oldColumn.Storage;
                var newStorage = string.IsNullOrEmpty(newColumn.Storage)
                    ? null
                    : newColumn.Storage;

                if (newStorage == null && oldStorage != null)
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(Resources.WarningUnableToDetermineStorageType, newTable.Name + '.' + newColumn.Name);

                    continue;
                }

                if (newStorage == null || newStorage.EqualsIgnoreCase(oldStorage))
                    continue;

                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("ALTER TABLE ONLY ");
                writer.Write(PgDiffUtils.GetQuotedName(newTable.Name));
                writer.Write(" ALTER COLUMN ");
                writer.Write(PgDiffUtils.GetQuotedName(newColumn.Name));
                writer.Write(" SET STORAGE ");
                writer.Write(newStorage);
                writer.Write(';');
            }
        }


        private static void AddCreateTableColumns(List<string> statements, PgDiffArguments arguments,
            PgTable oldTable, PgTable newTable, List<PgColumn> dropDefaultsColumns)
        {
            foreach (var column in newTable.Columns)
                if (!oldTable.ContainsColumn(column.Name))
                {
                    statements.Add("\tADD COLUMN " + column.GetFullDefinition(arguments.AddDefaults));

                    if (arguments.AddDefaults && !column.NullValue && string.IsNullOrEmpty(column.DefaultValue))
                        dropDefaultsColumns.Add(column);
                }
        }


        private static void AddDropTableColumns(List<string> statements, PgTable oldTable, PgTable newTable)
        {
            foreach (var column in oldTable.Columns)
                if (!newTable.ContainsColumn(column.Name))
                    statements.Add("\tDROP COLUMN " + PgDiffUtils.GetQuotedName(column.Name));
        }


        private static void AddModifyTableColumns(List<string> statements, PgDiffArguments arguments,
            PgTable oldTable, PgTable newTable, List<PgColumn> dropDefaultsColumns)
        {
            foreach (var newColumn in newTable.Columns)
            {
                if (!oldTable.ContainsColumn(newColumn.Name))
                    continue;

                var oldColumn = oldTable.GetColumn(newColumn.Name);
                var newColumnName = PgDiffUtils.GetQuotedName(newColumn.Name);

                if (!oldColumn.Type.Equals(newColumn.Type))
                    statements.Add("\tALTER COLUMN " + newColumnName + " TYPE "
                                   + newColumn.Type + " /* "
                                   + string.Format(Resources.TypeParameterChange,newTable.Name, oldColumn.Type,newColumn.Type) + " */");

                var oldDefault = oldColumn.DefaultValue ?? "";
                var newDefault = newColumn.DefaultValue ?? "";

                if (!oldDefault.Equals(newDefault))
                    statements.Add(newDefault.Length == 0
                        ? $"\tALTER COLUMN {newColumnName} DROP DEFAULT"
                        : $"\tALTER COLUMN {newColumnName} SET DEFAULT {newDefault}");

                if (oldColumn.NullValue == newColumn.NullValue)
                    continue;

                if (newColumn.NullValue)
                {
                    statements.Add($"\tALTER COLUMN {newColumnName} DROP NOT NULL");
                }
                else
                {
                    if (arguments.AddDefaults)
                    {
                        var defaultValue = PgColumnUtils.GetDefaultValue(newColumn.Type);
                        if (defaultValue != null)
                        {
                            statements.Add($"\tALTER COLUMN {newColumnName} SET DEFAULT {defaultValue}");
                            dropDefaultsColumns.Add(newColumn);
                        }
                    }
                    statements.Add($"\tALTER COLUMN {newColumnName} SET NOT NULL");
                }
            }
        }


        private static void CheckInherits(TextWriter writer, PgTable oldTable, PgTable newTable,SearchPathHelper searchPathHelper)
        {
            foreach (var tableName in oldTable.GetInherits())
                if (!newTable.GetInherits().Contains(tableName))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine("ALTER TABLE " + PgDiffUtils.GetQuotedName(newTable.Name));
                    writer.WriteLine("\tNO INHERIT " + PgDiffUtils.GetQuotedName(tableName) + ';');
                }

            foreach (var tableName in newTable.GetInherits())
                if (!oldTable.GetInherits().Contains(tableName))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine("ALTER TABLE " + PgDiffUtils.GetQuotedName(newTable.Name));
                    writer.WriteLine("\tINHERIT " + PgDiffUtils.GetQuotedName(tableName) + ';');
                }
        }


        private static void CheckWithOids(TextWriter writer, PgTable oldTable, PgTable newTable, SearchPathHelper searchPathHelper)
        {
            if (oldTable.With == null && newTable.With == null || oldTable.With != null && oldTable.With.Equals(newTable.With))
                return;

            searchPathHelper.OutputSearchPath(writer);
            writer.WriteLine();
            writer.WriteLine("ALTER TABLE " + PgDiffUtils.GetQuotedName(newTable.Name));

            if (newTable.With == null || "OIDS=false".EqualsIgnoreCase(newTable.With))
                writer.WriteLine("\tSET WITHOUT OIDS;");
            else if ("OIDS".EqualsIgnoreCase(newTable.With) || "OIDS=true".EqualsIgnoreCase(newTable.With))
                writer.WriteLine("\tSET WITH OIDS;");
            else
                writer.WriteLine("\tSET " + newTable.With + ";");
        }


        private static void CheckTablespace(TextWriter writer, PgTable oldTable, PgTable newTable, SearchPathHelper searchPathHelper)
        {
            if (oldTable.Tablespace == null && newTable.Tablespace == null
                || oldTable.Tablespace != null
                && oldTable.Tablespace.Equals(newTable.Tablespace))
                return;

            searchPathHelper.OutputSearchPath(writer);
            writer.WriteLine();
            writer.WriteLine("ALTER TABLE " + PgDiffUtils.GetQuotedName(newTable.Name));
            writer.WriteLine("\tTABLESPACE " + newTable.Tablespace + ';');
        }


        public static void CreateTables(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            foreach (var table in newSchema.GetTables())
                if (oldSchema == null
                    || !oldSchema.ContainsTable(table.Name))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(table.GetCreationSql());
                }
        }


        public static void DropTables(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            if (oldSchema == null)
                return;

            foreach (var table in oldSchema.GetTables())
                if (!newSchema.ContainsTable(table.Name))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(table.GetDropSql());
                }
        }


        private static void UpdateTableColumns(TextWriter writer, PgDiffArguments arguments, 
            PgTable oldTable, PgTable newTable, SearchPathHelper searchPathHelper)
        {
            var statements = new List<string>();

            var dropDefaultsColumns = new List<PgColumn>();
            AddDropTableColumns(statements, oldTable, newTable);
            AddCreateTableColumns(
                statements, arguments, oldTable, newTable, dropDefaultsColumns);
            AddModifyTableColumns(
                statements, arguments, oldTable, newTable, dropDefaultsColumns);

            if (statements.Count > 0)
            {
                var quotedTableName =
                    PgDiffUtils.GetQuotedName(newTable.Name);
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine("ALTER TABLE " + quotedTableName);

                for (var i = 0; i < statements.Count; i++)
                {
                    writer.Write(statements[i]);
                    writer.WriteLine(i + 1 < statements.Count ? "," : ";");
                }

                if (dropDefaultsColumns.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("ALTER TABLE " + quotedTableName);

                    for (var i = 0; i < dropDefaultsColumns.Count; i++)
                    {
                        writer.Write("\tALTER COLUMN ");
                        writer.Write(PgDiffUtils.GetQuotedName(dropDefaultsColumns[i].Name));
                        writer.Write(" DROP DEFAULT");
                        writer.WriteLine(i + 1 < dropDefaultsColumns.Count ? "," : ";");
                    }
                }
            }
        }


        private static void AlterComments(TextWriter writer, PgTable oldTable, PgTable newTable, SearchPathHelper searchPathHelper)
        {
            if (oldTable.Comment == null
                && newTable.Comment != null
                || oldTable.Comment != null
                && newTable.Comment != null
                && !oldTable.Comment.Equals(newTable.Comment))
            {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON TABLE ");
                writer.Write(PgDiffUtils.GetQuotedName(newTable.Name));
                writer.Write(" IS ");
                writer.Write(newTable.Comment);
                writer.WriteLine(';');
            }
            else if (oldTable.Comment != null && newTable.Comment == null)
            {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON TABLE ");
                writer.Write(PgDiffUtils.GetQuotedName(newTable.Name));
                writer.WriteLine(" IS NULL;");
            }

            foreach (var newColumn in newTable.Columns)
            {
                var oldColumn = oldTable.GetColumn(newColumn.Name);
                var oldComment = oldColumn?.Comment;
                var newComment = newColumn.Comment;

                if (newComment != null && (!oldComment?.Equals(newComment) ?? newComment != null))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON COLUMN ");
                    writer.Write(PgDiffUtils.GetQuotedName(newTable.Name));
                    writer.Write('.');
                    writer.Write(PgDiffUtils.GetQuotedName(newColumn.Name));
                    writer.Write(" IS ");
                    writer.Write(newColumn.Comment);
                    writer.WriteLine(';');
                }
                else if (oldComment != null && newComment == null)
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON COLUMN ");
                    writer.Write(PgDiffUtils.GetQuotedName(newTable.Name));
                    writer.Write('.');
                    writer.Write(PgDiffUtils.GetQuotedName(newColumn.Name));
                    writer.WriteLine(" IS NULL;");
                }
            }
        }
    }
}