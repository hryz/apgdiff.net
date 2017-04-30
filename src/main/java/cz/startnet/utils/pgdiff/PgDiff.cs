using System;
using System.IO;
using pgdiff.loader;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff {





public class PgDiff {

    
    public static void CreateDiff(TextWriter writer,
            PgDiffArguments arguments) {
        PgDatabase oldDatabase = PgDumpLoader.LoadDatabaseSchema(
                arguments.GetOldDumpFile(), arguments.GetInCharsetName(),
                arguments.IsOutputIgnoredStatements(),
                arguments.IsIgnoreSlonyTriggers());
        PgDatabase newDatabase = PgDumpLoader.LoadDatabaseSchema(
                arguments.GetNewDumpFile(), arguments.GetInCharsetName(),
                arguments.IsOutputIgnoredStatements(),
                arguments.IsIgnoreSlonyTriggers());

        DiffDatabaseSchemas(writer, arguments, oldDatabase, newDatabase);
    }

    
    public static void CreateDiff(TextWriter writer, PgDiffArguments arguments, String oldInputStream, String newInputStream) {
        PgDatabase oldDatabase = PgDumpLoader.LoadDatabaseSchema(
                oldInputStream, arguments.GetInCharsetName(),
                arguments.IsOutputIgnoredStatements(),
                arguments.IsIgnoreSlonyTriggers());
        PgDatabase newDatabase = PgDumpLoader.LoadDatabaseSchema(
                newInputStream, arguments.GetInCharsetName(),
                arguments.IsOutputIgnoredStatements(),
                arguments.IsIgnoreSlonyTriggers());

        DiffDatabaseSchemas(writer, arguments, oldDatabase, newDatabase);
    }

    
    private static void CreateNewSchemas(TextWriter writer,
            PgDatabase oldDatabase, PgDatabase newDatabase) {
        foreach (PgSchema newSchema in newDatabase.GetSchemas()) {
            if (oldDatabase.GetSchema(newSchema.GetName()) == null) {
                writer.WriteLine();
                writer.WriteLine(newSchema.GetCreationSql());
            }
        }
    }

    
    private static void DiffDatabaseSchemas(TextWriter writer,
            PgDiffArguments arguments, PgDatabase oldDatabase,
            PgDatabase newDatabase) {
        if (arguments.IsAddTransaction()) {
            writer.WriteLine("START TRANSACTION;");
        }

        if (oldDatabase.GetComment() == null
                && newDatabase.GetComment() != null
                || oldDatabase.GetComment() != null
                && newDatabase.GetComment() != null
                && !oldDatabase.GetComment().Equals(newDatabase.GetComment())) {
            writer.WriteLine();
            writer.Write("COMMENT ON DATABASE current_database() IS ");
            writer.Write(newDatabase.GetComment());
            writer.WriteLine(';');
        } else if (oldDatabase.GetComment() != null
                && newDatabase.GetComment() == null) {
            writer.WriteLine();
            writer.WriteLine("COMMENT ON DATABASE current_database() IS NULL;");
        }

        DropOldSchemas(writer, oldDatabase, newDatabase);
        CreateNewSchemas(writer, oldDatabase, newDatabase);
        UpdateSchemas(writer, arguments, oldDatabase, newDatabase);

        if (arguments.IsAddTransaction()) {
            writer.WriteLine();
            writer.WriteLine("COMMIT TRANSACTION;");
        }

        if (arguments.IsOutputIgnoredStatements()) {
            if (oldDatabase.GetIgnoredStatements().Count > 0) {
                writer.WriteLine();
                writer.Write("/* ");
                writer.WriteLine(Resources.OriginalDatabaseIgnoredStatements);

                foreach (String statement in oldDatabase.GetIgnoredStatements()) {
                    writer.WriteLine();
                    writer.WriteLine(statement);
                }

                writer.WriteLine("*/");
            }

            if (newDatabase.GetIgnoredStatements().Count > 0) {
                writer.WriteLine();
                writer.Write("/* ");
                writer.WriteLine(Resources.NewDatabaseIgnoredStatements);

                foreach (String statement in newDatabase.GetIgnoredStatements()) {
                    writer.WriteLine();
                    writer.WriteLine(statement);
                }

                writer.WriteLine("*/");
            }
        }
    }

    
    private static void DropOldSchemas(TextWriter writer,
            PgDatabase oldDatabase, PgDatabase newDatabase) {
        foreach (PgSchema oldSchema in oldDatabase.GetSchemas()) {
            if (newDatabase.GetSchema(oldSchema.GetName()) == null) {
                writer.WriteLine();
                writer.WriteLine("DROP SCHEMA "
                        + PgDiffUtils.GetQuotedName(oldSchema.GetName())
                        + " CASCADE;");
            }
        }
    }

    
    private static void UpdateSchemas(TextWriter writer,
            PgDiffArguments arguments, PgDatabase oldDatabase,
            PgDatabase newDatabase) {
        bool setSearchPath = newDatabase.GetSchemas().Count > 1
                || !newDatabase.GetSchemas()[0].GetName().Equals("public");

        foreach (PgSchema newSchema in newDatabase.GetSchemas()) {
            SearchPathHelper searchPathHelper;

            if (setSearchPath) {
                searchPathHelper = new SearchPathHelper("SET search_path = "
                        + PgDiffUtils.GetQuotedName(newSchema.GetName(), true)
                        + ", pg_catalog;");
            } else {
                searchPathHelper = new SearchPathHelper(null);
            }

            PgSchema oldSchema =
                    oldDatabase.GetSchema(newSchema.GetName());

            if (oldSchema != null) {
                if (oldSchema.GetComment() == null
                        && newSchema.GetComment() != null
                        || oldSchema.GetComment() != null
                        && newSchema.GetComment() != null
                        && !oldSchema.GetComment().Equals(
                        newSchema.GetComment())) {
                    writer.WriteLine();
                    writer.Write("COMMENT ON SCHEMA ");
                    writer.Write(
                            PgDiffUtils.GetQuotedName(newSchema.GetName()));
                    writer.Write(" IS ");
                    writer.Write(newSchema.GetComment());
                    writer.WriteLine(';');
                } else if (oldSchema.GetComment() != null
                        && newSchema.GetComment() == null) {
                    writer.WriteLine();
                    writer.Write("COMMENT ON SCHEMA ");
                    writer.Write(
                            PgDiffUtils.GetQuotedName(newSchema.GetName()));
                    writer.WriteLine(" IS NULL;");
                }
            }

            PgDiffTriggers.DropTriggers(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffFunctions.DropFunctions(
                    writer, arguments, oldSchema, newSchema, searchPathHelper);
            PgDiffViews.DropViews(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffConstraints.DropConstraints(
                    writer, oldSchema, newSchema, true, searchPathHelper);
            PgDiffConstraints.DropConstraints(
                    writer, oldSchema, newSchema, false, searchPathHelper);
            PgDiffIndexes.DropIndexes(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffTables.DropClusters(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffTables.DropTables(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffSequences.DropSequences(
                    writer, oldSchema, newSchema, searchPathHelper);

            PgDiffSequences.CreateSequences(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffSequences.AlterSequences(
                    writer, arguments, oldSchema, newSchema, searchPathHelper);
            PgDiffTables.CreateTables(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffTables.AlterTables(
                    writer, arguments, oldSchema, newSchema, searchPathHelper);
            PgDiffSequences.AlterCreatedSequences(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffFunctions.CreateFunctions(
                    writer, arguments, oldSchema, newSchema, searchPathHelper);
            PgDiffConstraints.CreateConstraints(
                    writer, oldSchema, newSchema, true, searchPathHelper);
            PgDiffConstraints.CreateConstraints(
                    writer, oldSchema, newSchema, false, searchPathHelper);
            PgDiffIndexes.CreateIndexes(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffTables.CreateClusters(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffTriggers.CreateTriggers(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffViews.CreateViews(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffViews.AlterViews(
                    writer, oldSchema, newSchema, searchPathHelper);

            PgDiffFunctions.AlterComments(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffConstraints.AlterComments(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffIndexes.AlterComments(
                    writer, oldSchema, newSchema, searchPathHelper);
            PgDiffTriggers.AlterComments(
                    writer, oldSchema, newSchema, searchPathHelper);
        }
    }

    
    private PgDiff() {
    }
}
}