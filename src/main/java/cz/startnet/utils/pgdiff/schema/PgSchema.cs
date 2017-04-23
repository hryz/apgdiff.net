
using System;
using System.Collections.Generic;
using System.Text;

namespace cz.startnet.utils.pgdiff.schema {






public class PgSchema {

    
    
    private List<PgFunction> functions = new ArrayList<PgFunction>();
    
    
    private List<PgSequence> sequences = new ArrayList<PgSequence>();
    
    
    private List<PgTable> tables = new ArrayList<PgTable>();
    
    
    private List<PgView> views = new ArrayList<PgView>();
    
    
    private List<PgIndex> indexes = new ArrayList<PgIndex>();
    
    
    private List<PgConstraint> primaryKeys =
            new ArrayList<PgConstraint>();
    
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
        sbSQL.append("CREATE SCHEMA ");
        sbSQL.append(PgDiffUtils.getQuotedName(getName()));

        if (getAuthorization() != null) {
            sbSQL.append(" AUTHORIZATION ");
            sbSQL.append(PgDiffUtils.getQuotedName(getAuthorization()));
        }

        sbSQL.append(';');

        if (comment != null && !comment.isEmpty()) {
            sbSQL.append("\n\nCOMMENT ON SCHEMA ");
            sbSQL.append(PgDiffUtils.getQuotedName(name));
            sbSQL.append(" IS ");
            sbSQL.append(comment);
            sbSQL.append(';');
        }

        return sbSQL.toString();
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
        return Collections.unmodifiableList(functions);
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
        return Collections.unmodifiableList(indexes);
    }

    
    public List<PgConstraint> getPrimaryKeys() {
        return Collections.unmodifiableList(primaryKeys);
    }

    
    public List<PgSequence> getSequences() {
        return Collections.unmodifiableList(sequences);
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
        return Collections.unmodifiableList(tables);
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
        return Collections.unmodifiableList(views);
    }

    
    public void addIndex(PgIndex index) {
        indexes.add(index);
    }

    
    public void addPrimaryKey(PgConstraint primaryKey) {
        primaryKeys.add(primaryKey);
    }

    
    public void addFunction(PgFunction function) {
        functions.add(function);
    }

    
    public void addSequence(PgSequence sequence) {
        sequences.add(sequence);
    }

    
    public void addTable(PgTable table) {
        tables.add(table);
    }

    
    public void addView(PgView view) {
        views.add(view);
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