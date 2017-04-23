
namespace cz.startnet.utils.pgdiff.schema {

import cz.startnet.utils.pgdiff.PgDiffUtils;





public class PgTable {

    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private List<PgColumn> columns = new ArrayList<PgColumn>();
    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private List<PgConstraint> constraints =
            new ArrayList<PgConstraint>();
    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private List<PgIndex> indexes = new ArrayList<PgIndex>();
    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private List<PgTrigger> triggers = new ArrayList<PgTrigger>();
    
    private String clusterIndexName;
    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private List<String> inherits = new ArrayList<String>();
    
    private String name;
    
    private String with;
    
    private String tablespace;
    
    private String comment;

    
    public PgTable(String name) {
        this.name = name;
    }

    
    public void setClusterIndexName(String name) {
        clusterIndexName = name;
    }

    
    public String getClusterIndexName() {
        return clusterIndexName;
    }

    
    public PgColumn getColumn(String name) {
        for (PgColumn column : columns) {
            if (column.getName().equals(name)) {
                return column;
            }
        }

        return null;
    }

    
    public List<PgColumn> getColumns() {
        return Collections.unmodifiableList(columns);
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(String comment) {
        this.comment = comment;
    }

    
    public PgConstraint getConstraint(String name) {
        for (PgConstraint constraint : constraints) {
            if (constraint.getName().equals(name)) {
                return constraint;
            }
        }

        return null;
    }

    
    public List<PgConstraint> getConstraints() {
        return Collections.unmodifiableList(constraints);
    }

    
    public String getCreationSQL() {
        StringBuilder sbSQL = new StringBuilder(1000);
        sbSQL.append("CREATE TABLE ");
        sbSQL.append(PgDiffUtils.getQuotedName(name));
        sbSQL.append(" (\n");

        boolean first = true;

        if (columns.isEmpty()) {
            sbSQL.append(')');
        } else {
            for (PgColumn column : columns) {
                if (first) {
                    first = false;
                } else {
                    sbSQL.append(",\n");
                }

                sbSQL.append("\t");
                sbSQL.append(column.getFullDefinition(false));
            }

            sbSQL.append("\n)");
        }

        if (inherits != null && !inherits.isEmpty()) {
            sbSQL.append("\nINHERITS (");

            first = true;

            for (String tableName : inherits) {
                if (first) {
                    first = false;
                } else {
                    sbSQL.append(", ");
                }

                sbSQL.append(tableName);
            }

            sbSQL.append(")");
        }

        if (with != null && !with.isEmpty()) {
            sbSQL.append("\n");

            if ("OIDS=false".equalsIgnoreCase(with)) {
                sbSQL.append("WITHOUT OIDS");
            } else {
                sbSQL.append("WITH ");

                if ("OIDS".equalsIgnoreCase(with)
                        || "OIDS=true".equalsIgnoreCase(with)) {
                    sbSQL.append("OIDS");
                } else {
                    sbSQL.append(with);
                }
            }
        }

        if (tablespace != null && !tablespace.isEmpty()) {
            sbSQL.append("\nTABLESPACE ");
            sbSQL.append(tablespace);
        }

        sbSQL.append(';');

        for (PgColumn column : getColumnsWithStatistics()) {
            sbSQL.append("\nALTER TABLE ONLY ");
            sbSQL.append(PgDiffUtils.getQuotedName(name));
            sbSQL.append(" ALTER COLUMN ");
            sbSQL.append(
                    PgDiffUtils.getQuotedName(column.getName()));
            sbSQL.append(" SET STATISTICS ");
            sbSQL.append(column.getStatistics());
            sbSQL.append(';');
        }

        if (comment != null && !comment.isEmpty()) {
            sbSQL.append("\n\nCOMMENT ON TABLE ");
            sbSQL.append(PgDiffUtils.getQuotedName(name));
            sbSQL.append(" IS ");
            sbSQL.append(comment);
            sbSQL.append(';');
        }

        for (PgColumn column : columns) {
            if (column.getComment() != null && !column.getComment().isEmpty()) {
                sbSQL.append("\n\nCOMMENT ON COLUMN ");
                sbSQL.append(PgDiffUtils.getQuotedName(name));
                sbSQL.append('.');
                sbSQL.append(PgDiffUtils.getQuotedName(column.getName()));
                sbSQL.append(" IS ");
                sbSQL.append(column.getComment());
                sbSQL.append(';');
            }
        }

        return sbSQL.toString();
    }

    
    public String getDropSQL() {
        return "DROP TABLE " + PgDiffUtils.getQuotedName(getName()) + ";";
    }

    
    public PgIndex getIndex(String name) {
        for (PgIndex index : indexes) {
            if (index.getName().equals(name)) {
                return index;
            }
        }

        return null;
    }

    
    public PgTrigger getTrigger(String name) {
        for (PgTrigger trigger : triggers) {
            if (trigger.getName().equals(name)) {
                return trigger;
            }
        }

        return null;
    }

    
    public List<PgIndex> getIndexes() {
        return Collections.unmodifiableList(indexes);
    }

    
    public void addInherits(String tableName) {
        inherits.add(tableName);
    }

    
    public List<String> getInherits() {
        return Collections.unmodifiableList(inherits);
    }

    
    public void setName(String name) {
        this.name = name;
    }

    
    public String getName() {
        return name;
    }

    
    public List<PgTrigger> getTriggers() {
        return Collections.unmodifiableList(triggers);
    }

    
    public void setWith(String with) {
        this.with = with;
    }

    
    public String getWith() {
        return with;
    }

    
    public String getTablespace() {
        return tablespace;
    }

    
    public void setTablespace(String tablespace) {
        this.tablespace = tablespace;
    }

    
    public void addColumn(PgColumn column) {
        columns.add(column);
    }

    
    public void addConstraint(PgConstraint constraint) {
        constraints.add(constraint);
    }

    
    public void addIndex(PgIndex index) {
        indexes.add(index);
    }

    
    public void addTrigger(PgTrigger trigger) {
        triggers.add(trigger);
    }

    
    public boolean containsColumn(String name) {
        for (PgColumn column : columns) {
            if (column.getName().equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public boolean containsConstraint(String name) {
        for (PgConstraint constraint : constraints) {
            if (constraint.getName().equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public boolean containsIndex(String name) {
        for (PgIndex index : indexes) {
            if (index.getName().equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    private List<PgColumn> getColumnsWithStatistics() {
        @SuppressWarnings("CollectionWithoutInitialCapacity")
        List<PgColumn> list = new ArrayList<PgColumn>();

        for (PgColumn column : columns) {
            if (column.getStatistics() != null) {
                list.add(column);
            }
        }

        return list;
    }
}
}