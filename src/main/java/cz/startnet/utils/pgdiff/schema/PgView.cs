using System.Collections.Generic;
using System.Text;

namespace pgdiff.schema
{
    public class PgView
    {
        public readonly List<DefaultValue> DefaultValues = new List<DefaultValue>();

        public string Name { get; set; }

        public List<ColumnComment> ColumnComments = new List<ColumnComment>();

        public List<string> ColumnNames;

        public string Comment;

        public string Query;


        public PgView(string name)
        {
            Name = name;
        }

        public string GetCreationSql()
        {
            var sbSql = new StringBuilder(Query.Length * 2);
            sbSql.Append("CREATE VIEW ");
            sbSql.Append(PgDiffUtils.GetQuotedName(Name));

            if (ColumnNames != null && ColumnNames.Count > 0)
            {
                sbSql.Append(" (");

                for (var i = 0; i < ColumnNames.Count; i++)
                {
                    if (i > 0)
                        sbSql.Append(", ");

                    sbSql.Append(PgDiffUtils.GetQuotedName(ColumnNames[i]));
                }
                sbSql.Append(')');
            }

            sbSql.Append(" AS\n\t");
            sbSql.Append(Query);
            sbSql.Append(';');

            foreach (var defaultValue in DefaultValues)
            {
                sbSql.Append("\n\nALTER VIEW ");
                sbSql.Append(PgDiffUtils.GetQuotedName(Name));
                sbSql.Append(" ALTER COLUMN ");
                sbSql.Append(
                    PgDiffUtils.GetQuotedName(defaultValue.ColumnName));
                sbSql.Append(" SET DEFAULT ");
                sbSql.Append(defaultValue._DefaultValue);
                sbSql.Append(';');
            }

            if (!string.IsNullOrEmpty(Comment))
            {
                sbSql.Append("\n\nCOMMENT ON VIEW ");
                sbSql.Append(PgDiffUtils.GetQuotedName(Name));
                sbSql.Append(" IS ");
                sbSql.Append(Comment);
                sbSql.Append(';');
            }

            foreach (var columnComment in ColumnComments)
                if (!string.IsNullOrEmpty(columnComment.Comment))
                {
                    sbSql.Append("\n\nCOMMENT ON COLUMN ");
                    sbSql.Append(PgDiffUtils.GetQuotedName(columnComment.ColumnName));
                    sbSql.Append(" IS ");
                    sbSql.Append(columnComment.Comment);
                    sbSql.Append(';');
                }

            return sbSql.ToString();
        }


        public string GetDropSql()
        {
            return "DROP VIEW " + PgDiffUtils.GetQuotedName(Name) + ";";
        }

        public void AddColumnDefaultValue(string columnName,
            string defaultValue)
        {
            RemoveColumnDefaultValue(columnName);
            DefaultValues.Add(new DefaultValue(columnName, defaultValue));
        }


        public void RemoveColumnDefaultValue(string columnName)
        {
            foreach (var item in DefaultValues)
                if (item.ColumnName.Equals(columnName))
                {
                    DefaultValues.Remove(item);
                    return;
                }
        }

        public void AddColumnComment(string columnName, string comment)
        {
            RemoveColumnDefaultValue(columnName);
            ColumnComments.Add(new ColumnComment(columnName, comment));
        }


        public void RemoveColumnComment(string columnName)
        {
            foreach (var item in ColumnComments)
                if (item.ColumnName.Equals(columnName))
                {
                    ColumnComments.Remove(item);
                    return;
                }
        }


        public class DefaultValue
        {
            public DefaultValue(string columnName, string defaultValue)
            {
                ColumnName = columnName;
                _DefaultValue = defaultValue;
            }

            public string ColumnName { get; }

            public string _DefaultValue { get; }
        }


        public class ColumnComment
        {
            public ColumnComment(string columnName, string comment)
            {
                ColumnName = columnName;
                Comment = comment;
            }

            public string ColumnName { get; }

            public string Comment { get; }
        }
    }
}