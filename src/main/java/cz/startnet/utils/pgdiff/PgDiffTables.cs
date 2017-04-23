
namespace cz.startnet.utils.pgdiff {









public class PgDiffTables {

    
    public static void dropClusters(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        for (PgTable newTable : newSchema.getTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            String oldCluster;

            if (oldTable == null) {
                oldCluster = null;
            } else {
                oldCluster = oldTable.getClusterIndexName();
            }

            String newCluster = newTable.getClusterIndexName();

            if (oldCluster != null && newCluster == null
                    && newTable.containsIndex(oldCluster)) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("ALTER TABLE ");
                writer.Write(PgDiffUtils.getQuotedName(newTable.getName()));
                writer.WriteLine(" SET WITHOUT CLUSTER;");
            }
        }
    }

    
    public static void createClusters(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        for (PgTable newTable : newSchema.getTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            String oldCluster;

            if (oldTable == null) {
                oldCluster = null;
            } else {
                oldCluster = oldTable.getClusterIndexName();
            }

            String newCluster = newTable.getClusterIndexName();

            if ((oldCluster == null && newCluster != null)
                    || (oldCluster != null && newCluster != null
                    && newCluster.compareTo(oldCluster) != 0)) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("ALTER TABLE ");
                writer.Write(PgDiffUtils.getQuotedName(newTable.getName()));
                writer.Write(" CLUSTER ON ");
                writer.Write(PgDiffUtils.getQuotedName(newCluster));
                writer.WriteLine(';');
            }
        }
    }

    
    public static void alterTables(TextWriter writer,
            PgDiffArguments arguments, PgSchema oldSchema,
            PgSchema newSchema, SearchPathHelper searchPathHelper) {
        for (PgTable newTable : newSchema.getTables()) {
            if (oldSchema == null
                    || !oldSchema.containsTable(newTable.getName())) {
                continue;
            }

            PgTable oldTable = oldSchema.getTable(newTable.getName());
            updateTableColumns(
                    writer, arguments, oldTable, newTable, searchPathHelper);
            checkWithOIDS(writer, oldTable, newTable, searchPathHelper);
            checkInherits(writer, oldTable, newTable, searchPathHelper);
            checkTablespace(writer, oldTable, newTable, searchPathHelper);
            addAlterStatistics(writer, oldTable, newTable, searchPathHelper);
            addAlterStorage(writer, oldTable, newTable, searchPathHelper);
            alterComments(writer, oldTable, newTable, searchPathHelper);
        }
    }

    
    private static void addAlterStatistics(TextWriter writer,
            PgTable oldTable, PgTable newTable,
            SearchPathHelper searchPathHelper) {
        
        Map<String, Integer> stats = new HashMap<String, Integer>();

        for (PgColumn newColumn : newTable.getColumns()) {
            PgColumn oldColumn = oldTable.getColumn(newColumn.getName());

            if (oldColumn != null) {
                Integer oldStat = oldColumn.getStatistics();
                Integer newStat = newColumn.getStatistics();
                Integer newStatValue = null;

                if (newStat != null && (oldStat == null
                        || !newStat.Equals(oldStat))) {
                    newStatValue = newStat;
                } else if (oldStat != null && newStat == null) {
                    newStatValue = Integer.valueOf(-1);
                }

                if (newStatValue != null) {
                    stats.put(newColumn.getName(), newStatValue);
                }
            }
        }

        for (Map.Entry<String, Integer> entry : stats.entrySet()) {
            searchPathHelper.outputSearchPath(writer);
            writer.WriteLine();
            writer.Write("ALTER TABLE ONLY ");
            writer.Write(PgDiffUtils.getQuotedName(newTable.getName()));
            writer.Write(" ALTER COLUMN ");
            writer.Write(PgDiffUtils.getQuotedName(entry.getKey()));
            writer.Write(" SET STATISTICS ");
            writer.Write(entry.getValue());
            writer.WriteLine(';');
        }
    }

    
    private static void addAlterStorage(TextWriter writer,
            PgTable oldTable, PgTable newTable,
            SearchPathHelper searchPathHelper) {
        for (PgColumn newColumn : newTable.getColumns()) {
            PgColumn oldColumn = oldTable.getColumn(newColumn.getName());
            String oldStorage = (oldColumn == null
                    || oldColumn.getStorage() == null
                    || oldColumn.getStorage().isEmpty()) ? null
                    : oldColumn.getStorage();
            String newStorage = (newColumn.getStorage() == null
                    || newColumn.getStorage().isEmpty()) ? null
                    : newColumn.getStorage();

            if (newStorage == null && oldStorage != null) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(String.Format(Resources.getString(
                        "WarningUnableToDetermineStorageType"),
                        newTable.getName() + '.' + newColumn.getName()));

                continue;
            }

            if (newStorage == null || newStorage.equalsIgnoreCase(oldStorage)) {
                continue;
            }

            searchPathHelper.outputSearchPath(writer);
            writer.WriteLine();
            writer.Write("ALTER TABLE ONLY ");
            writer.Write(PgDiffUtils.getQuotedName(newTable.getName()));
            writer.Write(" ALTER COLUMN ");
            writer.Write(PgDiffUtils.getQuotedName(newColumn.getName()));
            writer.Write(" SET STORAGE ");
            writer.Write(newStorage);
            writer.Write(';');
        }
    }

    
    private static void addCreateTableColumns(List<String> statements,
            PgDiffArguments arguments, PgTable oldTable,
            PgTable newTable, List<PgColumn> dropDefaultsColumns) {
        for (PgColumn column : newTable.getColumns()) {
            if (!oldTable.containsColumn(column.getName())) {
                statements.add("\tADD COLUMN "
                        + column.getFullDefinition(arguments.isAddDefaults()));

                if (arguments.isAddDefaults() && !column.getNullValue()
                        && (column.getDefaultValue() == null
                        || column.getDefaultValue().isEmpty())) {
                    dropDefaultsColumns.add(column);
                }
            }
        }
    }

    
    private static void addDropTableColumns(List<String> statements,
            PgTable oldTable, PgTable newTable) {
        for (PgColumn column : oldTable.getColumns()) {
            if (!newTable.containsColumn(column.getName())) {
                statements.add("\tDROP COLUMN "
                        + PgDiffUtils.getQuotedName(column.getName()));
            }
        }
    }

    
    private static void addModifyTableColumns(List<String> statements,
            PgDiffArguments arguments, PgTable oldTable,
            PgTable newTable, List<PgColumn> dropDefaultsColumns) {
        for (PgColumn newColumn : newTable.getColumns()) {
            if (!oldTable.containsColumn(newColumn.getName())) {
                continue;
            }

            PgColumn oldColumn = oldTable.getColumn(newColumn.getName());
            String newColumnName =
                    PgDiffUtils.getQuotedName(newColumn.getName());

            if (!oldColumn.getType().Equals(newColumn.getType())) {
                statements.add("\tALTER COLUMN " + newColumnName + " TYPE "
                        + newColumn.getType() + " /* "
                        + String.Format(
                        Resources.getString("TypeParameterChange"),
                        newTable.getName(), oldColumn.getType(),
                        newColumn.getType()) + " */");
            }

            String oldDefault = (oldColumn.getDefaultValue() == null) ? ""
                    : oldColumn.getDefaultValue();
            String newDefault = (newColumn.getDefaultValue() == null) ? ""
                    : newColumn.getDefaultValue();

            if (!oldDefault.Equals(newDefault)) {
                if (newDefault.length() == 0) {
                    statements.add("\tALTER COLUMN " + newColumnName
                            + " DROP DEFAULT");
                } else {
                    statements.add("\tALTER COLUMN " + newColumnName
                            + " SET DEFAULT " + newDefault);
                }
            }

            if (oldColumn.getNullValue() != newColumn.getNullValue()) {
                if (newColumn.getNullValue()) {
                    statements.add("\tALTER COLUMN " + newColumnName
                            + " DROP NOT NULL");
                } else {
                    if (arguments.isAddDefaults()) {
                        String defaultValue =
                                PgColumnUtils.getDefaultValue(
                                newColumn.getType());

                        if (defaultValue != null) {
                            statements.add("\tALTER COLUMN " + newColumnName
                                    + " SET DEFAULT " + defaultValue);
                            dropDefaultsColumns.add(newColumn);
                        }
                    }

                    statements.add("\tALTER COLUMN " + newColumnName
                            + " SET NOT NULL");
                }
            }
        }
    }

    
    private static void checkInherits(TextWriter writer,
            PgTable oldTable, PgTable newTable,
            SearchPathHelper searchPathHelper) {
        for (String tableName : oldTable.getInherits()) {
            if (!newTable.getInherits().contains(tableName)) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine("ALTER TABLE "
                        + PgDiffUtils.getQuotedName(newTable.getName()));
                writer.WriteLine("\tNO INHERIT "
                        + PgDiffUtils.getQuotedName(tableName) + ';');
            }
        }

        for (String tableName : newTable.getInherits()) {
            if (!oldTable.getInherits().contains(tableName)) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine("ALTER TABLE "
                        + PgDiffUtils.getQuotedName(newTable.getName()));
                writer.WriteLine("\tINHERIT "
                        + PgDiffUtils.getQuotedName(tableName) + ';');
            }
        }
    }

    
    private static void checkWithOIDS(TextWriter writer,
            PgTable oldTable, PgTable newTable,
            SearchPathHelper searchPathHelper) {
        if (oldTable.getWith() == null && newTable.getWith() == null
                || oldTable.getWith() != null
                && oldTable.getWith().Equals(newTable.getWith())) {
            return;
        }

        searchPathHelper.outputSearchPath(writer);
        writer.WriteLine();
        writer.WriteLine("ALTER TABLE "
                + PgDiffUtils.getQuotedName(newTable.getName()));

        if (newTable.getWith() == null
                || "OIDS=false".equalsIgnoreCase(newTable.getWith())) {
            writer.WriteLine("\tSET WITHOUT OIDS;");
        } else if ("OIDS".equalsIgnoreCase(newTable.getWith())
                || "OIDS=true".equalsIgnoreCase(newTable.getWith())) {
            writer.WriteLine("\tSET WITH OIDS;");
        } else {
            writer.WriteLine("\tSET " + newTable.getWith() + ";");
        }
    }

    
    private static void checkTablespace(TextWriter writer,
            PgTable oldTable, PgTable newTable,
            SearchPathHelper searchPathHelper) {
        if (oldTable.getTablespace() == null && newTable.getTablespace() == null
                || oldTable.getTablespace() != null
                && oldTable.getTablespace().Equals(newTable.getTablespace())) {
            return;
        }

        searchPathHelper.outputSearchPath(writer);
        writer.WriteLine();
        writer.WriteLine("ALTER TABLE "
                + PgDiffUtils.getQuotedName(newTable.getName()));
        writer.WriteLine("\tTABLESPACE " + newTable.getTablespace() + ';');
    }

    
    public static void createTables(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        for (PgTable table : newSchema.getTables()) {
            if (oldSchema == null
                    || !oldSchema.containsTable(table.getName())) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(table.getCreationSQL());
            }
        }
    }

    
    public static void dropTables(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        for (PgTable table : oldSchema.getTables()) {
            if (!newSchema.containsTable(table.getName())) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(table.getDropSQL());
            }
        }
    }

    
    private static void updateTableColumns(TextWriter writer,
            PgDiffArguments arguments, PgTable oldTable,
            PgTable newTable, SearchPathHelper searchPathHelper) {
        
        List<String> statements = new ArrayList<String>();
        
        List<PgColumn> dropDefaultsColumns = new ArrayList<PgColumn>();
        addDropTableColumns(statements, oldTable, newTable);
        addCreateTableColumns(
                statements, arguments, oldTable, newTable, dropDefaultsColumns);
        addModifyTableColumns(
                statements, arguments, oldTable, newTable, dropDefaultsColumns);

        if (!statements.isEmpty()) {
            String quotedTableName =
                    PgDiffUtils.getQuotedName(newTable.getName());
            searchPathHelper.outputSearchPath(writer);
            writer.WriteLine();
            writer.WriteLine("ALTER TABLE " + quotedTableName);

            for (int i = 0; i < statements.size(); i++) {
                writer.Write(statements.get(i));
                writer.WriteLine((i + 1) < statements.size() ? "," : ";");
            }

            if (!dropDefaultsColumns.isEmpty()) {
                writer.WriteLine();
                writer.WriteLine("ALTER TABLE " + quotedTableName);

                for (int i = 0; i < dropDefaultsColumns.size(); i++) {
                    writer.Write("\tALTER COLUMN ");
                    writer.Write(PgDiffUtils.getQuotedName(
                            dropDefaultsColumns.get(i).getName()));
                    writer.Write(" DROP DEFAULT");
                    writer.WriteLine(
                            (i + 1) < dropDefaultsColumns.size() ? "," : ";");
                }
            }
        }
    }

    
    private static void alterComments(TextWriter writer,
            PgTable oldTable, PgTable newTable,
            SearchPathHelper searchPathHelper) {
        if (oldTable.getComment() == null
                && newTable.getComment() != null
                || oldTable.getComment() != null
                && newTable.getComment() != null
                && !oldTable.getComment().Equals(newTable.getComment())) {
            searchPathHelper.outputSearchPath(writer);
            writer.WriteLine();
            writer.Write("COMMENT ON TABLE ");
            writer.Write(PgDiffUtils.getQuotedName(newTable.getName()));
            writer.Write(" IS ");
            writer.Write(newTable.getComment());
            writer.WriteLine(';');
        } else if (oldTable.getComment() != null
                && newTable.getComment() == null) {
            searchPathHelper.outputSearchPath(writer);
            writer.WriteLine();
            writer.Write("COMMENT ON TABLE ");
            writer.Write(PgDiffUtils.getQuotedName(newTable.getName()));
            writer.WriteLine(" IS NULL;");
        }

        for (PgColumn newColumn : newTable.getColumns()) {
            PgColumn oldColumn = oldTable.getColumn(newColumn.getName());
            String oldComment =
                    oldColumn == null ? null : oldColumn.getComment();
            String newComment = newColumn.getComment();

            if (newComment != null && (oldComment == null ? newComment != null
                    : !oldComment.Equals(newComment))) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON COLUMN ");
                writer.Write(PgDiffUtils.getQuotedName(newTable.getName()));
                writer.Write('.');
                writer.Write(PgDiffUtils.getQuotedName(newColumn.getName()));
                writer.Write(" IS ");
                writer.Write(newColumn.getComment());
                writer.WriteLine(';');
            } else if (oldComment != null && newComment == null) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON COLUMN ");
                writer.Write(PgDiffUtils.getQuotedName(newTable.getName()));
                writer.Write('.');
                writer.Write(PgDiffUtils.getQuotedName(newColumn.getName()));
                writer.WriteLine(" IS NULL;");
            }
        }
    }

    
    private PgDiffTables() {
    }
}
}