using System.Collections.Generic;
using System.Text;

namespace pgdiff.schema
{
    public class PgSchema
    {
        public string Authorization { get; set; }

        public string Comment { get; set; }

        public string Definition { get; set; }

        public string Name { get; set; }

        private readonly List<PgFunction> _functions = new List<PgFunction>();

        private readonly List<PgIndex> _indexes = new List<PgIndex>();

        private readonly List<PgConstraint> _primaryKeys = new List<PgConstraint>();

        private readonly List<PgSequence> _sequences = new List<PgSequence>();

        private readonly List<PgTable> _tables = new List<PgTable>();

        private readonly List<PgView> _views = new List<PgView>();


        public PgSchema(string name)
        {
            Name = name;
        }

        public string GetCreationSql()
        {
            var sbSql = new StringBuilder(50);
            sbSql.Append("CREATE SCHEMA ");
            sbSql.Append(PgDiffUtils.GetQuotedName(Name));

            if (Authorization != null)
            {
                sbSql.Append(" AUTHORIZATION ");
                sbSql.Append(PgDiffUtils.GetQuotedName(Authorization));
            }

            sbSql.Append(';');

            if (!string.IsNullOrEmpty(Comment))
            {
                sbSql.Append("\n\nCOMMENT ON SCHEMA ");
                sbSql.Append(PgDiffUtils.GetQuotedName(Name));
                sbSql.Append(" IS ");
                sbSql.Append(Comment);
                sbSql.Append(';');
            }

            return sbSql.ToString();
        }


        public PgFunction GetFunction(string signature)
        {
            foreach (var function in _functions)
                if (function.GetSignature().Equals(signature))
                    return function;

            return null;
        }


        public List<PgFunction> GetFunctions()
        {
            return new List<PgFunction>(_functions);
        }

        public PgIndex GetIndex(string name)
        {
            foreach (var index in _indexes)
                if (index.Name.Equals(name))
                    return index;

            return null;
        }


        public PgConstraint GetPrimaryKey(string name)
        {
            foreach (var constraint in _primaryKeys)
                if (constraint.Name.Equals(name))
                    return constraint;

            return null;
        }


        public PgSequence GetSequence(string name)
        {
            foreach (var sequence in _sequences)
                if (sequence.Name.Equals(name))
                    return sequence;

            return null;
        }


        public List<PgIndex> GetIndexes()
        {
            return new List<PgIndex>(_indexes);
        }


        public List<PgConstraint> GetPrimaryKeys()
        {
            return new List<PgConstraint>(_primaryKeys);
        }


        public List<PgSequence> GetSequences()
        {
            return new List<PgSequence>(_sequences);
        }


        public PgTable GetTable(string name)
        {
            foreach (var table in _tables)
                if (table.Name.Equals(name))
                    return table;

            return null;
        }


        public List<PgTable> GetTables()
        {
            return new List<PgTable>(_tables);
        }


        public PgView GetView(string name)
        {
            foreach (var view in _views)
                if (view.Name.Equals(name))
                    return view;

            return null;
        }


        public List<PgView> GetViews()
        {
            return new List<PgView>(_views);
        }


        public void AddIndex(PgIndex index)
        {
            _indexes.Add(index);
        }


        public void AddPrimaryKey(PgConstraint primaryKey)
        {
            _primaryKeys.Add(primaryKey);
        }


        public void AddFunction(PgFunction function)
        {
            _functions.Add(function);
        }


        public void AddSequence(PgSequence sequence)
        {
            _sequences.Add(sequence);
        }


        public void AddTable(PgTable table)
        {
            _tables.Add(table);
        }


        public void AddView(PgView view)
        {
            _views.Add(view);
        }


        public bool ContainsFunction(string signature)
        {
            foreach (var function in _functions)
                if (function.GetSignature().Equals(signature))
                    return true;

            return false;
        }


        public bool ContainsSequence(string name)
        {
            foreach (var sequence in _sequences)
                if (sequence.Name.Equals(name))
                    return true;

            return false;
        }


        public bool ContainsTable(string name)
        {
            foreach (var table in _tables)
                if (table.Name.Equals(name))
                    return true;

            return false;
        }


        public bool ContainsView(string name)
        {
            foreach (var view in _views)
                if (view.Name.Equals(name))
                    return true;

            return false;
        }
    }
}