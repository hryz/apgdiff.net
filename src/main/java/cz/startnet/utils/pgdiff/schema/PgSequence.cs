
namespace cz.startnet.utils.pgdiff.schema {

import cz.startnet.utils.pgdiff.PgDiffUtils;


public class PgSequence {

    
    private String cache;
    
    private String increment;
    
    private String maxValue;
    
    private String minValue;
    
    private String name;
    
    private String startWith;
    
    private boolean cycle;
    
    private String ownedBy;
    
    private String comment;

    
    public PgSequence(String name) {
        this.name = name;
    }

    
    public void setCache(String cache) {
        this.cache = cache;
    }

    
    public String getCache() {
        return cache;
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(String comment) {
        this.comment = comment;
    }

    
    public String getCreationSQL() {
        StringBuilder sbSQL = new StringBuilder(100);
        sbSQL.append("CREATE SEQUENCE ");
        sbSQL.append(PgDiffUtils.getQuotedName(name));

        if (startWith != null) {
            sbSQL.append("\n\tSTART WITH ");
            sbSQL.append(startWith);
        }

        if (increment != null) {
            sbSQL.append("\n\tINCREMENT BY ");
            sbSQL.append(increment);
        }

        sbSQL.append("\n\t");

        if (maxValue == null) {
            sbSQL.append("NO MAXVALUE");
        } else {
            sbSQL.append("MAXVALUE ");
            sbSQL.append(maxValue);
        }

        sbSQL.append("\n\t");

        if (minValue == null) {
            sbSQL.append("NO MINVALUE");
        } else {
            sbSQL.append("MINVALUE ");
            sbSQL.append(minValue);
        }

        if (cache != null) {
            sbSQL.append("\n\tCACHE ");
            sbSQL.append(cache);
        }

        if (cycle) {
            sbSQL.append("\n\tCYCLE");
        }

        sbSQL.append(';');

        if (comment != null && !comment.isEmpty()) {
            sbSQL.append("\n\nCOMMENT ON SEQUENCE ");
            sbSQL.append(PgDiffUtils.getQuotedName(name));
            sbSQL.append(" IS ");
            sbSQL.append(comment);
            sbSQL.append(';');
        }

        return sbSQL.toString();
    }

    
    public String getOwnedBySQL() {
        StringBuilder sbSQL = new StringBuilder(100);

        sbSQL.append("ALTER SEQUENCE ");
        sbSQL.append(PgDiffUtils.getQuotedName(name));

        if (ownedBy != null && !ownedBy.isEmpty()) {
            sbSQL.append("\n\tOWNED BY ");
            sbSQL.append(ownedBy);
        }

        sbSQL.append(';');

        return sbSQL.toString();
    }

    
    public void setCycle(boolean cycle) {
        this.cycle = cycle;
    }

    
    public boolean isCycle() {
        return cycle;
    }

    
    public String getDropSQL() {
        return "DROP SEQUENCE " + PgDiffUtils.getQuotedName(getName()) + ";";
    }

    
    public void setIncrement(String increment) {
        this.increment = increment;
    }

    
    public String getIncrement() {
        return increment;
    }

    
    public void setMaxValue(String maxValue) {
        this.maxValue = maxValue;
    }

    
    public String getMaxValue() {
        return maxValue;
    }

    
    public void setMinValue(String minValue) {
        this.minValue = minValue;
    }

    
    public String getMinValue() {
        return minValue;
    }

    
    public void setName(String name) {
        this.name = name;
    }

    
    public String getName() {
        return name;
    }

    
    public void setStartWith(String startWith) {
        this.startWith = startWith;
    }

    
    public String getStartWith() {
        return startWith;
    }

    
    public String getOwnedBy() {
        return ownedBy;
    }

    
    public void setOwnedBy(String ownedBy) {
        this.ownedBy = ownedBy;
    }
}
}