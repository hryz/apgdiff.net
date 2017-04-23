using System;
using System.Collections.Generic;

namespace pgdiff.schema {






public class PgDatabase {

    
    private List<PgSchema> schemas = new List<PgSchema>();


    private List<String> ignoredStatements = new List<string>();
    
    private PgSchema defaultSchema;
    
    private String comment;

    
    public PgDatabase() {
        schemas.Add(new PgSchema("public"));
        defaultSchema = schemas[0];
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
        return new List<string>(ignoredStatements);
    }

    
    public void addIgnoredStatement(String ignoredStatement) {
        ignoredStatements.Add(ignoredStatement);
    }

    
    public PgSchema getSchema(String name) {
        if (name == null) {
            return getDefaultSchema();
        }

        foreach(PgSchema schema in schemas) {
            if (schema.getName().Equals(name)) {
                return schema;
            }
        }

        return null;
    }

    
    public List<PgSchema> getSchemas() {
        return new List<PgSchema>(schemas);
    }

    
    public void addSchema(PgSchema schema) {
        schemas.Add(schema);
    }
}
}