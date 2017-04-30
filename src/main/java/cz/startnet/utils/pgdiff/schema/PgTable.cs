using System;
using System.Collections.Generic;
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

        public string Name { get; set; }

        public string Tablespace { get; set; }

        public string With { get; set; }

        public List<PgColumn> Columns { get; set; } = new List<PgColumn>();

        public List<PgConstraint> Constraints { get; set; } = new List<PgConstraint>();

        public List<PgIndex> Indexes { get; set; } = new List<PgIndex>();

        public List<string> Inherits { get; set; } = new List<string>();

        public List<PgTrigger> Triggers { get; set; } = new List<PgTrigger>();

        public PgColumn GetColumn(string name)
        {
            foreach (var column in Columns)
                if (column.Name.Equals(name))
                    return column;

            return null;
        }

        public PgConstraint GetConstraint(string name)
        {
            foreach (var constraint in Constraints)
                if (constraint.Name.Equals(name))
                    return constraint;

            return null;
        }


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

                if ("OIDS=false".Equals(With, StringComparison.InvariantCultureIgnoreCase))
                {
                    sbSql.Append("WITHOUT OIDS");
                }
                else
                {
                    sbSql.Append("WITH ");

                    if ("OIDS".Equals(With, StringComparison.InvariantCultureIgnoreCase)
                        || "OIDS=true".Equals(With, StringComparison.InvariantCultureIgnoreCase))
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

            if (string.IsNullOrEmpty(Comment))
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


        public string GetDropSql()
        {
            return "DROP TABLE " + PgDiffUtils.GetQuotedName(Name) + ";";
        }


        public PgIndex GetIndex(string name)
        {
            foreach (var index in Indexes)
                if (index.Name.Equals(name))
                    return index;

            return null;
        }


        public PgTrigger GetTrigger(string name)
        {
            foreach (var trigger in Triggers)
                if (trigger.Name.Equals(name))
                    return trigger;

            return null;
        }


        public List<PgIndex> GetIndexes()
        {
            return new List<PgIndex>(Indexes);
        }


        public void AddInherits(string tableName)
        {
            Inherits.Add(tableName);
        }


        public List<string> GetInherits()
        {
            return new List<string>(Inherits);
        }

        public List<PgTrigger> GetTriggers()
        {
            return new List<PgTrigger>(Triggers);
        }

        public void AddColumn(PgColumn column)
        {
            Columns.Add(column);
        }


        public void AddConstraint(PgConstraint constraint)
        {
            Constraints.Add(constraint);
        }


        public void AddIndex(PgIndex index)
        {
            Indexes.Add(index);
        }


        public void AddTrigger(PgTrigger trigger)
        {
            Triggers.Add(trigger);
        }


        public bool ContainsColumn(string name)
        {
            foreach (var column in Columns)
                if (column.Name.Equals(name))
                    return true;

            return false;
        }


        public bool ContainsConstraint(string name)
        {
            foreach (var constraint in Constraints)
                if (constraint.Name.Equals(name))
                    return true;

            return false;
        }


        public bool ContainsIndex(string name)
        {
            foreach (var index in Indexes)
                if (index.Name.Equals(name))
                    return true;

            return false;
        }


        private List<PgColumn> GetColumnsWithStatistics()
        {
            var list = new List<PgColumn>();

            foreach (var column in Columns)
                if (column.Statistics != null)
                    list.Add(column);

            return list;
        }
    }
}