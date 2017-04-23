
using System;
using System.Collections.Generic;
using System.Text;

namespace cz.startnet.utils.pgdiff.schema {






public class PgView {

    
    private List<String> columnNames;
    
    private String name;
    
    private String query;
    
    private List<DefaultValue> defaultValues =
            new ArrayList<DefaultValue>(0);
    
    private List<ColumnComment> columnComments =
            new ArrayList<ColumnComment>(0);
    
    private String comment;

    
    public PgView(String name) {
        this.name = name;
    }

    
    
    public void setColumnNames(List<String> columnNames) {
        this.columnNames = columnNames;
    }

    
    public List<String> getColumnNames() {
        return Collections.unmodifiableList(columnNames);
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(String comment) {
        this.comment = comment;
    }

    
    public String getCreationSQL() {
        StringBuilder sbSQL = new StringBuilder(query.length() * 2);
        sbSQL.append("CREATE VIEW ");
        sbSQL.append(PgDiffUtils.getQuotedName(name));

        if (columnNames != null && !columnNames.isEmpty()) {
            sbSQL.append(" (");

            for (int i = 0; i < columnNames.size(); i++) {
                if (i > 0) {
                    sbSQL.append(", ");
                }

                sbSQL.append(PgDiffUtils.getQuotedName(columnNames.get(i)));
            }
            sbSQL.append(')');
        }

        sbSQL.append(" AS\n\t");
        sbSQL.append(query);
        sbSQL.append(';');

        for (DefaultValue defaultValue : defaultValues) {
            sbSQL.append("\n\nALTER VIEW ");
            sbSQL.append(PgDiffUtils.getQuotedName(name));
            sbSQL.append(" ALTER COLUMN ");
            sbSQL.append(
                    PgDiffUtils.getQuotedName(defaultValue.getColumnName()));
            sbSQL.append(" SET DEFAULT ");
            sbSQL.append(defaultValue.getDefaultValue());
            sbSQL.append(';');
        }

        if (comment != null && !comment.isEmpty()) {
            sbSQL.append("\n\nCOMMENT ON VIEW ");
            sbSQL.append(PgDiffUtils.getQuotedName(name));
            sbSQL.append(" IS ");
            sbSQL.append(comment);
            sbSQL.append(';');
        }

        for (ColumnComment columnComment : columnComments) {
            if (columnComment.getComment() != null
                    && !columnComment.getComment().isEmpty()) {
                sbSQL.append("\n\nCOMMENT ON COLUMN ");
                sbSQL.append(PgDiffUtils.getQuotedName(
                        columnComment.getColumnName()));
                sbSQL.append(" IS ");
                sbSQL.append(columnComment.getComment());
                sbSQL.append(';');
            }
        }

        return sbSQL.toString();
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
        defaultValues.add(new DefaultValue(columnName, defaultValue));
    }

    
    public void removeColumnDefaultValue(String columnName) {
        for (DefaultValue item : defaultValues) {
            if (item.getColumnName().Equals(columnName)) {
                defaultValues.remove(item);
                return;
            }
        }
    }

    
    public List<DefaultValue> getDefaultValues() {
        return Collections.unmodifiableList(defaultValues);
    }

    
    public void addColumnComment(String columnName,
            String comment) {
        removeColumnDefaultValue(columnName);
        columnComments.add(new ColumnComment(columnName, comment));
    }

    
    public void removeColumnComment(String columnName) {
        for (ColumnComment item : columnComments) {
            if (item.getColumnName().Equals(columnName)) {
                columnComments.remove(item);
                return;
            }
        }
    }

    
    public List<ColumnComment> getColumnComments() {
        return Collections.unmodifiableList(columnComments);
    }

    
    
    public class DefaultValue {

        
        private String columnName;
        
        private String defaultValue;

        
        DefaultValue(String columnName, String defaultValue) {
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

        
        ColumnComment(String columnName, String comment) {
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