using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pgdiff.schema {






public class PgTrigger {

    
    public String Function { get; set; }

    public String Name { get; set; }

    public String TableName { get; set; }

    public bool Before { get; set; } = true;

    public bool ForEachRow { get; set; }

    public bool OnDelete { get; set; }

    public bool OnInsert { get; set; }

    public bool OnUpdate { get; set; }

    public bool OnTruncate { get; set; }


    public List<String> UpdateColumns { get; set; } = new List<string>();

    public void AddUpdateColumn(string item) => UpdateColumns.Add(item);

    public String When { get; set; }

    public String Comment { get; set; }





        public String GetCreationSql() {
        StringBuilder sbSql = new StringBuilder(100);
        sbSql.Append("CREATE TRIGGER ");
        sbSql.Append(PgDiffUtils.GetQuotedName(Name));
        sbSql.Append("\n\t");
        sbSql.Append(Before ? "BEFORE" : "AFTER");

        bool firstEvent = true;

        if (OnInsert) {
            sbSql.Append(" INSERT");
            firstEvent = false;
        }

        if (OnUpdate) {
            if (firstEvent) {
                firstEvent = false;
            } else {
                sbSql.Append(" OR");
            }

            sbSql.Append(" UPDATE");

            if (UpdateColumns.Count > 0) {
                sbSql.Append(" OF");

                bool first = true;

                foreach(String columnName in UpdateColumns) {
                    if (first) {
                        first = false;
                    } else {
                        sbSql.Append(',');
                    }

                    sbSql.Append(' ');
                    sbSql.Append(columnName);
                }
            }
        }

        if (OnDelete) {
            if (!firstEvent) {
                sbSql.Append(" OR");
            }

            sbSql.Append(" DELETE");
        }

        if (OnTruncate) {
            if (!firstEvent) {
                sbSql.Append(" OR");
            }

            sbSql.Append(" TRUNCATE");
        }

        sbSql.Append(" ON ");
        sbSql.Append(PgDiffUtils.GetQuotedName(TableName));
        sbSql.Append("\n\tFOR EACH ");
        sbSql.Append(ForEachRow ? "ROW" : "STATEMENT");

        if (!String.IsNullOrEmpty(When)) {
            sbSql.Append("\n\tWHEN (");
            sbSql.Append(When);
            sbSql.Append(')');
        }

        sbSql.Append("\n\tEXECUTE PROCEDURE ");
        sbSql.Append(Function);
        sbSql.Append(';');

        if (!String.IsNullOrEmpty(Comment)) {
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

    
    public String GetDropSql() {
        return "DROP TRIGGER " + PgDiffUtils.GetQuotedName(Name) + " ON "
                + PgDiffUtils.GetQuotedName(TableName) + ";";
    }

    
    

    
    

    public override bool Equals(Object @object) {
        bool equals = false;

        if (this == @object) {
            equals = true;
        } else if (@object is PgTrigger) {
            PgTrigger trigger = (PgTrigger) @object;
            equals = (Before == trigger.Before)
                    && (ForEachRow == trigger.ForEachRow)
                    && Function.Equals(trigger.Function)
                    && Name.Equals(trigger.Name)
                    && (OnDelete == trigger.OnDelete)
                    && (OnInsert == trigger.OnInsert)
                    && (OnUpdate == trigger.OnUpdate)
                    && (OnTruncate == trigger.OnTruncate)
                    && TableName.Equals(trigger.TableName);

            if (equals) {

                /*List<String> sorted1 = new List<string>(updateColumns);
                List<String> sorted2 = new List<String>(trigger.updateColumns);
                sorted1.Sort();
                sorted2.Sort();

                equals = sorted1.Equals(sorted2);*/
                if (UpdateColumns.Count == 0 && trigger.UpdateColumns.Count == 0)
                    equals = true;
                else
                    equals = UpdateColumns.All(
                        t1 => trigger.UpdateColumns.Any(
                            t2 => t1.Equals(t2, StringComparison.InvariantCultureIgnoreCase)));
            }
        }

        return equals;
    }

    public override int GetHashCode() {
        return (GetType().Name + "|" + Before + "|" + ForEachRow + "|"
                + Function + "|" + Name + "|" + OnDelete + "|" + OnInsert + "|"
                + OnUpdate + "|" + OnTruncate + "|" + TableName).GetHashCode();
    }
}
}