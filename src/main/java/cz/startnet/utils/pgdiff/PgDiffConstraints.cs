using System.Collections.Generic;
using System.IO;
using pgdiff.schema;

namespace pgdiff
{
    public class PgDiffConstraints
    {
        private PgDiffConstraints()
        {
        }

        public static void CreateConstraints(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, bool primaryKey,
            SearchPathHelper searchPathHelper)
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

        public static void DropConstraints(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, bool primaryKey,
            SearchPathHelper searchPathHelper)
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


        private static List<PgConstraint> GetDropConstraints(PgTable oldTable, PgTable newTable, bool primaryKey)
        {
            var list = new List<PgConstraint>();

            if (newTable != null && oldTable != null)
                foreach (var constraint in oldTable.Constraints)
                    if (constraint.IsPrimaryKeyConstraint() == primaryKey
                        && (!newTable.ContainsConstraint(constraint.Name)
                            || !newTable.GetConstraint(constraint.Name).Equals(constraint)))
                        list.Add(constraint);

            return list;
        }


        private static List<PgConstraint> GetNewConstraints(PgTable oldTable, PgTable newTable, bool primaryKey)
        {
            var list = new List<PgConstraint>();

            if (newTable != null)
                if (oldTable == null)
                {
                    foreach (var constraint in newTable.Constraints)
                        if (constraint.IsPrimaryKeyConstraint() == primaryKey)
                            list.Add(constraint);
                }
                else
                {
                    foreach (var constraint in newTable.Constraints)
                        if (constraint.IsPrimaryKeyConstraint() == primaryKey
                            && (!oldTable.ContainsConstraint(constraint.Name)
                                || !oldTable.GetConstraint(constraint.Name).Equals(constraint)))
                            list.Add(constraint);
                }

            return list;
        }


        public static void AlterComments(TextWriter writer, PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper)
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