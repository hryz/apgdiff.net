
package cz.startnet.utils.pgdiff.schema;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;


public class PgDatabase {

    
    private final List<PgSchema> schemas = new ArrayList<PgSchema>(1);
    
    @SuppressWarnings("CollectionWithoutInitialCapacity")
    private final List<String> ignoredStatements = new ArrayList<String>();
    
    private PgSchema defaultSchema;
    
    private String comment;

    
    public PgDatabase() {
        schemas.add(new PgSchema("public"));
        defaultSchema = schemas.get(0);
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(final String comment) {
        this.comment = comment;
    }

    
    public void setDefaultSchema(final String name) {
        defaultSchema = getSchema(name);
    }

    
    public PgSchema getDefaultSchema() {
        return defaultSchema;
    }

    
    public List<String> getIgnoredStatements() {
        return Collections.unmodifiableList(ignoredStatements);
    }

    
    public void addIgnoredStatement(final String ignoredStatement) {
        ignoredStatements.add(ignoredStatement);
    }

    
    public PgSchema getSchema(final String name) {
        if (name == null) {
            return getDefaultSchema();
        }

        for (final PgSchema schema : schemas) {
            if (schema.getName().equals(name)) {
                return schema;
            }
        }

        return null;
    }

    
    public List<PgSchema> getSchemas() {
        return Collections.unmodifiableList(schemas);
    }

    
    public void addSchema(final PgSchema schema) {
        schemas.add(schema);
    }
}
