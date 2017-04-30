using System;
using System.Collections.Generic;

namespace pgdiff.schema {






public class PgDatabase {

    
    private List<PgSchema> _schemas = new List<PgSchema>();


    private List<String> _ignoredStatements = new List<string>();
    
    private PgSchema _defaultSchema;
    
    private String _comment;

    
    public PgDatabase() {
        _schemas.Add(new PgSchema("public"));
        _defaultSchema = _schemas[0];
    }

    
    public String GetComment() {
        return _comment;
    }

    
    public void SetComment(String comment) {
        this._comment = comment;
    }

    
    public void SetDefaultSchema(String name) {
        _defaultSchema = GetSchema(name);
    }

    
    public PgSchema GetDefaultSchema() {
        return _defaultSchema;
    }

    
    public List<String> GetIgnoredStatements() {
        return new List<string>(_ignoredStatements);
    }

    
    public void AddIgnoredStatement(String ignoredStatement) {
        _ignoredStatements.Add(ignoredStatement);
    }

    
    public PgSchema GetSchema(String name) {
        if (name == null) {
            return GetDefaultSchema();
        }

        foreach(PgSchema schema in _schemas) {
            if (schema.GetName().Equals(name)) {
                return schema;
            }
        }

        return null;
    }

    
    public List<PgSchema> GetSchemas() {
        return new List<PgSchema>(_schemas);
    }

    
    public void AddSchema(PgSchema schema) {
        _schemas.Add(schema);
    }
}
}