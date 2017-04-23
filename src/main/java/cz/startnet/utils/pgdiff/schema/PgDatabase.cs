
using System;
using System.Collections.Generic;

namespace cz.startnet.utils.pgdiff.schema {






public class PgDatabase {

    
    private List<PgSchema> schemas = new ArrayList<PgSchema>(1);
    
    
    private List<String> ignoredStatements = new ArrayList<String>();
    
    private PgSchema defaultSchema;
    
    private String comment;

    
    public PgDatabase() {
        schemas.add(new PgSchema("public"));
        defaultSchema = schemas.get(0);
    }

    
    public String getComment() {
        return comment;
    }

    
    public void setComment(String comment) {
        this.comment = comment;
    }

    
    public void setDefaultSchema(String name) {
        defaultSchema = getSchema(name);
    }

    
    public PgSchema getDefaultSchema() {
        return defaultSchema;
    }

    
    public List<String> getIgnoredStatements() {
        return Collections.unmodifiableList(ignoredStatements);
    }

    
    public void addIgnoredStatement(String ignoredStatement) {
        ignoredStatements.add(ignoredStatement);
    }

    
    public PgSchema getSchema(String name) {
        if (name == null) {
            return getDefaultSchema();
        }

        for (PgSchema schema : schemas) {
            if (schema.getName().equals(name)) {
                return schema;
            }
        }

        return null;
    }

    
    public List<PgSchema> getSchemas() {
        return Collections.unmodifiableList(schemas);
    }

    
    public void addSchema(PgSchema schema) {
        schemas.add(schema);
    }
}
}