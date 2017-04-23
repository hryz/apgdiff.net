
using System;
using System.Text;

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
        sbSQL.Append("CREATE ");

        if (isUnique()) {
            sbSQL.Append("UNIQUE ");
        }

        sbSQL.Append("INDEX ");
        sbSQL.Append(PgDiffUtils.getQuotedName(getName()));
        sbSQL.Append(" ON ");
        sbSQL.Append(PgDiffUtils.getQuotedName(getTableName()));
        sbSQL.Append(' ');
        sbSQL.Append(getDefinition());
        sbSQL.Append(';');

        if ( !String.IsNullOrEmpty(comment)) {
            sbSQL.Append("\n\nCOMMENT ON INDEX ");
            sbSQL.Append(PgDiffUtils.getQuotedName(name));
            sbSQL.Append(" IS ");
            sbSQL.Append(comment);
            sbSQL.Append(';');
        }

        return sbSQL.ToString();
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

    
    
    public override bool Equals(Object @object) {
        bool equals = false;

        if (this == @object) {
            equals = true;
        } else if (@object is PgIndex) {
            PgIndex index = (PgIndex) @object;
            equals = definition.Equals(index.getDefinition())
                    && name.Equals(index.getName())
                    && tableName.Equals(index.getTableName())
                    && unique == index.isUnique();
        }

        return equals;
    }

    
    
    public override int GetHashCode() {
        return (GetType().Name + "|" + definition + "|" + name + "|" + tableName + "|" + unique).GetHashCode();
    }

    
    public bool isUnique() {
        return unique;
    }

    
    public void setUnique(bool unique) {
        this.unique = unique;
    }
}
}