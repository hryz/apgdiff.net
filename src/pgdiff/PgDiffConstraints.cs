using System.Collections.Generic;
using System.IO;
using System.Linq;
using pgdiff.schema;

namespace pgdiff
{
    public class PgDiffConstraints
    {
        private PgDiffConstraints()
        {
        }

        public static void CreateConstraints(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, bool primaryKey, SearchPathHelper searchPathHelper)
        {
            foreach (var newTable in newSchema.GetTables())
            {
                var oldTable = oldSchema?.GetTable(newTable.Name);

                // Add new constraints
                foreach (var constraint in GetNewConstraints(oldTable, newTable, primaryKey))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(constraint.GetCreationSql());
                }
            }
        }

        public static void DropConstraints(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, bool primaryKey, SearchPathHelper searchPathHelper)
        {
            foreach (var newTable in newSchema.GetTables())
            {
                var oldTable = oldSchema?.GetTable(newTable.Name);

                // Drop constraints that no more exist or are modified
                foreach (var constraint in GetDropConstraints(oldTable, newTable, primaryKey))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(constraint.GetDropSql());
                }
            }
        }


        private static IEnumerable<PgConstraint> GetDropConstraints(PgTable oldTable, PgTable newTable, bool primaryKey)
        {
            var list = new List<PgConstraint>();

            if (newTable == null || oldTable == null)
                return list;

            list.AddRange(oldTable.Constraints
                .Where(c => c.IsPrimaryKeyConstraint() == primaryKey
                            && (!newTable.ContainsConstraint(c.Name)
                                || !newTable.GetConstraint(c.Name).Equals(c))));

            return list;
        }


        private static List<PgConstraint> GetNewConstraints(PgTable oldTable, PgTable newTable, bool primaryKey)
        {
            var list = new List<PgConstraint>();

            if (newTable == null)
                return list;

            if (oldTable == null)
            {
                list.AddRange(newTable.Constraints.Where(c => c.IsPrimaryKeyConstraint() == primaryKey));
            }
            else
            {
                list.AddRange(newTable.Constraints
                    .Where(c => c.IsPrimaryKeyConstraint() == primaryKey
                                && (!oldTable.ContainsConstraint(c.Name)
                                    || !oldTable.GetConstraint(c.Name).Equals(c))));
            }

            return list;
        }


        public static void AlterComments(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            if (oldSchema == null)
                return;

            foreach (var oldTable in oldSchema.GetTables())
            {
                var newTable = newSchema.GetTable(oldTable.Name);

                if (newTable == null)
                    continue;

                foreach (var oldConstraint in oldTable.Constraints)
                {
                    var newConstraint =
                        newTable.GetConstraint(oldConstraint.Name);

                    if (newConstraint == null)
                        continue;

                    if (oldConstraint.Comment == null
                        && newConstraint.Comment != null
                        || oldConstraint.Comment != null
                        && newConstraint.Comment != null
                        && !oldConstraint.Comment.Equals(
                            newConstraint.Comment))
                    {
                        searchPathHelper.OutputSearchPath(writer);
                        writer.WriteLine();
                        writer.Write("COMMENT ON ");

                        if (newConstraint.IsPrimaryKeyConstraint())
                        {
                            writer.Write("INDEX ");
                            writer.Write(PgDiffUtils.GetQuotedName(newConstraint.Name));
                        }
                        else
                        {
                            writer.Write("CONSTRAINT ");
                            writer.Write(PgDiffUtils.GetQuotedName(newConstraint.Name));
                            writer.Write(" ON ");
                            writer.Write(PgDiffUtils.GetQuotedName(newConstraint.TableName));
                        }

                        writer.Write(" IS ");
                        writer.Write(newConstraint.Comment);
                        writer.WriteLine(';');
                    }
                    else if (oldConstraint.Comment != null && newConstraint.Comment == null)
                    {
                        searchPathHelper.OutputSearchPath(writer);
                        writer.WriteLine();
                        writer.Write("COMMENT ON ");

                        if (newConstraint.IsPrimaryKeyConstraint())
                        {
                            writer.Write("INDEX ");
                            writer.Write(PgDiffUtils.GetQuotedName(newConstraint.Name));
                        }
                        else
                        {
                            writer.Write("CONSTRAINT ");
                            writer.Write(PgDiffUtils.GetQuotedName(newConstraint.Name));
                            writer.Write(" ON ");
                            writer.Write(PgDiffUtils.GetQuotedName(newConstraint.TableName));
                        }

                        writer.WriteLine(" IS NULL;");
                    }
                }
            }
        }
    }
}