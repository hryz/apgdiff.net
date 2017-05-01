using System.IO;
using pgdiff.loader;
using pgdiff.Properties;
using pgdiff.schema;

namespace pgdiff
{
    public class PgDiff
    {
        private PgDiff()
        {
        }


        public static void CreateDiff(TextWriter writer, PgDiffArguments arguments)
        {
            var oldDatabase = PgDumpLoader.LoadDatabaseSchema(
                arguments.OldDumpFile, arguments.InCharsetName,
                arguments.OutputIgnoredStatements,
                arguments.IgnoreSlonyTriggers);

            var newDatabase = PgDumpLoader.LoadDatabaseSchema(
                arguments.NewDumpFile, arguments.InCharsetName,
                arguments.OutputIgnoredStatements,
                arguments.IgnoreSlonyTriggers);

            DiffDatabaseSchemas(writer, arguments, oldDatabase, newDatabase);
        }


        public static void CreateDiff(TextWriter writer, PgDiffArguments arguments, string oldInputStream, string newInputStream)
        {
            var oldDatabase = PgDumpLoader.LoadDatabaseSchema(
                oldInputStream, arguments.InCharsetName,
                arguments.OutputIgnoredStatements,
                arguments.IgnoreSlonyTriggers);

            var newDatabase = PgDumpLoader.LoadDatabaseSchema(
                newInputStream, arguments.InCharsetName,
                arguments.OutputIgnoredStatements,
                arguments.IgnoreSlonyTriggers);

            DiffDatabaseSchemas(writer, arguments, oldDatabase, newDatabase);
        }


        private static void CreateNewSchemas(TextWriter writer, PgDatabase oldDatabase, PgDatabase newDatabase)
        {
            foreach (var newSchema in newDatabase.Schemas)
                if (oldDatabase.GetSchema(newSchema.Name) == null)
                {
                    writer.WriteLine();
                    writer.WriteLine(newSchema.GetCreationSql());
                }
        }


        private static void DiffDatabaseSchemas(TextWriter writer, PgDiffArguments arguments, PgDatabase oldDatabase, PgDatabase newDatabase)
        {
            if (arguments.AddTransaction)
                writer.WriteLine("START TRANSACTION;");

            if (oldDatabase.Comment == null
                && newDatabase.Comment != null
                || oldDatabase.Comment != null
                && newDatabase.Comment != null
                && !oldDatabase.Comment.Equals(newDatabase.Comment))
            {
                writer.WriteLine();
                writer.Write("COMMENT ON DATABASE current_database() IS ");
                writer.Write(newDatabase.Comment);
                writer.WriteLine(';');
            }
            else if (oldDatabase.Comment != null
                     && newDatabase.Comment == null)
            {
                writer.WriteLine();
                writer.WriteLine("COMMENT ON DATABASE current_database() IS NULL;");
            }

            DropOldSchemas(writer, oldDatabase, newDatabase);
            CreateNewSchemas(writer, oldDatabase, newDatabase);
            UpdateSchemas(writer, arguments, oldDatabase, newDatabase);

            if (arguments.AddTransaction)
            {
                writer.WriteLine();
                writer.WriteLine("COMMIT TRANSACTION;");
            }

            if (arguments.OutputIgnoredStatements)
            {
                if (oldDatabase.IgnoredStatements.Count > 0)
                {
                    writer.WriteLine();
                    writer.Write("/* ");
                    writer.WriteLine(Resources.OriginalDatabaseIgnoredStatements);

                    foreach (var statement in oldDatabase.IgnoredStatements)
                    {
                        writer.WriteLine();
                        writer.WriteLine(statement);
                    }

                    writer.WriteLine("*/");
                }

                if (newDatabase.IgnoredStatements.Count > 0)
                {
                    writer.WriteLine();
                    writer.Write("/* ");
                    writer.WriteLine(Resources.NewDatabaseIgnoredStatements);

                    foreach (var statement in newDatabase.IgnoredStatements)
                    {
                        writer.WriteLine();
                        writer.WriteLine(statement);
                    }

                    writer.WriteLine("*/");
                }
            }
        }


