using System;
using System.Text;

namespace pgdiff.schema {



public class PgIndex {

    
    private String _definition;
    
    private String _name;
    
    private String _tableName;
    
    private bool _unique;
    
    private String _comment;

    
    public PgIndex(String name) {
        this._name = name;
    }

    
    public String GetComment() {
        return _comment;
    }

    
    public void SetComment(String comment) {
        this._comment = comment;
    }

    
    public String GetCreationSql() {
        StringBuilder sbSql = new StringBuilder(100);
        sbSql.Append("CREATE ");

        if (IsUnique()) {
            sbSql.Append("UNIQUE ");
        }

        sbSql.Append("INDEX ");
        sbSql.Append(PgDiffUtils.GetQuotedName(GetName()));
        sbSql.Append(" ON ");
        sbSql.Append(PgDiffUtils.GetQuotedName(GetTableName()));
        sbSql.Append(' ');
        sbSql.Append(GetDefinition());
        sbSql.Append(';');

        if ( !String.IsNullOrEmpty(_comment)) {
            sbSql.Append("\n\nCOMMENT ON INDEX ");
            sbSql.Append(PgDiffUtils.GetQuotedName(_name));
            sbSql.Append(" IS ");
            sbSql.Append(_comment);
            sbSql.Append(';');
        }

        return sbSql.ToString();
    }

    
    public void SetDefinition(String definition) {
        this._definition = definition;
    }

    
    public String GetDefinition() {
        return _definition;
    }

    
    public String GetDropSql() {
        return "DROP INDEX " + PgDiffUtils.GetQuotedName(GetName()) + ";";
    }

    
    public void SetName(String name) {
        this._name = name;
    }

    
    public String GetName() {
        return _name;
    }

    
    public void SetTableName(String tableName) {
        this._tableName = tableName;
    }

    
    public String GetTableName() {
        return _tableName;
    }

    
    
    public override bool Equals(Object @object) {
        bool equals = false;

        if (this == @object) {
            equals = true;
        } else if (@object is PgIndex) {
            PgIndex index = (PgIndex) @object;
            equals = _definition.Equals(index.GetDefinition())
                    && _name.Equals(index.GetName())
                    && _tableName.Equals(index.GetTableName())
                    && _unique == index.IsUnique();
        }

        return equals;
    }

    
    
    public override int GetHashCode() {
        return (GetType().Name + "|" + _definition + "|" + _name + "|" + _tableName + "|" + _unique).GetHashCode();
    }

    
    public bool IsUnique() {
        return _unique;
    }

    
    public void SetUnique(bool unique) {
        this._unique = unique;
    }
}
}