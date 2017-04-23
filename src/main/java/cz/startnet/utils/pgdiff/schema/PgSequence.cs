using System;
using System.Text;

namespace pgdiff.schema {



public class PgSequence {

    
    private String cache;
    
    private String increment;
    
    private String maxValue;
    
    private String minValue;
    
    private String name;
    
    private String startWith;
    
    private bool cycle;
    
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
        sbSQL.Append("CREATE SEQUENCE ");
        sbSQL.Append(PgDiffUtils.getQuotedName(name));

        if (startWith != null) {
            sbSQL.Append("\n\tSTART WITH ");
            sbSQL.Append(startWith);
        }

        if (increment != null) {
            sbSQL.Append("\n\tINCREMENT BY ");
            sbSQL.Append(increment);
        }

        sbSQL.Append("\n\t");

        if (maxValue == null) {
            sbSQL.Append("NO MAXVALUE");
        } else {
            sbSQL.Append("MAXVALUE ");
            sbSQL.Append(maxValue);
        }

        sbSQL.Append("\n\t");

        if (minValue == null) {
            sbSQL.Append("NO MINVALUE");
        } else {
            sbSQL.Append("MINVALUE ");
            sbSQL.Append(minValue);
        }

        if (cache != null) {
            sbSQL.Append("\n\tCACHE ");
            sbSQL.Append(cache);
        }

        if (cycle) {
            sbSQL.Append("\n\tCYCLE");
        }

        sbSQL.Append(';');

        if ( !String.IsNullOrEmpty(comment)) {
            sbSQL.Append("\n\nCOMMENT ON SEQUENCE ");
            sbSQL.Append(PgDiffUtils.getQuotedName(name));
            sbSQL.Append(" IS ");
            sbSQL.Append(comment);
            sbSQL.Append(';');
        }

        return sbSQL.ToString();
    }

    
    public String getOwnedBySQL() {
        StringBuilder sbSQL = new StringBuilder(100);

        sbSQL.Append("ALTER SEQUENCE ");
        sbSQL.Append(PgDiffUtils.getQuotedName(name));

        if ( !String.IsNullOrEmpty(ownedBy)) {
            sbSQL.Append("\n\tOWNED BY ");
            sbSQL.Append(ownedBy);
        }

        sbSQL.Append(';');

        return sbSQL.ToString();
    }

    
    public void setCycle(bool cycle) {
        this.cycle = cycle;
    }

    
    public bool isCycle() {
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