        private static void DropOldSchemas(TextWriter writer, PgDatabase oldDatabase, PgDatabase newDatabase)
        {
            foreach (var oldSchema in oldDatabase.Schemas)
                if (newDatabase.GetSchema(oldSchema.Name) == null)
                {
                    writer.WriteLine();
                    writer.WriteLine($"DROP SCHEMA {PgDiffUtils.GetQuotedName(oldSchema.Name)} CASCADE;");
                }
        }


        private static void UpdateSchemas(TextWriter writer, PgDiffArguments arguments, PgDatabase oldDatabase, PgDatabase newDatabase)
        {
            var setSearchPath = newDatabase.Schemas.Count > 1
                                || !newDatabase.Schemas[0].Name.Equals("public");

            foreach (var newSchema in newDatabase.Schemas)
            {
                var searchPathHelper = setSearchPath 
                    ? new SearchPathHelper($"SET search_path = {PgDiffUtils.GetQuotedName(newSchema.Name, true)}, pg_catalog;") 
                    : new SearchPathHelper(null);

                var oldSchema = oldDatabase.GetSchema(newSchema.Name);

                if (oldSchema != null)
                    if (oldSchema.Comment == null
                        && newSchema.Comment != null
                        || oldSchema.Comment != null
                        && newSchema.Comment != null
                        && !oldSchema.Comment.Equals(
                            newSchema.Comment))
                    {
                        writer.WriteLine();
                        writer.Write("COMMENT ON SCHEMA ");
                        writer.Write(PgDiffUtils.GetQuotedName(newSchema.Name));
                        writer.Write(" IS ");
                        writer.Write(newSchema.Comment);
                        writer.WriteLine(';');
                    }
                    else if (oldSchema.Comment != null && newSchema.Comment == null)
                    {
                        writer.WriteLine();
                        writer.Write("COMMENT ON SCHEMA ");
                        writer.Write(PgDiffUtils.GetQuotedName(newSchema.Name));
                        writer.WriteLine(" IS NULL;");
                    }

                PgDiffTriggers.DropTriggers(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffFunctions.DropFunctions(writer, arguments, oldSchema, newSchema, searchPathHelper);
                PgDiffViews.DropViews(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffConstraints.DropConstraints(writer, oldSchema, newSchema, true, searchPathHelper);
                PgDiffConstraints.DropConstraints(writer, oldSchema, newSchema, false, searchPathHelper);
                PgDiffIndexes.DropIndexes(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffTables.DropClusters(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffTables.DropTables(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffSequences.DropSequences(writer, oldSchema, newSchema, searchPathHelper);

                PgDiffSequences.CreateSequences(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffSequences.AlterSequences(writer, arguments, oldSchema, newSchema, searchPathHelper);
                PgDiffTables.CreateTables(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffTables.AlterTables(writer, arguments, oldSchema, newSchema, searchPathHelper);
                PgDiffSequences.AlterCreatedSequences(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffFunctions.CreateFunctions(writer, arguments, oldSchema, newSchema, searchPathHelper);
                PgDiffConstraints.CreateConstraints(writer, oldSchema, newSchema, true, searchPathHelper);
                PgDiffConstraints.CreateConstraints(writer, oldSchema, newSchema, false, searchPathHelper);
                PgDiffIndexes.CreateIndexes(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffTables.CreateClusters(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffTriggers.CreateTriggers(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffViews.CreateViews(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffViews.AlterViews(writer, oldSchema, newSchema, searchPathHelper);

                PgDiffFunctions.AlterComments(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffConstraints.AlterComments(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffIndexes.AlterComments(writer, oldSchema, newSchema, searchPathHelper);
                PgDiffTriggers.AlterComments(writer, oldSchema, newSchema, searchPathHelper);
            }
        }
    }
}