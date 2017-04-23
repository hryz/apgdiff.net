
using System;
using System.Collections.Generic;
using System.Text;

namespace cz.startnet.utils.pgdiff.schema {






public class PgTrigger {

    
    private String function;
    
    private String name;
    
    private String tableName;
    
    private bool before = true;
    
    private bool forEachRow;
    
    private bool onDelete;
    
    private bool onInsert;
    
    private bool onUpdate;
    
    private bool onTruncate;
    
    
    private List<String> updateColumns = new List<string>();
    
    private String when;
    
    private String comment;

    
    public void setBefore(bool before) {
        this.before = before;
    }

    
    public bool isBefore() {
        return before;
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(String comment) {
        this.comment = comment;
    }

    
    public String getCreationSQL() {
        StringBuilder sbSQL = new StringBuilder(100);
        sbSQL.Append("CREATE TRIGGER ");
        sbSQL.Append(PgDiffUtils.getQuotedName(getName()));
        sbSQL.Append("\n\t");
        sbSQL.Append(isBefore() ? "BEFORE" : "AFTER");

        bool firstEvent = true;

        if (isOnInsert()) {
            sbSQL.Append(" INSERT");
            firstEvent = false;
        }

        if (isOnUpdate()) {
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

        if (isOnDelete()) {
            if (!firstEvent) {
                sbSQL.Append(" OR");
            }

            sbSQL.Append(" DELETE");
        }

        if (isOnTruncate()) {
            if (!firstEvent) {
                sbSQL.Append(" OR");
            }

            sbSQL.Append(" TRUNCATE");
        }

        sbSQL.Append(" ON ");
        sbSQL.Append(PgDiffUtils.getQuotedName(getTableName()));
        sbSQL.Append("\n\tFOR EACH ");
        sbSQL.Append(isForEachRow() ? "ROW" : "STATEMENT");

        if (!String.IsNullOrEmpty(when)) {
            sbSQL.Append("\n\tWHEN (");
            sbSQL.Append(when);
            sbSQL.Append(')');
        }

        sbSQL.Append("\n\tEXECUTE PROCEDURE ");
        sbSQL.Append(getFunction());
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
        return "DROP TRIGGER " + PgDiffUtils.getQuotedName(getName()) + " ON "
                + PgDiffUtils.getQuotedName(getTableName()) + ";";
    }

    
    public void setForEachRow(bool forEachRow) {
        this.forEachRow = forEachRow;
    }

    
    public bool isForEachRow() {
        return forEachRow;
    }

    
    public void setFunction(String function) {
        this.function = function;
    }

    
    public String getFunction() {
        return function;
    }

    
    public void setName(String name) {
        this.name = name;
    }

    
    public String getName() {
        return name;
    }

    
    public void setOnDelete(bool onDelete) {
        this.onDelete = onDelete;
    }

    
    public bool isOnDelete() {
        return onDelete;
    }

    
    public void setOnInsert(bool onInsert) {
        this.onInsert = onInsert;
    }

    
    public bool isOnInsert() {
        return onInsert;
    }

    
    public void setOnUpdate(bool onUpdate) {
        this.onUpdate = onUpdate;
    }

    
    public bool isOnUpdate() {
        return onUpdate;
    }

    
    public bool isOnTruncate() {
        return onTruncate;
    }

    
    public void setOnTruncate(bool onTruncate) {
        this.onTruncate = onTruncate;
    }

    
    public void setTableName(String tableName) {
        this.tableName = tableName;
    }

    
    public String getTableName() {
        return tableName;
    }

    
    public List<String> getUpdateColumns() {
        return new List<string>(updateColumns);
    }

    
    public void addUpdateColumn(String columnName) {
        updateColumns.Add(columnName);
    }

    
    public String getWhen() {
        return when;
    }

    
    public void setWhen(String when) {
        this.when = when;
    }

    public override bool Equals(Object @object) {
        bool equals = false;

        if (this == @object) {
            equals = true;
        } else if (@object is PgTrigger) {
            PgTrigger trigger = (PgTrigger) @object;
            equals = (before == trigger.isBefore())
                    && (forEachRow == trigger.isForEachRow())
                    && function.Equals(trigger.getFunction())
                    && name.Equals(trigger.getName())
                    && (onDelete == trigger.isOnDelete())
                    && (onInsert == trigger.isOnInsert())
                    && (onUpdate == trigger.isOnUpdate())
                    && (onTruncate == trigger.isOnTruncate())
                    && tableName.Equals(trigger.getTableName());

            if (equals) {
                List<String> sorted1 = new List<string>(updateColumns);
                List<String> sorted2 = new List<String>(trigger.getUpdateColumns());
                sorted1.Sort();
                sorted2.Sort();

                equals = sorted1.Equals(sorted2);
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