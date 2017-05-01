using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pgdiff.schema
{
    public class PgTrigger
    {
        public string Function { get; set; }

        public string Name { get; set; }

        public string TableName { get; set; }

        public bool Before { get; set; } = true;

        public bool ForEachRow { get; set; }

        public bool OnDelete { get; set; }

        public bool OnInsert { get; set; }

        public bool OnUpdate { get; set; }

        public bool OnTruncate { get; set; }

        public List<string> UpdateColumns { get; set; } = new List<string>();

        public string When { get; set; }

        public string Comment { get; set; }

        public void AddUpdateColumn(string item)
        {
            UpdateColumns.Add(item);
        }


        public string GetCreationSql()
        {
            var sbSql = new StringBuilder(100);
            sbSql.Append("CREATE TRIGGER ");
            sbSql.Append(PgDiffUtils.GetQuotedName(Name));
            sbSql.Append("\n\t");
            sbSql.Append(Before ? "BEFORE" : "AFTER");

            var firstEvent = true;

            if (OnInsert)
            {
                sbSql.Append(" INSERT");
                firstEvent = false;
            }

            if (OnUpdate)
            {
                if (firstEvent)
                    firstEvent = false;
                else
                    sbSql.Append(" OR");

                sbSql.Append(" UPDATE");

                if (UpdateColumns.Count > 0)
                {
                    sbSql.Append(" OF");

                    var first = true;

                    foreach (var columnName in UpdateColumns)
                    {
                        if (first)
                            first = false;
                        else
                            sbSql.Append(',');

                        sbSql.Append(' ');
                        sbSql.Append(columnName);
                    }
                }
            }

            if (OnDelete)
            {
                if (!firstEvent)
                    sbSql.Append(" OR");

                sbSql.Append(" DELETE");
            }

            if (OnTruncate)
            {
                if (!firstEvent)
                    sbSql.Append(" OR");

                sbSql.Append(" TRUNCATE");
            }

            sbSql.Append(" ON ");
            sbSql.Append(PgDiffUtils.GetQuotedName(TableName));
            sbSql.Append("\n\tFOR EACH ");
            sbSql.Append(ForEachRow ? "ROW" : "STATEMENT");

            if (!string.IsNullOrEmpty(When))
            {
                sbSql.Append("\n\tWHEN (");
                sbSql.Append(When);
                sbSql.Append(')');
            }

            sbSql.Append("\n\tEXECUTE PROCEDURE ");
            sbSql.Append(Function);
            sbSql.Append(';');

            if (!string.IsNullOrEmpty(Comment))
            {
                sbSql.Append("\n\nCOMMENT ON TRIGGER ");
                sbSql.Append(PgDiffUtils.GetQuotedName(Name));
                sbSql.Append(" ON ");
                sbSql.Append(PgDiffUtils.GetQuotedName(TableName));
                sbSql.Append(" IS ");
                sbSql.Append(Comment);
                sbSql.Append(';');
            }

            return sbSql.ToString();
        }


        public string GetDropSql()
        {
            return $"DROP TRIGGER {PgDiffUtils.GetQuotedName(Name)} ON {PgDiffUtils.GetQuotedName(TableName)};";
        }


        public override bool Equals(object obj)
        {
            var equals = false;

            if (this == obj)
            {
                equals = true;
            }
            else if (obj is PgTrigger trigger)
            {
                equals = Before == trigger.Before
                         && ForEachRow == trigger.ForEachRow
                         && Function.Equals(trigger.Function)
                         && Name.Equals(trigger.Name)
                         && OnDelete == trigger.OnDelete
                         && OnInsert == trigger.OnInsert
                         && OnUpdate == trigger.OnUpdate
                         && OnTruncate == trigger.OnTruncate
                         && TableName.Equals(trigger.TableName);

                if (equals)
                    if (UpdateColumns.Count == 0 && trigger.UpdateColumns.Count == 0)
                        equals = true;
                    else
                        equals = UpdateColumns.All(
                            t1 => trigger.UpdateColumns.Any(
                                t2 => t1.Equals(t2, StringComparison.InvariantCultureIgnoreCase)));
            }

            return equals;
        }

        public override int GetHashCode()
        {
            return (GetType().Name + "|" + Before + "|" + ForEachRow + "|"
                    + Function + "|" + Name + "|" + OnDelete + "|" + OnInsert + "|"
                    + OnUpdate + "|" + OnTruncate + "|" + TableName).GetHashCode();
        }
    }
}