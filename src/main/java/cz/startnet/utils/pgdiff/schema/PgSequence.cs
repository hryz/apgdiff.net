using System;
using System.Text;

namespace pgdiff.schema {



public class PgSequence {

    
    private String _cache;
    
    private String _increment;
    
    private String _maxValue;
    
    private String _minValue;
    
    private String _name;
    
    private String _startWith;
    
    private bool _cycle;
    
    private String _ownedBy;
    
    private String _comment;

    
    public PgSequence(String name) {
        this._name = name;
    }

    
    public void SetCache(String cache) {
        this._cache = cache;
    }

    
    public String GetCache() {
        return _cache;
    }

    
    public String GetComment() {
        return _comment;
    }

    
    public void SetComment(String comment) {
        this._comment = comment;
    }

    
    public String GetCreationSql() {
        StringBuilder sbSql = new StringBuilder(100);
        sbSql.Append("CREATE SEQUENCE ");
        sbSql.Append(PgDiffUtils.GetQuotedName(_name));

        if (_startWith != null) {
            sbSql.Append("\n\tSTART WITH ");
            sbSql.Append(_startWith);
        }

        if (_increment != null) {
            sbSql.Append("\n\tINCREMENT BY ");
            sbSql.Append(_increment);
        }

        sbSql.Append("\n\t");

        if (_maxValue == null) {
            sbSql.Append("NO MAXVALUE");
        } else {
            sbSql.Append("MAXVALUE ");
            sbSql.Append(_maxValue);
        }

        sbSql.Append("\n\t");

        if (_minValue == null) {
            sbSql.Append("NO MINVALUE");
        } else {
            sbSql.Append("MINVALUE ");
            sbSql.Append(_minValue);
        }

        if (_cache != null) {
            sbSql.Append("\n\tCACHE ");
            sbSql.Append(_cache);
        }

        if (_cycle) {
            sbSql.Append("\n\tCYCLE");
        }

        sbSql.Append(';');

        if ( !String.IsNullOrEmpty(_comment)) {
            sbSql.Append("\n\nCOMMENT ON SEQUENCE ");
            sbSql.Append(PgDiffUtils.GetQuotedName(_name));
            sbSql.Append(" IS ");
            sbSql.Append(_comment);
            sbSql.Append(';');
        }

        return sbSql.ToString();
    }

    
    public String GetOwnedBySql() {
        StringBuilder sbSql = new StringBuilder(100);

        sbSql.Append("ALTER SEQUENCE ");
        sbSql.Append(PgDiffUtils.GetQuotedName(_name));

        if ( !String.IsNullOrEmpty(_ownedBy)) {
            sbSql.Append("\n\tOWNED BY ");
            sbSql.Append(_ownedBy);
        }

        sbSql.Append(';');

        return sbSql.ToString();
    }

    
    public void SetCycle(bool cycle) {
        this._cycle = cycle;
    }

    
    public bool IsCycle() {
        return _cycle;
    }

    
    public String GetDropSql() {
        return "DROP SEQUENCE " + PgDiffUtils.GetQuotedName(GetName()) + ";";
    }

    
    public void SetIncrement(String increment) {
        this._increment = increment;
    }

    
    public String GetIncrement() {
        return _increment;
    }

    
    public void SetMaxValue(String maxValue) {
        this._maxValue = maxValue;
    }

    
    public String GetMaxValue() {
        return _maxValue;
    }

    
    public void SetMinValue(String minValue) {
        this._minValue = minValue;
    }

    
    public String GetMinValue() {
        return _minValue;
    }

    
    public void SetName(String name) {
        this._name = name;
    }

    
    public String GetName() {
        return _name;
    }

    
    public void SetStartWith(String startWith) {
        this._startWith = startWith;
    }

    
    public String GetStartWith() {
        return _startWith;
    }

    
    public String GetOwnedBy() {
        return _ownedBy;
    }

    
    public void SetOwnedBy(String ownedBy) {
        this._ownedBy = ownedBy;
    }
}
}