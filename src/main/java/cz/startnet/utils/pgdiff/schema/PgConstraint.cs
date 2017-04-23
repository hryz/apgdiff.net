
namespace cz.startnet.utils.pgdiff.schema {




public class PgConstraint {

    
    private static Pattern PATTERN_PRIMARY_KEY =
            Pattern.compile(".*PRIMARY[\\s]+KEY.*", Pattern.CASE_INSENSITIVE);
    
    private String definition;
    
    private String name;
    
    private String tableName;
    
    private String comment;

    
    public PgConstraint(String name) {
        this.name = name;
    }

    
    public String getCreationSQL() {
        StringBuilder sbSQL = new StringBuilder(100);
        sbSQL.append("ALTER TABLE ");
        sbSQL.append(PgDiffUtils.getQuotedName(getTableName()));
        sbSQL.append("\n\tADD CONSTRAINT ");
        sbSQL.append(PgDiffUtils.getQuotedName(getName()));
        sbSQL.append(' ');
        sbSQL.append(getDefinition());
        sbSQL.append(';');

        if (comment != null && !comment.isEmpty()) {
            sbSQL.append("\n\nCOMMENT ON CONSTRAINT ");
            sbSQL.append(PgDiffUtils.getQuotedName(name));
            sbSQL.append(" ON ");
            sbSQL.append(PgDiffUtils.getQuotedName(tableName));
            sbSQL.append(" IS ");
            sbSQL.append(comment);
            sbSQL.append(';');
        }

        return sbSQL.toString();
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(String comment) {
        this.comment = comment;
    }

    
    public void setDefinition(String definition) {
        this.definition = definition;
    }

    
    public String getDefinition() {
        return definition;
    }

    
    public String getDropSQL() {
        StringBuilder sbSQL = new StringBuilder(100);
        sbSQL.append("ALTER TABLE ");
        sbSQL.append(PgDiffUtils.getQuotedName(getTableName()));
        sbSQL.append("\n\tDROP CONSTRAINT ");
        sbSQL.append(PgDiffUtils.getQuotedName(getName()));
        sbSQL.append(';');

        return sbSQL.toString();
    }

    
    public void setName(String name) {
        this.name = name;
    }

    
    public String getName() {
        return name;
    }

    
    public bool isPrimaryKeyConstraint() {
        return PATTERN_PRIMARY_KEY.matcher(definition).matches();
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
        } else if (object instanceof PgConstraint) {
            PgConstraint constraint = (PgConstraint) object;
            equals = definition.equals(constraint.getDefinition())
                    && name.equals(constraint.getName())
                    && tableName.equals(constraint.getTableName());
        }

        return equals;
    }

    
    @Override
    public int hashCode() {
        return (getClass().getName() + "|" + definition + "|" + name + "|"
                + tableName).hashCode();
    }
}
}