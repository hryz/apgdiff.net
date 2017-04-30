using System;
using System.Collections.Generic;
using System.Text;

namespace pgdiff.schema {






public class PgView {

    
    private List<String> _columnNames;
    
    private String _name;
    
    private String _query;
    
    private List<DefaultValue> _defaultValues = new List<DefaultValue>();
    
    private List<ColumnComment> _columnComments = new List<ColumnComment>();
    
    private String _comment;

    
    public PgView(String name) {
        this._name = name;
    }

    
    
    public void SetColumnNames(List<String> columnNames) {
        this._columnNames = columnNames;
    }

    
    public List<String> GetColumnNames() {
        return new List<string>(_columnNames);
    }

    
    public String GetComment() {
        return _comment;
    }

    
    public void SetComment(String comment) {
        this._comment = comment;
    }

    
    public String GetCreationSql() {
        StringBuilder sbSql = new StringBuilder(_query.Length * 2);
        sbSql.Append("CREATE VIEW ");
        sbSql.Append(PgDiffUtils.GetQuotedName(_name));

        if (_columnNames != null && _columnNames.Count >0) {
            sbSql.Append(" (");

            for (int i = 0; i < _columnNames.Count; i++) {
                if (i > 0) {
                    sbSql.Append(", ");
                }

                sbSql.Append(PgDiffUtils.GetQuotedName(_columnNames[i]));
            }
            sbSql.Append(')');
        }

        sbSql.Append(" AS\n\t");
        sbSql.Append(_query);
        sbSql.Append(';');

        foreach(DefaultValue defaultValue in _defaultValues) {
            sbSql.Append("\n\nALTER VIEW ");
            sbSql.Append(PgDiffUtils.GetQuotedName(_name));
            sbSql.Append(" ALTER COLUMN ");
            sbSql.Append(
                    PgDiffUtils.GetQuotedName(defaultValue.GetColumnName()));
            sbSql.Append(" SET DEFAULT ");
            sbSql.Append(defaultValue.GetDefaultValue());
            sbSql.Append(';');
        }

        if (!String.IsNullOrEmpty(_comment)) {
            sbSql.Append("\n\nCOMMENT ON VIEW ");
            sbSql.Append(PgDiffUtils.GetQuotedName(_name));
            sbSql.Append(" IS ");
            sbSql.Append(_comment);
            sbSql.Append(';');
        }

        foreach(ColumnComment columnComment in _columnComments) {
            if (!String.IsNullOrEmpty(columnComment.GetComment())) {
                sbSql.Append("\n\nCOMMENT ON COLUMN ");
                sbSql.Append(PgDiffUtils.GetQuotedName(columnComment.GetColumnName()));
                sbSql.Append(" IS ");
                sbSql.Append(columnComment.GetComment());
                sbSql.Append(';');
            }
        }

        return sbSql.ToString();
    }

    
    public String GetDropSql() {
        return "DROP VIEW " + PgDiffUtils.GetQuotedName(GetName()) + ";";
    }

    
    public String GetName() {
        return _name;
    }

    
    public void SetQuery(String query) {
        this._query = query;
    }

    
    public String GetQuery() {
        return _query;
    }

   
    public void AddColumnDefaultValue(String columnName,
            String defaultValue) {
        RemoveColumnDefaultValue(columnName);
        _defaultValues.Add(new DefaultValue(columnName, defaultValue));
    }

    
    public void RemoveColumnDefaultValue(String columnName) {
        foreach(DefaultValue item in _defaultValues) {
            if (item.GetColumnName().Equals(columnName)) {
                _defaultValues.Remove(item);
                return;
            }
        }
    }

    
    public List<DefaultValue> GetDefaultValues() {
        return new List<DefaultValue>(_defaultValues);
    }

    
    public void AddColumnComment(String columnName,
            String comment) {
        RemoveColumnDefaultValue(columnName);
        _columnComments.Add(new ColumnComment(columnName, comment));
    }

    
    public void RemoveColumnComment(String columnName) {
        foreach(ColumnComment item in _columnComments) {
            if (item.GetColumnName().Equals(columnName)) {
                _columnComments.Remove(item);
                return;
            }
        }
    }

    
    public List<ColumnComment> GetColumnComments() {
        return new List<ColumnComment>(_columnComments);
    }

    
    
    public class DefaultValue {

        
        private String _columnName;
        
        private String _defaultValue;

        
        public DefaultValue(String columnName, String defaultValue) {
            this._columnName = columnName;
            this._defaultValue = defaultValue;
        }

        
        public String GetColumnName() {
            return _columnName;
        }

        
        public String GetDefaultValue() {
            return _defaultValue;
        }
    }

    
    
    public class ColumnComment {

        
        private String _columnName;
        
        private String _comment;

        
        public ColumnComment(String columnName, String comment) {
            this._columnName = columnName;
            this._comment = comment;
        }

        
        public String GetColumnName() {
            return _columnName;
        }

        
        public String GetComment() {
            return _comment;
        }
    }
}
}