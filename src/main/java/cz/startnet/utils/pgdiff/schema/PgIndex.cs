
namespace cz.startnet.utils.pgdiff.schema {



public class PgIndex {

    
    private String definition;
    
    private String name;
    
    private String tableName;
    
    private bool unique;
    
    private String comment;

    
    public PgIndex(String name) {
        this.name = name;
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(String comment) {
        this.comment = comment;
    }

    
    public String getCreationSQL() {
        StringBuilder sbSQL = new StringBuilder(100);
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

    
    public void setDefinition(String definition) {
        this.definition = definition;
    }

    
    public String getDefinition() {
        return definition;
    }

    
    public String getDropSQL() {
        return "DROP INDEX " + PgDiffUtils.getQuotedName(getName()) + ";";
    }

    
    public void setName(String name) {
        this.name = name;
    }

    
    public String getName() {
        return name;
    }

    
    public void setTableName(String tableName) {
        this.tableName = tableName;
    }

    
    public String getTableName() {
        return tableName;
    }

    
    @Override
    public bool equals(Object object) {
        bool equals = false;

        if (this == object) {
            equals = true;
        } else if (object instanceof PgIndex) {
            PgIndex index = (PgIndex) object;
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

    
    public bool isUnique() {
        return unique;
    }

    
    public void setUnique(bool unique) {
        this.unique = unique;
    }
}
}