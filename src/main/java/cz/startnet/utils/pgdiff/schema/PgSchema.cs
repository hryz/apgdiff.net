
package cz.startnet.utils.pgdiff.schema;

import cz.startnet.utils.pgdiff.PgDiffUtils;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;


public class PgSchema {

    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private final List<PgFunction> functions = new ArrayList<PgFunction>();
    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private final List<PgSequence> sequences = new ArrayList<PgSequence>();
    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private final List<PgTable> tables = new ArrayList<PgTable>();
    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private final List<PgView> views = new ArrayList<PgView>();
    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private final List<PgIndex> indexes = new ArrayList<PgIndex>();
    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private final List<PgConstraint> primaryKeys =
            new ArrayList<PgConstraint>();
    
    private final String name;
    
    private String authorization;
    
    private String definition;
    
    private String comment;

    
    public PgSchema(final String name) {
        this.name = name;
    }

    
    public void setAuthorization(final String authorization) {
        this.authorization = authorization;
    }

    
    public String getAuthorization() {
        return authorization;
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(final String comment) {
        this.comment = comment;
    }

    
    public String getDefinition() {
        return definition;
    }

    
    public void setDefinition(final String definition) {
        this.definition = definition;
    }

    
    public String getCreationSQL() {
        final StringBuilder sbSQL = new StringBuilder(50);
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

    
    public PgFunction getFunction(final String signature) {
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

    
    public PgIndex getIndex(final String name) {
        for (PgIndex index : indexes) {
            if (index.getName().equals(name)) {
                return index;
            }
        }

        return null;
    }

    
    public PgConstraint getPrimaryKey(final String name) {
        for (PgConstraint constraint : primaryKeys) {
            if (constraint.getName().equals(name)) {
                return constraint;
            }
        }

        return null;
    }

    
    public PgSequence getSequence(final String name) {
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

    
    public PgTable getTable(final String name) {
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

    
    public PgView getView(final String name) {
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

    
    public void addIndex(final PgIndex index) {
        indexes.add(index);
    }

    
    public void addPrimaryKey(final PgConstraint primaryKey) {
        primaryKeys.add(primaryKey);
    }

    
    public void addFunction(final PgFunction function) {
        functions.add(function);
    }

    
    public void addSequence(final PgSequence sequence) {
        sequences.add(sequence);
    }

    
    public void addTable(final PgTable table) {
        tables.add(table);
    }

    
    public void addView(final PgView view) {
        views.add(view);
    }

    
    public boolean containsFunction(final String signature) {
        for (PgFunction function : functions) {
            if (function.getSignature().equals(signature)) {
                return true;
            }
        }

        return false;
    }

    
    public boolean containsSequence(final String name) {
        for (PgSequence sequence : sequences) {
            if (sequence.getName().equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public boolean containsTable(final String name) {
        for (PgTable table : tables) {
            if (table.getName().equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public boolean containsView(final String name) {
        for (PgView view : views) {
            if (view.getName().equals(name)) {
                return true;
            }
        }

        return false;
    }
}
