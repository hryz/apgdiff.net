using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using pgdiff.schema;

namespace pgdiff {







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
        }
        else
        {
            return !Enumerable.SequenceEqual(oldViewColumnNames, newViewColumnNames);
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
                    writer.WriteLine();
                    writer.Write("COMMENT ON COLUMN ");
                    writer.Write(PgDiffUtils.getQuotedName(newView.getName()));
                    writer.Write('.');
                    writer.Write(PgDiffUtils.getQuotedName(
                            newColumnComment.getColumnName()));
                    writer.Write(" IS ");
                    writer.Write(newColumnComment.getComment());
                    writer.WriteLine(';');
                } else if (oldColumnComment != null
                        && newColumnComment == null) {
                    searchPathHelper.outputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON COLUMN ");
                    writer.Write(PgDiffUtils.getQuotedName(newView.getName()));
                    writer.Write('.');
                    writer.Write(PgDiffUtils.getQuotedName(
                            oldColumnComment.getColumnName()));
                    writer.WriteLine(" IS NULL;");
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
        foreach(PgView.DefaultValue oldValue in oldValues) {
            bool found = false;

            foreach(PgView.DefaultValue newValue in newValues) {
                if (oldValue.getColumnName().Equals(newValue.getColumnName())) {
                    found = true;

                    if (!oldValue.getDefaultValue().Equals(
                            newValue.getDefaultValue())) {
                        searchPathHelper.outputSearchPath(writer);
                        writer.WriteLine();
                        writer.Write("ALTER TABLE ");
                        writer.Write(
                                PgDiffUtils.getQuotedName(newView.getName()));
                        writer.Write(" ALTER COLUMN ");
                        writer.Write(PgDiffUtils.getQuotedName(
                                newValue.getColumnName()));
                        writer.Write(" SET DEFAULT ");
                        writer.Write(newValue.getDefaultValue());
                        writer.WriteLine(';');
                    }

                    break;
                }
            }

            if (!found) {
                searchPathHelper.outputSearchPath(writer);
                writer.WriteLine();
                writer.Write("ALTER TABLE ");
                writer.Write(PgDiffUtils.getQuotedName(newView.getName()));
                writer.Write(" ALTER COLUMN ");
                writer.Write(PgDiffUtils.getQuotedName(
                        oldValue.getColumnName()));
                writer.WriteLine(" DROP DEFAULT;");
            }
        }

        // add new defaults
        foreach(PgView.DefaultValue newValue in newValues) {
            bool found = false;

            foreach(PgView.DefaultValue oldValue in oldValues) {
                if (newValue.getColumnName().Equals(oldValue.getColumnName())) {
                    found = true;
                    break;
                }
            }

            if (found) {
                continue;
            }

            searchPathHelper.outputSearchPath(writer);
            writer.WriteLine();
            writer.Write("ALTER TABLE ");
            writer.Write(PgDiffUtils.getQuotedName(newView.getName()));
            writer.Write(" ALTER COLUMN ");
            writer.Write(PgDiffUtils.getQuotedName(newValue.getColumnName()));
            writer.Write(" SET DEFAULT ");
            writer.Write(newValue.getDefaultValue());
            writer.WriteLine(';');
        }
    }

    
    private PgDiffViews() {
    }
}
}