using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using pgdiff.schema;

namespace pgdiff {







public class PgDiffViews {

    
    public static void CreateViews(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        foreach (PgView newView in newSchema.GetViews()) {
            if (oldSchema == null
                    || !oldSchema.ContainsView(newView.GetName())
                    || IsViewModified(
                    oldSchema.GetView(newView.GetName()), newView)) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(newView.GetCreationSql());
            }
        }
    }

    
    public static void DropViews(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        foreach (PgView oldView in oldSchema.GetViews()) {
            PgView newView = newSchema.GetView(oldView.GetName());

            if (newView == null || IsViewModified(oldView, newView)) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.WriteLine(oldView.GetDropSql());
            }
        }
    }

    
    private static bool IsViewModified(PgView oldView,
            PgView newView) {
        String[] oldViewColumnNames;

        if (oldView.GetColumnNames() == null|| oldView.GetColumnNames().Count == 0) {
            oldViewColumnNames = null;
        } else {
            oldViewColumnNames = oldView.GetColumnNames().ToArray();
        }

        String[] newViewColumnNames;

        if (newView.GetColumnNames() == null || newView.GetColumnNames().Count == 0) {
            newViewColumnNames = null;
        } else {
            newViewColumnNames = newView.GetColumnNames().ToArray();
        }

        if (oldViewColumnNames == null && newViewColumnNames == null) {
            return !oldView.GetQuery().Trim().Equals(newView.GetQuery().Trim());
        }
        else
        {
            return !Enumerable.SequenceEqual(oldViewColumnNames, newViewColumnNames);
        }
    }

    
    public static void AlterViews(TextWriter writer,
            PgSchema oldSchema, PgSchema newSchema,
            SearchPathHelper searchPathHelper) {
        if (oldSchema == null) {
            return;
        }

        foreach (PgView oldView in oldSchema.GetViews()) {
            PgView newView = newSchema.GetView(oldView.GetName());

            if (newView == null) {
                continue;
            }

            DiffDefaultValues(writer, oldView, newView, searchPathHelper);

            if (oldView.GetComment() == null
                    && newView.GetComment() != null
                    || oldView.GetComment() != null
                    && newView.GetComment() != null
                    && !oldView.GetComment().Equals(
                    newView.GetComment())) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON VIEW ");
                writer.Write(
                        PgDiffUtils.GetQuotedName(newView.GetName()));
                writer.Write(" IS ");
                writer.Write(newView.GetComment());
                writer.WriteLine(';');
            } else if (oldView.GetComment() != null
                    && newView.GetComment() == null) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("COMMENT ON VIEW ");
                writer.Write(PgDiffUtils.GetQuotedName(newView.GetName()));
                writer.WriteLine(" IS NULL;");
            }

            List<String> columnNames = new List<string>();

            foreach (PgView.ColumnComment columnComment in newView.GetColumnComments()) {
                columnNames.Add(columnComment.GetColumnName());
            }

            foreach (PgView.ColumnComment columnComment in oldView.GetColumnComments()) {
                if (!columnNames.Contains(columnComment.GetColumnName())) {
                    columnNames.Add(columnComment.GetColumnName());
                }
            }

            foreach (String columnName in columnNames) {
                PgView.ColumnComment oldColumnComment = null;
                PgView.ColumnComment newColumnComment = null;

                foreach (PgView.ColumnComment columnComment in oldView.GetColumnComments()) {
                    if (columnName.Equals(columnComment.GetColumnName())) {
                        oldColumnComment = columnComment;
                        break;
                    }
                }

                foreach (PgView.ColumnComment columnComment in newView.GetColumnComments()) {
                    if (columnName.Equals(columnComment.GetColumnName())) {
                        newColumnComment = columnComment;
                        break;
                    }
                }

                if (oldColumnComment == null && newColumnComment != null
                        || oldColumnComment != null && newColumnComment != null
                        && !oldColumnComment.GetComment().Equals(
                        newColumnComment.GetComment())) {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON COLUMN ");
                    writer.Write(PgDiffUtils.GetQuotedName(newView.GetName()));
                    writer.Write('.');
                    writer.Write(PgDiffUtils.GetQuotedName(
                            newColumnComment.GetColumnName()));
                    writer.Write(" IS ");
                    writer.Write(newColumnComment.GetComment());
                    writer.WriteLine(';');
                } else if (oldColumnComment != null
                        && newColumnComment == null) {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON COLUMN ");
                    writer.Write(PgDiffUtils.GetQuotedName(newView.GetName()));
                    writer.Write('.');
                    writer.Write(PgDiffUtils.GetQuotedName(
                            oldColumnComment.GetColumnName()));
                    writer.WriteLine(" IS NULL;");
                }
            }
        }
    }

    
    private static void DiffDefaultValues(TextWriter writer,
            PgView oldView, PgView newView,
            SearchPathHelper searchPathHelper) {
        List<PgView.DefaultValue> oldValues =
                oldView.GetDefaultValues();
        List<PgView.DefaultValue> newValues =
                newView.GetDefaultValues();

        // modify defaults that are in old view
        foreach(PgView.DefaultValue oldValue in oldValues) {
            bool found = false;

            foreach(PgView.DefaultValue newValue in newValues) {
                if (oldValue.GetColumnName().Equals(newValue.GetColumnName())) {
                    found = true;

                    if (!oldValue.GetDefaultValue().Equals(
                            newValue.GetDefaultValue())) {
                        searchPathHelper.OutputSearchPath(writer);
                        writer.WriteLine();
                        writer.Write("ALTER TABLE ");
                        writer.Write(
                                PgDiffUtils.GetQuotedName(newView.GetName()));
                        writer.Write(" ALTER COLUMN ");
                        writer.Write(PgDiffUtils.GetQuotedName(
                                newValue.GetColumnName()));
                        writer.Write(" SET DEFAULT ");
                        writer.Write(newValue.GetDefaultValue());
                        writer.WriteLine(';');
                    }

                    break;
                }
            }

            if (!found) {
                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("ALTER TABLE ");
                writer.Write(PgDiffUtils.GetQuotedName(newView.GetName()));
                writer.Write(" ALTER COLUMN ");
                writer.Write(PgDiffUtils.GetQuotedName(
                        oldValue.GetColumnName()));
                writer.WriteLine(" DROP DEFAULT;");
            }
        }

        // add new defaults
        foreach(PgView.DefaultValue newValue in newValues) {
            bool found = false;

            foreach(PgView.DefaultValue oldValue in oldValues) {
                if (newValue.GetColumnName().Equals(oldValue.GetColumnName())) {
                    found = true;
                    break;
                }
            }

            if (found) {
                continue;
            }

            searchPathHelper.OutputSearchPath(writer);
            writer.WriteLine();
            writer.Write("ALTER TABLE ");
            writer.Write(PgDiffUtils.GetQuotedName(newView.GetName()));
            writer.Write(" ALTER COLUMN ");
            writer.Write(PgDiffUtils.GetQuotedName(newValue.GetColumnName()));
            writer.Write(" SET DEFAULT ");
            writer.Write(newValue.GetDefaultValue());
            writer.WriteLine(';');
        }
    }

    
    private PgDiffViews() {
    }
}
}