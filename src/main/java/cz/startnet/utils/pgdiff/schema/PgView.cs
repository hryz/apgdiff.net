
package cz.startnet.utils.pgdiff.schema;

import cz.startnet.utils.pgdiff.PgDiffUtils;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;


public class PgView {

    
    private List<String> columnNames;
    
    private final String name;
    
    private String query;
    
    private final List<DefaultValue> defaultValues =
            new ArrayList<DefaultValue>(0);
    
    private final List<ColumnComment> columnComments =
            new ArrayList<ColumnComment>(0);
    
    private String comment;

    
    public PgView(final String name) {
        this.name = name;
    }

    
    @SuppressWarnings("AssignmentToCollectionOrArrayFieldFromParameter")
    public void setColumnNames(final List<String> columnNames) {
        this.columnNames = columnNames;
    }

    
    public List<String> getColumnNames() {
        return Collections.unmodifiableList(columnNames);
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(final String comment) {
        this.comment = comment;
    }

    
    public String getCreationSQL() {
        final StringBuilder sbSQL = new StringBuilder(query.length() * 2);
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

        for (final DefaultValue defaultValue : defaultValues) {
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

        for (final ColumnComment columnComment : columnComments) {
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

    
    public void setQuery(final String query) {
        this.query = query;
    }

    
    public String getQuery() {
        return query;
    }

   
    public void addColumnDefaultValue(final String columnName,
            final String defaultValue) {
        removeColumnDefaultValue(columnName);
        defaultValues.add(new DefaultValue(columnName, defaultValue));
    }

    
    public void removeColumnDefaultValue(final String columnName) {
        for (final DefaultValue item : defaultValues) {
            if (item.getColumnName().equals(columnName)) {
                defaultValues.remove(item);
                return;
            }
        }
    }

    
    public List<DefaultValue> getDefaultValues() {
        return Collections.unmodifiableList(defaultValues);
    }

    
    public void addColumnComment(final String columnName,
            final String comment) {
        removeColumnDefaultValue(columnName);
        columnComments.add(new ColumnComment(columnName, comment));
    }

    
    public void removeColumnComment(final String columnName) {
        for (final ColumnComment item : columnComments) {
            if (item.getColumnName().equals(columnName)) {
                columnComments.remove(item);
                return;
            }
        }
    }

    
    public List<ColumnComment> getColumnComments() {
        return Collections.unmodifiableList(columnComments);
    }

    
    @SuppressWarnings("PublicInnerClass")
    public class DefaultValue {

        
        private final String columnName;
        
        private final String defaultValue;

        
        DefaultValue(final String columnName, final String defaultValue) {
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

    
    @SuppressWarnings("PublicInnerClass")
    public class ColumnComment {

        
        private final String columnName;
        
        private final String comment;

        
        ColumnComment(final String columnName, final String comment) {
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
