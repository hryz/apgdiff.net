
package cz.startnet.utils.pgdiff;

import cz.startnet.utils.pgdiff.schema.PgConstraint;
import cz.startnet.utils.pgdiff.schema.PgSchema;
import cz.startnet.utils.pgdiff.schema.PgTable;
import java.io.PrintWriter;
import java.util.ArrayList;
import java.util.List;


public class PgDiffConstraints {

    
    public static void createConstraints(final PrintWriter writer,
            final PgSchema oldSchema, final PgSchema newSchema,
            final boolean primaryKey, final SearchPathHelper searchPathHelper) {
        for (final PgTable newTable : newSchema.getTables()) {
            final PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            // Add new constraints
            for (final PgConstraint constraint :
                    getNewConstraints(oldTable, newTable, primaryKey)) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.println(constraint.getCreationSQL());
            }
        }
    }

    public static void dropConstraints(final PrintWriter writer,
            final PgSchema oldSchema, final PgSchema newSchema,
            final boolean primaryKey, final SearchPathHelper searchPathHelper) {
        for (final PgTable newTable : newSchema.getTables()) {
            final PgTable oldTable;

            if (oldSchema == null) {
                oldTable = null;
            } else {
                oldTable = oldSchema.getTable(newTable.getName());
            }

            // Drop constraints that no more exist or are modified
            for (final PgConstraint constraint :
                    getDropConstraints(oldTable, newTable, primaryKey)) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.println(constraint.getDropSQL());
            }
        }
    }

    
    private static List<PgConstraint> getDropConstraints(final PgTable oldTable,
            final PgTable newTable, final boolean primaryKey) {
        @SuppressWarnings("CollectionWithoutInitialCapacity")
        final List<PgConstraint> list = new ArrayList<PgConstraint>();

        if (newTable != null && oldTable != null) {
            for (final PgConstraint constraint : oldTable.getConstraints()) {
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

    
    private static List<PgConstraint> getNewConstraints(final PgTable oldTable,
            final PgTable newTable, final boolean primaryKey) {
        @SuppressWarnings("CollectionWithoutInitialCapacity")
        final List<PgConstraint> list = new ArrayList<PgConstraint>();

        if (newTable != null) {
            if (oldTable == null) {
                for (final PgConstraint constraint :
                        newTable.getConstraints()) {
                    if (constraint.isPrimaryKeyConstraint() == primaryKey) {
                        list.add(constraint);
                    }
                }
            } else {
                for (final PgConstraint constraint :
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

    
    public static void alterComments(final PrintWriter writer,
            final PgSchema oldSchema, final PgSchema newSchema,
            final SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        for (PgTable oldTable : oldSchema.getTables()) {
            final PgTable newTable = newSchema.getTable(oldTable.getName());

            if (newTable == null) {
                continue;
            }

            for (final PgConstraint oldConstraint : oldTable.getConstraints()) {
                final PgConstraint newConstraint =
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
