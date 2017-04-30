using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pgdiff.schema {






public class PgTrigger {

    
    public String function { get; set; }

    public String name { get; set; }

    public String tableName { get; set; }

    public bool before { get; set; } = true;

    public bool forEachRow { get; set; }

    public bool onDelete { get; set; }

    public bool onInsert { get; set; }

    public bool onUpdate { get; set; }

    public bool onTruncate { get; set; }


    public List<String> updateColumns { get; set; } = new List<string>();

    public void addUpdateColumn(string item) => updateColumns.Add(item);

    public String when { get; set; }

    public String comment { get; set; }





        public String getCreationSQL() {
        StringBuilder sbSQL = new StringBuilder(100);
        sbSQL.Append("CREATE TRIGGER ");
        sbSQL.Append(PgDiffUtils.getQuotedName(name));
        sbSQL.Append("\n\t");
        sbSQL.Append(before ? "BEFORE" : "AFTER");

        bool firstEvent = true;

        if (onInsert) {
            sbSQL.Append(" INSERT");
            firstEvent = false;
        }

        if (onUpdate) {
            if (firstEvent) {
                firstEvent = false;
            } else {
                sbSQL.Append(" OR");
            }

            sbSQL.Append(" UPDATE");

            if (updateColumns.Count > 0) {
                sbSQL.Append(" OF");

                bool first = true;

                foreach(String columnName in updateColumns) {
                    if (first) {
                        first = false;
                    } else {
                        sbSQL.Append(',');
                    }

                    sbSQL.Append(' ');
                    sbSQL.Append(columnName);
                }
            }
        }

        if (onDelete) {
            if (!firstEvent) {
                sbSQL.Append(" OR");
            }

            sbSQL.Append(" DELETE");
        }

        if (onTruncate) {
            if (!firstEvent) {
                sbSQL.Append(" OR");
            }

            sbSQL.Append(" TRUNCATE");
        }

        sbSQL.Append(" ON ");
        sbSQL.Append(PgDiffUtils.getQuotedName(tableName));
        sbSQL.Append("\n\tFOR EACH ");
        sbSQL.Append(forEachRow ? "ROW" : "STATEMENT");

        if (!String.IsNullOrEmpty(when)) {
            sbSQL.Append("\n\tWHEN (");
            sbSQL.Append(when);
            sbSQL.Append(')');
        }

        sbSQL.Append("\n\tEXECUTE PROCEDURE ");
        sbSQL.Append(function);
        sbSQL.Append(';');

        if (!String.IsNullOrEmpty(comment)) {
            sbSQL.Append("\n\nCOMMENT ON TRIGGER ");
            sbSQL.Append(PgDiffUtils.getQuotedName(name));
            sbSQL.Append(" ON ");
            sbSQL.Append(PgDiffUtils.getQuotedName(tableName));
            sbSQL.Append(" IS ");
            sbSQL.Append(comment);
            sbSQL.Append(';');
        }

        return sbSQL.ToString();
    }

    
    public String getDropSQL() {
        return "DROP TRIGGER " + PgDiffUtils.getQuotedName(name) + " ON "
                + PgDiffUtils.getQuotedName(tableName) + ";";
    }

    
    

    
    

    public override bool Equals(Object @object) {
        bool equals = false;

        if (this == @object) {
            equals = true;
        } else if (@object is PgTrigger) {
            PgTrigger trigger = (PgTrigger) @object;
            equals = (before == trigger.before)
                    && (forEachRow == trigger.forEachRow)
                    && function.Equals(trigger.function)
                    && name.Equals(trigger.name)
                    && (onDelete == trigger.onDelete)
                    && (onInsert == trigger.onInsert)
                    && (onUpdate == trigger.onUpdate)
                    && (onTruncate == trigger.onTruncate)
                    && tableName.Equals(trigger.tableName);

            if (equals) {

                /*List<String> sorted1 = new List<string>(updateColumns);
                List<String> sorted2 = new List<String>(trigger.updateColumns);
                sorted1.Sort();
                sorted2.Sort();

                equals = sorted1.Equals(sorted2);*/
                if (updateColumns.Count == 0 && trigger.updateColumns.Count == 0)
                    equals = true;
                else
                    equals = updateColumns.All(
                        t1 => trigger.updateColumns.Any(
                            t2 => t1.Equals(t2, StringComparison.InvariantCultureIgnoreCase)));
            }
        }

        return equals;
    }

    public override int GetHashCode() {
        return (GetType().Name + "|" + before + "|" + forEachRow + "|"
                + function + "|" + name + "|" + onDelete + "|" + onInsert + "|"
                + onUpdate + "|" + onTruncate + "|" + tableName).GetHashCode();
    }
}
}