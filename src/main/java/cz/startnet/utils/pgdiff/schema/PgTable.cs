using System;
using System.Collections.Generic;
using System.Text;

namespace pgdiff.schema {






public class PgTable
{



    private List<PgColumn> columns = new List<PgColumn>();
    
    
    private List<PgConstraint> constraints = new List<PgConstraint>();
    
    
    private List<PgIndex> indexes = new List<PgIndex>();
    
    
    private List<PgTrigger> triggers = new List<PgTrigger>();
    
    private String clusterIndexName;
    
    
    private List<String> inherits = new List<String>();
    
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
        foreach(PgColumn column in columns) {
            if (column.getName().Equals(name)) {
                return column;
            }
        }

        return null;
    }

    
    public List<PgColumn> getColumns() {
        return new List<PgColumn>(columns);
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(String comment) {
        this.comment = comment;
    }

    
    public PgConstraint getConstraint(String name) {
        foreach(PgConstraint constraint in constraints) {
            if (constraint.getName().Equals(name)) {
                return constraint;
            }
        }

        return null;
    }

    
    public List<PgConstraint> getConstraints() {
        return new List<PgConstraint>(constraints);
    }

    
    public String getCreationSQL() {
        StringBuilder sbSQL = new StringBuilder(1000);
        sbSQL.Append("CREATE TABLE ");
        sbSQL.Append(PgDiffUtils.getQuotedName(name));
        sbSQL.Append(" (\n");

        bool first = true;

        if (columns.Count == 0) {
            sbSQL.Append(')');
        } else {
            foreach(PgColumn column in columns) {
                if (first) {
                    first = false;
                } else {
                    sbSQL.Append(",\n");
                }

                sbSQL.Append("\t");
                sbSQL.Append(column.getFullDefinition(false));
            }

            sbSQL.Append("\n)");
        }

        if (inherits != null && inherits.Count != 0) {
            sbSQL.Append("\nINHERITS (");

            first = true;

            foreach(String tableName in inherits) {
                if (first) {
                    first = false;
                } else {
                    sbSQL.Append(", ");
                }

                sbSQL.Append(tableName);
            }

            sbSQL.Append(")");
        }

        if (!string.IsNullOrEmpty(with)) {
            sbSQL.Append("\n");

            if ("OIDS=false".Equals(with,StringComparison.InvariantCultureIgnoreCase)) {
                sbSQL.Append("WITHOUT OIDS");
            } else {
                sbSQL.Append("WITH ");

                if ("OIDS".Equals(with,StringComparison.InvariantCultureIgnoreCase)
                        || "OIDS=true".Equals(with,StringComparison.InvariantCultureIgnoreCase)) {
                    sbSQL.Append("OIDS");
                } else {
                    sbSQL.Append(with);
                }
            }
        }

        if (!String.IsNullOrEmpty(tablespace)) {
            sbSQL.Append("\nTABLESPACE ");
            sbSQL.Append(tablespace);
        }

        sbSQL.Append(';');

        foreach (PgColumn column in getColumnsWithStatistics()) {
            sbSQL.Append("\nALTER TABLE ONLY ");
            sbSQL.Append(PgDiffUtils.getQuotedName(name));
            sbSQL.Append(" ALTER COLUMN ");
            sbSQL.Append(
                    PgDiffUtils.getQuotedName(column.getName()));
            sbSQL.Append(" SET STATISTICS ");
            sbSQL.Append(column.getStatistics());
            sbSQL.Append(';');
        }

        if (String.IsNullOrEmpty(comment)) {
            sbSQL.Append("\n\nCOMMENT ON TABLE ");
            sbSQL.Append(PgDiffUtils.getQuotedName(name));
            sbSQL.Append(" IS ");
            sbSQL.Append(comment);
            sbSQL.Append(';');
        }

        foreach(PgColumn column in columns) {
            if ( !String.IsNullOrEmpty(column.getComment())) {
                sbSQL.Append("\n\nCOMMENT ON COLUMN ");
                sbSQL.Append(PgDiffUtils.getQuotedName(name));
                sbSQL.Append('.');
                sbSQL.Append(PgDiffUtils.getQuotedName(column.getName()));
                sbSQL.Append(" IS ");
                sbSQL.Append(column.getComment());
                sbSQL.Append(';');
            }
        }

        return sbSQL.ToString();
    }

    
    public String getDropSQL() {
        return "DROP TABLE " + PgDiffUtils.getQuotedName(getName()) + ";";
    }

    
    public PgIndex getIndex(String name) {
        foreach(PgIndex index in indexes) {
            if (index.getName().Equals(name)) {
                return index;
            }
        }

        return null;
    }

    
    public PgTrigger getTrigger(String name) {
        foreach(PgTrigger trigger in triggers) {
            if (trigger.name.Equals(name)) {
                return trigger;
            }
        }

        return null;
    }

    
    public List<PgIndex> getIndexes() {
        return new List<PgIndex>(indexes);
    }

    
    public void addInherits(String tableName) {
        inherits.Add(tableName);
    }

    
    public List<String> getInherits() {
        return new List<string>(inherits);
    }

    
    public void setName(String name) {
        this.name = name;
    }

    
    public String getName() {
        return name;
    }

    
    public List<PgTrigger> getTriggers() {
        return new List<PgTrigger>(triggers);
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
        columns.Add(column);
    }

    
    public void addConstraint(PgConstraint constraint) {
        constraints.Add(constraint);
    }

    
    public void addIndex(PgIndex index) {
        indexes.Add(index);
    }

    
    public void addTrigger(PgTrigger trigger) {
        triggers.Add(trigger);
    }

    
    public bool containsColumn(String name) {
        foreach(PgColumn column in columns) {
            if (column.getName().Equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public bool containsConstraint(String name) {
        foreach(PgConstraint constraint in constraints) {
            if (constraint.getName().Equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public bool containsIndex(String name) {
        foreach(PgIndex index in indexes) {
            if (index.getName().Equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    private List<PgColumn> getColumnsWithStatistics() {
        
        List<PgColumn> list = new List<PgColumn>();

        foreach(PgColumn column in columns) {
            if (column.getStatistics() != null) {
                list.Add(column);
            }
        }

        return list;
    }
}
}