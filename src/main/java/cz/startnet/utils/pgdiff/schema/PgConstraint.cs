using System;
using System.Text;
using System.Text.RegularExpressions;

namespace pgdiff.schema {




public class PgConstraint {

    
    private static Regex _patternPrimaryKey = new Regex(".*PRIMARY[\\s]+KEY.*", RegexOptions.Compiled|RegexOptions.IgnoreCase);
    
    private String _definition;
    
    private String _name;
    
    private String _tableName;
    
    private String _comment;

    
    public PgConstraint(String name) {
        this._name = name;
    }

    
    public String GetCreationSql() {
        StringBuilder sbSql = new StringBuilder(100);
        sbSql.Append("ALTER TABLE ");
        sbSql.Append(PgDiffUtils.GetQuotedName(GetTableName()));
        sbSql.Append("\n\tADD CONSTRAINT ");
        sbSql.Append(PgDiffUtils.GetQuotedName(GetName()));
        sbSql.Append(' ');
        sbSql.Append(GetDefinition());
        sbSql.Append(';');

        if (_comment != null && !String.IsNullOrEmpty(_comment)) {
            sbSql.Append("\n\nCOMMENT ON CONSTRAINT ");
            sbSql.Append(PgDiffUtils.GetQuotedName(_name));
            sbSql.Append(" ON ");
            sbSql.Append(PgDiffUtils.GetQuotedName(_tableName));
            sbSql.Append(" IS ");
            sbSql.Append(_comment);
            sbSql.Append(';');
        }

        return sbSql.ToString();
    }

    
    public String GetComment() {
        return _comment;
    }

    
    public void SetComment(String comment) {
        this._comment = comment;
    }

    
    public void SetDefinition(String definition) {
        this._definition = definition;
    }

    
    public String GetDefinition() {
        return _definition;
    }

    
    public String GetDropSql() {
        StringBuilder sbSql = new StringBuilder(100);
        sbSql.Append("ALTER TABLE ");
        sbSql.Append(PgDiffUtils.GetQuotedName(GetTableName()));
        sbSql.Append("\n\tDROP CONSTRAINT ");
        sbSql.Append(PgDiffUtils.GetQuotedName(GetName()));
        sbSql.Append(';');

        return sbSql.ToString();
    }

    
    public void SetName(String name) {
        this._name = name;
    }

    
    public String GetName() {
        return _name;
    }

    
    public bool IsPrimaryKeyConstraint() {
        return _patternPrimaryKey.IsMatch(_definition);
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
        } else if (@object is PgConstraint) {
            PgConstraint constraint = (PgConstraint) @object;
            equals = _definition.Equals(constraint.GetDefinition())
                    && _name.Equals(constraint.GetName())
                    && _tableName.Equals(constraint.GetTableName());
        }

        return equals;
    }

    
    
    public override int GetHashCode() {
        return (GetType().Name + "|" + _definition + "|" + _name + "|" + _tableName).GetHashCode();
    }
}
}