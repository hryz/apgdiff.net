using System;
using System.Collections.Generic;
using System.Text;

namespace pgdiff.schema {






public class PgTable
{



    private List<PgColumn> _columns = new List<PgColumn>();
    
    
    private List<PgConstraint> _constraints = new List<PgConstraint>();
    
    
    private List<PgIndex> _indexes = new List<PgIndex>();
    
    
    private List<PgTrigger> _triggers = new List<PgTrigger>();
    
    private String _clusterIndexName;
    
    
    private List<String> _inherits = new List<String>();
    
    private String _name;
    
    private String _with;
    
    private String _tablespace;
    
    private String _comment;

    
    public PgTable(String name) {
        this._name = name;
    }

    
    public void SetClusterIndexName(String name) {
        _clusterIndexName = name;
    }

    
    public String GetClusterIndexName() {
        return _clusterIndexName;
    }

    
    public PgColumn GetColumn(String name) {
        foreach(PgColumn column in _columns) {
            if (column.GetName().Equals(name)) {
                return column;
            }
        }

        return null;
    }

    
    public List<PgColumn> GetColumns() {
        return new List<PgColumn>(_columns);
    }

    
    public String GetComment() {
        return _comment;
    }

    
    public void SetComment(String comment) {
        this._comment = comment;
    }

    
    public PgConstraint GetConstraint(String name) {
        foreach(PgConstraint constraint in _constraints) {
            if (constraint.GetName().Equals(name)) {
                return constraint;
            }
        }

        return null;
    }

    
    public List<PgConstraint> GetConstraints() {
        return new List<PgConstraint>(_constraints);
    }

    
    public String GetCreationSql() {
        StringBuilder sbSql = new StringBuilder(1000);
        sbSql.Append("CREATE TABLE ");
        sbSql.Append(PgDiffUtils.GetQuotedName(_name));
        sbSql.Append(" (\n");

        bool first = true;

        if (_columns.Count == 0) {
            sbSql.Append(')');
        } else {
            foreach(PgColumn column in _columns) {
                if (first) {
                    first = false;
                } else {
                    sbSql.Append(",\n");
                }

                sbSql.Append("\t");
                sbSql.Append(column.GetFullDefinition(false));
            }

            sbSql.Append("\n)");
        }

        if (_inherits != null && _inherits.Count != 0) {
            sbSql.Append("\nINHERITS (");

            first = true;

            foreach(String tableName in _inherits) {
                if (first) {
                    first = false;
                } else {
                    sbSql.Append(", ");
                }

                sbSql.Append(tableName);
            }

            sbSql.Append(")");
        }

        if (!string.IsNullOrEmpty(_with)) {
            sbSql.Append("\n");

            if ("OIDS=false".Equals(_with,StringComparison.InvariantCultureIgnoreCase)) {
                sbSql.Append("WITHOUT OIDS");
            } else {
                sbSql.Append("WITH ");

                if ("OIDS".Equals(_with,StringComparison.InvariantCultureIgnoreCase)
                        || "OIDS=true".Equals(_with,StringComparison.InvariantCultureIgnoreCase)) {
                    sbSql.Append("OIDS");
                } else {
                    sbSql.Append(_with);
                }
            }
        }

        if (!String.IsNullOrEmpty(_tablespace)) {
            sbSql.Append("\nTABLESPACE ");
            sbSql.Append(_tablespace);
        }

        sbSql.Append(';');

        foreach (PgColumn column in GetColumnsWithStatistics()) {
            sbSql.Append("\nALTER TABLE ONLY ");
            sbSql.Append(PgDiffUtils.GetQuotedName(_name));
            sbSql.Append(" ALTER COLUMN ");
            sbSql.Append(
                    PgDiffUtils.GetQuotedName(column.GetName()));
            sbSql.Append(" SET STATISTICS ");
            sbSql.Append(column.GetStatistics());
            sbSql.Append(';');
        }

        if (String.IsNullOrEmpty(_comment)) {
            sbSql.Append("\n\nCOMMENT ON TABLE ");
            sbSql.Append(PgDiffUtils.GetQuotedName(_name));
            sbSql.Append(" IS ");
            sbSql.Append(_comment);
            sbSql.Append(';');
        }

        foreach(PgColumn column in _columns) {
            if ( !String.IsNullOrEmpty(column.GetComment())) {
                sbSql.Append("\n\nCOMMENT ON COLUMN ");
                sbSql.Append(PgDiffUtils.GetQuotedName(_name));
                sbSql.Append('.');
                sbSql.Append(PgDiffUtils.GetQuotedName(column.GetName()));
                sbSql.Append(" IS ");
                sbSql.Append(column.GetComment());
                sbSql.Append(';');
            }
        }

        return sbSql.ToString();
    }

    
    public String GetDropSql() {
        return "DROP TABLE " + PgDiffUtils.GetQuotedName(GetName()) + ";";
    }

    
    public PgIndex GetIndex(String name) {
        foreach(PgIndex index in _indexes) {
            if (index.GetName().Equals(name)) {
                return index;
            }
        }

        return null;
    }

    
    public PgTrigger GetTrigger(String name) {
        foreach(PgTrigger trigger in _triggers) {
            if (trigger.Name.Equals(name)) {
                return trigger;
            }
        }

        return null;
    }

    
    public List<PgIndex> GetIndexes() {
        return new List<PgIndex>(_indexes);
    }

    
    public void AddInherits(String tableName) {
        _inherits.Add(tableName);
    }

    
    public List<String> GetInherits() {
        return new List<string>(_inherits);
    }

    
    public void SetName(String name) {
        this._name = name;
    }

    
    public String GetName() {
        return _name;
    }

    
    public List<PgTrigger> GetTriggers() {
        return new List<PgTrigger>(_triggers);
    }

    
    public void SetWith(String with) {
        this._with = with;
    }

    
    public String GetWith() {
        return _with;
    }

    
    public String GetTablespace() {
        return _tablespace;
    }

    
    public void SetTablespace(String tablespace) {
        this._tablespace = tablespace;
    }

    
    public void AddColumn(PgColumn column) {
        _columns.Add(column);
    }

    
    public void AddConstraint(PgConstraint constraint) {
        _constraints.Add(constraint);
    }

    
    public void AddIndex(PgIndex index) {
        _indexes.Add(index);
    }

    
    public void AddTrigger(PgTrigger trigger) {
        _triggers.Add(trigger);
    }

    
    public bool ContainsColumn(String name) {
        foreach(PgColumn column in _columns) {
            if (column.GetName().Equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public bool ContainsConstraint(String name) {
        foreach(PgConstraint constraint in _constraints) {
            if (constraint.GetName().Equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public bool ContainsIndex(String name) {
        foreach(PgIndex index in _indexes) {
            if (index.GetName().Equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    private List<PgColumn> GetColumnsWithStatistics() {
        
        List<PgColumn> list = new List<PgColumn>();

        foreach(PgColumn column in _columns) {
            if (column.GetStatistics() != null) {
                list.Add(column);
            }
        }

        return list;
    }
}
}