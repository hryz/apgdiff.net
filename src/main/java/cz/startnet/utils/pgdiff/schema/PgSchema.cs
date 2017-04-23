using System;
using System.Collections.Generic;
using System.Text;

namespace pgdiff.schema {






public class PgSchema {

    
    
    private List<PgFunction> functions = new List<PgFunction>();
    
    
    private List<PgSequence> sequences = new List<PgSequence>();
    
    
    private List<PgTable> tables = new List<PgTable>();
    
    
    private List<PgView> views = new List<PgView>();
    
    
    private List<PgIndex> indexes = new List<PgIndex>();
    
    
    private List<PgConstraint> primaryKeys = new List<PgConstraint>();
    
    private String name;
    
    private String authorization;
    
    private String definition;
    
    private String comment;

    
    public PgSchema(String name) {
        this.name = name;
    }

    
    public void setAuthorization(String authorization) {
        this.authorization = authorization;
    }

    
    public String getAuthorization() {
        return authorization;
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(String comment) {
        this.comment = comment;
    }

    
    public String getDefinition() {
        return definition;
    }

    
    public void setDefinition(String definition) {
        this.definition = definition;
    }

    
    public String getCreationSQL() {
        StringBuilder sbSQL = new StringBuilder(50);
        sbSQL.Append("CREATE SCHEMA ");
        sbSQL.Append(PgDiffUtils.getQuotedName(getName()));

        if (getAuthorization() != null) {
            sbSQL.Append(" AUTHORIZATION ");
            sbSQL.Append(PgDiffUtils.getQuotedName(getAuthorization()));
        }

        sbSQL.Append(';');

        if ( !String.IsNullOrEmpty(comment)) {
            sbSQL.Append("\n\nCOMMENT ON SCHEMA ");
            sbSQL.Append(PgDiffUtils.getQuotedName(name));
            sbSQL.Append(" IS ");
            sbSQL.Append(comment);
            sbSQL.Append(';');
        }

        return sbSQL.ToString();
    }

    
    public PgFunction getFunction(String signature) {
        foreach(PgFunction function in functions) {
            if (function.getSignature().Equals(signature)) {
                return function;
            }
        }

        return null;
    }

    
    public List<PgFunction> getFunctions() {
        return new List<PgFunction>(functions);
    }

    
    public String getName() {
        return name;
    }

    
    public PgIndex getIndex(String name) {
        foreach(PgIndex index in indexes) {
            if (index.getName().Equals(name)) {
                return index;
            }
        }

        return null;
    }

    
    public PgConstraint getPrimaryKey(String name) {
        foreach(PgConstraint constraint in primaryKeys) {
            if (constraint.getName().Equals(name)) {
                return constraint;
            }
        }

        return null;
    }

    
    public PgSequence getSequence(String name) {
        foreach(PgSequence sequence in sequences) {
            if (sequence.getName().Equals(name)) {
                return sequence;
            }
        }

        return null;
    }

    
    public List<PgIndex> getIndexes() {
        return new List<PgIndex>(indexes);
    }

    
    public List<PgConstraint> getPrimaryKeys() {
        return new List<PgConstraint>(primaryKeys);
    }

    
    public List<PgSequence> getSequences() {
        return new List<PgSequence>(sequences);
    }

    
    public PgTable getTable(String name) {
        foreach(PgTable table in tables) {
            if (table.getName().Equals(name)) {
                return table;
            }
        }

        return null;
    }

    
    public List<PgTable> getTables() {
        return new List<PgTable>(tables);
    }

    
    public PgView getView(String name) {
        foreach(PgView view in views) {
            if (view.getName().Equals(name)) {
                return view;
            }
        }

        return null;
    }

    
    public List<PgView> getViews() {
        return new List<PgView>(views);
    }

    
    public void addIndex(PgIndex index) {
        indexes.Add(index);
    }

    
    public void addPrimaryKey(PgConstraint primaryKey) {
        primaryKeys.Add(primaryKey);
    }

    
    public void addFunction(PgFunction function) {
        functions.Add(function);
    }

    
    public void addSequence(PgSequence sequence) {
        sequences.Add(sequence);
    }

    
    public void addTable(PgTable table) {
        tables.Add(table);
    }

    
    public void addView(PgView view) {
        views.Add(view);
    }

    
    public bool containsFunction(String signature) {
        foreach(PgFunction function in functions) {
            if (function.getSignature().Equals(signature)) {
                return true;
            }
        }

        return false;
    }

    
    public bool containsSequence(String name) {
        foreach(PgSequence sequence in sequences) {
            if (sequence.getName().Equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public bool containsTable(String name) {
        foreach(PgTable table in tables) {
            if (table.getName().Equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public bool containsView(String name) {
        foreach(PgView view in views) {
            if (view.getName().Equals(name)) {
                return true;
            }
        }

        return false;
    }
}
}