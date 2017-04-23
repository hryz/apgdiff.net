
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using cz.startnet.utils.pgdiff.schema;

namespace cz.startnet.utils.pgdiff {







public class PgDiffViews {

    
    public static void createViews(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        foreach (PgView newView in newSchema.getViews()) {
            if (oldSchema == null
                    || !oldSchema.containsView(newView.getName())
                    || isViewModified(
                    oldSchema.getView(newView.getName()), newView)) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(newView.getCreationSQL());
            }
        }
    }

    
    public static void dropViews(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        foreach (PgView oldView in oldSchema.getViews()) {
            PgView newView = newSchema.getView(oldView.getName());

            if (newView == null || isViewModified(oldView, newView)) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(oldView.getDropSQL());
            }
        }
    }

    
    private static bool isViewModified(PgView oldView,
            PgView newView) {
        String[] oldViewColumnNames;

        if (oldView.getColumnNames() == null|| oldView.getColumnNames().Count == 0) {
            oldViewColumnNames = null;
        } else {
            oldViewColumnNames = oldView.getColumnNames().ToArray();
        }

        String[] newViewColumnNames;

        if (newView.getColumnNames() == null || newView.getColumnNames().Count == 0) {
            newViewColumnNames = null;
        } else {
            newViewColumnNames = newView.getColumnNames().ToArray();
        }

        if (oldViewColumnNames == null && newViewColumnNames == null) {
            return !oldView.getQuery().Trim().Equals(newView.getQuery().Trim());
        } else {
            return !Arrays.Equals(oldViewColumnNames, newViewColumnNames); //todo!
        }
    }

    
    public static void alterViews(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        foreach (PgView oldView in oldSchema.getViews()) {
            PgView newView = newSchema.getView(oldView.getName());

            if (newView == null) {
                continue;
            }

            diffDefaultValues(writer, oldView, newView, searchPathHelper);

            if (oldView.getComment() == null
                    && newView.getComment() != null
                    || oldView.getComment() != null
                    && newView.getComment() != null
                    && !oldView.getComment().Equals(
                    newView.getComment())) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON VIEW ");
                writer.Write(
                        PgDiffUtils.getQuotedName(newView.getName()));
                writer.Write(" IS ");
                writer.Write(newView.getComment());
                writer.WriteLine(';');
            } else if (oldView.getComment() != null
                    && newView.getComment() == null) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON VIEW ");
                writer.Write(PgDiffUtils.getQuotedName(newView.getName()));
                writer.WriteLine(" IS NULL;");
            }

            List<String> columnNames = new List<string>();

            foreach (PgView.ColumnComment columnComment in newView.getColumnComments()) {
                columnNames.Add(columnComment.getColumnName());
            }

            foreach (PgView.ColumnComment columnComment in oldView.getColumnComments()) {
                if (!columnNames.Contains(columnComment.getColumnName())) {
                    columnNames.Add(columnComment.getColumnName());
                }
            }

            foreach (String columnName in columnNames) {
                PgView.ColumnComment oldColumnComment = null;
                PgView.ColumnComment newColumnComment = null;

                foreach (PgView.ColumnComment columnComment in oldView.getColumnComments()) {
                    if (columnName.Equals(columnComment.getColumnName())) {
                        oldColumnComment = columnComment;
                        break;
                    }
                }

                foreach (PgView.ColumnComment columnComment in newView.getColumnComments()) {
                    if (columnName.Equals(columnComment.getColumnName())) {
                        newColumnComment = columnComment;
                        break;
                    }
                }

                if (oldColumnComment == null && newColumnComment != null
                        || oldColumnComment != null && newColumnComment != null
                        && !oldColumnComment.getComment().Equals(
                        newColumnComment.getComment())) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.println();
                    writer.print("COMMENT ON COLUMN ");
                    writer.print(PgDiffUtils.getQuotedName(newView.getName()));
                    writer.print('.');
                    writer.print(PgDiffUtils.getQuotedName(
                            newColumnComment.getColumnName()));
                    writer.print(" IS ");
                    writer.print(newColumnComment.getComment());
                    writer.println(';');
                } else if (oldColumnComment != null
                        && newColumnComment == null) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.println();
                    writer.print("COMMENT ON COLUMN ");
                    writer.print(PgDiffUtils.getQuotedName(newView.getName()));
                    writer.print('.');
                    writer.print(PgDiffUtils.getQuotedName(
                            oldColumnComment.getColumnName()));
                    writer.println(" IS NULL;");
                }
            }
        }
    }

    
    private static void diffDefaultValues(TextWriter writer,
            PgView oldView, PgView newView,
            SearchPathHelper searchPathHelper) {
        List<PgView.DefaultValue> oldValues =
                oldView.getDefaultValues();
        List<PgView.DefaultValue> newValues =
                newView.getDefaultValues();

        // modify defaults that are in old view
        for (PgView.DefaultValue oldValue : oldValues) {
            bool found = false;

            for (PgView.DefaultValue newValue : newValues) {
                if (oldValue.getColumnName().Equals(newValue.getColumnName())) {
                    found = true;

                    if (!oldValue.getDefaultValue().Equals(
                            newValue.getDefaultValue())) {
                        searchPathHelper.outputSearchPath(writer);
                        writer.println();
                        writer.print("ALTER TABLE ");
                        writer.print(
                                PgDiffUtils.getQuotedName(newView.getName()));
                        writer.print(" ALTER COLUMN ");
                        writer.print(PgDiffUtils.getQuotedName(
                                newValue.getColumnName()));
                        writer.print(" SET DEFAULT ");
                        writer.print(newValue.getDefaultValue());
                        writer.println(';');
                    }

                    break;
                }
            }

            if (!found) {
                searchPathHelper.outputSearchPath(writer);
                writer.println();
                writer.print("ALTER TABLE ");
                writer.print(PgDiffUtils.getQuotedName(newView.getName()));
                writer.print(" ALTER COLUMN ");
                writer.print(PgDiffUtils.getQuotedName(
                        oldValue.getColumnName()));
                writer.println(" DROP DEFAULT;");
            }
        }

        // add new defaults
        for (PgView.DefaultValue newValue : newValues) {
            bool found = false;

            for (PgView.DefaultValue oldValue : oldValues) {
                if (newValue.getColumnName().Equals(oldValue.getColumnName())) {
                    found = true;
                    break;
                }
            }

            if (found) {
                continue;
            }

            searchPathHelper.outputSearchPath(writer);
            writer.println();
            writer.print("ALTER TABLE ");
            writer.print(PgDiffUtils.getQuotedName(newView.getName()));
            writer.print(" ALTER COLUMN ");
            writer.print(PgDiffUtils.getQuotedName(newValue.getColumnName()));
            writer.print(" SET DEFAULT ");
            writer.print(newValue.getDefaultValue());
            writer.println(';');
        }
    }

    
    private PgDiffViews() {
    }
}
}