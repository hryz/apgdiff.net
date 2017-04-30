using System.Collections.Generic;
using System.IO;
using pgdiff.schema;

namespace pgdiff {






public class PgDiffConstraints {

    
    public static void CreateConstraints(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            bool primaryKey, SearchPathHelper searchPathHelper) {
        foreach (PgTable newTable in newSchema.GetTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.GetTable(newTable.GetName());
            }

            // Add new constraints
            foreach (PgConstraint constraint in GetNewConstraints(oldTable, newTable, primaryKey)) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(constraint.GetCreationSql());
            }
        }
    }

    public static void DropConstraints(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            bool primaryKey, SearchPathHelper searchPathHelper) {
        foreach (PgTable newTable in newSchema.GetTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.GetTable(newTable.GetName());
            }

            // Drop constraints that no more exist or are modified
            foreach (PgConstraint constraint in GetDropConstraints(oldTable, newTable, primaryKey)) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(constraint.GetDropSql());
            }
        }
    }

    
    private static List<PgConstraint> GetDropConstraints(PgTable oldTable,
            PgTable newTable, bool primaryKey) {
        
        List<PgConstraint> list = new List<PgConstraint>();

        if (newTable != null && oldTable != null) {
            foreach (PgConstraint constraint in oldTable.GetConstraints()) {
                if (constraint.IsPrimaryKeyConstraint() == primaryKey
                        && (!newTable.ContainsConstraint(constraint.GetName())
                        || !newTable.GetConstraint(constraint.GetName()).Equals(
                        constraint))) {
                    list.Add(constraint);
                }
            }
        }

        return list;
    }

    
    private static List<PgConstraint> GetNewConstraints(PgTable oldTable,
            PgTable newTable, bool primaryKey) {
        
        List<PgConstraint> list = new List<PgConstraint>();

        if (newTable != null) {
            if (oldTable == null) {
                foreach (PgConstraint constraint in newTable.GetConstraints()) {
                    if (constraint.IsPrimaryKeyConstraint() == primaryKey) {
                        list.Add(constraint);
                    }
                }
            } else {
                foreach (PgConstraint constraint in newTable.GetConstraints()) {
                    if ((constraint.IsPrimaryKeyConstraint() == primaryKey)
                            && (!oldTable.ContainsConstraint(
                            constraint.GetName())
                            || !oldTable.GetConstraint(constraint.GetName()).
                            Equals(constraint))) {
                        list.Add(constraint);
                    }
                }
            }
        }

        return list;
    }

    
    public static void AlterComments(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        foreach (PgTable oldTable in oldSchema.GetTables()) {
            PgTable newTable = newSchema.GetTable(oldTable.GetName());

            if (newTable == null) {
                continue;
            }

            foreach (PgConstraint oldConstraint in  oldTable.GetConstraints()) {
                PgConstraint newConstraint =
                        newTable.GetConstraint(oldConstraint.GetName());

                if (newConstraint == null) {
                    continue;
                }

                if (oldConstraint.GetComment() == null
                        && newConstraint.GetComment() != null
                        || oldConstraint.GetComment() != null
                        && newConstraint.GetComment() != null
                        && !oldConstraint.GetComment().Equals(
                        newConstraint.GetComment())) {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON ");

                    if (newConstraint.IsPrimaryKeyConstraint()) {
                        writer.Write("INDEX ");
                        writer.Write(PgDiffUtils.GetQuotedName(
                                newConstraint.GetName()));
                    } else {
                        writer.Write("CONSTRAINT ");
                        writer.Write(PgDiffUtils.GetQuotedName(
                                newConstraint.GetName()));
                        writer.Write(" ON ");
                        writer.Write(PgDiffUtils.GetQuotedName(
                                newConstraint.GetTableName()));
                    }

                    writer.Write(" IS ");
                    writer.Write(newConstraint.GetComment());
                    writer.WriteLine(';');
                } else if (oldConstraint.GetComment() != null
                        && newConstraint.GetComment() == null) {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON ");

                    if (newConstraint.IsPrimaryKeyConstraint()) {
                        writer.Write("INDEX ");
                        writer.Write(PgDiffUtils.GetQuotedName(
                                newConstraint.GetName()));
                    } else {
                        writer.Write("CONSTRAINT ");
                        writer.Write(PgDiffUtils.GetQuotedName(
                                newConstraint.GetName()));
                        writer.Write(" ON ");
                        writer.Write(PgDiffUtils.GetQuotedName(
                                newConstraint.GetTableName()));
                    }

                    writer.WriteLine(" IS NULL;");
                }
            }
        }
    }

    
    private PgDiffConstraints() {
    }
}
}