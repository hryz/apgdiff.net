using System.Collections.Generic;
using System.IO;
using System.Linq;
using pgdiff.schema;

namespace pgdiff
{
    public class PgDiffViews
    {
        private PgDiffViews()
        {
        }


        public static void CreateViews(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            foreach (var newView in newSchema.GetViews())
                if (oldSchema == null
                    || !oldSchema.ContainsView(newView.Name)
                    || IsViewModified(
                        oldSchema.GetView(newView.Name), newView))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(newView.GetCreationSql());
                }
        }


        public static void DropViews(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            if (oldSchema == null)
                return;

            foreach (var oldView in oldSchema.GetViews())
            {
                var newView = newSchema.GetView(oldView.Name);

                if (newView == null || IsViewModified(oldView, newView))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.WriteLine(oldView.GetDropSql());
                }
            }
        }


        private static bool IsViewModified(PgView oldView, PgView newView)
        {
            string[] oldViewColumnNames;

            if (oldView.ColumnNames == null || oldView.ColumnNames.Count == 0)
                oldViewColumnNames = null;
            else
                oldViewColumnNames = oldView.ColumnNames.ToArray();

            string[] newViewColumnNames;

            if (newView.ColumnNames == null || newView.ColumnNames.Count == 0)
                newViewColumnNames = null;
            else
                newViewColumnNames = newView.ColumnNames.ToArray();

            if (oldViewColumnNames == null && newViewColumnNames == null)
                return !oldView.Query.Trim().Equals(newView.Query.Trim());
            return !oldViewColumnNames.SequenceEqual(newViewColumnNames);
        }


        public static void AlterViews(TextWriter writer, PgSchema oldSchema, PgSchema newSchema, SearchPathHelper searchPathHelper)
        {
            if (oldSchema == null)
                return;

            foreach (var oldView in oldSchema.GetViews())
            {
                var newView = newSchema.GetView(oldView.Name);
                if (newView == null)
                    continue;

                DiffDefaultValues(writer, oldView, newView, searchPathHelper);

                if (oldView.Comment == null
                    && newView.Comment != null
                    || oldView.Comment != null
                    && newView.Comment != null
                    && !oldView.Comment.Equals(
                        newView.Comment))
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON VIEW ");
                    writer.Write(PgDiffUtils.GetQuotedName(newView.Name));
                    writer.Write(" IS ");
                    writer.Write(newView.Comment);
                    writer.WriteLine(';');
                }
                else if (oldView.Comment != null && newView.Comment == null)
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("COMMENT ON VIEW ");
                    writer.Write(PgDiffUtils.GetQuotedName(newView.Name));
                    writer.WriteLine(" IS NULL;");
                }

                var columnNames = newView.ColumnComments.Select(c => c.ColumnName).ToList();

                foreach (var columnComment in oldView.ColumnComments)
                    if (!columnNames.Contains(columnComment.ColumnName))
                        columnNames.Add(columnComment.ColumnName);

                foreach (var columnName in columnNames)
                {
                    var oldColumnComment = oldView.ColumnComments.FirstOrDefault(cc => columnName.Equals(cc.ColumnName));
                    var newColumnComment = newView.ColumnComments.FirstOrDefault(cc => columnName.Equals(cc.ColumnName));

                    if (oldColumnComment == null && newColumnComment != null
                        || oldColumnComment != null && newColumnComment != null
                        && !oldColumnComment.Comment.Equals(
                            newColumnComment.Comment))
                    {
                        searchPathHelper.OutputSearchPath(writer);
                        writer.WriteLine();
                        writer.Write("COMMENT ON COLUMN ");
                        writer.Write(PgDiffUtils.GetQuotedName(newView.Name));
                        writer.Write('.');
                        writer.Write(PgDiffUtils.GetQuotedName(newColumnComment.ColumnName));
                        writer.Write(" IS ");
                        writer.Write(newColumnComment.Comment);
                        writer.WriteLine(';');
                    }
                    else if (oldColumnComment != null
                             && newColumnComment == null)
                    {
                        searchPathHelper.OutputSearchPath(writer);
                        writer.WriteLine();
                        writer.Write("COMMENT ON COLUMN ");
                        writer.Write(PgDiffUtils.GetQuotedName(newView.Name));
                        writer.Write('.');
                        writer.Write(PgDiffUtils.GetQuotedName(oldColumnComment.ColumnName));
                        writer.WriteLine(" IS NULL;");
                    }
                }
            }
        }


        private static void DiffDefaultValues(TextWriter writer, PgView oldView, PgView newView, SearchPathHelper searchPathHelper)
        {
            var oldValues = oldView.DefaultValues;
            var newValues = newView.DefaultValues;

            // modify defaults that are in old view
            foreach (var oldValue in oldValues)
            {
                var found = false;

                foreach (var newValue in newValues)
                    if (oldValue.ColumnName.Equals(newValue.ColumnName))
                    {
                        found = true;

                        if (!oldValue._DefaultValue.Equals(newValue._DefaultValue))
                        {
                            searchPathHelper.OutputSearchPath(writer);
                            writer.WriteLine();
                            writer.Write("ALTER TABLE ");
                            writer.Write(PgDiffUtils.GetQuotedName(newView.Name));
                            writer.Write(" ALTER COLUMN ");
                            writer.Write(PgDiffUtils.GetQuotedName(newValue.ColumnName));
                            writer.Write(" SET DEFAULT ");
                            writer.Write(newValue._DefaultValue);
                            writer.WriteLine(';');
                        }

                        break;
                    }

                if (!found)
                {
                    searchPathHelper.OutputSearchPath(writer);
                    writer.WriteLine();
                    writer.Write("ALTER TABLE ");
                    writer.Write(PgDiffUtils.GetQuotedName(newView.Name));
                    writer.Write(" ALTER COLUMN ");
                    writer.Write(PgDiffUtils.GetQuotedName(oldValue.ColumnName));
                    writer.WriteLine(" DROP DEFAULT;");
                }
            }

            // add new defaults
            foreach (var newValue in newValues)
            {
                var found = oldValues.Any(ov => newValue.ColumnName.Equals(ov.ColumnName));

                if (found)
                    continue;

                searchPathHelper.OutputSearchPath(writer);
                writer.WriteLine();
                writer.Write("ALTER TABLE ");
                writer.Write(PgDiffUtils.GetQuotedName(newView.Name));
                writer.Write(" ALTER COLUMN ");
                writer.Write(PgDiffUtils.GetQuotedName(newValue.ColumnName));
                writer.Write(" SET DEFAULT ");
                writer.Write(newValue._DefaultValue);
                writer.WriteLine(';');
            }
        }
    }
}