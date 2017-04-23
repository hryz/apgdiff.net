
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace cz.startnet.utils.pgdiff.schema {




public class PgConstraint {

    
    private static Regex PATTERN_PRIMARY_KEY = new Regex(".*PRIMARY[\\s]+KEY.*", RegexOptions.Compiled|RegexOptions.IgnoreCase);
    
    private String definition;
    
    private String name;
    
    private String tableName;
    
    private String comment;

    
    public PgConstraint(String name) {
        this.name = name;
    }

    
    public String getCreationSQL() {
        StringBuilder sbSQL = new StringBuilder(100);
        sbSQL.Append("ALTER TABLE ");
        sbSQL.Append(PgDiffUtils.getQuotedName(getTableName()));
        sbSQL.Append("\n\tADD CONSTRAINT ");
        sbSQL.Append(PgDiffUtils.getQuotedName(getName()));
        sbSQL.Append(' ');
        sbSQL.Append(getDefinition());
        sbSQL.Append(';');

        if (comment != null && !String.IsNullOrEmpty(comment)) {
            sbSQL.Append("\n\nCOMMENT ON CONSTRAINT ");
            sbSQL.Append(PgDiffUtils.getQuotedName(name));
            sbSQL.Append(" ON ");
            sbSQL.Append(PgDiffUtils.getQuotedName(tableName));
            sbSQL.Append(" IS ");
            sbSQL.Append(comment);
            sbSQL.Append(';');
        }

        return sbSQL.ToString();
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
        sbSQL.Append("ALTER TABLE ");
        sbSQL.Append(PgDiffUtils.getQuotedName(getTableName()));
        sbSQL.Append("\n\tDROP CONSTRAINT ");
        sbSQL.Append(PgDiffUtils.getQuotedName(getName()));
        sbSQL.Append(';');

        return sbSQL.ToString();
    }

    
    public void setName(String name) {
        this.name = name;
    }

    
    public String getName() {
        return name;
    }

    
    public bool isPrimaryKeyConstraint() {
        return PATTERN_PRIMARY_KEY.IsMatch(definition);
    }

    
    public void setTableName(String tableName) {
        this.tableName = tableName;
    }

    
    public String getTableName() {
        return tableName;
    }

    
    public override bool Equals(Object @object) {
        bool equals = false;

        if (this == @object) {
            equals = true;
        } else if (@object is PgConstraint) {
            PgConstraint constraint = (PgConstraint) @object;
            equals = definition.Equals(constraint.getDefinition())
                    && name.Equals(constraint.getName())
                    && tableName.Equals(constraint.getTableName());
        }

        return equals;
    }

    
    
    public override int GetHashCode() {
        return (GetType().Name + "|" + definition + "|" + name + "|" + tableName).GetHashCode();
    }
}
}