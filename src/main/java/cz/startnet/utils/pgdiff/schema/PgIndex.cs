
package cz.startnet.utils.pgdiff.schema;

import cz.startnet.utils.pgdiff.PgDiffUtils;


public class PgIndex {

    
    private String definition;
    
    private String name;
    
    private String tableName;
    
    private boolean unique;
    
    private String comment;

    
    public PgIndex(final String name) {
        this.name = name;
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(final String comment) {
        this.comment = comment;
    }

    
    public String getCreationSQL() {
        final StringBuilder sbSQL = new StringBuilder(100);
        sbSQL.append("CREATE ");

        if (isUnique()) {
            sbSQL.append("UNIQUE ");
        }

        sbSQL.append("INDEX ");
        sbSQL.append(PgDiffUtils.getQuotedName(getName()));
        sbSQL.append(" ON ");
        sbSQL.append(PgDiffUtils.getQuotedName(getTableName()));
        sbSQL.append(' ');
        sbSQL.append(getDefinition());
        sbSQL.append(';');

        if (comment != null && !comment.isEmpty()) {
            sbSQL.append("\n\nCOMMENT ON INDEX ");
            sbSQL.append(PgDiffUtils.getQuotedName(name));
            sbSQL.append(" IS ");
            sbSQL.append(comment);
            sbSQL.append(';');
        }

        return sbSQL.toString();
    }

    
    public void setDefinition(final String definition) {
        this.definition = definition;
    }

    
    public String getDefinition() {
        return definition;
    }

    
    public String getDropSQL() {
        return "DROP INDEX " + PgDiffUtils.getQuotedName(getName()) + ";";
    }

    
    public void setName(final String name) {
        this.name = name;
    }

    
    public String getName() {
        return name;
    }

    
    public void setTableName(final String tableName) {
        this.tableName = tableName;
    }

    
    public String getTableName() {
        return tableName;
    }

    
    @Override
    public boolean equals(final Object object) {
        boolean equals = false;

        if (this == object) {
            equals = true;
        } else if (object instanceof PgIndex) {
            final PgIndex index = (PgIndex) object;
            equals = definition.equals(index.getDefinition())
                    && name.equals(index.getName())
                    && tableName.equals(index.getTableName())
                    && unique == index.isUnique();
        }

        return equals;
    }

    
    @Override
    public int hashCode() {
        return (getClass().getName() + "|" + definition + "|" + name + "|"
                + tableName + "|" + unique).hashCode();
    }

    
    public boolean isUnique() {
        return unique;
    }

    
    public void setUnique(final boolean unique) {
        this.unique = unique;
    }
}
