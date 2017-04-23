
using System.Collections.Generic;
using cz.startnet.utils.pgdiff.schema;

namespace cz.startnet.utils.pgdiff {






public class PgDiffConstraints {

    
    public static void createConstraints(PrintWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            bool primaryKey, SearchPathHelper searchPathHelper) {
        for (PgTable newTable : newSchema.getTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            // Add new constraints
            for (PgConstraint constraint :
                    getNewConstraints(oldTable, newTable, primaryKey)) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.println(constraint.getCreationSQL());
            }
        }
    }

    public static void dropConstraints(PrintWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            bool primaryKey, SearchPathHelper searchPathHelper) {
        for (PgTable newTable : newSchema.getTables()) {
            PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            // Drop constraints that no more exist or are modified
            for (PgConstraint constraint :
                    getDropConstraints(oldTable, newTable, primaryKey)) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.println(constraint.getDropSQL());
            }
        }
    }

    
    private static List<PgConstraint> getDropConstraints(PgTable oldTable,
            PgTable newTable, bool primaryKey) {
        
        List<PgConstraint> list = new ArrayList<PgConstraint>();

        if (newTable != null && oldTable != null) {
            for (PgConstraint constraint : oldTable.getConstraints()) {
                if (constraint.isPrimaryKeyConstraint() == primaryKey
                        && (!newTable.containsConstraint(constraint.getName())
                        || !newTable.getConstraint(constraint.getName()).equals(
                        constraint))) {
                    list.add(constraint);
                }
            }
        }

        return list;
    }

    
    private static List<PgConstraint> getNewConstraints(PgTable oldTable,
            PgTable newTable, bool primaryKey) {
        
        List<PgConstraint> list = new ArrayList<PgConstraint>();

        if (newTable != null) {
            if (oldTable == null) {
                for (PgConstraint constraint :
                        newTable.getConstraints()) {
                    if (constraint.isPrimaryKeyConstraint() == primaryKey) {
                        list.add(constraint);
                    }
                }
            } else {
                for (PgConstraint constraint :
                        newTable.getConstraints()) {
                    if ((constraint.isPrimaryKeyConstraint() == primaryKey)
                            && (!oldTable.containsConstraint(
                            constraint.getName())
                            || !oldTable.getConstraint(constraint.getName()).
                            equals(constraint))) {
                        list.add(constraint);
                    }
                }
            }
        }

        return list;
    }

    
    public static void alterComments(PrintWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        for (PgTable oldTable : oldSchema.getTables()) {
            PgTable newTable = newSchema.getTable(oldTable.getName());

            if (newTable == null) {
                continue;
            }

            for (PgConstraint oldConstraint : oldTable.getConstraints()) {
                PgConstraint newConstraint =
                        newTable.getConstraint(oldConstraint.getName());

                if (newConstraint == null) {
                    continue;
                }

                if (oldConstraint.getComment() == null
                        && newConstraint.getComment() != null
                        || oldConstraint.getComment() != null
                        && newConstraint.getComment() != null
                        && !oldConstraint.getComment().equals(
                        newConstraint.getComment())) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.println();
                    writer.print("COMMENT ON ");

                    if (newConstraint.isPrimaryKeyConstraint()) {
                        writer.print("INDEX ");
                        writer.print(PgDiffUtils.getQuotedName(
                                newConstraint.getName()));
                    } else {
                        writer.print("CONSTRAINT ");
                        writer.print(PgDiffUtils.getQuotedName(
                                newConstraint.getName()));
                        writer.print(" ON ");
                        writer.print(PgDiffUtils.getQuotedName(
                                newConstraint.getTableName()));
                    }

                    writer.print(" IS ");
                    writer.print(newConstraint.getComment());
                    writer.println(';');
                } else if (oldConstraint.getComment() != null
                        && newConstraint.getComment() == null) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.println();
                    writer.print("COMMENT ON ");

                    if (newConstraint.isPrimaryKeyConstraint()) {
                        writer.print("INDEX ");
                        writer.print(PgDiffUtils.getQuotedName(
                                newConstraint.getName()));
                    } else {
                        writer.print("CONSTRAINT ");
                        writer.print(PgDiffUtils.getQuotedName(
                                newConstraint.getName()));
                        writer.print(" ON ");
                        writer.print(PgDiffUtils.getQuotedName(
                                newConstraint.getTableName()));
                    }

                    writer.println(" IS NULL;");
                }
            }
        }
    }

    
    private PgDiffConstraints() {
    }
}
}