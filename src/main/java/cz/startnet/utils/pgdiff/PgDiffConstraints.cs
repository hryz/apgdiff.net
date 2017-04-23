using System.Collections.Generic;
using System.IO;
using pgdiff.schema;

namespace pgdiff {






public class PgDiffConstraints {

    
    public static void createConstraints(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            bool primaryKey, SearchPathHelper searchPathHelper) {
        foreach (PgTable newTable in newSchema.getTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            // Add new constraints
            foreach (PgConstraint constraint in getNewConstraints(oldTable, newTable, primaryKey)) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(constraint.getCreationSQL());
            }
        }
    }

    public static void dropConstraints(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            bool primaryKey, SearchPathHelper searchPathHelper) {
        foreach (PgTable newTable in newSchema.getTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            // Drop constraints that no more exist or are modified
            foreach (PgConstraint constraint in getDropConstraints(oldTable, newTable, primaryKey)) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(constraint.getDropSQL());
            }
        }
    }

    
    private static List<PgConstraint> getDropConstraints(PgTable oldTable,
            PgTable newTable, bool primaryKey) {
        
        List<PgConstraint> list = new List<PgConstraint>();

        if (newTable != null && oldTable != null) {
            foreach (PgConstraint constraint in oldTable.getConstraints()) {
                if (constraint.isPrimaryKeyConstraint() == primaryKey
                        && (!newTable.containsConstraint(constraint.getName())
                        || !newTable.getConstraint(constraint.getName()).Equals(
                        constraint))) {
                    list.Add(constraint);
                }
            }
        }

        return list;
    }

    
    private static List<PgConstraint> getNewConstraints(PgTable oldTable,
            PgTable newTable, bool primaryKey) {
        
        List<PgConstraint> list = new List<PgConstraint>();

        if (newTable != null) {
            if (oldTable == null) {
                foreach (PgConstraint constraint in newTable.getConstraints()) {
                    if (constraint.isPrimaryKeyConstraint() == primaryKey) {
                        list.Add(constraint);
                    }
                }
            } else {
                foreach (PgConstraint constraint in newTable.getConstraints()) {
                    if ((constraint.isPrimaryKeyConstraint() == primaryKey)
                            && (!oldTable.containsConstraint(
                            constraint.getName())
                            || !oldTable.getConstraint(constraint.getName()).
                            Equals(constraint))) {
                        list.Add(constraint);
                    }
                }
            }
        }

        return list;
    }

    
    public static void alterComments(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        foreach (PgTable oldTable in oldSchema.getTables()) {
            PgTable newTable = newSchema.getTable(oldTable.getName());

            if (newTable == null) {
                continue;
            }

            foreach (PgConstraint oldConstraint in  oldTable.getConstraints()) {
                PgConstraint newConstraint =
                        newTable.getConstraint(oldConstraint.getName());

                if (newConstraint == null) {
                    continue;
                }

                if (oldConstraint.getComment() == null
                        && newConstraint.getComment() != null
                        || oldConstraint.getComment() != null
                        && newConstraint.getComment() != null
                        && !oldConstraint.getComment().Equals(
                        newConstraint.getComment())) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON ");

                    if (newConstraint.isPrimaryKeyConstraint()) {
                        writer.Write("INDEX ");
                        writer.Write(PgDiffUtils.getQuotedName(
                                newConstraint.getName()));
                    } else {
                        writer.Write("CONSTRAINT ");
                        writer.Write(PgDiffUtils.getQuotedName(
                                newConstraint.getName()));
                        writer.Write(" ON ");
                        writer.Write(PgDiffUtils.getQuotedName(
                                newConstraint.getTableName()));
                    }

                    writer.Write(" IS ");
                    writer.Write(newConstraint.getComment());
                    writer.WriteLine(';');
                } else if (oldConstraint.getComment() != null
                        && newConstraint.getComment() == null) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON ");

                    if (newConstraint.isPrimaryKeyConstraint()) {
                        writer.Write("INDEX ");
                        writer.Write(PgDiffUtils.getQuotedName(
                                newConstraint.getName()));
                    } else {
                        writer.Write("CONSTRAINT ");
                        writer.Write(PgDiffUtils.getQuotedName(
                                newConstraint.getName()));
                        writer.Write(" ON ");
                        writer.Write(PgDiffUtils.getQuotedName(
                                newConstraint.getTableName()));
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