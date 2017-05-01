using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pgdiff.schema
{
    public class PgTable
    {
        public PgTable(string name)
        {
            Name = name;
        }

        public string ClusterIndexName { get; set; }

        public string Comment { get; set; }

        public string Name { get; }

        public string Tablespace { get; set; }

        public string With { get; set; }

        public List<PgColumn> Columns { get; set; } = new List<PgColumn>();

        public List<PgConstraint> Constraints { get; set; } = new List<PgConstraint>();

        public List<PgIndex> Indexes { get; set; } = new List<PgIndex>();

        public List<string> Inherits { get; set; } = new List<string>();

        public List<PgTrigger> Triggers { get; set; } = new List<PgTrigger>();

        public PgColumn GetColumn(string name) => Columns.FirstOrDefault(column => column.Name.Equals(name));

        public PgConstraint GetConstraint(string name) => Constraints.FirstOrDefault(constraint => constraint.Name.Equals(name));


        public string GetCreationSql()
        {
            var sbSql = new StringBuilder(1000);
            sbSql.Append("CREATE TABLE ");
            sbSql.Append(PgDiffUtils.GetQuotedName(Name));
            sbSql.Append(" (\n");

            var first = true;

            if (Columns.Count == 0)
            {
                sbSql.Append(')');
            }
            else
            {
                foreach (var column in Columns)
                {
                    if (first)
                        first = false;
                    else
                        sbSql.Append(",\n");

                    sbSql.Append("\t");
                    sbSql.Append(column.GetFullDefinition(false));
                }

                sbSql.Append("\n)");
            }

            if (Inherits != null && Inherits.Count != 0)
            {
                sbSql.Append("\nINHERITS (");

                first = true;

                foreach (var tableName in Inherits)
                {
                    if (first)
                        first = false;
                    else
                        sbSql.Append(", ");

                    sbSql.Append(tableName);
                }

                sbSql.Append(")");
            }

            if (!string.IsNullOrEmpty(With))
            {
                sbSql.Append("\n");

                if ("OIDS=false".EqualsIgnoreCase(With))
                {
                    sbSql.Append("WITHOUT OIDS");
                }
                else
                {
                    sbSql.Append("WITH ");

                    if ("OIDS".EqualsIgnoreCase(With) || "OIDS=true".EqualsIgnoreCase(With))
                        sbSql.Append("OIDS");
                    else
                        sbSql.Append(With);
                }
            }

            if (!string.IsNullOrEmpty(Tablespace))
            {
                sbSql.Append("\nTABLESPACE ");
                sbSql.Append(Tablespace);
            }

            sbSql.Append(';');

            foreach (var column in GetColumnsWithStatistics())
            {
                sbSql.Append("\nALTER TABLE ONLY ");
                sbSql.Append(PgDiffUtils.GetQuotedName(Name));
                sbSql.Append(" ALTER COLUMN ");
                sbSql.Append(PgDiffUtils.GetQuotedName(column.Name));
                sbSql.Append(" SET STATISTICS ");
                sbSql.Append(column.Statistics);
                sbSql.Append(';');
            }

            if (!string.IsNullOrEmpty(Comment))
            {
                sbSql.Append("\n\nCOMMENT ON TABLE ");
                sbSql.Append(PgDiffUtils.GetQuotedName(Name));
                sbSql.Append(" IS ");
                sbSql.Append(Comment);
                sbSql.Append(';');
            }

            foreach (var column in Columns)
                if (!string.IsNullOrEmpty(column.Comment))
                {
                    sbSql.Append("\n\nCOMMENT ON COLUMN ");
                    sbSql.Append(PgDiffUtils.GetQuotedName(Name));
                    sbSql.Append('.');
                    sbSql.Append(PgDiffUtils.GetQuotedName(column.Name));
                    sbSql.Append(" IS ");
                    sbSql.Append(column.Comment);
                    sbSql.Append(';');
                }

            return sbSql.ToString();
        }


        public string GetDropSql() => $"DROP TABLE {PgDiffUtils.GetQuotedName(Name)};";
        
        public PgIndex GetIndex(string name) => Indexes.FirstOrDefault(index => index.Name.Equals(name));
        
        public PgTrigger GetTrigger(string name) => Triggers.FirstOrDefault(trigger => trigger.Name.Equals(name));
        
        public List<PgIndex> GetIndexes() => Indexes;
        
        public void AddInherits(string tableName) => Inherits.Add(tableName);
        
        public List<string> GetInherits() => Inherits;

        public List<PgTrigger> GetTriggers() => Triggers;

        public void AddColumn(PgColumn column) => Columns.Add(column);
        
        public void AddConstraint(PgConstraint constraint) => Constraints.Add(constraint);
        
        public void AddIndex(PgIndex index) => Indexes.Add(index);
        
        public void AddTrigger(PgTrigger trigger) => Triggers.Add(trigger);
        
        public bool ContainsColumn(string name) => Columns.Any(column => column.Name.Equals(name));
        
        public bool ContainsConstraint(string name) => Constraints.Any(constraint => constraint.Name.Equals(name));
        
        public bool ContainsIndex(string name) => Indexes.Any(index => index.Name.Equals(name));
        
        private IEnumerable<PgColumn> GetColumnsWithStatistics() => Columns.Where(column => column.Statistics != null);
    }
}