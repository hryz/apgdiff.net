using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pgdiff.schema
{
    public class PgSchema
    {
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

        public string Authorization { get; set; }

        public string Comment { get; set; }

        public string Definition { get; set; }

        public string Name { get; }

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


        public PgFunction GetFunction(string signature) => _functions.FirstOrDefault(function => function.GetSignature().Equals(signature));

        public List<PgFunction> GetFunctions() => _functions;

        public PgIndex GetIndex(string name) => _indexes.FirstOrDefault(index => index.Name.Equals(name));

        public PgConstraint GetPrimaryKey(string name) => _primaryKeys.FirstOrDefault(constraint => constraint.Name.Equals(name));

        public PgSequence GetSequence(string name) => _sequences.FirstOrDefault(sequence => sequence.Name.Equals(name));

        public List<PgIndex> GetIndexes() => _indexes;

        public List<PgConstraint> GetPrimaryKeys() => _primaryKeys;

        public List<PgSequence> GetSequences() => _sequences;

        public PgTable GetTable(string name) => _tables.FirstOrDefault(table => table.Name.Equals(name));

        public List<PgTable> GetTables() => _tables;

        public PgView GetView(string name) => _views.FirstOrDefault(view => view.Name.Equals(name));
        
        public List<PgView> GetViews() => _views;

        public void AddIndex(PgIndex index) => _indexes.Add(index);

        public void AddPrimaryKey(PgConstraint primaryKey) => _primaryKeys.Add(primaryKey);

        public void AddFunction(PgFunction function) => _functions.Add(function);

        public void AddSequence(PgSequence sequence) => _sequences.Add(sequence);
        
        public void AddTable(PgTable table) => _tables.Add(table);

        public void AddView(PgView view) => _views.Add(view);

        public bool ContainsFunction(string signature) => _functions.Any(function => function.GetSignature().Equals(signature));
        
        public bool ContainsSequence(string name) => _sequences.Any(sequence => sequence.Name.Equals(name));
        
        public bool ContainsTable(string name) => _tables.Any(table => table.Name.Equals(name));
        
        public bool ContainsView(string name) => _views.Any(view => view.Name.Equals(name));
    }
}