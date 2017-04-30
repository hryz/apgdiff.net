using System;
using System.Collections.Generic;
using System.Text;

namespace pgdiff.schema {






public class PgSchema {

    
    
    private List<PgFunction> _functions = new List<PgFunction>();
    
    
    private List<PgSequence> _sequences = new List<PgSequence>();
    
    
    private List<PgTable> _tables = new List<PgTable>();
    
    
    private List<PgView> _views = new List<PgView>();
    
    
    private List<PgIndex> _indexes = new List<PgIndex>();
    
    
    private List<PgConstraint> _primaryKeys = new List<PgConstraint>();
    
    private String _name;
    
    private String _authorization;
    
    private String _definition;
    
    private String _comment;

    
    public PgSchema(String name) {
        this._name = name;
    }

    
    public void SetAuthorization(String authorization) {
        this._authorization = authorization;
    }

    
    public String GetAuthorization() {
        return _authorization;
    }

    
    public String GetComment() {
        return _comment;
    }

    
    public void SetComment(String comment) {
        this._comment = comment;
    }

    
    public String GetDefinition() {
        return _definition;
    }

    
    public void SetDefinition(String definition) {
        this._definition = definition;
    }

    
    public String GetCreationSql() {
        StringBuilder sbSql = new StringBuilder(50);
        sbSql.Append("CREATE SCHEMA ");
        sbSql.Append(PgDiffUtils.GetQuotedName(GetName()));

        if (GetAuthorization() != null) {
            sbSql.Append(" AUTHORIZATION ");
            sbSql.Append(PgDiffUtils.GetQuotedName(GetAuthorization()));
        }

        sbSql.Append(';');

        if ( !String.IsNullOrEmpty(_comment)) {
            sbSql.Append("\n\nCOMMENT ON SCHEMA ");
            sbSql.Append(PgDiffUtils.GetQuotedName(_name));
            sbSql.Append(" IS ");
            sbSql.Append(_comment);
            sbSql.Append(';');
        }

        return sbSql.ToString();
    }

    
    public PgFunction GetFunction(String signature) {
        foreach(PgFunction function in _functions) {
            if (function.GetSignature().Equals(signature)) {
                return function;
            }
        }

        return null;
    }

    
    public List<PgFunction> GetFunctions() {
        return new List<PgFunction>(_functions);
    }

    
    public String GetName() {
        return _name;
    }

    
    public PgIndex GetIndex(String name) {
        foreach(PgIndex index in _indexes) {
            if (index.GetName().Equals(name)) {
                return index;
            }
        }

        return null;
    }

    
    public PgConstraint GetPrimaryKey(String name) {
        foreach(PgConstraint constraint in _primaryKeys) {
            if (constraint.GetName().Equals(name)) {
                return constraint;
            }
        }

        return null;
    }

    
    public PgSequence GetSequence(String name) {
        foreach(PgSequence sequence in _sequences) {
            if (sequence.GetName().Equals(name)) {
                return sequence;
            }
        }

        return null;
    }

    
    public List<PgIndex> GetIndexes() {
        return new List<PgIndex>(_indexes);
    }

    
    public List<PgConstraint> GetPrimaryKeys() {
        return new List<PgConstraint>(_primaryKeys);
    }

    
    public List<PgSequence> GetSequences() {
        return new List<PgSequence>(_sequences);
    }

    
    public PgTable GetTable(String name) {
        foreach(PgTable table in _tables) {
            if (table.GetName().Equals(name)) {
                return table;
            }
        }

        return null;
    }

    
    public List<PgTable> GetTables() {
        return new List<PgTable>(_tables);
    }

    
    public PgView GetView(String name) {
        foreach(PgView view in _views) {
            if (view.GetName().Equals(name)) {
                return view;
            }
        }

        return null;
    }

    
    public List<PgView> GetViews() {
        return new List<PgView>(_views);
    }

    
    public void AddIndex(PgIndex index) {
        _indexes.Add(index);
    }

    
    public void AddPrimaryKey(PgConstraint primaryKey) {
        _primaryKeys.Add(primaryKey);
    }

    
    public void AddFunction(PgFunction function) {
        _functions.Add(function);
    }

    
    public void AddSequence(PgSequence sequence) {
        _sequences.Add(sequence);
    }

    
    public void AddTable(PgTable table) {
        _tables.Add(table);
    }

    
    public void AddView(PgView view) {
        _views.Add(view);
    }

    
    public bool ContainsFunction(String signature) {
        foreach(PgFunction function in _functions) {
            if (function.GetSignature().Equals(signature)) {
                return true;
            }
        }

        return false;
    }

    
    public bool ContainsSequence(String name) {
        foreach(PgSequence sequence in _sequences) {
            if (sequence.GetName().Equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public bool ContainsTable(String name) {
        foreach(PgTable table in _tables) {
            if (table.GetName().Equals(name)) {
                return true;
            }
        }

        return false;
    }

    
    public bool ContainsView(String name) {
        foreach(PgView view in _views) {
            if (view.GetName().Equals(name)) {
                return true;
            }
        }

        return false;
    }
}
}