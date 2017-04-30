using System;
using System.Text;
using System.Text.RegularExpressions;

namespace pgdiff.schema {





public class PgColumn {

    
    private static Regex _patternNull = new Regex("^(.+)[\\s]+NULL$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
    
    private static Regex _patternNotNull = new Regex("^(.+)[\\s]+NOT[\\s]+NULL$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static Regex _patternDefault = new Regex("^(.+)[\\s]+DEFAULT[\\s]+(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private Int32? _statistics;
    
    private String _defaultValue;
    
    private String _name;
    
    private String _type;
    
    private bool _nullValue = true;
    
    private String _storage;
    
    private String _comment;

    
    public PgColumn(String name) {
        this._name = name;
    }

    
    public String GetComment() {
        return _comment;
    }

    
    public void SetComment(String comment) {
        this._comment = comment;
    }

    
    public void SetDefaultValue(String defaultValue) {
        this._defaultValue = defaultValue;
    }

    
    public String GetDefaultValue() {
        return _defaultValue;
    }

    
    public String GetFullDefinition(bool addDefaults) {
        StringBuilder sbDefinition = new StringBuilder(100);
        sbDefinition.Append(PgDiffUtils.GetQuotedName(_name));
        sbDefinition.Append(' ');
        sbDefinition.Append(_type);

        if (_defaultValue != null && !String.IsNullOrEmpty(_defaultValue)) {
            sbDefinition.Append(" DEFAULT ");
            sbDefinition.Append(_defaultValue);
        } else if (!_nullValue && addDefaults) {
            String defaultColValue = PgColumnUtils.GetDefaultValue(_type);

            if (defaultColValue != null) {
                sbDefinition.Append(" DEFAULT ");
                sbDefinition.Append(defaultColValue);
            }
        }

        if (!_nullValue) {
            sbDefinition.Append(" NOT NULL");
        }

        return sbDefinition.ToString();
    }

    
    public void SetName(String name) {
        this._name = name;
    }

    
    public String GetName() {
        return _name;
    }

    
    public void SetNullValue(bool nullValue) {
        this._nullValue = nullValue;
    }

    
    public bool GetNullValue() {
        return _nullValue;
    }

    
    public void SetStatistics(Int32 statistics) {
        this._statistics = statistics;
    }

    
    public Int32? GetStatistics() {
        return _statistics;
    }

    
    public String GetStorage() {
        return _storage;
    }

    
    public void SetStorage(String storage) {
        this._storage = storage;
    }

    
    public void SetType(String type) {
        this._type = type;
    }

    
    public String getType() {
        return _type;
    }

    
    public void ParseDefinition(String definition) {
        String @string = definition;

        Regex matcher = _patternNotNull;

        if (matcher.IsMatch(@string)) {
            @string = matcher.Matches(@string)[0].Groups[1].Value.Trim();
            SetNullValue(false);
        } else {
            matcher = _patternNull;

            if (matcher.IsMatch(@string)) {
                @string = matcher.Matches(@string)[0].Groups[1].Value.Trim();
                    SetNullValue(true);
            }
        }

        matcher = _patternDefault;

        if (matcher.IsMatch(@string))
        {
            var matches = matcher.Matches(@string);
            @string = matches[0].Groups[1].Value.Trim(); 
            SetDefaultValue(matches[0].Groups[2].Value.Trim());
        }

        SetType(@string);
    }
}
}