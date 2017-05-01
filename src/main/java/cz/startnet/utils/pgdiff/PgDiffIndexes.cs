using System.Collections.Generic;
using System.IO;
using System.Linq;
using pgdiff.schema;

namespace pgdiff
{
    public class PgDiffIndexes
    {
        private PgDiffIndexes()
        {
        }


        public static void CreateIndexes(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            foreach (var newTable in newSchema.GetTables())
            {
                var newTableName = newTable.Name;

                // Add new indexes
                if (oldSchema == null)
                    foreach (var index in newTable.GetIndexes())
                    {
                        searchPathHelper.OutputSearchPath(writer);
                        writer.WriteLine();
                        writer.WriteLine(index.GetCreationSql());
                    }
                else
                    foreach (var index in GetNewIndexes(
                        oldSchema.GetTable(newTableName), newTable))
                    {
                        searchPathHelper.OutputSearchPath(writer);
                        writer.WriteLine();
                        writer.WriteLine(index.GetCreationSql());
                    }
            }
        }


        public static void DropIndexes(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            foreach (var newTable in newSchema.GetTables())
            {
                var newTableName = newTable.Name;
                var oldTable = oldSchema?.GetTable(newTableName);

                // Drop indexes that do not exist in new schema or are modified
                foreach (var index in GetDropIndexes(oldTable, newTable))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(index.GetDropSql());
                }
            }
        }


        private static IEnumerable<PgIndex> GetDropIndexes(PgTable oldTable, PgTable newTable)
        {
            var list = new List<PgIndex>();

            if (newTable == null || oldTable == null)
                return list;

            list.AddRange(oldTable.GetIndexes()
                .Where(i => !newTable.ContainsIndex(i.Name)
                            || !newTable.GetIndex(i.Name).Equals(i)));

            return list;
        }


        private static IEnumerable<PgIndex> GetNewIndexes(PgTable oldTable, PgTable newTable)
        {
            var list = new List<PgIndex>();

            if (newTable == null)
                return list;

            list.AddRange(oldTable == null
                ? newTable.GetIndexes()
                : newTable.GetIndexes()
                    .Where(i => !oldTable.ContainsIndex(i.Name)
                                || !oldTable.GetIndex(i.Name).Equals(i)));

            return list;
        }


        public static void AlterComments(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            if (oldSchema == null)
                return;

            foreach (var oldIndex in oldSchema.GetIndexes())
            {
                var newIndex = newSchema.GetIndex(oldIndex.Name);

                if (newIndex == null)
                    continue;

                if (oldIndex.Comment == null
                    && newIndex.Comment != null
                    || oldIndex.Comment != null
                    && newIndex.Comment != null
                    && !oldIndex.Comment.Equals(
                        newIndex.Comment))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON INDEX ");
                    writer.Write(PgDiffUtils.GetQuotedName(newIndex.Name));
                    writer.Write(" IS ");
                    writer.Write(newIndex.Comment);
                    writer.WriteLine(';');
                }
                else if (oldIndex.Comment != null && newIndex.Comment == null)
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON INDEX ");
                    writer.Write(PgDiffUtils.GetQuotedName(newIndex.Name));
                    writer.WriteLine(" IS NULL;");
                }
            }
        }
    }
}