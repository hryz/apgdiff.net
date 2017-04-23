
package cz.startnet.utils.pgdiff;

import cz.startnet.utils.pgdiff.loader.PgDumpLoader;
import cz.startnet.utils.pgdiff.schema.PgDatabase;
import cz.startnet.utils.pgdiff.schema.PgSchema;
import java.io.InputStream;
import java.io.PrintWriter;


public class PgDiff {

    
    public static void createDiff(PrintWriter writer,
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

    
    public static void createDiff(PrintWriter writer,
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

    
    private static void createNewSchemas(PrintWriter writer,
            PgDatabase oldDatabase, PgDatabase newDatabase) {
        for (PgSchema newSchema : newDatabase.getSchemas()) {
            if (oldDatabase.getSchema(newSchema.getName()) == null) {
                writer.println();
                writer.println(newSchema.getCreationSQL());
            }
        }
    }

    
    private static void diffDatabaseSchemas(PrintWriter writer,
            PgDiffArguments arguments, PgDatabase oldDatabase,
            PgDatabase newDatabase) {
        if (arguments.isAddTransaction()) {
            writer.println("START TRANSACTION;");
        }

        if (oldDatabase.getComment() == null
                && newDatabase.getComment() != null
                || oldDatabase.getComment() != null
                && newDatabase.getComment() != null
                && !oldDatabase.getComment().equals(newDatabase.getComment())) {
            writer.println();
            writer.print("COMMENT ON DATABASE current_database() IS ");
            writer.print(newDatabase.getComment());
            writer.println(';');
        } else if (oldDatabase.getComment() != null
                && newDatabase.getComment() == null) {
            writer.println();
            writer.println("COMMENT ON DATABASE current_database() IS NULL;");
        }

        dropOldSchemas(writer, oldDatabase, newDatabase);
        createNewSchemas(writer, oldDatabase, newDatabase);
        updateSchemas(writer, arguments, oldDatabase, newDatabase);

        if (arguments.isAddTransaction()) {
            writer.println();
            writer.println("COMMIT TRANSACTION;");
        }

        if (arguments.isOutputIgnoredStatements()) {
            if (!oldDatabase.getIgnoredStatements().isEmpty()) {
                writer.println();
                writer.print("/* ");
                writer.println(Resources.getString(
                        "OriginalDatabaseIgnoredStatements"));

                for (String statement :
                        oldDatabase.getIgnoredStatements()) {
                    writer.println();
                    writer.println(statement);
                }

                writer.println("*/");
            }

            if (!newDatabase.getIgnoredStatements().isEmpty()) {
                writer.println();
                writer.print("/* ");
                writer.println(
                        Resources.getString("NewDatabaseIgnoredStatements"));

                for (String statement :
                        newDatabase.getIgnoredStatements()) {
                    writer.println();
                    writer.println(statement);
                }

                writer.println("*/");
            }
        }
    }

    
    private static void dropOldSchemas(PrintWriter writer,
            PgDatabase oldDatabase, PgDatabase newDatabase) {
        for (PgSchema oldSchema : oldDatabase.getSchemas()) {
            if (newDatabase.getSchema(oldSchema.getName()) == null) {
                writer.println();
                writer.println("DROP SCHEMA "
                        + PgDiffUtils.getQuotedName(oldSchema.getName())
                        + " CASCADE;");
            }
        }
    }

    
    private static void updateSchemas(PrintWriter writer,
            PgDiffArguments arguments, PgDatabase oldDatabase,
            PgDatabase newDatabase) {
        boolean setSearchPath = newDatabase.getSchemas().size() > 1
                || !newDatabase.getSchemas().get(0).getName().equals("public");

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
                        && !oldSchema.getComment().equals(
                        newSchema.getComment())) {
                    writer.println();
                    writer.print("COMMENT ON SCHEMA ");
                    writer.print(
                            PgDiffUtils.getQuotedName(newSchema.getName()));
                    writer.print(" IS ");
                    writer.print(newSchema.getComment());
                    writer.println(';');
                } else if (oldSchema.getComment() != null
                        && newSchema.getComment() == null) {
                    writer.println();
                    writer.print("COMMENT ON SCHEMA ");
                    writer.print(
                            PgDiffUtils.getQuotedName(newSchema.getName()));
                    writer.println(" IS NULL;");
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
