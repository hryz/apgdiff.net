
namespace cz.startnet.utils.pgdiff.schema {

import cz.startnet.utils.pgdiff.PgDiffUtils;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;


public class PgTrigger {

    
    private String function;
    
    private String name;
    
    private String tableName;
    
    private boolean before = true;
    
    private boolean forEachRow;
    
    private boolean onDelete;
    
    private boolean onInsert;
    
    private boolean onUpdate;
    
    private boolean onTruncate;
    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private List<String> updateColumns = new ArrayList<String>();
    
    private String when;
    
    private String comment;

    
    public void setBefore(boolean before) {
        this.before = before;
    }

    
    public boolean isBefore() {
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
        sbSQL.append("CREATE TRIGGER ");
        sbSQL.append(PgDiffUtils.getQuotedName(getName()));
        sbSQL.append("\n\t");
        sbSQL.append(isBefore() ? "BEFORE" : "AFTER");

        boolean firstEvent = true;

        if (isOnInsert()) {
            sbSQL.append(" INSERT");
            firstEvent = false;
        }

        if (isOnUpdate()) {
            if (firstEvent) {
                firstEvent = false;
            } else {
                sbSQL.append(" OR");
            }

            sbSQL.append(" UPDATE");

            if (!updateColumns.isEmpty()) {
                sbSQL.append(" OF");

                boolean first = true;

                for (String columnName : updateColumns) {
                    if (first) {
                        first = false;
                    } else {
                        sbSQL.append(',');
                    }

                    sbSQL.append(' ');
                    sbSQL.append(columnName);
                }
            }
        }

        if (isOnDelete()) {
            if (!firstEvent) {
                sbSQL.append(" OR");
            }

            sbSQL.append(" DELETE");
        }

        if (isOnTruncate()) {
            if (!firstEvent) {
                sbSQL.append(" OR");
            }

            sbSQL.append(" TRUNCATE");
        }

        sbSQL.append(" ON ");
        sbSQL.append(PgDiffUtils.getQuotedName(getTableName()));
        sbSQL.append("\n\tFOR EACH ");
        sbSQL.append(isForEachRow() ? "ROW" : "STATEMENT");

        if (when != null && !when.isEmpty()) {
            sbSQL.append("\n\tWHEN (");
            sbSQL.append(when);
            sbSQL.append(')');
        }

        sbSQL.append("\n\tEXECUTE PROCEDURE ");
        sbSQL.append(getFunction());
        sbSQL.append(';');

        if (comment != null && !comment.isEmpty()) {
            sbSQL.append("\n\nCOMMENT ON TRIGGER ");
            sbSQL.append(PgDiffUtils.getQuotedName(name));
            sbSQL.append(" ON ");
            sbSQL.append(PgDiffUtils.getQuotedName(tableName));
            sbSQL.append(" IS ");
            sbSQL.append(comment);
            sbSQL.append(';');
        }

        return sbSQL.toString();
    }

    
    public String getDropSQL() {
        return "DROP TRIGGER " + PgDiffUtils.getQuotedName(getName()) + " ON "
                + PgDiffUtils.getQuotedName(getTableName()) + ";";
    }

    
    public void setForEachRow(boolean forEachRow) {
        this.forEachRow = forEachRow;
    }

    
    public boolean isForEachRow() {
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

    
    public void setOnDelete(boolean onDelete) {
        this.onDelete = onDelete;
    }

    
    public boolean isOnDelete() {
        return onDelete;
    }

    
    public void setOnInsert(boolean onInsert) {
        this.onInsert = onInsert;
    }

    
    public boolean isOnInsert() {
        return onInsert;
    }

    
    public void setOnUpdate(boolean onUpdate) {
        this.onUpdate = onUpdate;
    }

    
    public boolean isOnUpdate() {
        return onUpdate;
    }

    
    public boolean isOnTruncate() {
        return onTruncate;
    }

    
    public void setOnTruncate(boolean onTruncate) {
        this.onTruncate = onTruncate;
    }

    
    public void setTableName(String tableName) {
        this.tableName = tableName;
    }

    
    public String getTableName() {
        return tableName;
    }

    
    public List<String> getUpdateColumns() {
        return Collections.unmodifiableList(updateColumns);
    }

    
    public void addUpdateColumn(String columnName) {
        updateColumns.add(columnName);
    }

    
    public String getWhen() {
        return when;
    }

    
    public void setWhen(String when) {
        this.when = when;
    }

    @Override
    public boolean equals(Object object) {
        boolean equals = false;

        if (this == object) {
            equals = true;
        } else if (object instanceof PgTrigger) {
            PgTrigger trigger = (PgTrigger) object;
            equals = (before == trigger.isBefore())
                    && (forEachRow == trigger.isForEachRow())
                    && function.equals(trigger.getFunction())
                    && name.equals(trigger.getName())
                    && (onDelete == trigger.isOnDelete())
                    && (onInsert == trigger.isOnInsert())
                    && (onUpdate == trigger.isOnUpdate())
                    && (onTruncate == trigger.isOnTruncate())
                    && tableName.equals(trigger.getTableName());

            if (equals) {
                List<String> sorted1 =
                        new ArrayList<String>(updateColumns);
                List<String> sorted2 =
                        new ArrayList<String>(trigger.getUpdateColumns());
                Collections.sort(sorted1);
                Collections.sort(sorted2);

                equals = sorted1.equals(sorted2);
            }
        }

        return equals;
    }

    @Override
    public int hashCode() {
        return (getClass().getName() + "|" + before + "|" + forEachRow + "|"
                + function + "|" + name + "|" + onDelete + "|" + onInsert + "|"
                + onUpdate + "|" + onTruncate + "|" + tableName).hashCode();
    }
}
}