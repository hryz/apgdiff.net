
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
        for (PgFunction function : functions) {
            if (function.getSignature().equals(signature)) {
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
        for (PgIndex index : indexes) {
            if (index.getName().equals(name)) {
                return index;
            }
        }

        return null;
    }

    
    public PgConstraint getPrimaryKey(String name) {
        for (PgConstraint constraint : primaryKeys) {
            if (constraint.getName().equals(name)) {
                return constraint;
            }
        }

        return null;
    }

    
    public PgSequence getSequence(String name) {
        for (PgSequence sequence : sequences) {
            if (sequence.getName().equals(name)) {
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
        for (PgTable table : tables) {
            if (table.getName().equals(name)) {
                return table;
            }
        }

        return null;
    }

    
    public List<PgTable> getTables() {
        return Collections.unmodifiableList(tables);
    }

    
    public PgView getView(String name) {
        for (PgView view : views) {
            if (view.getName().equals(name)) {
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

    
    public boolean containsFunction(String signature) {
        for (PgFunction function : functions) {
            if (function.getSignature().equals(signature)) {
                return true;
            }
        }

        return false;
    }

    
    public boolean containsSequence(String name) {
        for (PgSequence sequence : sequences) {
            if (sequence.getName().equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public boolean containsTable(String name) {
        for (PgTable table : tables) {
            if (table.getName().equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public boolean containsView(String name) {
        for (PgView view : views) {
            if (view.getName().equals(name)) {
                return true;
            }
        }

        return false;
    }
}
}