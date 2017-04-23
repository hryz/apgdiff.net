
using System;
using cz.startnet.utils.pgdiff.loader;
using cz.startnet.utils.pgdiff.schema;

namespace cz.startnet.utils.pgdiff {





public class PgDiff {

    
    public static void createDiff(TextWriter writer,
            PgDiffArguments arguments) {
        PgDatabase oldDatabase = PgDumpLoader.loadDatabaseSchema(
                arguments.getOldDumpFile(), arguments.getInCharsetName(),
                arguments.isOutputIgnoredStatements(),
                arguments.isIgnoreSlonyTriggers());
        PgDatabase newDatabase = PgDumpLoader.loadDatabaseSchema(
                arguments.getNewDumpFile(), arguments.getInCharsetName(),
                arguments.isOutputIgnoredStatements(),
                arguments.isIgnoreSlonyTriggers());

        diffDatabaseSchemas(writer, arguments, oldDatabase, newDatabase);
    }

    
    public static void createDiff(TextWriter writer,
            PgDiffArguments arguments, InputStream oldInputStream,
            InputStream newInputStream) {
        PgDatabase oldDatabase = PgDumpLoader.loadDatabaseSchema(
                oldInputStream, arguments.getInCharsetName(),
                arguments.isOutputIgnoredStatements(),
                arguments.isIgnoreSlonyTriggers());
        PgDatabase newDatabase = PgDumpLoader.loadDatabaseSchema(
                newInputStream, arguments.getInCharsetName(),
                arguments.isOutputIgnoredStatements(),
                arguments.isIgnoreSlonyTriggers());

        diffDatabaseSchemas(writer, arguments, oldDatabase, newDatabase);
    }

    
    private static void createNewSchemas(TextWriter writer,
            PgDatabase oldDatabase, PgDatabase newDatabase) {
        for (PgSchema newSchema : newDatabase.getSchemas()) {
            if (oldDatabase.getSchema(newSchema.getName()) == null) {
                writer.WriteLine();
                writer.WriteLine(newSchema.getCreationSQL());
            }
        }
    }

    
    private static void diffDatabaseSchemas(TextWriter writer,
            PgDiffArguments arguments, PgDatabase oldDatabase,
            PgDatabase newDatabase) {
        if (arguments.isAddTransaction()) {
            writer.WriteLine("START TRANSACTION;");
        }

        if (oldDatabase.getComment() == null
                && newDatabase.getComment() != null
                || oldDatabase.getComment() != null
                && newDatabase.getComment() != null
                && !oldDatabase.getComment().Equals(newDatabase.getComment())) {
            writer.WriteLine();
            writer.Write("COMMENT ON DATABASE current_database() IS ");
            writer.Write(newDatabase.getComment());
            writer.WriteLine(';');
        } else if (oldDatabase.getComment() != null
                && newDatabase.getComment() == null) {
            writer.WriteLine();
            writer.WriteLine("COMMENT ON DATABASE current_database() IS NULL;");
        }

        dropOldSchemas(writer, oldDatabase, newDatabase);
        createNewSchemas(writer, oldDatabase, newDatabase);
        updateSchemas(writer, arguments, oldDatabase, newDatabase);

        if (arguments.isAddTransaction()) {
            writer.WriteLine();
            writer.WriteLine("COMMIT TRANSACTION;");
        }

        if (arguments.isOutputIgnoredStatements()) {
            if (!oldDatabase.getIgnoredStatements().isEmpty()) {
                writer.WriteLine();
                writer.Write("/* ");
                writer.WriteLine(Resources.getString(
                        "OriginalDatabaseIgnoredStatements"));

                for (String statement :
                        oldDatabase.getIgnoredStatements()) {
                    writer.WriteLine();
                    writer.WriteLine(statement);
                }

                writer.WriteLine("*/");
            }

            if (!newDatabase.getIgnoredStatements().isEmpty()) {
                writer.WriteLine();
                writer.Write("/* ");
                writer.WriteLine(
                        Resources.getString("NewDatabaseIgnoredStatements"));

                for (String statement :
                        newDatabase.getIgnoredStatements()) {
                    writer.WriteLine();
                    writer.WriteLine(statement);
                }

                writer.WriteLine("*/");
            }
        }
    }

    
    private static void dropOldSchemas(TextWriter writer,
            PgDatabase oldDatabase, PgDatabase newDatabase) {
        for (PgSchema oldSchema : oldDatabase.getSchemas()) {
            if (newDatabase.getSchema(oldSchema.getName()) == null) {
                writer.WriteLine();
                writer.WriteLine("DROP SCHEMA "
                        + PgDiffUtils.getQuotedName(oldSchema.getName())
                        + " CASCADE;");
            }
        }
    }

    
    private static void updateSchemas(TextWriter writer,
            PgDiffArguments arguments, PgDatabase oldDatabase,
            PgDatabase newDatabase) {
        bool setSearchPath = newDatabase.getSchemas().size() > 1
                || !newDatabase.getSchemas().get(0).getName().Equals("public");

        for (PgSchema newSchema : newDatabase.getSchemas()) {
            SearchPathHelper searchPathHelper;

            if (setSearchPath) {
                searchPathHelper = new SearchPathHelper("SET search_path = "
                        + PgDiffUtils.getQuotedName(newSchema.getName(), true)
                        + ", pg_catalog;");
            } else {
                searchPathHelper = new SearchPathHelper(null);
            }

            PgSchema oldSchema =
                    oldDatabase.getSchema(newSchema.getName());

            if (oldSchema != null) {
                if (oldSchema.getComment() == null
                        && newSchema.getComment() != null
                        || oldSchema.getComment() != null
                        && newSchema.getComment() != null
                        && !oldSchema.getComment().Equals(
                        newSchema.getComment())) {
                    writer.WriteLine();
                    writer.Write("COMMENT ON SCHEMA ");
                    writer.Write(
                            PgDiffUtils.getQuotedName(newSchema.getName()));
                    writer.Write(" IS ");
                    writer.Write(newSchema.getComment());
                    writer.WriteLine(';');
                } else if (oldSchema.getComment() != null
                        && newSchema.getComment() == null) {
                    writer.WriteLine();
                    writer.Write("COMMENT ON SCHEMA ");
                    writer.Write(
                            PgDiffUtils.getQuotedName(newSchema.getName()));
                    writer.WriteLine(" IS NULL;");
                }
            }

            PgDiffTriggers.dropTriggers(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffFunctions.dropFunctions(
                    writer, arguments, oldSchema, newSchema, searchPathHelper);
            PgDiffViews.dropViews(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffConstraints.dropConstraints(
                    writer, oldSchema, newSchema, true, searchPathHelper);
            PgDiffConstraints.dropConstraints(
                    writer, oldSchema, newSchema, false, searchPathHelper);
            PgDiffIndexes.dropIndexes(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffTables.dropClusters(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffTables.dropTables(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffSequences.dropSequences(
                    writer, oldSchema, newSchema, searchPathHelper);

            PgDiffSequences.createSequences(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffSequences.alterSequences(
                    writer, arguments, oldSchema, newSchema, searchPathHelper);
            PgDiffTables.createTables(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffTables.alterTables(
                    writer, arguments, oldSchema, newSchema, searchPathHelper);
            PgDiffSequences.alterCreatedSequences(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffFunctions.createFunctions(
                    writer, arguments, oldSchema, newSchema, searchPathHelper);
            PgDiffConstraints.createConstraints(
                    writer, oldSchema, newSchema, true, searchPathHelper);
            PgDiffConstraints.createConstraints(
                    writer, oldSchema, newSchema, false, searchPathHelper);
            PgDiffIndexes.createIndexes(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffTables.createClusters(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffTriggers.createTriggers(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffViews.createViews(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffViews.alterViews(
                    writer, oldSchema, newSchema, searchPathHelper);

            PgDiffFunctions.alterComments(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffConstraints.alterComments(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffIndexes.alterComments(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffTriggers.alterComments(
                    writer, oldSchema, newSchema, searchPathHelper);
        }
    }

    
    private PgDiff() {
    }
}
}