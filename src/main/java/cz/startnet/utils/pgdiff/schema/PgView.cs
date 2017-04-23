using System;
using System.Collections.Generic;
using System.Text;

namespace pgdiff.schema {






public class PgView {

    
    private List<String> columnNames;
    
    private String name;
    
    private String query;
    
    private List<DefaultValue> defaultValues = new List<DefaultValue>();
    
    private List<ColumnComment> columnComments = new List<ColumnComment>();
    
    private String comment;

    
    public PgView(String name) {
        this.name = name;
    }

    
    
    public void setColumnNames(List<String> columnNames) {
        this.columnNames = columnNames;
    }

    
    public List<String> getColumnNames() {
        return new List<string>(columnNames);
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(String comment) {
        this.comment = comment;
    }

    
    public String getCreationSQL() {
        StringBuilder sbSQL = new StringBuilder(query.Length * 2);
        sbSQL.Append("CREATE VIEW ");
        sbSQL.Append(PgDiffUtils.getQuotedName(name));

        if (columnNames != null && columnNames.Count >0) {
            sbSQL.Append(" (");

            for (int i = 0; i < columnNames.Count; i++) {
                if (i > 0) {
                    sbSQL.Append(", ");
                }

                sbSQL.Append(PgDiffUtils.getQuotedName(columnNames[i]));
            }
            sbSQL.Append(')');
        }

        sbSQL.Append(" AS\n\t");
        sbSQL.Append(query);
        sbSQL.Append(';');

        foreach(DefaultValue defaultValue in defaultValues) {
            sbSQL.Append("\n\nALTER VIEW ");
            sbSQL.Append(PgDiffUtils.getQuotedName(name));
            sbSQL.Append(" ALTER COLUMN ");
            sbSQL.Append(
                    PgDiffUtils.getQuotedName(defaultValue.getColumnName()));
            sbSQL.Append(" SET DEFAULT ");
            sbSQL.Append(defaultValue.getDefaultValue());
            sbSQL.Append(';');
        }

        if (!String.IsNullOrEmpty(comment)) {
            sbSQL.Append("\n\nCOMMENT ON VIEW ");
            sbSQL.Append(PgDiffUtils.getQuotedName(name));
            sbSQL.Append(" IS ");
            sbSQL.Append(comment);
            sbSQL.Append(';');
        }

        foreach(ColumnComment columnComment in columnComments) {
            if (!String.IsNullOrEmpty(columnComment.getComment())) {
                sbSQL.Append("\n\nCOMMENT ON COLUMN ");
                sbSQL.Append(PgDiffUtils.getQuotedName(columnComment.getColumnName()));
                sbSQL.Append(" IS ");
                sbSQL.Append(columnComment.getComment());
                sbSQL.Append(';');
            }
        }

        return sbSQL.ToString();
    }

    
    public String getDropSQL() {
        return "DROP VIEW " + PgDiffUtils.getQuotedName(getName()) + ";";
    }

    
    public String getName() {
        return name;
    }

    
    public void setQuery(String query) {
        this.query = query;
    }

    
    public String getQuery() {
        return query;
    }

   
    public void addColumnDefaultValue(String columnName,
            String defaultValue) {
        removeColumnDefaultValue(columnName);
        defaultValues.Add(new DefaultValue(columnName, defaultValue));
    }

    
    public void removeColumnDefaultValue(String columnName) {
        foreach(DefaultValue item in defaultValues) {
            if (item.getColumnName().Equals(columnName)) {
                defaultValues.Remove(item);
                return;
            }
        }
    }

    
    public List<DefaultValue> getDefaultValues() {
        return new List<DefaultValue>(defaultValues);
    }

    
    public void addColumnComment(String columnName,
            String comment) {
        removeColumnDefaultValue(columnName);
        columnComments.Add(new ColumnComment(columnName, comment));
    }

    
    public void removeColumnComment(String columnName) {
        foreach(ColumnComment item in columnComments) {
            if (item.getColumnName().Equals(columnName)) {
                columnComments.Remove(item);
                return;
            }
        }
    }

    
    public List<ColumnComment> getColumnComments() {
        return new List<ColumnComment>(columnComments);
    }

    
    
    public class DefaultValue {

        
        private String columnName;
        
        private String defaultValue;

        
        public DefaultValue(String columnName, String defaultValue) {
            this.columnName = columnName;
            this.defaultValue = defaultValue;
        }

        
        public String getColumnName() {
            return columnName;
        }

        
        public String getDefaultValue() {
            return defaultValue;
        }
    }

    
    
    public class ColumnComment {

        
        private String columnName;
        
        private String comment;

        
        public ColumnComment(String columnName, String comment) {
            this.columnName = columnName;
            this.comment = comment;
        }

        
        public String getColumnName() {
            return columnName;
        }

        
        public String getComment() {
            return comment;
        }
    }
}
}